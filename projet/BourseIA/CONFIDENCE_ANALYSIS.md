# 🎯 Guide Complet - Analyse de Confiance des Prédictions

## 📋 Vue d'ensemble

L'analyse de confiance évalue la fiabilité des prédictions d'actions boursières retournées par l'API de détection. Elle fournit :

- ✅ Score de confiance global (0-100%)
- ✅ Analyse des prédictions par seuil de confiance
- ✅ Probabilités de tendance (UP/DOWN/HOLD)
- ✅ Rapport d'analyse détaillé
- ✅ Filtrage automatique des prédictions

---

## 🔧 Architecture

### Composants

1. **`ConfidenceAnalysisService.cs`** : Logique métier d'analyse
2. **`PredictionDtos.cs`** : Modèles de données
3. **`PredictController.cs`** : Endpoints HTTP
4. **`prediction.js`** : Client côté client
5. **`prediction.html`** : Interface utilisateur

---

## 📊 Endpoints API

### 1. Prédiction avec Analyse de Confiance (par défaut)

```http
POST /api/predict
Content-Type: multipart/form-data

file: <image>
```

**Réponse** :
```json
{
  "success": true,
  "prediction": {
    "predictions": [
      {
        "label": "AAPL",
        "confidence": 0.95,
        "signal": "HOLD",
        "probabilities": {
          "UP": 0.55,
          "DOWN": 0.30,
          "HOLD": 0.15
        }
      }
    ]
  },
  "confidence_analysis": {
    "average_confidence": 0.92,
    "max_confidence": 0.95,
    "min_confidence": 0.87,
    "predictions_count": 3,
    "high_confidence_predictions": [...],
    "low_confidence_predictions": [],
    "confidence_threshold": 0.7
  },
  "timestamp": "2026-04-24T19:27:14Z"
}
```

### 2. Prédiction Brute (sans analyse)

```http
POST /api/predict/raw
Content-Type: multipart/form-data

file: <image>
```

**Réponse** : Réponse brute de l'API de détection (pas de traitement)

### 3. Analyse avec Seuil Personnalisé

```http
POST /api/predict/analyze?confidenceThreshold=0.8
Content-Type: multipart/form-data

file: <image>
```

**Paramètres** :
- `confidenceThreshold` (query, decimal, 0-1) : Seuil de confiance (défaut: 0.7)

---

## 📈 Structure des Données

### `PredictionResponseDto`
```csharp
{
    "predictions": List<PredictionDto>,       // Liste des prédictions
    "confidence_score": decimal,               // Score global
    "timestamp": DateTime,                     // Horodatage
    "message": string,                         // Message optionnel
    "signal": string,                          // Signal global (UP/DOWN/HOLD)
    "probabilities": ProbabilitiesDto          // Probabilités globales
}
```

### `PredictionDto`
```csharp
{
    "label": string,                          // Code action (AAPL, MSFT, etc.)
    "confidence": decimal,                     // Confiance (0-1)
    "signal": string,                          // Signal (UP/DOWN/HOLD)
    "probabilities": ProbabilitiesDto          // Probabilités
}
```

### `ConfidenceAnalysisDto`
```csharp
{
    "average_confidence": decimal,              // Moyenne
    "max_confidence": decimal,                  // Maximum
    "min_confidence": decimal,                  // Minimum
    "predictions_count": int,                   // Nombre total
    "high_confidence_predictions": List,        // ≥ seuil
    "low_confidence_predictions": List,         // < seuil
    "confidence_threshold": decimal             // Seuil appliqué
}
```

---

## 💻 Utilisation Client

### JavaScript (avec analyse)

```javascript
const service = new PredictionService()

// Avec seuil par défaut (0.7)
const result = await service.predictWithConfidenceAnalysis(file)

// Avec seuil personnalisé
const result = await service.predictWithConfidenceAnalysis(file, 0.8)

// Afficher l'analyse
console.log(result.confidence_analysis)
console.log(result.prediction.predictions)
```

### Vue.js

```vue
<template>
  <div>
    <input type="file" @change="handleFile" accept="image/*" />
    <button @click="analyze">Analyser</button>
    
    <div v-if="analysis">
      <h3>Confiance Moyenne: {{ (analysis.average_confidence * 100).toFixed(2) }}%</h3>
      
      <div v-for="pred in analysis.high_confidence_predictions">
        ✅ {{ pred.label }}: {{ (pred.confidence * 100).toFixed(2) }}%
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      service: new PredictionService(),
      file: null,
      analysis: null
    }
  },
  methods: {
    handleFile(e) {
      this.file = e.target.files[0]
    },
    async analyze() {
      this.analysis = await this.service.predictWithConfidenceAnalysis(this.file)
    }
  }
}
</script>
```

### React

```jsx
import { PredictionService } from './services/prediction'

function AnalysisComponent() {
  const [file, setFile] = useState(null)
  const [analysis, setAnalysis] = useState(null)
  const service = new PredictionService()

  const handleAnalyze = async () => {
    const result = await service.predictWithConfidenceAnalysis(file)
    setAnalysis(result.confidence_analysis)
  }

  return (
    <div>
      <input type="file" onChange={(e) => setFile(e.target.files[0])} />
      <button onClick={handleAnalyze}>Analyser</button>
      
      {analysis && (
        <div>
          <h3>Confiance Moyenne: {(analysis.average_confidence * 100).toFixed(2)}%</h3>
          {analysis.high_confidence_predictions.map(pred => (
            <div key={pred.label}>
              ✅ {pred.label}: {(pred.confidence * 100).toFixed(2)}%
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
```

---

## 📊 Interprétation des Scores

### Niveaux de Confiance

| Confiance | Interprétation | Action |
|-----------|---|---|
| **> 0.90** | Très haute | ✅ Fiable, action recommandée |
| **0.75 - 0.90** | Haute | ⚠️ Bonne, vérifier contexte |
| **0.60 - 0.75** | Modérée | ⚡ À considérer avec prudence |
| **0.50 - 0.60** | Faible | ❌ Peu fiable, nécessite confirmation |
| **< 0.50** | Très faible | 🚫 Ignorer ou revérifier |

### Signaux

- **UP** : Tendance haussière détectée
- **DOWN** : Tendance baissière détectée
- **HOLD** : Tendance stable / indécis

---

## 🎯 Cas d'Usage

### 1. Filtrer les Prédictions Fiables

```javascript
// Récupérer uniquement les prédictions avec confiance ≥ 75%
const highConfidence = await service.predictWithConfidenceAnalysis(file, 0.75)
console.log(highConfidence.confidence_analysis.high_confidence_predictions)
```

### 2. Générer un Rapport

```javascript
const result = await service.predictWithConfidenceAnalysis(file)
console.log(result.report)  // Affiche le rapport formaté
```

### 3. Décider d'une Action

```javascript
const analysis = result.confidence_analysis

if (analysis.average_confidence >= 0.8) {
  // Acheter / Trader
} else if (analysis.average_confidence >= 0.6) {
  // Observer
} else {
  // Ignorer
}
```

---

## 🔍 Détails Techniques

### Calcul de Confiance Globale

```
Score = (Moyenne × 0.6) + ((1 - ÉcartType) × 0.4)
```

- **60%** : Poids de la moyenne des confiances
- **40%** : Poids de la stabilité (inverse de l'écart-type)
- **Plafond** : Score max = 1.0 (100%)

### Filtrage

```csharp
// Haute confiance (≥ seuil)
var high = predictions.Where(p => p.Confidence >= 0.7)

// Faible confiance (< seuil)
var low = predictions.Where(p => p.Confidence < 0.7)
```

---

## ⚙️ Configuration

### Modifier le Seuil par Défaut

**Dans `PredictController.cs`** :
```csharp
[HttpPost]
[Route("analyze")]
public async Task<IActionResult> PostWithAnalysis(
    [FromForm] IFormFile file, 
    [FromQuery] decimal confidenceThreshold = 0.75m)  // ← Modifier ici
```

### Ajuster les Poids de Calcul

**Dans `ConfidenceAnalysisService.cs`** :
```csharp
// Modifier les poids (actuellement 0.6 et 0.4)
var globalScore = (average * 0.7m) + ((1 - (decimal)stdDev) * 0.3m)
```

---

## 📚 Exemple Complet

```bash
# 1. Uploader et analyser une image
curl -X POST http://localhost:5000/api/predict \
  -F "file=@chart.png"

# 2. Analyser avec seuil personnalisé
curl -X POST "http://localhost:5000/api/predict/analyze?confidenceThreshold=0.85" \
  -F "file=@chart.png"

# 3. Obtenir la réponse brute
curl -X POST http://localhost:5000/api/predict/raw \
  -F "file=@chart.png"
```

---

## 🐛 Dépannage

| Problème | Solution |
|----------|----------|
| Confiance très basse | Améliorer la qualité/clarté de l'image |
| Probabilités UP/DOWN/HOLD nulles | L'API n'inclut pas ces données |
| Analyse prend du temps | Réduire la taille de l'image |
| Scores incohérents | Vérifier le seuil de confiance appliqué |

---

## 📖 Fichiers Concernés

- ✅ `Controllers/PredictController.cs` - Endpoints
- ✅ `Services/ConfidenceAnalysisService.cs` - Logique
- ✅ `DTOs/PredictionDtos.cs` - Modèles
- ✅ `wwwroot/js/prediction.js` - Client JS
- ✅ `wwwroot/prediction.html` - Interface web
- ✅ `Program.cs` - Injection de dépendances

---

**Statut** : ✅ Intégration complète de la confiance
**Version** : 2.0
**Date** : 2026-04-24

