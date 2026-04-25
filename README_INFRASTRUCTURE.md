# Coin System - Production-Ready Infrastructure

## 🎯 Résumé

Architecture complète et professionnelle pour déployer le système Coin avec:
- ✅ **Docker Compose** pour développement/test
- ✅ **Kubernetes** avec manifests et Kustomize
- ✅ **Helm Charts** pour gestion d'application
- ✅ **Monitoring complet** (Prometheus, Grafana, Jaeger, Loki)
- ✅ **CI/CD** avec GitHub Actions
- ✅ **Security scanning** et validation
- ✅ **High Availability** et Auto-scaling

---

## 📦 Fichiers créés

### Configuration Docker
- `docker-compose.prod.yml` - Stack complète avec monitoring
- `projet/BourseIA/Dockerfile.prod` - API .NET optimisée (multi-stage)
- `coin_ai/Dockerfile.prod` - Service Python optimisé (multi-stage)

### Configuration Observabilité
```
observability/
├── otel-collector-config.yaml      # OpenTelemetry Collector
├── prometheus.yml                  # Configuration Prometheus
├── alert-rules.yml                 # Règles d'alerte
├── alertmanager.yml                # Configuration Alertmanager
├── loki-config.yaml               # Configuration Loki
└── promtail-config.yaml           # Configuration Promtail
```

### Kubernetes (k8s/)
```
k8s/
├── base/                          # Configuration de base
│   ├── namespace.yaml             # Namespace et PVC
│   ├── database.yaml              # StatefulSet PostgreSQL
│   ├── api.yaml                   # Deployment API + HPA + PDB
│   ├── ai.yaml                    # Deployment AI + HPA + PDB
│   ├── prometheus.yaml            # Prometheus + RBAC
│   ├── grafana.yaml               # Grafana
│   ├── loki.yaml                  # Loki + Promtail DaemonSet
│   ├── observability.yaml         # Jaeger + OTel Collector
│   ├── alertmanager.yaml          # Alertmanager
│   └── kustomization.yaml         # Orchestration Kustomize
│
└── overlays/
    ├── development/               # Overlay dev (1 replicas)
    │   └── kustomization.yaml
    └── production/                # Overlay prod (3-5 replicas)
        └── kustomization.yaml
```

### Helm Charts (helm/)
```
helm/coin-system/
├── Chart.yaml                     # Metadata du chart
├── values.yaml                    # Valeurs par défaut
├── templates/
│   ├── _helpers.tpl              # Templates helpers
│   ├── secrets.yaml              # Secret management
│   ├── database.yaml             # Database StatefulSet
│   ├── api.yaml                  # API Deployment + HPA
│   ├── ai.yaml                   # AI Deployment + HPA
│   ├── prometheus.yaml           # Prometheus
│   └── grafana.yaml              # Grafana
```

### CI/CD (GitHub Actions)
```
.github/workflows/
├── build-deploy.yml              # Build, test, deploy
└── infra-tests.yml               # Validation infra
```

### Configuration & Scripts
```
├── .env.prod                      # Variables production
├── .env.dev                       # Variables développement
├── deploy.sh                      # Script de déploiement interactif
└── DEPLOYMENT_GUIDE.md            # Guide complet
```

---

## 🚀 Quick Start

### Option 1: Docker Compose (Recommandé pour démarrer)

```bash
# Rendre le script exécutable
chmod +x deploy.sh

# Lancer le déploiement
./deploy.sh

# Sélectionner option 1 (Docker Compose)
```

Ensuite accédez à:
- API: http://localhost:5000
- Grafana: http://localhost:3000
- Prometheus: http://localhost:9090
- Jaeger: http://localhost:16686

### Option 2: Kubernetes avec Kustomize

```bash
# Déployer
kubectl apply -k k8s/base/

# Ou avec overlay dev/prod
kubectl apply -k k8s/overlays/production/

# Vérifier
kubectl get all -n coin-system

# Port forward pour accéder
kubectl port-forward -n coin-system svc/grafana 3000:3000
```

### Option 3: Helm (Recommandé pour production)

```bash
# Installer
helm install coin-system helm/coin-system \
  --namespace coin-system \
  --create-namespace \
  -f helm/coin-system/values.yaml

# Vérifier
helm status coin-system -n coin-system

# Accéder
kubectl port-forward -n coin-system svc/coin-system-grafana 3000:3000
```

---

## 📊 Composants Observabilité

### Prometheus
- Scrape tous les services (API, AI, DB, nodes)
- Retention: 30 jours
- Alertes basées sur règles

### Grafana
- Dashboards pre-configured
- Multi-datasource (Prometheus, Loki, Jaeger)
- Alerting rules

### Jaeger
- Distributed tracing
- Support gRPC et HTTP
- Stockage en mémoire

### Loki
- Log aggregation
- Scrape logs Docker
- Retention configurable

### OpenTelemetry Collector
- Centralise traces, metrics, logs
- Export vers Jaeger, Prometheus
- Sampling policies

---

## 🔒 Security Features

✅ Non-root containers
✅ Resource limits (CPU/Memory)
✅ Network policies (optionnel)
✅ Pod security standards
✅ RBAC (Role-based access control)
✅ Secrets encrypted
✅ Health checks (liveness/readiness)

---

## 📈 High Availability

✅ **Réplication**: 3+ replicas en production
✅ **Auto-scaling**: HPA basé sur CPU/Memory
✅ **Pod Disruption Budget**: Tolérance aux interruptions
✅ **Pod Anti-Affinity**: Distribution sur les nodes
✅ **Rolling Updates**: 0 downtime deployments

---

## 📝 Fichiers Configuration Clés

### .env.prod
```bash
# Personnaliser:
DB_PASSWORD=secret123           # Changer!
GRAFANA_PASSWORD=admin123       # Changer!
SLACK_WEBHOOK_URL=             # Ajouter webhook Slack
PAGERDUTY_SERVICE_KEY=         # Ajouter PagerDuty
```

### values.yaml (Helm)
```yaml
# Personnaliser:
api:
  replicaCount: 3              # Nombre de replicas
  resources:
    limits:
      memory: "512Mi"          # Limite mémoire API

observability:
  prometheus:
    retention: 30d             # Retention données
```

---

## 🧪 Tests & Validation

### Linting
```bash
# Kubernetes
kubectl apply -f k8s/base/ --dry-run=client

# Helm
helm lint helm/coin-system

# Docker Compose
docker-compose -f docker-compose.prod.yml config
```

### Health Checks
```bash
# API
curl http://localhost:5000/health

# AI
curl http://localhost:8000/health

# Prometheus
curl http://localhost:9090/-/healthy

# Services
docker-compose -f docker-compose.prod.yml ps
```

---

## 📚 Documentation

Voir `DEPLOYMENT_GUIDE.md` pour:
- Architecture détaillée
- Instructions complètes de déploiement
- Configuration par environnement
- Troubleshooting
- Monitoring avancé

---

## 🔄 CI/CD Pipeline

### Déclenché par:
- `push` sur main (production)
- `push` sur develop (staging)
- Pull requests

### Étapes:
1. Linting & Tests
2. Build Docker images
3. Security scanning (Trivy)
4. Deploy staging (si develop)
5. Integration tests
6. Deploy production (si main)
7. Smoke tests

### Secrets GitHub à configurer:
```
HELM_REPO_URL
SLACK_WEBHOOK
SONAR_TOKEN
```

---

## 🛠️ Maintenance

### Monitoring
```bash
# Logs API
kubectl logs -f deployment/api -n coin-system

# Metrics
kubectl top pods -n coin-system

# Events
kubectl get events -n coin-system --sort-by='.lastTimestamp'
```

### Updates
```bash
# Helm
helm upgrade coin-system helm/coin-system -n coin-system

# Kubernetes
kubectl set image deployment/api api=image:new-tag -n coin-system

# Docker Compose
docker-compose -f docker-compose.prod.yml pull
docker-compose -f docker-system.prod.yml up -d
```

### Backup
```bash
# PostgreSQL
kubectl exec -it postgres-0 -n coin-system -- \
  pg_dump -U bourseia bourseia_db > backup.sql

# Prometheus data
kubectl exec -it <prometheus-pod> -n coin-system -- \
  tar -czf /tmp/prometheus-backup.tar.gz /prometheus
```

---

## 📞 Support

Pour des questions:
1. Vérifier les logs: `docker-compose logs` ou `kubectl logs`
2. Consulter DEPLOYMENT_GUIDE.md
3. Vérifier la santé des services
4. Consulter les dashboards Grafana
5. Vérifier les traces Jaeger

---

## ✨ Améliorations Futures

- [ ] Istio pour service mesh
- [ ] Cert-manager pour SSL/TLS
- [ ] External-DNS pour DNS auto
- [ ] Velero pour backup/restore
- [ ] Sealed-Secrets pour secret management
- [ ] ArgoCD pour GitOps
- [ ] Vault pour secret centralisé

---

**Version**: 1.0.0  
**Dernière mise à jour**: 2024  
**Statut**: Production Ready ✅

