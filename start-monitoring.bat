@echo off
REM CHD Coordination - Grafana Monitoring Quick Start (Windows)

echo ======================================
echo CHD Coordination - Monitoring Setup
echo ======================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker is not installed. Please install Docker Desktop first.
    pause
    exit /b 1
)

docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker Compose is not installed. Please install Docker Compose first.
    pause
    exit /b 1
)

REM Check if monitoring stack is already running
docker ps | findstr "chd-grafana" >nul 2>&1
if %errorlevel% equ 0 (
    echo [WARNING] Monitoring stack is already running.
    echo.
    set /p restart="Do you want to restart it? (y/n): "
    if /i "%restart%"=="y" (
        echo.
        echo [INFO] Stopping existing stack...
        docker-compose -f docker-compose.monitoring.yml down
    ) else (
        echo.
        echo [OK] Keeping existing stack running.
        echo.
        echo Access services at:
        echo   - Grafana: http://localhost:3000 (admin/admin^)
        echo   - Prometheus: http://localhost:9090
        echo   - Redis: localhost:6379
        pause
        exit /b 0
    )
)

REM Start monitoring stack
echo.
echo [INFO] Starting monitoring stack...
echo   - Redis
echo   - Prometheus
echo   - Grafana
echo   - Node Exporter
echo.

docker-compose -f docker-compose.monitoring.yml up -d

REM Wait for services to be ready
echo [INFO] Waiting for services to start...
timeout /t 10 /nobreak >nul

REM Check if services are running
echo.
echo [INFO] Checking service status...

docker ps | findstr "chd-redis" >nul 2>&1
if %errorlevel% equ 0 (
    echo   [OK] Redis is running
) else (
    echo   [ERROR] Redis failed to start
)

docker ps | findstr "chd-prometheus" >nul 2>&1
if %errorlevel% equ 0 (
    echo   [OK] Prometheus is running
) else (
    echo   [ERROR] Prometheus failed to start
)

docker ps | findstr "chd-grafana" >nul 2>&1
if %errorlevel% equ 0 (
    echo   [OK] Grafana is running
) else (
    echo   [ERROR] Grafana failed to start
)

docker ps | findstr "chd-node-exporter" >nul 2>&1
if %errorlevel% equ 0 (
    echo   [OK] Node Exporter is running
) else (
    echo   [ERROR] Node Exporter failed to start
)

echo.
echo ======================================
echo Monitoring stack is ready!
echo ======================================
echo.
echo Access services:
echo   - Grafana:    http://localhost:3000
echo     Username: admin
echo     Password: admin
echo.
echo   - Prometheus: http://localhost:9090
echo   - Redis:      localhost:6379
echo.
echo View dashboard:
echo   http://localhost:3000/d/chd-coordination
echo.
echo Run load tests:
echo   k6 run loadtest.js
echo.
echo To stop:
echo   docker-compose -f docker-compose.monitoring.yml down
echo.
pause
