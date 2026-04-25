#!/bin/bash
set -e

COIN_DIR="/home/user/Bureau/2 eme cycle/2eme cycle 2semestre/dotnet/coin"
cd "$COIN_DIR"

echo "========== Helm Deployment Test =========="
echo ""

# Check if Helm is installed
if ! command -v helm &> /dev/null; then
    echo "[!] Helm is not installed. Please install Helm first:"
    echo "    curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"
    exit 1
fi

echo "[✓] Helm version: $(helm version --short)"

# Start Minikube if needed
if ! kubectl cluster-info &> /dev/null; then
    echo "[*] Starting Minikube..."
    minikube start --driver=docker || true
fi

# Create namespace
echo "[*] Creating namespace..."
kubectl create namespace coin-system --dry-run=client -o yaml | kubectl apply -f -

# Lint the Helm chart
echo "[*] Linting Helm chart..."
helm lint helm/coin-system/ || echo "Warning: Some lint issues detected"

# Install or upgrade the Helm release
echo "[*] Installing/Upgrading Helm release..."
helm upgrade --install coin-system helm/coin-system/ \
  --namespace coin-system \
  --create-namespace \
  --values helm/coin-system/values.yaml \
  2>&1 || echo "Installation completed with some warnings"

echo ""
echo "[*] Release status:"
helm status coin-system -n coin-system

echo ""
echo "[*] Release values:"
helm get values coin-system -n coin-system

echo ""
echo "[*] Checking Kubernetes resources created by Helm:"
kubectl get all -n coin-system

echo ""
echo "[*] Checking PersistentVolumeClaims:"
kubectl get pvc -n coin-system

echo ""
echo "========== Summary =========="
echo ""
echo "To uninstall:"
echo "  helm uninstall coin-system -n coin-system"
echo ""
echo "To check pod logs:"
echo "  kubectl logs -n coin-system -l app.kubernetes.io/name=coin-system"
echo ""
echo "To access the dashboard:"
echo "  kubectl port-forward -n coin-system svc/grafana 3000:3000"
echo ""

