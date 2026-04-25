"""
Coin AI v3 - Entraînement optimisé pour >75% de confiance

Stratégie:
1. Phases 1-8:  Backbone gelé → entraîne seulement le classifieur
2. Phases 9+:   Fine-tuning complet avec LR réduit
3. Label smoothing pour éviter l'overconfidence sur du bruit
4. WeightedRandomSampler pour équilibrer UP/DOWN
5. CosineAnnealing LR + Early stopping
6. Augmentation forte pour régularisation
"""

import os
import sys
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, random_split, WeightedRandomSampler
from torchvision import datasets, transforms
from tqdm import tqdm
import json
import numpy as np

from config import (
    CHARTS_DIR, MODEL_PATH, MODEL_DIR,
    CHART_IMAGE_SIZE, NUM_CLASSES, DEVICE,
    BATCH_SIZE, LEARNING_RATE, NUM_EPOCHS, TRAIN_SPLIT, PATIENCE
)
from model import get_model


def get_device():
    if DEVICE == "cuda" and torch.cuda.is_available():
        d = torch.device("cuda")
        print(f"🚀 GPU: {torch.cuda.get_device_name(0)}")
        return d
    d = torch.device("cpu")
    print("💻 CPU")
    return d


def count_images():
    """Compte les images par classe."""
    counts = {}
    for cls in os.listdir(CHARTS_DIR):
        cls_dir = os.path.join(CHARTS_DIR, cls)
        if os.path.isdir(cls_dir):
            n = len([f for f in os.listdir(cls_dir) if f.endswith('.png')])
            counts[cls] = n
    return counts


def main():
    print("=" * 60)
    print("🧠 Coin AI v3 - Entraînement optimisé (objectif: >75%)")
    print("=" * 60)

    if not os.path.exists(CHARTS_DIR):
        print("❌ Exécutez d'abord: python generate_charts.py")
        sys.exit(1)

    device = get_device()

    # Vérifier les images
    img_counts = count_images()
    total_imgs = sum(img_counts.values())
    print(f"\n📷 Images disponibles: {total_imgs}")
    for cls, n in sorted(img_counts.items()):
        print(f"   {cls}: {n}")

    if total_imgs < 1000:
        print("⚠️  Moins de 1000 images! Résultats possiblement médiocres.")
        print("   Exécutez: python generate_charts.py pour plus de données.")

    # === TRANSFORMS ===
    # Augmentation forte pour l'entraînement
    train_transform = transforms.Compose([
        transforms.Resize((256, 256)),
        transforms.RandomCrop(CHART_IMAGE_SIZE),
        transforms.RandomHorizontalFlip(p=0.5),
        transforms.RandomRotation(degrees=10),
        transforms.ColorJitter(brightness=0.3, contrast=0.3, saturation=0.15),
        transforms.RandomAffine(degrees=0, translate=(0.06, 0.06), scale=(0.94, 1.06)),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
        transforms.RandomErasing(p=0.15, scale=(0.02, 0.1)),
    ])

    val_transform = transforms.Compose([
        transforms.Resize(CHART_IMAGE_SIZE),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
    ])

    # === DATASET ===
    full_dataset = datasets.ImageFolder(root=CHARTS_DIR, transform=train_transform)
    if len(full_dataset) == 0:
        print("❌ Aucune image!")
        sys.exit(1)

    class_names = full_dataset.classes
    print(f"\n📊 Classes: {class_names}")

    # Split
    train_size = int(TRAIN_SPLIT * len(full_dataset))
    val_size = len(full_dataset) - train_size
    train_dataset, val_dataset_raw = random_split(
        full_dataset, [train_size, val_size],
        generator=torch.Generator().manual_seed(42)
    )

    # Validation avec transforms propres (sans augmentation)
    val_dataset_clean = datasets.ImageFolder(root=CHARTS_DIR, transform=val_transform)
    val_dataset = torch.utils.data.Subset(val_dataset_clean, val_dataset_raw.indices)

    print(f"📊 Train: {len(train_dataset)} | Val: {len(val_dataset)}")

    # === SAMPLER PONDÉRÉ (équilibrage des classes) ===
    train_labels = [full_dataset.targets[i] for i in train_dataset.indices]
    class_count = np.bincount(train_labels)
    class_weights = 1.0 / class_count.astype(float)
    sample_weights = [class_weights[label] for label in train_labels]
    sampler = WeightedRandomSampler(sample_weights, num_samples=len(sample_weights), replacement=True)

    # DataLoaders
    train_loader = DataLoader(
        train_dataset, batch_size=BATCH_SIZE, sampler=sampler,
        num_workers=2, pin_memory=True, drop_last=True
    )
    val_loader = DataLoader(
        val_dataset, batch_size=BATCH_SIZE, shuffle=False,
        num_workers=2, pin_memory=True
    )

    # === MODÈLE ===
    model = get_model(architecture="efficientnet", num_classes=NUM_CLASSES, pretrained=True)

    # Phase 1: Geler le backbone
    freeze_epochs = 8
    for param in model.backbone.features.parameters():
        param.requires_grad = False

    model = model.to(device)
    print(f"\n🏗️  EfficientNet-B0 | Classifieur: 1280→512→128→2")
    print(f"🔒 Backbone gelé pour {freeze_epochs} premières epochs")

    # Loss avec label smoothing (réduit l'overconfidence)
    total_samples = sum(class_count)
    loss_weights = [total_samples / (len(class_names) * c) for c in class_count]
    criterion = nn.CrossEntropyLoss(
        weight=torch.FloatTensor(loss_weights).to(device),
        label_smoothing=0.05  # Légère smoothing
    )
    print(f"⚖️  Poids classes: {dict(zip(class_names, [f'{w:.2f}' for w in loss_weights]))}")
    print(f"🏷️  Label smoothing: 0.05")

    # Optimizer phase 1 (classifieur seulement)
    optimizer = optim.AdamW(
        filter(lambda p: p.requires_grad, model.parameters()),
        lr=LEARNING_RATE, weight_decay=1e-4
    )
    scheduler = optim.lr_scheduler.CosineAnnealingLR(optimizer, T_max=freeze_epochs, eta_min=1e-6)

    print(f"\n⚙️  LR={LEARNING_RATE} | Batch={BATCH_SIZE} | Epochs={NUM_EPOCHS}")
    print(f"🛑 Early stopping: patience={PATIENCE}")
    print()
    print("-" * 70)

    best_val_acc = 0.0
    patience_counter = 0
    history = {"train_loss": [], "train_acc": [], "val_loss": [], "val_acc": []}

    for epoch in range(NUM_EPOCHS):
        # === DÉGEL du backbone après freeze_epochs ===
        if epoch == freeze_epochs:
            print()
            print("🔓 DÉGEL du backbone → Fine-tuning complet")
            print("-" * 70)
            for param in model.backbone.features.parameters():
                param.requires_grad = True

            # Nouvel optimizer avec LR plus bas pour le backbone
            optimizer = optim.AdamW([
                {'params': model.backbone.features.parameters(), 'lr': LEARNING_RATE * 0.05},
                {'params': model.backbone.classifier.parameters(), 'lr': LEARNING_RATE * 0.5},
            ], weight_decay=1e-4)
            scheduler = optim.lr_scheduler.CosineAnnealingLR(
                optimizer, T_max=NUM_EPOCHS - freeze_epochs, eta_min=1e-7
            )
            patience_counter = 0  # Reset patience

        # === TRAIN ===
        model.train()
        t_loss, t_correct, t_total = 0.0, 0, 0

        pbar = tqdm(train_loader, desc=f"Epoch {epoch+1:2d}/{NUM_EPOCHS} [Train]", leave=False)
        for images, labels in pbar:
            images, labels = images.to(device), labels.to(device)

            optimizer.zero_grad()
            outputs = model(images)
            loss = criterion(outputs, labels)
            loss.backward()
            torch.nn.utils.clip_grad_norm_(model.parameters(), max_norm=1.0)
            optimizer.step()

            t_loss += loss.item() * images.size(0)
            t_correct += (outputs.argmax(1) == labels).sum().item()
            t_total += labels.size(0)

        # === VALIDATION ===
        model.eval()
        v_loss, v_correct, v_total = 0.0, 0, 0

        with torch.no_grad():
            for images, labels in tqdm(val_loader, desc=f"Epoch {epoch+1:2d}/{NUM_EPOCHS} [Valid]", leave=False):
                images, labels = images.to(device), labels.to(device)
                outputs = model(images)
                loss = criterion(outputs, labels)
                v_loss += loss.item() * images.size(0)
                v_correct += (outputs.argmax(1) == labels).sum().item()
                v_total += labels.size(0)

        scheduler.step()

        train_acc = t_correct / t_total
        val_acc = v_correct / v_total
        train_loss = t_loss / t_total
        val_loss = v_loss / v_total
        lr_now = optimizer.param_groups[0]['lr']

        history["train_loss"].append(train_loss)
        history["train_acc"].append(train_acc)
        history["val_loss"].append(val_loss)
        history["val_acc"].append(val_acc)

        phase = "GELÉ" if epoch < freeze_epochs else "FINE "
        print(f"  [{phase}] Epoch {epoch+1:2d}: "
              f"Train={train_acc:.4f} | Val={val_acc:.4f} | "
              f"VLoss={val_loss:.4f} | LR={lr_now:.2e}")

        if val_acc > best_val_acc:
            best_val_acc = val_acc
            patience_counter = 0
            torch.save(model.state_dict(), MODEL_PATH)
            print(f"         💾 Meilleur modèle! (val_acc={val_acc:.4f})")
        else:
            patience_counter += 1
            if patience_counter >= PATIENCE:
                print(f"\n🛑 Early stopping après {epoch+1} epochs (patience={PATIENCE})")
                break

    # === SAUVEGARDER ===
    with open(os.path.join(MODEL_DIR, "training_history.json"), "w") as f:
        json.dump(history, f, indent=2)

    metadata = {
        "architecture": "efficientnet_b0",
        "num_classes": NUM_CLASSES,
        "class_names": class_names,
        "best_val_acc": best_val_acc,
        "chart_window": 40,
        "prediction_horizon": 10,
        "image_size": list(CHART_IMAGE_SIZE),
        "total_images": len(full_dataset),
        "train_size": train_size,
        "val_size": val_size,
        "epochs_trained": epoch + 1,
        "learning_rate": LEARNING_RATE,
        "batch_size": BATCH_SIZE,
        "label_smoothing": 0.05,
        "min_price_change": 0.02,
    }
    with open(os.path.join(MODEL_DIR, "model_metadata.json"), "w") as f:
        json.dump(metadata, f, indent=2)

    print()
    print("=" * 60)
    print(f"✅ Entraînement terminé!")
    print(f"   🏆 Meilleure accuracy validation: {best_val_acc:.4f} ({best_val_acc*100:.1f}%)")
    print(f"   💾 Modèle: {MODEL_PATH}")
    if best_val_acc >= 0.75:
        print(f"   🎯 OBJECTIF >75% ATTEINT!")
    else:
        print(f"   ⚠️  Objectif 75% non atteint ({best_val_acc*100:.1f}%)")
        print(f"   💡 Essayez: plus de données (augmenter MAX_SYMBOLS dans config.py)")
    print("=" * 60)


if __name__ == "__main__":
    main()
