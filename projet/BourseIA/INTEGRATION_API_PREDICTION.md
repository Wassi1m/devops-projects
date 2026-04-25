# Intégration API de Détection d'Actions Boursières

## 📋 Résumé des modifications

### 1. **Nouveau Contrôleur : `PredictController.cs`**
   - **Route** : `POST /api/predict`
   - **Fonctionnalité** : Envoie une image à l'API de détection d'actions boursières
   - **Paramètres** : Fichier (`multipart/form-data`)
   - **Réponse** : Résultat JSON de l'API de détection

### 2. **Configuration**
   - **appsettings.json** : URL par défaut pour Docker → `http://host.docker.internal:8000/predict`
   - **appsettings.Development.json** : URL pour développement local → `http://localhost:8000/predict`
   - **docker-compose.yml** : Environnement configuré + `extra_hosts` pour accéder à l'hôte

### 3. **Correctifs**
   - ✅ Résolution du problème d'ID NULL dans la base de données
   - ✅ Configuration PostgreSQL pour auto-incrémentation

---

## 🚀 Utilisation

### Via Postman/curl

```bash
curl -X POST http://localhost:5000/api/predict \
  -F "file=@/chemin/vers/image.png"
```

### Via JavaScript (Client)

```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch('http://localhost:5000/api/predict', {
  method: 'POST',
  body: formData
});

const result = await response.json();
console.log(result);
```

---

## 🐳 Configuration Docker

### Variables d'environnement

```yaml
DetectionApi__Url: "http://host.docker.internal:8000/predict"
extra_hosts:
  - "host.docker.internal:host-gateway"  # Pour Linux/Mac
```

### Démarrage

```bash
docker-compose up --build -d
```

---

## 📝 Statuts HTTP

| Code | Signification |
|------|---------------|
| 200  | Succès |
| 400  | Fichier manquant ou invalide |
| 503  | API de détection indisponible |
| 500  | Erreur serveur |

---

## 🔧 Modification de l'URL de l'API

### Option 1 : Variable d'environnement Docker

```bash
docker-compose exec api sh
export DetectionApi__Url="http://nouvelle-url:8000/predict"
```

### Option 2 : Dans `docker-compose.yml`

```yaml
environment:
  DetectionApi__Url: "http://votre-api:8000/predict"
```

### Option 3 : En développement local

Modifier `appsettings.Development.json` :

```json
"DetectionApi": {
  "Url": "http://localhost:8000/predict"
}
```

---

## ✅ Vérifications

Assurez-vous que :

1. ✅ L'API de détection tourne sur le port **8000**
2. ✅ L'endpoint est **`/predict`** et accepte du **`multipart/form-data`** avec un champ **`file`**
3. ✅ Docker peut accéder à l'hôte (surtout sur Linux avec `extra_hosts`)
4. ✅ La base de données PostgreSQL est accessible

---

## 🐛 Dépannage

### Erreur 503 - Service indisponible

- Vérifiez que l'API de détection tourne : `curl http://localhost:8000/predict`
- Vérifiez l'URL dans les fichiers de configuration

### Erreur 404 - Endpoint non trouvé

- Vérifiez que le contrôleur est compilé dans l'image Docker
- Reconstruisez avec `docker-compose up --build`

### Fichier non transmis correctement

- Assurez-vous d'utiliser `multipart/form-data`
- Le champ doit s'appeler `file`

---

## 📦 Fichiers modifiés

- ✅ `Controllers/PredictController.cs` (nouveau)
- ✅ `Program.cs` (ajout `AddHttpClient()`)
- ✅ `appsettings.json` (ajout `DetectionApi`)
- ✅ `appsettings.Development.json` (ajout `DetectionApi`)
- ✅ `docker-compose.yml` (ajout variables et `extra_hosts`)
- ✅ `Models/Utilisateur.cs` (correction collections)
- ✅ `Data/AppDbContext.cs` (configuration PostgreSQL)

---

## 📚 Ressources

- Documentation officielle Microsoft : https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
- Documentation PostgreSQL : https://www.postgresql.org/docs/
- Documentation Docker : https://docs.docker.com/compose/

