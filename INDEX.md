# 📑 Index de Documentation - Infrastructure Coin

Bienvenue! Ce document vous aide à naviguer dans la documentation d'infrastructure du système Coin.

---

## 🎯 Pour Commencer Rapidement

### 1️⃣ Première Visite?
**Lisez d'abord**: [`README_CORRECTIONS.md`](README_CORRECTIONS.md)
- Résumé des corrections apportées
- État avant/après
- Points importants

### 2️⃣ Valider l'Infrastructure
**Exécutez**: `./validate-infrastructure.sh`
- Vérifie tous les composants
- Affiche les erreurs/avertissements
- Prêt en 30 secondes

### 3️⃣ Choisir une Méthode de Déploiement

| Méthode | Pour Qui | Commande |
|---------|----------|----------|
| **Docker Compose** | Développement | `./test-docker-compose.sh` |
| **Kubernetes** | Production | `./test-kubernetes.sh` |
| **Helm** | Production + Recommandé | `./test-helm.sh` |

---

## 📖 Documentation Détaillée

### 1. **README_CORRECTIONS.md** (Ce que vous devez savoir)
**Contenu**:
- Résumé des 5 problèmes corrigés
- État avant/après les corrections
- Fichiers modifiés
- Points importants

**Quand le lire**: Au démarrage du projet

### 2. **DEPLOYMENT_GUIDE.md** (Complet - 150+ lignes)
**Sections**:
- Configuration rapide
- Docker Compose (Développement)
- Kubernetes (Production)
- Helm (Production Recommandé)
- Troubleshooting détaillé
- Monitoring et Observabilité

**Quand le lire**: Avant de déployer

### 3. **CORRECTIONS_INFRASTRUCTURE.md** (Technique)
**Sections**:
- Problème 1: docker-compose.prod.yml
- Problème 2: k8s/base/loki.yaml (doublons)
- Problème 3: k8s/base/loki.yaml (PVC)
- Problème 4: helm templates
- Problème 5: helm secrets.yaml

**Quand le lire**: Pour comprendre techniquement les changements

---

## 🛠️ Scripts d'Aide

### Scripts de Déploiement

| Script | Description | Usage |
|--------|---|---|
| `test-docker-compose.sh` | Deploy Docker Compose | `bash test-docker-compose.sh` |
| `test-kubernetes.sh` | Deploy Kubernetes | `bash test-kubernetes.sh` |
| `test-helm.sh` | Deploy Helm | `bash test-helm.sh` |

### Script de Validation

| Script | Description | Usage |
|--------|---|---|
| `validate-infrastructure.sh` | Vérifier tous les composants | `bash validate-infrastructure.sh` |

---

## 🗂️ Structure des Fichiers

```
coin/
├── docker-compose.prod.yml          ✅ Corrigé (volumes)
├── k8s/
│   └── base/
│       ├── loki.yaml                ✅ Restructuré (PVC + ConfigMaps)
│       ├── kustomization.yaml
│       └── [autres fichiers]
├── helm/
│   └── coin-system/
│       └── templates/
│           ├── _helpers.tpl         ✅ Recréé (structure)
│           ├── secrets.yaml
│           └── [autres templates]
├── observability/
│   ├── prometheus.yml
│   ├── loki-config.yaml
│   ├── promtail-config.yaml
│   ├── alertmanager.yml
│   └── [autres configs]
├── coin_ai/                         Service d'IA
├── projet/BourseIA/                 API .NET
├── README_CORRECTIONS.md            📍 Lisez d'abord!
├── DEPLOYMENT_GUIDE.md              Guide complet
├── CORRECTIONS_INFRASTRUCTURE.md    Détails techniques
├── validate-infrastructure.sh       ✓ Script de validation
├── test-docker-compose.sh          Deploy script
├── test-kubernetes.sh              Deploy script
└── test-helm.sh                    Deploy script
```

---

## 🎓 Guide par Cas d'Usage

### Cas 1: Je commence le projet
```
1. Lisez: README_CORRECTIONS.md (5 min)
2. Exécutez: validate-infrastructure.sh (1 min)
3. Choisissez votre méthode: Docker / K8s / Helm
```

### Cas 2: Je déploie localement (Développement)
```
1. Lisez: DEPLOYMENT_GUIDE.md → Section "Docker Compose"
2. Exécutez: test-docker-compose.sh
3. Accédez: http://localhost:3000 (Grafana)
```

### Cas 3: Je déploie en production (Kubernetes)
```
1. Lisez: DEPLOYMENT_GUIDE.md → Section "Kubernetes"
2. Exécutez: test-kubernetes.sh
3. Vérifiez: kubectl get pods -n coin-system
```

### Cas 4: Je déploie en production (Helm - Recommandé)
```
1. Lisez: DEPLOYMENT_GUIDE.md → Section "Helm"
2. Exécutez: test-helm.sh
3. Vérifiez: helm status coin-system
```

### Cas 5: J'ai un problème
```
1. Consulter: DEPLOYMENT_GUIDE.md → Section "Troubleshooting"
2. Chercher par erreur:
   - "Error mounting" → Docker Compose section
   - "ImagePullBackOff" → Kubernetes section
   - "Template error" → Helm section
3. Exécuter la commande suggérée
```

---

## 🔗 Quick Links

### Accès aux Services (après déploiement)
- 🖥️ **Grafana**: http://localhost:3000 (admin/admin)
- 📊 **Prometheus**: http://localhost:9090
- 📝 **Loki**: http://localhost:3100
- 🔍 **Jaeger**: http://localhost:16686
- 🚨 **Alertmanager**: http://localhost:9093
- 🔗 **API**: http://localhost:5000
- 🤖 **AI Service**: http://localhost:8000

### Commandes Essentielles

#### Docker Compose
```bash
# Démarrer
docker compose -f docker-compose.prod.yml up -d

# Arrêter
docker compose -f docker-compose.prod.yml down

# Logs
docker compose -f docker-compose.prod.yml logs -f prometheus
```

#### Kubernetes
```bash
# Vérifier les pods
kubectl get pods -n coin-system

# Voir les logs
kubectl logs -f deployment/prometheus -n coin-system

# Port forward
kubectl port-forward -n coin-system svc/grafana 3000:3000
```

#### Helm
```bash
# Installer
helm install coin-system helm/coin-system/ -n coin-system --create-namespace

# Statut
helm status coin-system -n coin-system

# Logs
kubectl logs -f -l app.kubernetes.io/name=coin-system -n coin-system
```

---

## ✅ Checklist de Validation

Avant de considérer le déploiement comme terminé :

- [ ] `validate-infrastructure.sh` passe sans erreurs
- [ ] Services Docker Compose démarrent correctement
- [ ] Kubernetes pods sont tous en état "Running"
- [ ] Helm chart s'installe sans erreurs
- [ ] Accès à Grafana (http://localhost:3000)
- [ ] Prometheus collecte les métriques
- [ ] Loki collecte les logs
- [ ] API et AI Service répondent

---

## 🆘 Support et Troubleshooting

### Problème Commun 1: Volumes Docker
```bash
# Erreur: "Error mounting volume"
# Solution: Consulter DEPLOYMENT_GUIDE.md → Troubleshooting → Docker Compose
```

### Problème Commun 2: Images Kubernetes
```bash
# Erreur: "ImagePullBackOff"
# Solution: Consulter DEPLOYMENT_GUIDE.md → Troubleshooting → Kubernetes
```

### Problème Commun 3: Template Helm
```bash
# Erreur: "Template error"
# Solution: Consulter DEPLOYMENT_GUIDE.md → Troubleshooting → Helm
```

---

## 📚 Ressources Externes

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Helm Documentation](https://helm.sh/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Loki Documentation](https://grafana.com/docs/loki/)

---

## 📞 Prochaines Étapes

1. ✅ Lire `README_CORRECTIONS.md` (5 min)
2. ✅ Exécuter `validate-infrastructure.sh` (1 min)
3. ✅ Choisir une méthode de déploiement
4. ✅ Lire la section correspondante dans `DEPLOYMENT_GUIDE.md`
5. ✅ Exécuter le script de déploiement
6. ✅ Accéder aux services de monitoring

---

**Document mis à jour**: 25 avril 2026
**Dernière version**: 1.0.0
**Status**: ✅ Prêt pour la production

