@echo off
echo ============================================
echo CHD Coordination - Metrics Test
echo ============================================
echo.

echo [1] Restarting monitoring stack...
cd /d "%~dp0"
docker-compose -f docker-compose.monitoring.yml down
timeout /t 2 /nobreak >nul
docker-compose -f docker-compose.monitoring.yml up -d
echo.

echo [2] Waiting for services to start (15 seconds)...
timeout /t 15 /nobreak >nul
echo.

echo [3] Opening Prometheus targets page...
start http://localhost:9090/targets
timeout /t 2 /nobreak >nul
echo.

echo [4] Opening metrics endpoint...
start http://localhost:9091/metrics
timeout /t 2 /nobreak >nul
echo.

echo [5] Opening Grafana dashboard...
timeout /t 3 /nobreak >nul
start http://localhost:3000/d/chd-coordination
echo.

echo ============================================
echo Instructions:
echo ============================================
echo 1. Check Prometheus targets (all should be UP)
echo 2. Check metrics endpoint (should show chd_* metrics after running tests)
echo 3. In Grafana:
echo    - Login: admin/admin
echo    - You should see the CHD Coordination dashboard
echo    - Run load tests from Visual Studio (F5)
echo    - Metrics will appear in real-time
echo.
echo Press any key to exit...
pause >nul
