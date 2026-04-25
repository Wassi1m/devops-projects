// ═══════════════════════════════════════════════════════════
// 🎯 CLIENT DE PRÉDICTION - BourseIA
// ═══════════════════════════════════════════════════════════

/**
 * Service pour interagir avec l'API de prédiction
 */
class PredictionService {
    constructor(baseUrl = 'http://localhost:5000/api') {
        this.baseUrl = baseUrl;
        this.endpoint = '/predict';
    }

    /**
     * Envoie une image à l'API de détection
     * @param {File} file - Le fichier image
     * @returns {Promise<Object>} - Résultat de la prédiction
     */
    async predictFromFile(file) {
        if (!file) {
            throw new Error('Aucun fichier sélectionné');
        }

        // Validation du type de fichier
        const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            throw new Error(`Type de fichier non autorisé: ${file.type}`);
        }

        // Validation de la taille (max 10MB)
        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) {
            throw new Error(`Fichier trop volumineux: ${(file.size / 1024 / 1024).toFixed(2)}MB (max 10MB)`);
        }

        return this._sendPredictionRequest(file, `${this.baseUrl}${this.endpoint}`);
    }

    /**
     * Envoie une image avec analyse de confiance
     * @param {File} file - Le fichier image
     * @param {number} confidenceThreshold - Seuil de confiance (0-1, défaut: 0.7)
     * @returns {Promise<Object>} - Résultat avec analyse de confiance
     */
    async predictWithConfidenceAnalysis(file, confidenceThreshold = 0.7) {
        if (!file) {
            throw new Error('Aucun fichier sélectionné');
        }

        if (confidenceThreshold < 0 || confidenceThreshold > 1) {
            throw new Error('Le seuil de confiance doit être entre 0 et 1');
        }

        const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            throw new Error(`Type de fichier non autorisé: ${file.type}`);
        }

        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) {
            throw new Error(`Fichier trop volumineux: ${(file.size / 1024 / 1024).toFixed(2)}MB (max 10MB)`);
        }

        const url = `${this.baseUrl}/predict/analyze?confidenceThreshold=${confidenceThreshold}`;
        return this._sendPredictionRequest(file, url);
    }

    /**
     * Envoie une image sans analyse (réponse brute)
     * @param {File} file - Le fichier image
     * @returns {Promise<Object>} - Réponse brute de l'API
     */
    async predictRaw(file) {
        if (!file) {
            throw new Error('Aucun fichier sélectionné');
        }

        const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            throw new Error(`Type de fichier non autorisé: ${file.type}`);
        }

        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) {
            throw new Error(`Fichier trop volumineux: ${(file.size / 1024 / 1024).toFixed(2)}MB (max 10MB)`);
        }

        const url = `${this.baseUrl}/predict/raw`;
        return this._sendPredictionRequest(file, url);
    }
}

// ═══════════════════════════════════════════════════════════
// 🎨 INTERFACE UTILISATEUR
// ═══════════════════════════════════════════════════════════

class PredictionUI {
    constructor() {
        this.service = new PredictionService();
        this.initializeElements();
        this.attachEventListeners();
    }

    initializeElements() {
        this.fileInput = document.getElementById('imageInput');
        this.uploadBtn = document.getElementById('uploadBtn');
        this.resultDiv = document.getElementById('result');
        this.loadingDiv = document.getElementById('loading');
    }

    attachEventListeners() {
        this.uploadBtn?.addEventListener('click', () => this.handleFileUpload());
        this.fileInput?.addEventListener('change', (e) => this.previewImage(e));
    }

    /**
     * Traite l'upload de fichier
     */
    async handleFileUpload() {
        if (!this.fileInput?.files[0]) {
            alert('Veuillez sélectionner une image');
            return;
        }

        try {
            this.showLoading(true);
            const file = this.fileInput.files[0];
            const result = await this.service.predictWithConfidenceAnalysis(file);
            this.displayResult(result);
        } catch (error) {
            this.displayError(error.message);
        } finally {
            this.showLoading(false);
        }
    }

    /**
     * Affiche un aperçu de l'image
     */
    previewImage(event) {
        const file = event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            const preview = document.getElementById('imagePreview');
            if (preview) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            }
        };
        reader.readAsDataURL(file);
    }

    /**
     * Affiche le résultat de la prédiction avec analyse de confiance
     */
    displayResult(result) {
        if (!this.resultDiv) return;

        // Si le résultat est un objet avec une propriété 'detail'
        if (result.detail && Array.isArray(result.detail)) {
            // Erreur de validation
            this.resultDiv.innerHTML = `
                <div class="alert alert-danger">
                    <h4>Erreur de validation</h4>
                    <ul>
                        ${result.detail.map(err => `<li>${err.msg} (${err.loc.join(' > ')})</li>`).join('')}
                    </ul>
                </div>
            `;
            return;
        }

        // Affichage avec analyse de confiance
        if (result.success && result.confidence_analysis) {
            const analysis = result.confidence_analysis;
            const prediction = result.prediction;

            const confidenceHTML = `
                <div class="alert alert-success">
                    <h4>✅ Prédiction réussie avec analyse de confiance</h4>
                </div>

                <div class="card mb-3">
                    <div class="card-header bg-primary text-white">
                        <h5>📊 Analyse de Confiance</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <div class="stat-box">
                                    <div class="stat-value">${(analysis.average_confidence * 100).toFixed(2)}%</div>
                                    <div class="stat-label">Confiance Moyenne</div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box">
                                    <div class="stat-value">${(analysis.max_confidence * 100).toFixed(2)}%</div>
                                    <div class="stat-label">Confiance Max</div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box">
                                    <div class="stat-value">${(analysis.min_confidence * 100).toFixed(2)}%</div>
                                    <div class="stat-label">Confiance Min</div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box">
                                    <div class="stat-value">${analysis.predictions_count}</div>
                                    <div class="stat-label">Prédictions</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                ${this._generateHighConfidenceHTML(analysis)}
                ${this._generateLowConfidenceHTML(analysis)}
                ${this._generatePredictionsTable(prediction)}
                ${result.report ? `<pre class="report">${result.report}</pre>` : ''}
            `;

            this.resultDiv.innerHTML = confidenceHTML;
            return;
        }

        // Format personnalisé pour le résultat simple
        const resultHTML = this._formatResult(result);
        this.resultDiv.innerHTML = `
            <div class="alert alert-success">
                <h4>Prédiction réussie ✅</h4>
                <pre>${JSON.stringify(result, null, 2)}</pre>
            </div>
            ${resultHTML}
        `;
    }

    /**
     * Génère le HTML pour les prédictions de haute confiance
     * @private
     */
    _generateHighConfidenceHTML(analysis) {
        if (analysis.high_confidence_predictions.length === 0) return '';

        const predictions = analysis.high_confidence_predictions
            .sort((a, b) => b.confidence - a.confidence)
            .map(pred => `
                <div class="prediction-item high-confidence">
                    <div class="prediction-label">${pred.label}</div>
                    <div class="confidence-bar">
                        <div class="confidence-fill" style="width: ${pred.confidence * 100}%"></div>
                    </div>
                    <div class="confidence-value">${(pred.confidence * 100).toFixed(2)}%</div>
                    ${pred.signal ? `<div class="signal-badge">${pred.signal}</div>` : ''}
                </div>
            `).join('');

        return `
            <div class="card mb-3">
                <div class="card-header bg-success text-white">
                    <h5>✅ Prédictions de Haute Confiance (≥ ${(analysis.confidence_threshold * 100).toFixed(0)}%)</h5>
                </div>
                <div class="card-body">
                    ${predictions}
                </div>
            </div>
        `;
    }

    /**
     * Génère le HTML pour les prédictions de faible confiance
     * @private
     */
    _generateLowConfidenceHTML(analysis) {
        if (analysis.low_confidence_predictions.length === 0) return '';

        const predictions = analysis.low_confidence_predictions
            .sort((a, b) => a.confidence - b.confidence)
            .map(pred => `
                <div class="prediction-item low-confidence">
                    <div class="prediction-label">${pred.label}</div>
                    <div class="confidence-bar">
                        <div class="confidence-fill" style="width: ${pred.confidence * 100}%; background-color: #ff9800;"></div>
                    </div>
                    <div class="confidence-value">${(pred.confidence * 100).toFixed(2)}%</div>
                </div>
            `).join('');

        return `
            <div class="card mb-3">
                <div class="card-header bg-warning text-dark">
                    <h5>⚠️ Prédictions de Faible Confiance (< ${(analysis.confidence_threshold * 100).toFixed(0)}%)</h5>
                </div>
                <div class="card-body">
                    ${predictions}
                </div>
            </div>
        `;
    }

    /**
     * Génère un tableau de toutes les prédictions
     * @private
     */
    _generatePredictionsTable(prediction) {
        if (!prediction || !prediction.predictions || prediction.predictions.length === 0) return '';

        const rows = prediction.predictions
            .sort((a, b) => b.confidence - a.confidence)
            .map(pred => `
                <tr>
                    <td><strong>${pred.label}</strong></td>
                    <td>${(pred.confidence * 100).toFixed(2)}%</td>
                    <td>${pred.signal || '-'}</td>
                    <td>
                        ${pred.probabilities ? `
                            UP: ${(pred.probabilities.UP * 100).toFixed(1)}% |
                            DOWN: ${(pred.probabilities.DOWN * 100).toFixed(1)}% |
                            HOLD: ${(pred.probabilities.HOLD * 100).toFixed(1)}%
                        ` : '-'}
                    </td>
                </tr>
            `).join('');

        return `
            <div class="card">
                <div class="card-header">
                    <h5>📋 Tableau Complet des Prédictions</h5>
                </div>
                <div class="card-body" style="overflow-x: auto;">
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>Action</th>
                                <th>Confiance</th>
                                <th>Signal</th>
                                <th>Probabilités</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${rows}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    }

    /**
     * Formate le résultat pour affichage
     * @private
     */
    _formatResult(result) {
        // Adapter selon la structure de votre API de détection
        if (result.predictions) {
            return `
                <div class="predictions">
                    <h5>Prédictions détectées :</h5>
                    <ul>
                        ${result.predictions.map(pred => 
                            `<li><strong>${pred.label}</strong>: ${(pred.confidence * 100).toFixed(2)}%</li>`
                        ).join('')}
                    </ul>
                </div>
            `;
        }
        return '';
    }

    /**
     * Affiche un message d'erreur
     */
    displayError(message) {
        if (!this.resultDiv) return;
        this.resultDiv.innerHTML = `
            <div class="alert alert-danger">
                <h4>Erreur ❌</h4>
                <p>${message}</p>
            </div>
        `;
    }

    /**
     * Affiche/masque l'indicateur de chargement
     */
    showLoading(show) {
        if (this.loadingDiv) {
            this.loadingDiv.style.display = show ? 'block' : 'none';
        }
    }
}

// ═══════════════════════════════════════════════════════════
// 🚀 INITIALISATION
// ═══════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', () => {
    new PredictionUI();
});

// Export pour utilisation en module
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { PredictionService, PredictionUI };
}

