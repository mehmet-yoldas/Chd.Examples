#!/bin/bash
# CHD Coordination - NBomber Load Tests Quick Start (Linux/Mac)

echo "======================================"
echo "CHD Coordination - Load Tests"
echo "======================================"
echo ""

# Check if Redis is running
if ! docker ps | grep -q "redis"; then
    echo "[WARNING] Redis is not running."
    echo ""
    read -p "Do you want to start Redis? (y/n): " start_redis
    if [ "$start_redis" = "y" ] || [ "$start_redis" = "Y" ]; then
        echo ""
        echo "[INFO] Starting Redis..."
        docker run -d -p 6379:6379 --name chd-redis redis:7-alpine
        sleep 3
    else
        echo ""
        echo "[ERROR] Redis is required for load tests."
        echo "Please start Redis manually:"
        echo "  docker run -d -p 6379:6379 redis:7-alpine"
        exit 1
    fi
fi

echo "[INFO] Redis is running"
echo ""

# Check if monitoring stack is running
if ! docker ps | grep -q "chd-grafana"; then
    echo "[INFO] Monitoring stack is not running."
    read -p "Start Grafana monitoring? (y/n): " start_monitoring
    if [ "$start_monitoring" = "y" ] || [ "$start_monitoring" = "Y" ]; then
        echo ""
        ./start-monitoring.sh
        echo ""
    fi
fi

# Run load tests
echo ""
echo "======================================"
echo "Starting NBomber Load Tests"
echo "======================================"
echo ""

cd Chd.LoadTests
dotnet run --configuration Release

cd ..

echo ""
echo "======================================"
echo "Load Tests Completed"
echo "======================================"
echo ""
echo "View HTML reports in: ./Chd.LoadTests/reports/"
echo ""

if [ -f "docker-compose.monitoring.yml" ]; then
    if docker ps | grep -q "chd-grafana"; then
        echo "View live metrics:"
        echo "  http://localhost:3000/d/chd-coordination"
        echo ""
    fi
fi
