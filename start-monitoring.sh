#!/bin/bash

# CHD Coordination - Grafana Monitoring Quick Start
# This script sets up the complete monitoring stack

set -e

echo "🚀 CHD Coordination - Monitoring Setup"
echo "======================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Check if monitoring stack is already running
if docker ps | grep -q "chd-grafana"; then
    echo "⚠️  Monitoring stack is already running."
    echo ""
    read -p "Do you want to restart it? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🔄 Stopping existing stack..."
        docker-compose -f docker-compose.monitoring.yml down
    else
        echo "✅ Keeping existing stack running."
        echo ""
        echo "Access services at:"
        echo "  - Grafana: http://localhost:3000 (admin/admin)"
        echo "  - Prometheus: http://localhost:9090"
        echo "  - Redis: localhost:6379"
        exit 0
    fi
fi

# Start monitoring stack
echo "📦 Starting monitoring stack..."
echo "  - Redis"
echo "  - Prometheus"
echo "  - Grafana"
echo "  - Node Exporter"
echo ""

docker-compose -f docker-compose.monitoring.yml up -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 10

# Check if services are running
echo ""
echo "🔍 Checking service status..."

if docker ps | grep -q "chd-redis"; then
    echo "  ✅ Redis is running"
else
    echo "  ❌ Redis failed to start"
fi

if docker ps | grep -q "chd-prometheus"; then
    echo "  ✅ Prometheus is running"
else
    echo "  ❌ Prometheus failed to start"
fi

if docker ps | grep -q "chd-grafana"; then
    echo "  ✅ Grafana is running"
else
    echo "  ❌ Grafana failed to start"
fi

if docker ps | grep -q "chd-node-exporter"; then
    echo "  ✅ Node Exporter is running"
else
    echo "  ❌ Node Exporter failed to start"
fi

echo ""
echo "✨ Monitoring stack is ready!"
echo ""
echo "📊 Access services:"
echo "  - Grafana:    http://localhost:3000"
echo "    Username: admin"
echo "    Password: admin"
echo ""
echo "  - Prometheus: http://localhost:9090"
echo "  - Redis:      localhost:6379"
echo ""
echo "📈 View dashboard:"
echo "  http://localhost:3000/d/chd-coordination"
echo ""
echo "🧪 Run load tests:"
echo "  k6 run loadtest.js"
echo ""
echo "🛑 To stop:"
echo "  docker-compose -f docker-compose.monitoring.yml down"
echo ""
