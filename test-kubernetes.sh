#!/bin/bash
set -e

COIN_DIR="/home/user/Bureau/2 eme cycle/2eme cycle 2semestre/dotnet/coin"
cd "$COIN_DIR"

echo "========== Kubernetes Deployment Test =========="
echo ""

# Start Minikube if needed
if ! kubectl cluster-info &> /dev/null; then
    echo "[*] Starting Minikube..."
    minikube start --driver=docker || true
fi

# Wait for API
echo "[*] Waiting for Kubernetes API..."
kubectl wait --for=condition=ready node --all --timeout=300s 2>/dev/null || true

# Create namespace
echo "[*] Creating namespace..."
kubectl create namespace coin-system --dry-run=client -o yaml | kubectl apply -f -

# Apply configurations
echo "[*] Applying Kustomize configuration..."
kubectl apply -k k8s/base/ 2>&1 || echo "Note: Some resources may have failed - this is expected"

# Check status
echo ""
echo "[*] Checking PersistentVolumeClaim status..."
kubectl get pvc -n coin-system

echo ""
echo "[*] Checking Deployment status..."
kubectl get deployments -n coin-system

echo ""
echo "[*] Checking Services..."
kubectl get svc -n coin-system

echo ""
echo "[*] Checking Pods..."
kubectl get pods -n coin-system -o wide

echo ""
echo "========== Summary =========="
echo ""
echo "To access services via port-forward:"
echo "  kubectl port-forward -n coin-system svc/prometheus 9090:9090"
echo "  kubectl port-forward -n coin-system svc/grafana 3000:3000"
echo "  kubectl port-forward -n coin-system svc/loki 3100:3100"
echo ""
echo "To see logs:"
echo "  kubectl logs -f deployment/prometheus -n coin-system"
echo "  kubectl logs -f deployment/loki -n coin-system"
echo ""

