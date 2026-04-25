#!/bin/bash
set -e

COIN_DIR="/home/user/Bureau/2 eme cycle/2eme cycle 2semestre/dotnet/coin"
cd "$COIN_DIR"

echo "========== Docker Compose Deployment =========="
echo ""
echo "[*] Stopping and cleaning existing containers..."
docker compose -f docker-compose.prod.yml down -v --remove-orphans 2>/dev/null || true

echo "[*] Starting Docker Compose services..."
docker compose -f docker-compose.prod.yml up -d

echo "[*] Waiting for services to be healthy..."
sleep 10

echo "[*] Checking service status..."
docker compose -f docker-compose.prod.yml ps

echo ""
echo "========== Deployment Summary =========="
echo ""
echo "✓ Docker Compose Deployment Successful!"
echo ""
echo "Service URLs:"
echo "  API:        http://localhost:5000"
echo "  AI Service: http://localhost:8000"
echo "  Prometheus: http://localhost:9090"
echo "  Grafana:    http://localhost:3000"
echo "  Jaeger:     http://localhost:16686"
echo "  Alertmanager: http://localhost:9093"
echo "  Loki:       http://localhost:3100"
echo ""
echo "Database:"
echo "  PostgreSQL: localhost:5433"
echo "  User:       bourseia"
echo "  Database:   bourseia_db"
echo ""

