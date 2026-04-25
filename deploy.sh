#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔═══════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Coin System Deployment Script        ║${NC}"
echo -e "${BLUE}║  Professional Production Setup        ║${NC}"
echo -e "${BLUE}╚═══════════════════════════════════════╝${NC}"
echo ""

# Check prerequisites
check_prerequisites() {
    echo -e "${YELLOW}[*] Checking prerequisites...${NC}"

    local missing_tools=()

    # Check Docker
    if ! command -v docker &> /dev/null; then
        missing_tools+=("docker")
    fi

    # Check Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        missing_tools+=("docker-compose")
    fi

    # Check kubectl (optional for K8s)
    if ! command -v kubectl &> /dev/null; then
        echo -e "${YELLOW}[!] kubectl not found - Kubernetes deployments won't work${NC}"
    fi

    # Check Helm (optional for Helm)
    if ! command -v helm &> /dev/null; then
        echo -e "${YELLOW}[!] helm not found - Helm deployments won't work${NC}"
    fi

    if [ ${#missing_tools[@]} -gt 0 ]; then
        echo -e "${RED}[✗] Missing required tools: ${missing_tools[*]}${NC}"
        exit 1
    fi

    echo -e "${GREEN}[✓] All prerequisites met${NC}"
}

# Deploy with Docker Compose
deploy_docker_compose() {
    echo -e "${BLUE}\n[*] Deploying with Docker Compose...${NC}"

    # Load environment
    if [ -f ".env.prod" ]; then
        set -a
        source .env.prod
        set +a
    fi

    # Create data directories
    mkdir -p data/{postgres,prometheus,grafana,loki}
    chmod 777 data/*

    # Start services
    echo -e "${YELLOW}[*] Starting services...${NC}"
    docker-compose -f docker-compose.prod.yml up -d

    # Wait for services to be healthy
    echo -e "${YELLOW}[*] Waiting for services to be healthy...${NC}"
    sleep 10

    # Check health
    echo -e "${YELLOW}[*] Checking service health...${NC}"

    local services_healthy=true

    # Check API
    if curl -s http://localhost:5000/health > /dev/null; then
        echo -e "${GREEN}[✓] API is healthy${NC}"
    else
        echo -e "${RED}[✗] API is not healthy${NC}"
        services_healthy=false
    fi

    # Check AI Service
    if curl -s http://localhost:8000/health > /dev/null; then
        echo -e "${GREEN}[✓] AI Service is healthy${NC}"
    else
        echo -e "${RED}[✗] AI Service is not healthy${NC}"
        services_healthy=false
    fi

    # Check Prometheus
    if curl -s http://localhost:9090/-/healthy > /dev/null; then
        echo -e "${GREEN}[✓] Prometheus is healthy${NC}"
    else
        echo -e "${YELLOW}[!] Prometheus may not be ready${NC}"
    fi

    # Check Grafana
    if curl -s http://localhost:3000/api/health > /dev/null; then
        echo -e "${GREEN}[✓] Grafana is healthy${NC}"
    else
        echo -e "${YELLOW}[!] Grafana may not be ready${NC}"
    fi

    if [ "$services_healthy" = true ]; then
        echo -e "${GREEN}[✓] All services are healthy!${NC}"
        return 0
    else
        echo -e "${RED}[✗] Some services are not healthy${NC}"
        echo -e "${YELLOW}[*] Check logs with: docker-compose -f docker-compose.prod.yml logs${NC}"
        return 1
    fi
}

check_kubernetes() {
    echo "[*] Starting Minikube..."

    minikube start --driver=docker

    echo "[*] Switching Docker environment to Minikube..."
    eval $(minikube docker-env)

    echo "[*] Waiting for Kubernetes API..."

    until kubectl get nodes &>/dev/null; do
        echo "Waiting..."
        sleep 3
    done

    echo "[✓] Kubernetes is ready"
}
# Deploy with Kubernetes
deploy_kubernetes() {
    check_kubernetes
    echo -e "${BLUE}\n[*] Deploying with Kubernetes...${NC}"

    # Create namespace
    echo -e "${YELLOW}[*] Creating namespace...${NC}"
    kubectl create namespace coin-system --dry-run=client -o yaml | kubectl apply -f -

    # Apply Kustomize base
    echo -e "${YELLOW}[*] Applying Kustomize base configuration...${NC}"
    kubectl apply -k k8s/base/

    # Wait for deployments
    echo -e "${YELLOW}[*] Waiting for deployments...${NC}"
    kubectl rollout status deployment/api -n coin-system --timeout=5m || true
    kubectl rollout status deployment/coin-ai -n coin-system --timeout=5m || true

    # Show status
    echo -e "${YELLOW}[*] Deployment status:${NC}"
    kubectl get pods -n coin-system

    echo -e "${GREEN}[✓] Kubernetes deployment completed!${NC}"
}

# Deploy with Helm
deploy_helm() {
    echo -e "${BLUE}\n[*] Deploying with Helm...${NC}"

    # Create namespace
    kubectl create namespace coin-system --dry-run=client -o yaml | kubectl apply -f -

    # Install/Upgrade Helm chart (namespace dédié pour éviter les conflits Kustomize)
    echo -e "${YELLOW}[*] Installing/Upgrading Helm chart...${NC}"
    kubectl create namespace coin-helm --dry-run=client -o yaml | kubectl apply -f -
    helm upgrade --install coin-system helm/coin-system \
        --namespace coin-helm \
        --set namespace.name=coin-helm \
        -f helm/coin-system/values.yaml

    echo -e "${GREEN}[✓] Helm deployment completed!${NC}"
}

# Show access information
show_access_info() {
    echo ""
    echo -e "${BLUE}╔═══════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║         SERVICE ACCESS URLS           ║${NC}"
    echo -e "${BLUE}╚═══════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${GREEN}API Service${NC}"
    echo "  URL: http://localhost:5000"
    echo "  Health: http://localhost:5000/health"
    echo ""
    echo -e "${GREEN}AI Service${NC}"
    echo "  URL: http://localhost:8000"
    echo "  Health: http://localhost:8000/health"
    echo ""
    echo -e "${GREEN}Monitoring${NC}"
    echo "  Grafana: http://localhost:3000"
    echo "    Default credentials: admin / admin123"
    echo "  Prometheus: http://localhost:9090"
    echo "  Jaeger: http://localhost:16686"
    echo "  Alertmanager: http://localhost:9093"
    echo ""
    echo -e "${GREEN}Database${NC}"
    echo "  PostgreSQL: localhost:5433"
    echo "    User: bourseia"
    echo "    Database: bourseia_db"
    echo ""
}

# Main menu
show_menu() {
    echo -e "${BLUE}╔═══════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║       Deployment Method              ║${NC}"
    echo -e "${BLUE}╚═══════════════════════════════════════╝${NC}"
    echo ""
    echo "1) Docker Compose (Development/Testing)"
    echo "2) Kubernetes (Production)"
    echo "3) Helm (Production - Recommended)"
    echo "4) Exit"
    echo ""
}

# Main logic
main() {
    check_prerequisites

    while true; do
        show_menu
        read -p "Select option (1-4): " choice

        case $choice in
            1)
                deploy_docker_compose
                show_access_info
                ;;
            2)
                deploy_kubernetes
                show_access_info
                ;;
            3)
                deploy_helm
                show_access_info
                ;;
            4)
                echo -e "${YELLOW}Exiting...${NC}"
                exit 0
                ;;
            *)
                echo -e "${RED}Invalid option. Please try again.${NC}"
                ;;
        esac

        echo ""
        read -p "Continue? (y/n): " continue_choice
        if [ "$continue_choice" != "y" ]; then
            break
        fi
    done
}

# Run main function
main

