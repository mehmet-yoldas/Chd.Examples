@echo off
REM CHD Coordination - NBomber Load Tests Quick Start (Windows)

echo ======================================
echo CHD Coordination - Load Tests
echo ======================================
echo.

REM Check if Redis is running
docker ps | findstr "redis" >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] Redis is not running.
    echo.
    set /p start_redis="Do you want to start Redis? (y/n): "
    if /i "%start_redis%"=="y" (
        echo.
        echo [INFO] Starting Redis...
        docker run -d -p 6379:6379 --name chd-redis redis:7-alpine
        timeout /t 3 /nobreak >nul
    ) else (
        echo.
        echo [ERROR] Redis is required for load tests.
        echo Please start Redis manually:
        echo   docker run -d -p 6379:6379 redis:7-alpine
        pause
        exit /b 1
    )
)

echo [INFO] Redis is running
echo.

REM Check if monitoring stack is running
docker ps | findstr "chd-grafana" >nul 2>&1
if %errorlevel% neq 0 (
    echo [INFO] Monitoring stack is not running.
    set /p start_monitoring="Start Grafana monitoring? (y/n): "
    if /i "%start_monitoring%"=="y" (
        echo.
        call start-monitoring.bat
        echo.
    )
)

REM Run load tests
echo.
echo ======================================
echo Starting NBomber Load Tests
echo ======================================
echo.

cd Chd.LoadTests
dotnet run --configuration Release

cd ..

echo.
echo ======================================
echo Load Tests Completed
echo ======================================
echo.
echo View HTML reports in: ./Chd.LoadTests/reports/
echo.

if exist "docker-compose.monitoring.yml" (
    docker ps | findstr "chd-grafana" >nul 2>&1
    if %errorlevel% equ 0 (
        echo View live metrics:
        echo   http://localhost:3000/d/chd-coordination
        echo.
    )
)

pause
