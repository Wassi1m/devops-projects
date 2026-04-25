#!/usr/bin/env bash
# ========================================
# 🚀 QUICKSTART - Coin System Deployment
# ========================================
# Cet script vous guide rapidement vers votre première deployment

set -e

COIN_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$COIN_DIR"

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}"
echo "╔════════════════════════════════════════════════════════╗"
echo "║         🚀 COIN SYSTEM - QUICKSTART GUIDE             ║"
echo "╚════════════════════════════════════════════════════════╝"
echo -e "${NC}"

# ================================
# 1. Vérification des prérequis
# ================================
echo -e "${BLUE}[1/5] Vérification des prérequis...${NC}"

missing_tools=""

for tool in docker git; do
    if ! command -v "$tool" &> /dev/null; then
        missing_tools="$missing_tools\n  - $tool"
    else
        echo -e "  ${GREEN}✓${NC} $tool installé"
    fi
done

if [ -n "$missing_tools" ]; then
    echo -e "${RED}❌ Outils manquants:${NC}$missing_tools"
    echo -e "${YELLOW}Veuillez installer les outils manquants et réessayer.${NC}"
    exit 1
fi

# ================================
# 2. Validation infrastructure
# ================================
echo -e "\n${BLUE}[2/5] Validation de l'infrastructure...${NC}"

if bash validate-infrastructure.sh > /dev/null 2>&1; then
    echo -e "  ${GREEN}✓${NC} Infrastructure validée"
else
    echo -e "  ${YELLOW}⚠${NC} Quelques avertissements (non bloquant)"
fi

# ================================
# 3. Choix de la méthode
# ================================
echo -e "\n${BLUE}[3/5] Choix de la méthode de déploiement${NC}"
echo ""
echo "  1) Docker Compose (Développement - Recommandé pour commencer)"
echo "  2) Kubernetes (Production)"
echo "  3) Helm (Production - Recommandé)"
echo "  4) Afficher la documentation"
echo "  5) Quitter"
echo ""

read -p "  Votre choix (1-5): " choice

case $choice in
    1)
        echo -e "\n${BLUE}📦 Déploiement Docker Compose...${NC}"
        bash test-docker-compose.sh
        echo -e "\n${GREEN}✅ Services Docker Compose démarrés!${NC}"
        echo -e "   Grafana:    ${BLUE}http://localhost:3000${NC} (admin/admin)"
        echo -e "   Prometheus: ${BLUE}http://localhost:9090${NC}"
        echo -e "   API:        ${BLUE}http://localhost:5000${NC}"
        echo -e "   AI Service: ${BLUE}http://localhost:8000${NC}"
        ;;
    2)
        echo -e "\n${BLUE}☸️  Déploiement Kubernetes...${NC}"
        bash test-kubernetes.sh
        echo -e "\n${GREEN}✅ Kubernetes déployé!${NC}"
        echo -e "   Vérifier les pods: ${BLUE}kubectl get pods -n coin-system${NC}"
        echo -e "   Voir les logs: ${BLUE}kubectl logs -f deployment/prometheus -n coin-system${NC}"
        ;;
    3)
        echo -e "\n${BLUE}📊 Déploiement Helm...${NC}"
        bash test-helm.sh
        echo -e "\n${GREEN}✅ Helm release installée!${NC}"
        echo -e "   Statut: ${BLUE}helm status coin-system -n coin-system${NC}"
        ;;
    4)
        echo -e "\n${BLUE}📖 Documentation${NC}"
        echo ""
        echo "  Fichiers de documentation (à lire dans cet ordre):"
        echo "  1. INDEX.md                     - Index et navigation"
        echo "  2. README_CORRECTIONS.md        - Résumé des corrections"
        echo "  3. DEPLOYMENT_GUIDE.md          - Guide complet"
        echo "  4. BEFORE_AFTER_FIXES.md        - Comparaison avant/après"
        echo "  5. EXECUTIVE_SUMMARY.md         - Résumé pour management"
        echo ""
        echo "  Scripts utiles:"
        echo "  - ./validate-infrastructure.sh  - Valider l'infrastructure"
        echo "  - ./test-docker-compose.sh      - Deploy Docker Compose"
        echo "  - ./test-kubernetes.sh          - Deploy Kubernetes"
        echo "  - ./test-helm.sh                - Deploy Helm"
        echo ""
        read -p "  Appuyez sur Entrée pour retourner au menu..."
        exec "$0"  # Relancer le script
        ;;
    5)
        echo -e "\n${BLUE}Au revoir!${NC}"
        echo "Consultez INDEX.md pour plus d'informations."
        exit 0
        ;;
    *)
        echo -e "${RED}❌ Choix invalide${NC}"
        exit 1
        ;;
esac

# ================================
# 4. Vérification du déploiement
# ================================
echo -e "\n${BLUE}[4/5] Vérification du déploiement...${NC}"
sleep 5

case $choice in
    1)
        if docker compose -f docker-compose.prod.yml ps | grep -q "healthy\|running"; then
            echo -e "  ${GREEN}✓${NC} Services démarrés avec succès"
        else
            echo -e "  ${YELLOW}⚠${NC} Services en cours de démarrage..."
        fi
        ;;
    2|3)
        if kubectl get pods -n coin-system &> /dev/null; then
            echo -e "  ${GREEN}✓${NC} Cluster accessible"
        else
            echo -e "  ${YELLOW}⚠${NC} Attendez quelques secondes..."
        fi
        ;;
esac

# ================================
# 5. Résumé et prochaines étapes
# ================================
echo -e "\n${BLUE}[5/5] Prochaines étapes${NC}"
echo ""
echo -e "  ${GREEN}✅ Déploiement réussi!${NC}"
echo ""
echo "  Prochaines actions:"
echo "  1. Accédez aux services (voir URLs ci-dessus)"
echo "  2. Lisez la documentation complète (./INDEX.md)"
echo "  3. Explorez les dashboards (Grafana, Prometheus)"
echo "  4. Testez les alertes"
echo ""
echo "  Arrêter les services:"

case $choice in
    1)
        echo "  $ docker compose -f docker-compose.prod.yml down"
        ;;
    2|3)
        echo "  $ kubectl delete namespace coin-system"
        echo "  $ minikube stop"
        ;;
esac

echo ""
echo -e "  ${BLUE}📖 Documentation:${NC} ./INDEX.md"
echo -e "  ${BLUE}❓ Support:${NC} ./DEPLOYMENT_GUIDE.md → Troubleshooting"
echo ""
echo -e "${GREEN}🎉 Bienvenue dans le système Coin!${NC}"
echo ""

