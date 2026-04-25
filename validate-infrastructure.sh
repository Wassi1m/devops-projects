#!/bin/bash
# Infrastructure Validation Script
# Vérifie que tous les composants d'infrastructure sont correctement configurés

set -e

COIN_DIR="/home/user/Bureau/2 eme cycle/2eme cycle 2semestre/dotnet/coin"
ERRORS=0
WARNINGS=0

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Infrastructure Validation - Coin System  ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""

# Fonction pour afficher les résultats
check_file() {
    local file=$1
    local description=$2

    if [ -f "$COIN_DIR/$file" ]; then
        echo -e "${GREEN}✓${NC} $description"
        return 0
    else
        echo -e "${RED}✗${NC} $description (File not found: $file)"
        ((ERRORS++))
        return 1
    fi
}

check_dir() {
    local dir=$1
    local description=$2

    if [ -d "$COIN_DIR/$dir" ]; then
        echo -e "${GREEN}✓${NC} $description"
        return 0
    else
        echo -e "${RED}✗${NC} $description (Directory not found: $dir)"
        ((ERRORS++))
        return 1
    fi
}

check_command() {
    local cmd=$1
    local description=$2

    if command -v "$cmd" &> /dev/null; then
        echo -e "${GREEN}✓${NC} $description"
        return 0
    else
        echo -e "${YELLOW}⚠${NC} $description (Command not found: $cmd)"
        ((WARNINGS++))
        return 1
    fi
}

check_yaml_syntax() {
    local file=$1
    local description=$2

    if ! command -v yq &> /dev/null && ! command -v python3 &> /dev/null; then
        echo -e "${YELLOW}⚠${NC} Skipping YAML validation for $description (yq/python3 not found)"
        return 0
    fi

    if [ -f "$COIN_DIR/$file" ]; then
        if command -v python3 &> /dev/null; then
            if python3 -m yaml "$COIN_DIR/$file" &> /dev/null; then
                echo -e "${GREEN}✓${NC} YAML syntax valid: $description"
                return 0
            else
                echo -e "${RED}✗${NC} YAML syntax error in $description"
                ((ERRORS++))
                return 1
            fi
        fi
    fi
}

# ============================================
echo -e "${BLUE}1. Docker Compose Files${NC}"
# ============================================
check_file "docker-compose.prod.yml" "Production Docker Compose file"

# ============================================
echo ""
echo -e "${BLUE}2. Kubernetes Configuration${NC}"
# ============================================
check_dir "k8s" "Kubernetes directory"
check_dir "k8s/base" "Kubernetes base directory"
check_file "k8s/base/kustomization.yaml" "Kustomization file"
check_file "k8s/base/namespace.yaml" "Namespace definition"
check_file "k8s/base/api.yaml" "API deployment"
check_file "k8s/base/ai.yaml" "AI service deployment"
check_file "k8s/base/database.yaml" "Database deployment"
check_file "k8s/base/prometheus.yaml" "Prometheus deployment"
check_file "k8s/base/grafana.yaml" "Grafana deployment"
check_file "k8s/base/loki.yaml" "Loki deployment"
check_file "k8s/base/alertmanager.yaml" "Alertmanager deployment"
check_file "k8s/base/observability.yaml" "Observability configuration"

# ============================================
echo ""
echo -e "${BLUE}3. Helm Configuration${NC}"
# ============================================
check_dir "helm" "Helm directory"
check_dir "helm/coin-system" "Helm chart directory"
check_file "helm/coin-system/Chart.yaml" "Helm Chart manifest"
check_file "helm/coin-system/values.yaml" "Helm Values file"
check_dir "helm/coin-system/templates" "Helm templates directory"
check_file "helm/coin-system/templates/_helpers.tpl" "Helm helpers template"
check_file "helm/coin-system/templates/secrets.yaml" "Helm secrets template"

# ============================================
echo ""
echo -e "${BLUE}4. Observability Configuration${NC}"
# ============================================
check_dir "observability" "Observability directory"
check_file "observability/prometheus.yml" "Prometheus configuration"
check_file "observability/alert-rules.yml" "Prometheus alert rules"
check_file "observability/loki-config.yaml" "Loki configuration"
check_file "observability/promtail-config.yaml" "Promtail configuration"
check_file "observability/alertmanager.yml" "Alertmanager configuration"
check_file "observability/otel-collector-config.yaml" "OpenTelemetry collector configuration"

# ============================================
echo ""
echo -e "${BLUE}5. Application Directories${NC}"
# ============================================
check_dir "coin_ai" "AI service directory"
check_file "coin_ai/Dockerfile" "AI service Dockerfile"
check_file "coin_ai/requirements.txt" "AI service requirements"
check_dir "projet/BourseIA" "API service directory"
check_file "projet/BourseIA/Dockerfile" "API service Dockerfile"
check_file "projet/BourseIA/BourseIA.csproj" "API service project file"

# ============================================
echo ""
echo -e "${BLUE}6. System Prerequisites${NC}"
# ============================================
check_command "docker" "Docker installed"
check_command "docker-compose" "Docker Compose installed"
check_command "kubectl" "kubectl installed"
check_command "helm" "Helm installed"
check_command "git" "Git installed"

# ============================================
echo ""
echo -e "${BLUE}7. Kubernetes Cluster${NC}"
# ============================================
if kubectl cluster-info &> /dev/null; then
    echo -e "${GREEN}✓${NC} Kubernetes cluster accessible"

    # Check namespace
    if kubectl get namespace coin-system &> /dev/null; then
        echo -e "${GREEN}✓${NC} coin-system namespace exists"
    else
        echo -e "${YELLOW}⚠${NC} coin-system namespace does not exist (will be created)"
    fi
else
    echo -e "${YELLOW}⚠${NC} Kubernetes cluster not accessible (use 'minikube start' if testing locally)"
fi

# ============================================
echo ""
echo -e "${BLUE}8. Volume and Storage${NC}"
# ============================================
# Check Docker volumes
if docker volume ls -q | grep -q coin; then
    echo -e "${GREEN}✓${NC} Docker volumes exist"
else
    echo -e "${YELLOW}⚠${NC} Docker volumes not yet created (will be created on first run)"
fi

# ============================================
echo ""
echo -e "${BLUE}9. Networking${NC}"
# ============================================
# Check if Docker network exists
if docker network ls | grep -q coin; then
    echo -e "${GREEN}✓${NC} Docker network 'coin' exists"
else
    echo -e "${YELLOW}⚠${NC} Docker network 'coin' not yet created (will be created on first run)"
fi

# ============================================
echo ""
echo -e "${BLUE}10. Documentation Files${NC}"
# ============================================
check_file "CORRECTIONS_INFRASTRUCTURE.md" "Infrastructure corrections guide"
check_file "DEPLOYMENT_GUIDE.md" "Deployment guide"

# ============================================
echo ""
echo -e "${BLUE}11. Deployment Scripts${NC}"
# ============================================
check_file "test-docker-compose.sh" "Docker Compose test script"
check_file "test-kubernetes.sh" "Kubernetes test script"
check_file "test-helm.sh" "Helm test script"
check_file "validate-infrastructure.sh" "Infrastructure validation script"

# ============================================
echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}Summary${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✓ All checks passed! Infrastructure is ready for deployment.${NC}"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo -e "${YELLOW}⚠ $WARNINGS warnings found. Infrastructure is mostly ready.${NC}"
    exit 0
else
    echo -e "${RED}✗ $ERRORS errors and $WARNINGS warnings found. Please fix the issues above.${NC}"
    exit 1
fi

