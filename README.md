# BourseIA — Système de Prédiction Boursière

[![Build and Deploy](https://github.com/bourseia/coin-system/actions/workflows/build-deploy.yml/badge.svg)](https://github.com/bourseia/coin-system/actions/workflows/build-deploy.yml)
[![Infrastructure Tests](https://github.com/bourseia/coin-system/actions/workflows/infra-tests.yml/badge.svg)](https://github.com/bourseia/coin-system/actions/workflows/infra-tests.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bourseia_coin-system&metric=alert_status)](https://sonarcloud.io/project/overview?id=bourseia_coin-system)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=bourseia_coin-system&metric=security_rating)](https://sonarcloud.io/project/overview?id=bourseia_coin-system)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> Plateforme de prédiction du cours des actions basée sur l'IA — déployable en **Docker Compose**, **Kubernetes** et **Helm**, avec stack d'observabilité complète.

---

## Table des matières

- [Architecture](#architecture)
- [Stack technique](#stack-technique)
- [Prérequis](#prérequis)
- [Démarrage rapide](#démarrage-rapide)
- [Déploiement](#déploiement)
- [Observabilité](#observabilité)
- [CI/CD](#cicd)
- [Sécurité](#sécurité)
- [Structure du projet](#structure-du-projet)
- [Variables d'environnement](#variables-denvironnement)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENT                                │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTP/WebSocket
┌───────────────────────────▼─────────────────────────────────┐
│              API .NET 8 (ASP.NET Core)                       │
│         Auth JWT · SignalR · OpenTelemetry                   │
└──────┬──────────────────────────────────┬────────────────────┘
       │ SQL                              │ HTTP
┌──────▼──────┐                 ┌─────────▼──────────┐
│ PostgreSQL  │                 │  Service IA Python  │
│    16       │                 │  (Prédiction ML)   │
└─────────────┘                 └─────────────────────┘
       │                                  │
       └──────────────┬───────────────────┘
                      │ Traces / Métriques / Logs
┌─────────────────────▼──────────────────────────────────────┐
│                  Stack Observabilité                         │
│  OTel Collector → Jaeger (traces)                            │
│  Prometheus → Grafana (métriques)                            │
│  Loki + Promtail → Grafana (logs)                            │
│  Alertmanager (alertes)                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## Stack technique

| Couche | Technologie | Version |
|--------|-------------|---------|
| **API Backend** | .NET / ASP.NET Core | 8.0 |
| **Service IA** | Python / FastAPI | 3.11 |
| **Base de données** | PostgreSQL | 16-alpine |
| **Métriques** | Prometheus + Grafana | Latest |
| **Logs** | Loki + Promtail | Latest |
| **Tracing** | Jaeger + OpenTelemetry | Latest |
| **Alertes** | Alertmanager | Latest |
| **Conteneurs** | Docker Compose | 3.9 |
| **Orchestration** | Kubernetes / Minikube | 1.32 |
| **Charts** | Helm | 3.x |
| **CI/CD** | GitHub Actions | — |
| **Qualité code** | SonarCloud | — |
| **Sécurité images** | Trivy | Latest |

---

## Prérequis

| Outil | Version minimale | Installation |
|-------|-----------------|--------------|
| Docker | 24.x | [docs.docker.com](https://docs.docker.com/get-docker/) |
| Docker Compose | 2.x | Inclus avec Docker Desktop |
| kubectl | 1.28+ | [kubernetes.io](https://kubernetes.io/docs/tasks/tools/) |
| Minikube | 1.32+ | [minikube.sigs.k8s.io](https://minikube.sigs.k8s.io/docs/start/) |
| Helm | 3.x | [helm.sh](https://helm.sh/docs/intro/install/) |

---

## Démarrage rapide

```bash
# Cloner le dépôt
git clone https://github.com/bourseia/coin-system.git
cd coin-system

# Déploiement interactif (recommandé)
./deploy.sh

# Ou directement en Docker Compose
docker compose -f docker-compose.prod.yml up -d
```

Accès après démarrage :

| Service | URL | Identifiants |
|---------|-----|--------------|
| API | http://localhost:5000 | — |
| Service IA | http://localhost:8000 | — |
| Grafana | http://localhost:3000 | admin / admin |
| Prometheus | http://localhost:9090 | — |
| Jaeger | http://localhost:16686 | — |
| Alertmanager | http://localhost:9093 | — |
| PostgreSQL | localhost:5433 | bourseia / secret123 |

---

## Déploiement

### Option 1 — Docker Compose (Développement / Test)

```bash
docker compose -f docker-compose.prod.yml up -d

# Vérifier la santé des services
docker compose -f docker-compose.prod.yml ps
curl http://localhost:5000/health
```

### Option 2 — Kubernetes avec Kustomize (Production)

```bash
# Démarrer Minikube
minikube start --driver=docker

# Charger les images locales
eval $(minikube docker-env)
docker build -t bourseia-api:latest -f projet/BourseIA/Dockerfile.prod projet/BourseIA/
docker build -t coin-ai:latest coin_ai/

# Déployer
kubectl apply -k k8s/base/

# Suivre le déploiement
kubectl rollout status deployment/api -n coin-system
kubectl get pods -n coin-system
```

### Option 3 — Helm (Production recommandé)

```bash
# Installation
helm upgrade --install coin-system helm/coin-system \
  --namespace coin-helm \
  --create-namespace \
  --set namespace.name=coin-helm \
  -f helm/coin-system/values.yaml

# Vérification
helm status coin-system -n coin-helm
```

### Script automatisé

```bash
./deploy.sh
# 1) Docker Compose    — développement / test
# 2) Kubernetes        — production Minikube
# 3) Helm              — production recommandé
```

---

## Observabilité

### Dashboards Grafana (http://localhost:3000)

| Dashboard | Description |
|-----------|-------------|
| **Node Exporter Full** | Métriques système (CPU, RAM, I/O) |
| **Logs / App** | Logs des conteneurs via Loki |
| **K8s Cluster Monitoring** | Métriques Kubernetes (nécessite K8s) |

### Datasources préconfigurées

- **Prometheus** — `http://prometheus:9090` (défaut)
- **Loki** — `http://loki:3100`

### Alertes

Les règles d'alerte Prometheus sont définies dans `observability/alert-rules.yml`.  
Alertmanager est accessible sur http://localhost:9093.

### Tracing distribué

L'API et le service IA envoient des traces vers OpenTelemetry Collector → Jaeger.  
Visualisation : http://localhost:16686

---

## CI/CD

### Workflows GitHub Actions

```
.github/workflows/
├── build-deploy.yml    # Pipeline principal
└── infra-tests.yml     # Validation infrastructure
```

### Pipeline `build-deploy.yml`

```
push/PR → lint-and-test (SonarCloud)
        → security-scan (Trivy)
        → build-api (ghcr.io)
        → build-ai  (ghcr.io)
        → deploy-staging  (branche develop)
        → deploy-production (branche main)
```

### Secrets GitHub requis

| Secret | Description |
|--------|-------------|
| `SONAR_TOKEN` | Token SonarCloud |
| `HELM_REPO_URL` | URL du registry Helm |
| `SLACK_WEBHOOK` | Webhook Slack pour notifications |

---

## Sécurité

### SonarCloud (Qualité du code)

Analyse statique automatique sur chaque PR et push.  
Configuration : `sonar-project.properties`

Métriques surveillées :
- Bugs et vulnérabilités
- Code smells
- Couverture de tests
- Duplications

### Trivy (Scan de vulnérabilités)

Scan du système de fichiers et des images Docker sur chaque pipeline.  
Les résultats sont envoyés dans l'onglet **Security** de GitHub.

```bash
# Scan local
trivy fs --ignorefile .trivyignore .

# Scan image
trivy image bourseia-api:latest
```

Fichier d'exclusions : `.trivyignore`

---

## Structure du projet

```
coin/
├── .github/
│   └── workflows/
│       ├── build-deploy.yml        # CI/CD principal
│       └── infra-tests.yml         # Tests infra
├── coin_ai/                        # Service IA Python
│   ├── Dockerfile.prod
│   ├── main.py
│   ├── model.py
│   └── requirements.txt
├── helm/
│   └── coin-system/                # Chart Helm
│       ├── Chart.yaml
│       ├── values.yaml
│       └── templates/
├── k8s/
│   ├── base/                       # Manifestes Kustomize
│   └── overlays/                   # Overlays par environnement
├── observability/
│   ├── grafana/                    # Dashboards et datasources
│   ├── loki-config.yaml
│   ├── promtail-config.yaml
│   ├── otel-collector-config.yaml
│   ├── prometheus.yml
│   ├── alertmanager.yml
│   └── alert-rules.yml
├── projet/
│   └── BourseIA/                   # API .NET 8
│       ├── Dockerfile.prod
│       ├── Program.cs
│       └── ...
├── docker-compose.prod.yml
├── deploy.sh                       # Script de déploiement interactif
├── sonar-project.properties        # Config SonarCloud
├── .trivyignore                    # Exclusions Trivy
└── README.md
```

---

## Variables d'environnement

Copiez `.env.dev` ou `.env.prod` et adaptez :

```env
# Base de données
POSTGRES_USER=bourseia
POSTGRES_PASSWORD=secret123
POSTGRES_DB=bourseia_db

# JWT
JWT_SECRET=your-secret-key-here
JWT_ISSUER=BourseIA
JWT_AUDIENCE=BourseIA-Client

# Grafana
GRAFANA_USER=admin
GRAFANA_PASSWORD=admin123

# Slack (optionnel)
SLACK_WEBHOOK=https://hooks.slack.com/...
```

---

## Développement local

```bash
# API .NET (depuis projet/BourseIA/)
dotnet run

# Service IA Python (depuis coin_ai/)
pip install -r requirements.txt
uvicorn main:app --reload --port 8000

# Base de données uniquement
docker compose -f docker-compose.prod.yml up -d postgres
```

---

## Licence

MIT — voir [LICENSE](LICENSE)
