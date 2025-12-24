#!/bin/bash

# E2E Test Runner - Local
# Runs the same steps as the GitHub workflow but locally

set -e  # Exit on error

# Configuration
export MSSQL_SA_PASSWORD='E2ETest123!Strong'
export MSSQL_PORT=1434
export MSSQL_CONTAINER_NAME=sqlserver-e2e
export ADMIN_USERNAME=testadmin
export ADMIN_PASSWORD='TestAdmin123!'
export AppSettings__PasswordKey='test-password-key-minimum-32-chars-long'

CONNECTION_STRING="Server=localhost,${MSSQL_PORT};Database=DigitalStorefront;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True"

# Get script directory and repo root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

cleanup() {
    echo ""
    echo "Cleaning up..."

    # Kill API if running
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null || true
    fi

    # Stop only the E2E container (not dev)
    docker rm -f "$MSSQL_CONTAINER_NAME" 2>/dev/null || true

    echo "Cleanup complete"
}

# Set trap to cleanup on exit
trap cleanup EXIT

echo "Starting E2E Tests Locally"
echo "================================"

# 1. Start SQL Server
echo ""
echo "Starting SQL Server container..."
cd "$REPO_ROOT"
docker compose -f docker/docker-compose.yml up -d

# 2. Wait for healthy
echo "Waiting for SQL Server to be healthy..."
for i in {1..30}; do
    if docker compose -f docker/docker-compose.yml ps | grep -q "healthy"; then
        echo "SQL Server is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "Timed out waiting for SQL Server"
        exit 1
    fi
    echo "  Still waiting..."
    sleep 2
done

# 3. Create database
echo ""
echo "Creating database..."
docker exec "$MSSQL_CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DigitalStorefront') CREATE DATABASE DigitalStorefront;"

# 4. Run migrations
echo ""
echo "Running database migrations..."
cd "$REPO_ROOT/server"
dotnet run --project DatabaseManagement -- --reset "$CONNECTION_STRING"

# 4. Start API in background
echo ""
echo "Starting API server..."
cd "$REPO_ROOT/server/API"
ASPNETCORE_ENVIRONMENT=E2E \
ASPNETCORE_URLS=http://localhost:5000 \
ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
dotnet run &
API_PID=$!

# 5. Wait for API
echo "Waiting for API to be ready..."
for i in {1..30}; do
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo "API is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "Timed out waiting for API"
        exit 1
    fi
    sleep 1
done

# 6. Run Playwright tests
echo ""
echo "Running Playwright tests..."
cd "$REPO_ROOT/client"
VITE_API_URL=http://localhost:5000 VITE_PLAYWRIGHT=true npx playwright test --grep-invert "Checkout"

echo ""
echo "E2E tests completed successfully"
