# 🚀 Guide Complet du Déploiement - Système Coin

## 📖 Table des Matières
1. [Configuration Rapide](#configuration-rapide)
2. [Docker Compose (Développement)](#docker-compose-développement)
3. [Kubernetes (Production)](#kubernetes-production)
4. [Helm (Production Recommandé)](#helm-production-recommandé)
5. [Troubleshooting](#troubleshooting)

---

## Configuration Rapide

### Prérequis
- Docker & Docker Compose
- kubectl (pour Kubernetes)
- Helm (pour Helm)
- Minikube (pour tester Kubernetes localement)

### Cloner et Accéder au Projet
```bash
cd "/home/user/Bureau/2 eme cycle/2eme cycle 2semestre/dotnet/coin"
```

---

## Docker Compose (Développement)

### Démarrage Rapide
```bash
# Arrêter et nettoyer (optionnel)
docker compose -f docker-compose.prod.yml down -v

# Démarrer tous les services
docker compose -f docker-compose.prod.yml up -d

# Vérifier le statut
docker compose -f docker-compose.prod.yml ps
```

### URLs d'Accès
| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **API Health** | http://localhost:5000/health | - |
| **AI Service** | http://localhost:8000 | - |
| **Grafana** | http://localhost:3000 | admin / admin |
| **Prometheus** | http://localhost:9090 | - |
| **Loki** | http://localhost:3100 | - |
| **Jaeger** | http://localhost:16686 | - |
| **Alertmanager** | http://localhost:9093 | - |
| **PostgreSQL** | localhost:5433 | bourseia / secret123 |

### Logs en Temps Réel
```bash
# Tous les services
docker compose -f docker-compose.prod.yml logs -f

# Service spécifique
docker compose -f docker-compose.prod.yml logs -f prometheus
docker compose -f docker-compose.prod.yml logs -f loki
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f coin-ai
```

### Arrêter les Services
```bash
# Arrêter (garder les données)
docker compose -f docker-compose.prod.yml stop

# Arrêter et supprimer les volumes (reset complet)
docker compose -f docker-compose.prod.yml down -v
```

---

## Kubernetes (Production)

### Prérequis
- Minikube installé (ou cluster Kubernetes existant)
- kubectl configuré

### Déploiement avec Kustomize

#### 1. Démarrer Minikube
```bash
minikube start --driver=docker

# Attendre que Kubernetes soit prêt
kubectl cluster-info
```

#### 2. Créer le Namespace
```bash
kubectl create namespace coin-system --dry-run=client -o yaml | kubectl apply -f -
```

#### 3. Appliquer les Configurations
```bash
# Appliquer toutes les ressources
kubectl apply -k k8s/base/

# Vérifier l'application
kubectl get all -n coin-system
kubectl get pvc -n coin-system
```

#### 4. Attendre que les Pods Soient Prêts
```bash
# Vérifier le statut des pods
kubectl get pods -n coin-system -w

# Attendre que tous les pods soient Running
kubectl wait --for=condition=ready pod --all -n coin-system --timeout=300s
```

### Accès aux Services

#### Option 1 : Port Forward
```bash
# Prometheus
kubectl port-forward -n coin-system svc/prometheus 9090:9090

# Grafana
kubectl port-forward -n coin-system svc/grafana 3000:3000

# API
kubectl port-forward -n coin-system svc/api 5000:8080

# AI Service
kubectl port-forward -n coin-system svc/coin-ai 8000:8000

# Loki
kubectl port-forward -n coin-system svc/loki 3100:3100

# Jaeger
kubectl port-forward -n coin-system svc/jaeger 16686:16686
```

#### Option 2 : Ingress (optionnel)
```bash
# Créer un service LoadBalancer pour accès direct
kubectl patch svc prometheus -n coin-system -p '{"spec": {"type": "LoadBalancer"}}'
```

### Logs des Pods
```bash
# Logs d'un deployment
kubectl logs -f deployment/prometheus -n coin-system
kubectl logs -f deployment/loki -n coin-system
kubectl logs -f deployment/api -n coin-system

# Logs d'un pod spécifique
kubectl logs -f pod/prometheus-xxxxx -n coin-system

# Dernières lignes
kubectl logs -f deployment/prometheus -n coin-system --tail=50
```

### Dépannage Kubernetes
```bash
# Décrire les ressources
kubectl describe pod/prometheus-xxxxx -n coin-system
kubectl describe pvc loki-pvc -n coin-system

# Vérifier les événements
kubectl get events -n coin-system --sort-by='.lastTimestamp'

# Shell dans un pod
kubectl exec -it pod/prometheus-xxxxx -n coin-system -- /bin/sh
```

---

## Helm (Production Recommandé)

### Installation de Helm (si nécessaire)
```bash
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
```

### Déploiement avec Helm

#### 1. Linter la Chart
```bash
helm lint helm/coin-system/
```

#### 2. Installer la Release
```bash
helm install coin-system helm/coin-system/ \
  --namespace coin-system \
  --create-namespace \
  --values helm/coin-system/values.yaml
```

#### 3. Vérifier l'Installation
```bash
# Statut de la release
helm status coin-system -n coin-system

# Valeurs utilisées
helm get values coin-system -n coin-system

# Toutes les ressources
helm get all coin-system -n coin-system
```

### Mettre à Jour une Release
```bash
helm upgrade coin-system helm/coin-system/ \
  --namespace coin-system \
  --values helm/coin-system/values.yaml
```

### Supprimer une Release
```bash
helm uninstall coin-system -n coin-system
```

### Valeurs Helm Personnalisées
Créer un fichier `my-values.yaml`:
```yaml
replicaCount:
  api: 3
  coinAi: 2

database:
  credentials:
    username: bourseia
    password: secret123
    database: bourseia_db

grafana:
  adminPassword: "secure-password-here"
```

Puis installer :
```bash
helm install coin-system helm/coin-system/ \
  -f my-values.yaml \
  --namespace coin-system \
  --create-namespace
```

---

## Troubleshooting

### Docker Compose

#### Issue: "Error mounting volume"
```bash
# Solution 1: Vérifier que les fichiers existent
ls -la observability/prometheus.yml
ls -la observability/loki-config.yaml
ls -la observability/alertmanager.yml

# Solution 2: Nettoyer et recommencer
docker compose -f docker-compose.prod.yml down -v
docker system prune -a --volumes
docker compose -f docker-compose.prod.yml up -d
```

#### Issue: "Service unreachable"
```bash
# Vérifier les logs
docker compose -f docker-compose.prod.yml logs prometheus
docker compose -f docker-compose.prod.yml logs loki

# Vérifier les ports
docker ps | grep -E 'prometheus|loki|grafana'

# Tester la connectivité
curl http://localhost:9090/metrics
```

### Kubernetes

#### Issue: "ImagePullBackOff"
```bash
# Si les images n'existent pas, les construire
eval $(minikube docker-env)
docker build -t bourseia-api ./projet/BourseIA
docker build -t coin-ai ./coin_ai

# Ou spécifier imagePullPolicy
kubectl set image deployment/api api=bourseia-api:latest \
  --record -n coin-system
```

#### Issue: "CrashLoopBackOff"
```bash
# Vérifier les logs du pod
kubectl logs -f pod/<pod-name> -n coin-system

# Décrire le pod pour plus de détails
kubectl describe pod/<pod-name> -n coin-system

# Vérifier la ConfigMap
kubectl get configmap -n coin-system
kubectl describe configmap loki-config -n coin-system
```

#### Issue: "Pending PVC"
```bash
# Vérifier le storage class
kubectl get storageclass

# Vérifier les PVCs
kubectl get pvc -n coin-system
kubectl describe pvc loki-pvc -n coin-system

# Solution: Créer un PV manuel si nécessaire
kubectl create -f - <<EOF
apiVersion: v1
kind: PersistentVolume
metadata:
  name: pv-loki
spec:
  capacity:
    storage: 10Gi
  accessModes:
    - ReadWriteOnce
  hostPath:
    path: "/mnt/data/loki"
EOF
```

#### Issue: "Port forward not working"
```bash
# Tuer les processus port-forward existants
pkill -f "port-forward"

# Relancer
kubectl port-forward -n coin-system svc/prometheus 9090:9090
```

### Helm

#### Issue: "Template error"
```bash
# Valider la chart
helm lint helm/coin-system/

# Voir le template rendu
helm template coin-system helm/coin-system/ -n coin-system

# Installer en dry-run
helm install coin-system helm/coin-system/ \
  --namespace coin-system \
  --dry-run --debug
```

#### Issue: "Release not found"
```bash
# Lister les releases
helm list -n coin-system

# Supprimer et réinstaller
helm uninstall coin-system -n coin-system
helm install coin-system helm/coin-system/ \
  --namespace coin-system \
  --create-namespace
```

---

## Monitoring et Observabilité

### Prometheus
- **URL**: http://localhost:9090
- **Fonction**: Collecte les métriques
- **Configurations**: `observability/prometheus.yml`

### Grafana
- **URL**: http://localhost:3000
- **User**: admin
- **Password**: admin
- **Fonction**: Visualisation et dashboards

### Loki
- **URL**: http://localhost:3100
- **Fonction**: Stockage et requête des logs
- **Configurations**: `observability/loki-config.yaml`

### Jaeger
- **URL**: http://localhost:16686
- **Fonction**: Tracing distribué
- **Port**: 16686

### Alertmanager
- **URL**: http://localhost:9093
- **Fonction**: Gestion des alertes
- **Configurations**: `observability/alertmanager.yml`

---

## Scripts Utiles

### Script Docker Compose
```bash
chmod +x test-docker-compose.sh
./test-docker-compose.sh
```

### Script Kubernetes
```bash
chmod +x test-kubernetes.sh
./test-kubernetes.sh
```

### Script Helm
```bash
chmod +x test-helm.sh
./test-helm.sh
```

---

## Ressources Supplémentaires

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Helm Documentation](https://helm.sh/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Loki Documentation](https://grafana.com/docs/loki/)

---

**Dernière mise à jour**: 2026-04-25
**Version**: 1.0.0
**Status**: ✅ Production Ready

