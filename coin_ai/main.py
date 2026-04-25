"""
Coin AI - API FastAPI
Agent IA de prédiction des tendances boursières à partir d'images de graphiques.

Endpoints:
    POST /predict       → Prédire la tendance à partir d'une image
    GET  /health        → Vérifier l'état de l'API
    GET  /model/info    → Informations sur le modèle
"""

import os
import io
import json
import torch
from contextlib import asynccontextmanager
from PIL import Image
from torchvision import transforms
from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional

from config import (
    MODEL_PATH, MODEL_DIR, CHART_IMAGE_SIZE,
    CONFIDENCE_THRESHOLD_BUY, CONFIDENCE_THRESHOLD_SELL
)
from model import load_trained_model

# ============================================================
# Variables globales
# ============================================================

model = None
device = None
class_names = ["DOWN", "UP"]
model_metadata = None

inference_transform = transforms.Compose([
    transforms.Resize(CHART_IMAGE_SIZE),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
])


# ============================================================
# Lifespan
# ============================================================

@asynccontextmanager
async def lifespan(app: FastAPI):
    global model, device, model_metadata

    print("🚀 Coin AI - Démarrage de l'API...")

    if torch.cuda.is_available():
        device = torch.device("cuda")
        print(f"   🚀 GPU: {torch.cuda.get_device_name(0)}")
    else:
        device = torch.device("cpu")
        print("   💻 CPU")

    if os.path.exists(MODEL_PATH):
        try:
            model = load_trained_model(MODEL_PATH, architecture="efficientnet", device=str(device))
            print(f"   ✅ Modèle chargé: {MODEL_PATH}")
        except Exception as e:
            print(f"   ⚠️  Erreur chargement modèle: {e}")
            model = None
    else:
        print(f"   ⚠️  Modèle introuvable: {MODEL_PATH}")
        print("   ℹ️  Exécutez: python generate_charts.py && python train.py")
        model = None

    metadata_path = os.path.join(MODEL_DIR, "model_metadata.json")
    if os.path.exists(metadata_path):
        with open(metadata_path, "r") as f:
            model_metadata = json.load(f)
        print("   ✅ Métadonnées chargées")

    print("🪙 Coin AI prêt!")
    print("   📖 Documentation: http://localhost:8001/docs")

    yield

    print("🛑 Coin AI - Arrêt...")
    model = None


# ============================================================
# Application FastAPI
# ============================================================

app = FastAPI(
    lifespan=lifespan,
    title="🪙 Coin AI",
    description="""
## Agent IA de Prédiction des Tendances Boursières

Coin AI analyse les images de graphiques boursiers (candlestick, courbes)
et prédit la direction future du marché.

### Fonctionnement
1. **Envoyez** une image de graphique boursier (PNG/JPG)
2. **L'agent analyse** les patterns visuels avec un CNN (EfficientNet)
3. **Recevez** la prédiction: UP 📈 ou DOWN 📉 avec le niveau de confiance

### Signaux
- **BUY** 🟢 : Tendance haussière détectée (confiance > 65%)
- **SELL** 🔴 : Tendance baissière détectée (confiance > 65%)
- **HOLD** 🟡 : Signal incertain, rester en attente
    """,
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ============================================================
# Modèles Pydantic
# ============================================================

class PredictionResponse(BaseModel):
    prediction: str
    confidence: float
    signal: str
    probabilities: dict
    message: str


class HealthResponse(BaseModel):
    status: str
    model_loaded: bool
    device: str
    version: str


class ModelInfoResponse(BaseModel):
    architecture: str
    num_classes: int
    class_names: list
    best_val_accuracy: Optional[float]
    image_size: list
    chart_window: int
    prediction_horizon: int


# ============================================================
# Endpoints
# ============================================================

@app.get("/", tags=["Info"])
async def root():
    return {
        "name": "🪙 Coin AI",
        "description": "Agent IA de prédiction des tendances boursières",
        "version": "1.0.0",
        "endpoints": {
            "POST /predict": "Prédire la tendance à partir d'une image",
            "GET /health": "Vérifier l'état de l'API",
            "GET /model/info": "Informations sur le modèle",
            "GET /docs": "Documentation interactive (Swagger UI)"
        }
    }


@app.get("/health", response_model=HealthResponse, tags=["Info"])
async def health_check():
    return HealthResponse(
        status="healthy" if model is not None else "model_not_loaded",
        model_loaded=model is not None,
        device=str(device) if device else "unknown",
        version="1.0.0"
    )


@app.get("/model/info", response_model=ModelInfoResponse, tags=["Info"])
async def get_model_info():
    if model_metadata:
        return ModelInfoResponse(
            architecture=model_metadata.get("architecture", "efficientnet_b0"),
            num_classes=model_metadata.get("num_classes", 2),
            class_names=model_metadata.get("class_names", class_names),
            best_val_accuracy=model_metadata.get("best_val_acc"),
            image_size=model_metadata.get("image_size", list(CHART_IMAGE_SIZE)),
            chart_window=model_metadata.get("chart_window", 30),
            prediction_horizon=model_metadata.get("prediction_horizon", 5),
        )
    return ModelInfoResponse(
        architecture="efficientnet_b0", num_classes=2, class_names=class_names,
        best_val_accuracy=None, image_size=list(CHART_IMAGE_SIZE),
        chart_window=30, prediction_horizon=5,
    )


@app.post("/predict", response_model=PredictionResponse, tags=["Prediction"])
async def predict(file: UploadFile = File(..., description="Image du graphique boursier (PNG/JPG)")):
    """
    🔮 Prédire la tendance du marché à partir d'une image de graphique boursier.

    - **file**: Image PNG ou JPG d'un graphique (candlestick, courbe, etc.)
    - **Retourne**: prediction (UP/DOWN), confidence (%), signal (BUY/SELL/HOLD)
    """
    if model is None:
        raise HTTPException(status_code=503, detail="Modèle non chargé. Exécutez: python train.py")

    if file.content_type not in ["image/png", "image/jpeg", "image/jpg"]:
        raise HTTPException(status_code=400, detail=f"Format non supporté: {file.content_type}. Accepté: PNG, JPG")

    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents)).convert("RGB")
        input_tensor = inference_transform(image).unsqueeze(0).to(device)

        model.eval()
        with torch.no_grad():
            outputs = model(input_tensor)
            probabilities = torch.softmax(outputs, dim=1)

        probs = probabilities[0].cpu().numpy()
        predicted_class = int(probs.argmax())
        confidence = float(probs[predicted_class]) * 100
        prediction = class_names[predicted_class]

        prob_dict = {class_names[i]: round(float(probs[i]) * 100, 2) for i in range(len(class_names))}

        if prediction == "UP" and confidence >= CONFIDENCE_THRESHOLD_BUY * 100:
            signal, emoji, direction = "BUY", "📈", "HAUSSIÈRE"
        elif prediction == "DOWN" and confidence >= CONFIDENCE_THRESHOLD_SELL * 100:
            signal, emoji, direction = "SELL", "📉", "BAISSIÈRE"
        else:
            signal, emoji, direction = "HOLD", "⏸️", "INCERTAINE"

        if signal == "HOLD":
            message = f"⏸️ Tendance {direction} - Confiance insuffisante ({confidence:.1f}%) → Signal HOLD"
        else:
            message = f"{emoji} Tendance {direction} détectée avec {confidence:.1f}% de confiance → Signal {signal}"

        return PredictionResponse(
            prediction=prediction, confidence=round(confidence, 2),
            signal=signal, probabilities=prob_dict, message=message
        )

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur de prédiction: {str(e)}")


# ============================================================
# Point d'entrée
# ============================================================

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8001, reload=True)




