"""
Configuration du projet Coin AI v3
Optimisé pour une confiance > 75%
"""

import os

# === Chemins ===
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
DATA_DIR = os.path.join(BASE_DIR, "data_set", "archive (2)")
ETFS_DIR = os.path.join(DATA_DIR, "etfs")
STOCKS_DIR = os.path.join(DATA_DIR, "stocks")
META_CSV = os.path.join(DATA_DIR, "symbols_valid_meta.csv")

CHARTS_DIR = os.path.join(BASE_DIR, "charts")
CHARTS_UP_DIR = os.path.join(CHARTS_DIR, "UP")
CHARTS_DOWN_DIR = os.path.join(CHARTS_DIR, "DOWN")

MODEL_DIR = os.path.join(BASE_DIR, "models")
MODEL_PATH = os.path.join(MODEL_DIR, "coin_ai_model.pth")

# === Paramètres de génération ===
CHART_WINDOW = 40              # 40 jours de données pour le graphique
PREDICTION_HORIZON = 10        # 10 jours pour vérifier la tendance future
CHART_IMAGE_SIZE = (224, 224)
MIN_PRICE_CHANGE = 0.02        # Filtre: 2% minimum pour label clair
MAX_CHARTS_PER_SYMBOL = 40     # Graphiques par symbole
MAX_SYMBOLS = 800              # Nombre de symboles à traiter

# === Paramètres du modèle ===
NUM_CLASSES = 2
BATCH_SIZE = 32
LEARNING_RATE = 0.0003
NUM_EPOCHS = 30
TRAIN_SPLIT = 0.8
PATIENCE = 8                   # Early stopping
DEVICE = "cpu"

# === Seuils de prédiction ===
CONFIDENCE_THRESHOLD_BUY = 0.60
CONFIDENCE_THRESHOLD_SELL = 0.60

# Créer les dossiers
for d in [CHARTS_UP_DIR, CHARTS_DOWN_DIR, MODEL_DIR]:
    os.makedirs(d, exist_ok=True)
