"""
Coin AI v3 - Génération de graphiques boursiers optimisée
Clé de la performance: labels CLAIRS (seuil 2%) + indicateurs techniques riches.
"""

import os
import sys
import random
import pandas as pd
import numpy as np
import mplfinance as mpf
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from tqdm import tqdm

from config import (
    ETFS_DIR, STOCKS_DIR, CHARTS_UP_DIR, CHARTS_DOWN_DIR,
    CHART_WINDOW, PREDICTION_HORIZON, MIN_PRICE_CHANGE,
    MAX_CHARTS_PER_SYMBOL, MAX_SYMBOLS
)


def load_csv(filepath: str) -> pd.DataFrame:
    """Charge et nettoie un fichier CSV boursier."""
    try:
        df = pd.read_csv(filepath, parse_dates=["Date"])
        df = df.sort_values("Date").reset_index(drop=True)
        required = {"Date", "Open", "High", "Low", "Close", "Volume"}
        if not required.issubset(set(df.columns)):
            return None
        df = df.dropna(subset=["Open", "High", "Low", "Close", "Volume"])
        df = df[df["Volume"] > 0]
        df = df[df["Close"] > 0]
        df = df[df["Open"] > 0]
        if len(df) < CHART_WINDOW + PREDICTION_HORIZON + 20:
            return None
        df = df.set_index("Date")
        return df
    except Exception:
        return None


def get_label(df, start_idx: int, window: int, horizon: int):
    """
    Label avec seuil de clarté.
    Utilise la moyenne des prix futurs vs prix actuel.
    Retourne None si la variation est trop faible (cas ambigu).
    """
    end_idx = start_idx + window
    future_end = end_idx + horizon

    if future_end >= len(df):
        return None

    current_close = df.iloc[end_idx - 1]["Close"]
    if current_close <= 0:
        return None

    # Moyenne pondérée des prix futurs (plus de poids sur les derniers jours)
    future_closes = df.iloc[end_idx:future_end]["Close"].values
    weights = np.linspace(0.5, 1.5, len(future_closes))
    future_avg = np.average(future_closes, weights=weights)

    pct_change = (future_avg - current_close) / current_close

    # Rejeter les cas ambigus
    if abs(pct_change) < MIN_PRICE_CHANGE:
        return None

    return "UP" if pct_change > 0 else "DOWN"


def generate_chart_image(df_window: pd.DataFrame, save_path: str) -> bool:
    """
    Génère un graphique candlestick enrichi:
    - Bougies japonaises
    - SMA 7 (court terme) en cyan
    - SMA 20 (moyen terme) en jaune
    - Bandes de Bollinger (SMA20 ± 2*std) en gris
    - Volume avec couleurs
    """
    try:
        # Calculer les indicateurs
        close = df_window["Close"]
        sma7 = close.rolling(window=7, min_periods=1).mean()
        sma20 = close.rolling(window=20, min_periods=1).mean()
        std20 = close.rolling(window=20, min_periods=1).std().fillna(0)
        bb_upper = sma20 + 2 * std20
        bb_lower = sma20 - 2 * std20

        # Lignes additionnelles
        apds = [
            mpf.make_addplot(sma7, color='#00e5ff', width=1.0),       # SMA7 cyan
            mpf.make_addplot(sma20, color='#ffd600', width=1.0),      # SMA20 jaune
            mpf.make_addplot(bb_upper, color='#555555', width=0.7, linestyle='--'),  # BB haut
            mpf.make_addplot(bb_lower, color='#555555', width=0.7, linestyle='--'),  # BB bas
        ]

        mc = mpf.make_marketcolors(
            up='#26a69a', down='#ef5350',
            edge='inherit',
            wick={'up': '#26a69a', 'down': '#ef5350'},
            volume={'up': '#26a69a', 'down': '#ef5350'}
        )
        style = mpf.make_mpf_style(
            marketcolors=mc,
            base_mpf_style='nightclouds',
            gridstyle='',
            y_on_right=False,
            facecolor='#0d1117'
        )

        fig, axes = mpf.plot(
            df_window,
            type='candle',
            volume=True,
            style=style,
            addplot=apds,
            figsize=(4, 4),
            returnfig=True,
            tight_layout=True
        )

        # Nettoyer les axes
        for ax in axes:
            ax.set_xticks([])
            ax.set_yticks([])
            ax.set_xlabel('')
            ax.set_ylabel('')
            for spine in ax.spines.values():
                spine.set_visible(False)

        fig.savefig(save_path, dpi=72, bbox_inches='tight',
                    pad_inches=0.01, facecolor='#0d1117')
        plt.close(fig)
        return True
    except Exception:
        plt.close('all')
        return False


def process_symbol(filepath: str, symbol: str, max_charts: int) -> dict:
    """Traite un symbole et génère les graphiques avec labels clairs."""
    df = load_csv(filepath)
    if df is None:
        return {"up": 0, "down": 0}

    counts = {"up": 0, "down": 0}
    total_possible = len(df) - CHART_WINDOW - PREDICTION_HORIZON

    if total_possible <= 0:
        return counts

    # Échantillonner avec un pas régulier pour diversité temporelle
    step = max(1, total_possible // (max_charts * 3))
    indices = list(range(0, total_possible, step))
    random.shuffle(indices)

    generated = 0
    for idx in indices:
        if generated >= max_charts:
            break

        label = get_label(df, idx, CHART_WINDOW, PREDICTION_HORIZON)
        if label is None:
            continue  # Cas ambigu, on skip

        df_window = df.iloc[idx:idx + CHART_WINDOW]
        if len(df_window) < CHART_WINDOW - 2:
            continue

        save_dir = CHARTS_UP_DIR if label == "UP" else CHARTS_DOWN_DIR
        filename = f"{symbol}_{idx}.png"
        save_path = os.path.join(save_dir, filename)

        if os.path.exists(save_path):
            counts["up" if label == "UP" else "down"] += 1
            generated += 1
            continue

        if generate_chart_image(df_window, save_path):
            counts["up" if label == "UP" else "down"] += 1
            generated += 1

    return counts


def main():
    print("=" * 60)
    print("🎨 Coin AI v3 - Génération optimisée pour >75% confiance")
    print("=" * 60)

    csv_files = []
    for directory, src in [(STOCKS_DIR, "Stock"), (ETFS_DIR, "ETF")]:
        if os.path.exists(directory):
            for f in os.listdir(directory):
                if f.endswith(".csv"):
                    csv_files.append({
                        "path": os.path.join(directory, f),
                        "symbol": f.replace(".csv", "")
                    })

    if not csv_files:
        print("❌ Aucun fichier CSV trouvé!")
        sys.exit(1)

    random.seed(42)
    random.shuffle(csv_files)
    csv_files = csv_files[:MAX_SYMBOLS]

    print(f"📊 Symboles: {len(csv_files)}")
    print(f"📈 Fenêtre: {CHART_WINDOW}j | Horizon: {PREDICTION_HORIZON}j")
    print(f"🔍 Seuil label: ±{MIN_PRICE_CHANGE*100:.0f}% (rejette les cas ambigus)")
    print(f"🖼️  Max {MAX_CHARTS_PER_SYMBOL} charts/symbole")
    print(f"📐 Indicateurs: Candlestick + SMA7 + SMA20 + Bollinger Bands")
    print()

    total_up, total_down = 0, 0

    for item in tqdm(csv_files, desc="Génération"):
        counts = process_symbol(item["path"], item["symbol"], MAX_CHARTS_PER_SYMBOL)
        total_up += counts["up"]
        total_down += counts["down"]

    total = total_up + total_down
    balance = min(total_up, total_down) / max(total_up, total_down) * 100 if total > 0 else 0

    print()
    print("=" * 60)
    print(f"✅ Génération terminée!")
    print(f"   📈 UP:      {total_up} images")
    print(f"   📉 DOWN:    {total_down} images")
    print(f"   📊 Total:   {total} images")
    print(f"   ⚖️  Balance: {balance:.1f}%")
    print("=" * 60)


if __name__ == "__main__":
    main()
