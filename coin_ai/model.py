"""
Coin AI v3 - Architecture CNN optimisée
EfficientNet-B0 avec classifieur renforcé et dropout adapté.
"""

import torch
import torch.nn as nn
from torchvision import models


class CoinAIModel(nn.Module):
    """
    CNN basé sur EfficientNet-B0 (transfer learning)
    Classifieur renforcé avec 2 couches cachées pour mieux capturer les patterns.
    """

    def __init__(self, num_classes: int = 2, pretrained: bool = True, dropout: float = 0.4):
        super(CoinAIModel, self).__init__()

        if pretrained:
            weights = models.EfficientNet_B0_Weights.IMAGENET1K_V1
        else:
            weights = None

        self.backbone = models.efficientnet_b0(weights=weights)
        in_features = self.backbone.classifier[1].in_features  # 1280

        # Classifieur renforcé: 1280 → 512 → 128 → 2
        self.backbone.classifier = nn.Sequential(
            nn.Dropout(p=dropout),
            nn.Linear(in_features, 512),
            nn.ReLU(),
            nn.BatchNorm1d(512),
            nn.Dropout(p=dropout * 0.5),
            nn.Linear(512, 128),
            nn.ReLU(),
            nn.BatchNorm1d(128),
            nn.Dropout(p=0.1),
            nn.Linear(128, num_classes)
        )

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        return self.backbone(x)

    def predict_proba(self, x: torch.Tensor) -> torch.Tensor:
        self.eval()
        with torch.no_grad():
            logits = self.forward(x)
            return torch.softmax(logits, dim=1)


def get_model(architecture: str = "efficientnet", num_classes: int = 2,
              pretrained: bool = True) -> nn.Module:
    """Factory pour créer le modèle."""
    if architecture == "efficientnet":
        return CoinAIModel(num_classes=num_classes, pretrained=pretrained)
    else:
        raise ValueError(f"Architecture inconnue: {architecture}")


def load_trained_model(model_path: str, architecture: str = "efficientnet",
                       device: str = "cpu") -> nn.Module:
    """Charge un modèle entraîné pour l'inférence."""
    model = get_model(architecture=architecture, num_classes=2, pretrained=False)
    state_dict = torch.load(model_path, map_location=device, weights_only=True)
    model.load_state_dict(state_dict)
    model.to(device)
    model.eval()
    return model
