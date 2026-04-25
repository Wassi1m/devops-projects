# ✅ Intégration Complète - API de Détection d'Actions Boursières

## 📊 Résumé des modifications apportées

### 🎯 Problèmes résolus

1. **❌ Erreur NULL ID** 
   - ✅ Configuration PostgreSQL auto-incrémentation
   - ✅ `Models/Utilisateur.cs` : Correction des collections
   - ✅ `Data/AppDbContext.cs` : Configuration `.ValueGeneratedOnAdd()`

2. **❌ Service HttpClient manquant**
   - ✅ `Program.cs` : Ajout `builder.Services.AddHttpClient()`

3. **❌ Configuration API Détection manquante**
   - ✅ `appsettings.json` & `appsettings.Development.json` : Clé `DetectionApi:Url`
   - ✅ `docker-compose.yml` : Variables d'environnement + `extra_hosts`

---

## 📁 Fichiers créés/modifiés

| Fichier | Type | Description |
|---------|------|-------------|
| `Controllers/PredictController.cs` | ✨ NOUVEAU | Contrôleur pour `/api/predict` |
| `wwwroot/js/prediction.js` | ✨ NOUVEAU | Client JavaScript réutilisable |
| `wwwroot/prediction.html` | ✨ NOUVEAU | Page de test avec interface |
| `INTEGRATION_API_PREDICTION.md` | ✨ NOUVEAU | Documentation complète |
| `Program.cs` | 🔧 MODIFIÉ | Ajout HttpClient générique |
| `appsettings.json` | 🔧 MODIFIÉ | Ajout configuration `DetectionApi` |
| `appsettings.Development.json` | 🔧 MODIFIÉ | Ajout configuration développement |
| `docker-compose.yml` | 🔧 MODIFIÉ | Variables + `extra_hosts` |
| `Models/Utilisateur.cs` | 🔧 MODIFIÉ | Correction collections |
| `Data/AppDbContext.cs` | 🔧 MODIFIÉ | Configuration PostgreSQL |

---

## 🚀 Guide de démarrage rapide

### 1. Vérifier que tout fonctionne

```bash
# Vérifier l'état des conteneurs
docker ps

# Tester l'enregistrement (doit retourner un token JWT)
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "prenom":"Test",
    "nom":"User",
    "email":"test@example.com",
    "motDePasse":"password123",
    "profilInvestisseur":"Débutant"
  }'
```

### 2. Lancer l'API de détection (sur l'hôte)

```bash
# Si elle tourne sur votre machine hôte
# Assurez-vous qu'elle écoute sur http://localhost:8000/predict
python app.py  # ou votre commande de démarrage
```

### 3. Tester le contrôleur de prédiction

#### Via curl
```bash
curl -X POST http://localhost:5000/api/predict \
  -F "file=@/chemin/vers/image.png"
```

#### Via Postman
- **Method** : POST
- **URL** : `http://localhost:5000/api/predict`
- **Body** : form-data
- **Key** : `file` (File)
- **Value** : Sélectionner une image

#### Via l'interface web
```
http://localhost:5000/prediction.html
```

---

## 🔌 Intégration dans votre frontend Vue/React/Angular

### Exemple Vue 3
```javascript
import { PredictionService } from '@/services/prediction.js'

export default {
  data() {
    return {
      service: new PredictionService(),
      result: null
    }
  },
  methods: {
    async analyzeImage(file) {
      try {
        this.result = await this.service.predictFromFile(file)
      } catch (error) {
        console.error(error)
      }
    }
  }
}
```

### Exemple React
```javascript
import { PredictionService } from './services/prediction'

function PredictionComponent() {
  const [result, setResult] = useState(null)
  const service = new PredictionService()

  const handleUpload = async (file) => {
    try {
      const result = await service.predictFromFile(file)
      setResult(result)
    } catch (error) {
      console.error(error)
    }
  }

  return (
    <div>
      <input type="file" onChange={(e) => handleUpload(e.target.files[0])} />
      {result && <pre>{JSON.stringify(result, null, 2)}</pre>}
    </div>
  )
}
```

---

## 🔧 Configuration avancée

### Modifier l'URL de l'API de détection

**En production (Docker)** :
```yaml
# docker-compose.yml
environment:
  DetectionApi__Url: "http://votre-serveur-prediction:8000/predict"
```

**En développement local** :
```json
// appsettings.Development.json
"DetectionApi": {
  "Url": "http://localhost:8000/predict"
}
```

### Ajouter l'authentification JWT pour `/api/predict`

```csharp
// Dans PredictController.cs
[Authorize]
[HttpPost]
public async Task<IActionResult> Post(IFormFile file)
{
    // ... reste du code
}
```

### Limiter la taille des fichiers

```csharp
// Dans Program.cs
builder.Services.Configure<FormOptions>(options => 
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});
```

---

## 📚 Structure de réponse attendue

L'API de détection devrait retourner un JSON structuré :

```json
{
  "predictions": [
    {
      "label": "AAPL",
      "confidence": 0.95
    },
    {
      "label": "MSFT",
      "confidence": 0.87
    }
  ],
  "timestamp": "2026-04-24T19:27:14Z"
}
```

---

## 🐛 Troubleshooting

| Problème | Solution |
|----------|----------|
| **503 Service indisponible** | Vérifier que l'API détection tourne sur port 8000 |
| **404 Not Found** | Reconstruire Docker : `docker-compose up --build` |
| **Timeout** | Augmenter le timeout dans `Program.cs` |
| **Fichier non envoyé** | Vérifier que le champ s'appelle `file` (multipart) |
| **CORS error** | Vérifier la politique CORS dans `Program.cs` |

---

## ✨ Prochaines étapes recommandées

1. ✅ Tester avec l'interface web : `http://localhost:5000/prediction.html`
2. ✅ Intégrer `PredictionService` dans votre frontend
3. ✅ Ajouter l'authentification JWT si souhaité
4. ✅ Implémenter la gestion des erreurs personnalisée
5. ✅ Logger les prédictions en base de données
6. ✅ Ajouter des métriques/analytics

---

## 📞 Support

- Documentation complète : `INTEGRATION_API_PREDICTION.md`
- Code du contrôleur : `Controllers/PredictController.cs`
- Client JavaScript : `wwwroot/js/prediction.js`
- Page de test : `wwwroot/prediction.html`

---

**Statut** : ✅ Intégration complète et fonctionnelle
**Version** : 1.0
**Date** : 2026-04-24

