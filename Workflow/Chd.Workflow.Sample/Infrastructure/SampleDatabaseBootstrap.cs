using Npgsql;

namespace Chd.Workflow.Sample.Infrastructure;

/// <summary>
/// Ensures the sample PostgreSQL server and database exist before Chd.Workflow migrations run.
/// </summary>
public static class SampleDatabaseBootstrap
{
    public static async Task EnsureAsync(string connectionString, string contentRoot, CancellationToken cancellationToken = default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(target.Database))
            throw new InvalidOperationException("Connection string is missing a Database name.");

        var admin = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" };

        switch (await ProbeAsync(target.ConnectionString, cancellationToken))
        {
            case ProbeResult.Ready:
                await EnsurePackageSchemaAsync(target.ConnectionString, cancellationToken);
                Console.WriteLine($"PostgreSQL database '{target.Database}' is reachable. Workflow tables are created on startup if they are missing.");
                return;
            case ProbeResult.DatabaseMissing:
                await CreateDatabaseAsync(admin.ConnectionString, target.Database, cancellationToken);
                await EnsurePackageSchemaAsync(target.ConnectionString, cancellationToken);
                return;
            case ProbeResult.AuthFailed:
                throw new InvalidOperationException(
                    "PostgreSQL is running, but the sample login was rejected. " +
                    "Check ConnectionStrings:PostgreSQL in appsettings.json.");
        }

        Console.WriteLine($"PostgreSQL is not reachable at {target.Host}:{target.Port}. Starting it with Docker...");
        await StartDockerPostgresAsync(contentRoot, cancellationToken);
        await WaitUntilReadyAsync(admin.ConnectionString, cancellationToken);
        await CreateDatabaseAsync(admin.ConnectionString, target.Database, cancellationToken);
        await EnsurePackageSchemaAsync(target.ConnectionString, cancellationToken);
        Console.WriteLine($"PostgreSQL database '{target.Database}' is ready. Workflow tables are created on startup if they are missing.");
    }

    private static async Task CreateDatabaseAsync(string adminConnectionString, string database, CancellationToken cancellationToken)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(database, "^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new InvalidOperationException($"Database name '{database}' is not a simple identifier.");

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        exists.Parameters.AddWithValue("name", database);
        if (await exists.ExecuteScalarAsync(cancellationToken) != null)
            return;

        Console.WriteLine($"Creating PostgreSQL database '{database}'...");
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
        await create.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Chd.Workflow 10.0.3 moves PostgreSQL tables into schema "Workflow".
    /// That schema must exist before the packaged migration runs.
    /// </summary>
    private static async Task EnsurePackageSchemaAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS \"Workflow\"", connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// The published 10.0.3 migrations create <c>workflow_instances</c> without
    /// <c>definition_snapshot_json</c>, which the current model selects.
    /// </summary>
    public static async Task EnsureCurrentColumnsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DO $body$
            BEGIN
              IF EXISTS (
                  SELECT 1 FROM information_schema.tables
                  WHERE table_schema = 'Workflow' AND table_name = 'workflow_instances')
              THEN
                ALTER TABLE "Workflow".workflow_instances
                  ADD COLUMN IF NOT EXISTS definition_snapshot_json TEXT NULL;
              END IF;

              IF EXISTS (
                  SELECT 1 FROM information_schema.tables
                  WHERE table_schema = 'work_flow' AND table_name = 'workflow_instances')
              THEN
                ALTER TABLE work_flow.workflow_instances
                  ADD COLUMN IF NOT EXISTS definition_snapshot_json TEXT NULL;
              END IF;
            END
            $body$;
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task WaitUntilReadyAsync(string connectionString, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ProbeAsync(connectionString, cancellationToken) == ProbeResult.Ready)
                return;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception ex)
            {
                last = ex;
            }
        }

        throw new InvalidOperationException(
            "Docker started the PostgreSQL container, but the server did not accept connections in time.", last);
    }

    private static async Task StartDockerPostgresAsync(string contentRoot, CancellationToken cancellationToken)
    {
        var docker = await DockerStatusAsync(cancellationToken);
        if (docker == DockerStatus.NotInstalled)
        {
            throw new InvalidOperationException(
                "Docker is not installed. This sample creates PostgreSQL in Docker when the database server is not already running. " +
                "Install Docker Desktop, start it, and run the application again: https://docs.docker.com/get-docker/");
        }

        if (docker == DockerStatus.NotRunning)
        {
            throw new InvalidOperationException(
                "Docker is installed, but the Docker engine is not running. Start Docker Desktop and run the application again.");
        }

        var composeFile = Path.Combine(contentRoot, "docker-compose.yml");
        if (!File.Exists(composeFile))
            composeFile = Path.Combine(AppContext.BaseDirectory, "docker-compose.yml");
        if (!File.Exists(composeFile))
            throw new InvalidOperationException("docker-compose.yml was not found next to the sample project.");

        var (code, output) = await RunProcessAsync("docker", $"compose -f \"{composeFile}\" up -d", cancellationToken, TimeSpan.FromMinutes(5));
        if (code != 0)
        {
            throw new InvalidOperationException(
                "Docker could not start the sample PostgreSQL container." + Environment.NewLine + output);
        }
    }

    private static async Task<DockerStatus> DockerStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (code, output) = await RunProcessAsync("docker", "info", cancellationToken, TimeSpan.FromSeconds(20));
            if (code == 0)
                return DockerStatus.Ready;

            if (output.Contains("not recognized", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("No such file", StringComparison.OrdinalIgnoreCase))
                return DockerStatus.NotInstalled;

            return DockerStatus.NotRunning;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return DockerStatus.NotInstalled;
        }
    }

    private static async Task<ProbeResult> ProbeAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return ProbeResult.Ready;
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000")
        {
            return ProbeResult.DatabaseMissing;
        }
        catch (PostgresException ex) when (ex.SqlState == "28P01")
        {
            return ProbeResult.AuthFailed;
        }
        catch (NpgsqlException)
        {
            return ProbeResult.Unreachable;
        }
    }

    private static async Task<(int ExitCode, string Output)> RunProcessAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already exited */ }
            throw new InvalidOperationException($"'{fileName} {arguments}' timed out.");
        }

        var output = (await stdout) + (await stderr);
        return (process.ExitCode, output.Trim());
    }

    private enum ProbeResult
    {
        Ready,
        DatabaseMissing,
        AuthFailed,
        Unreachable,
    }

    private enum DockerStatus
    {
        Ready,
        NotInstalled,
        NotRunning,
    }
}
