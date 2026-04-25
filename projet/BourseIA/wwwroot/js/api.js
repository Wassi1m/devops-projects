const API_BASE = '/api';

function getToken() {
  return localStorage.getItem('bourseIA_token');
}

function setToken(token) {
  localStorage.setItem('bourseIA_token', token);
}

function clearToken() {
  localStorage.removeItem('bourseIA_token');
  localStorage.removeItem('bourseIA_user');
}

function getUser() {
  const u = localStorage.getItem('bourseIA_user');
  return u ? JSON.parse(u) : null;
}

function setUser(user) {
  localStorage.setItem('bourseIA_user', JSON.stringify(user));
}

async function apiFetch(endpoint, options = {}) {
  const token = getToken();
  const headers = { 'Content-Type': 'application/json', ...options.headers };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const response = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });

  if (response.status === 401) {
    clearToken();
    showPage('auth');
    throw new Error('Session expirée. Veuillez vous reconnecter.');
  }

  const text = await response.text();
  const data = text ? JSON.parse(text) : {};

  if (!response.ok) {
    const msg = data.message || data.title || extractErrors(data.errors) || `Erreur ${response.status}`;
    throw new Error(msg);
  }
  return data;
}

function extractErrors(errors) {
  if (!errors) return null;
  return Object.values(errors).flat().join(' ');
}

async function apiUpload(endpoint, formData) {
  const token = getToken();
  const headers = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  const response = await fetch(`${API_BASE}${endpoint}`, { method: 'POST', headers, body: formData });

  if (response.status === 401) {
    clearToken();
    showPage('auth');
    throw new Error('Session expirée.');
  }
  const data = await response.json();
  if (!response.ok) throw new Error(data.message || `Erreur ${response.status}`);
  return data;
}

// ─── Auth ─────────────────────────────────────────────────────────────────
const AuthApi = {
  async register(dto) { return apiFetch('/auth/register', { method: 'POST', body: JSON.stringify(dto) }); },
  async login(dto) { return apiFetch('/auth/login', { method: 'POST', body: JSON.stringify(dto) }); },
  async getProfil() { return apiFetch('/auth/profil'); },
  async getDashboard() { return apiFetch('/auth/tableau-de-bord'); },
  async updateProfil(dto) { return apiFetch('/auth/profil', { method: 'PUT', body: JSON.stringify(dto) }); }
};

// ─── Upload ───────────────────────────────────────────────────────────────
const UploadApi = {
  async upload(fichier, typeAction) {
    const form = new FormData();
    form.append('fichier', fichier);
    if (typeAction) form.append('typeAction', typeAction);
    return apiUpload('/upload', form);
  },
  async getMesCourbes() {
    return apiFetch('/upload');
  },
  async supprimer(id) {
    return apiFetch(`/upload/${id}`, { method: 'DELETE' });
  }
};

// ─── Analyse ──────────────────────────────────────────────────────────────
const AnalyseApi = {
  async analyser(courbeId) {
    return apiFetch(`/analysis/analyser/${courbeId}`, { method: 'POST' });
  },
  async getHistorique() {
    return apiFetch('/analysis/historique');
  }
};

// ─── Idées ────────────────────────────────────────────────────────────────
const IdeaApi = {
  async creer(dto) {
    return apiFetch('/idea', { method: 'POST', body: JSON.stringify(dto) });
  },
  async getMesIdees() {
    return apiFetch('/idea/mes-idees');
  },
  async getPubliques() {
    return apiFetch('/idea/publiques');
  },
  async supprimer(id) {
    return apiFetch(`/idea/${id}`, { method: 'DELETE' });
  }
};

// ─── Teams ────────────────────────────────────────────────────────────────
const TeamApi = {
  async creer(dto) { return apiFetch('/team', { method: 'POST', body: JSON.stringify(dto) }); },
  async getMesTeams() { return apiFetch('/team'); },
  async getDetail(id) { return apiFetch(`/team/${id}`); },
  async inviter(dto) { return apiFetch('/team/inviter', { method: 'POST', body: JSON.stringify(dto) }); },
  async supprimer(id) { return apiFetch(`/team/${id}`, { method: 'DELETE' }); },
  async quitter(id) { return apiFetch(`/team/${id}/quitter`, { method: 'DELETE' }); },
  async repondre(dto) { return apiFetch('/team/invitation/repondre', { method: 'POST', body: JSON.stringify(dto) }); },
  async kick(teamId, membreId) { return apiFetch(`/team/${teamId}/membre/${membreId}/kick`, { method: 'DELETE' }); },
  async ban(teamId, membreId) { return apiFetch(`/team/${teamId}/membre/${membreId}/ban`, { method: 'POST' }); },
  async debloquer(teamId, membreId) { return apiFetch(`/team/${teamId}/membre/${membreId}/debloquer`, { method: 'POST' }); },
  async changerRole(teamId, membreId, role) { return apiFetch(`/team/${teamId}/membre/${membreId}/role`, { method: 'PUT', body: JSON.stringify({ membreId, nouveauRole: role }) }); },
  async getInvitations() { return apiFetch('/team/invitations'); },
  async getCourbsEquipe(teamId) { return apiFetch(`/team/${teamId}/courbes`); },
  async retirerPartage(teamId, courbeId) { return apiFetch(`/team/${teamId}/courbes/${courbeId}`, { method: 'DELETE' }); },
  async partager(dto) { return apiFetch('/team/partager', { method: 'POST', body: JSON.stringify(dto) }); }
};

// ─── Chat ─────────────────────────────────────────────────────────────────
const ChatApi = {
  async envoyer(dto) { return apiFetch('/chat/envoyer', { method: 'POST', body: JSON.stringify(dto) }); },
  async getConversations() { return apiFetch('/chat/conversations'); },
  async getConversationPrivee(contactId) { return apiFetch(`/chat/prive/${contactId}`); },
  async getMessagesTeam(teamId) { return apiFetch(`/chat/equipe/${teamId}`); },
  async marquerLu(contactId, teamId) {
    const q = contactId ? `?contactId=${contactId}` : `?teamId=${teamId}`;
    return apiFetch(`/chat/marquer-lu${q}`, { method: 'POST' });
  }
};

// ─── Notifications ────────────────────────────────────────────────────────
const NotifApi = {
  async getAll() { return apiFetch('/notification'); },
  async marquerToutesLues() { return apiFetch('/notification/tout-lire', { method: 'POST' }); }
};

