// ═══ Navigation ══════════════════════════════════════════════════════════════
let currentPage = 'home';
let currentTeamId = null;

function showPage(name) {
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  document.getElementById(`page-${name}`)?.classList.add('active');
  currentPage = name;
  closeDropdowns();
  if (name === 'dashboard')    loadDashboard();
  if (name === 'upload')       loadCourbes();
  if (name === 'ideas')        loadIdeas('mes');
  if (name === 'teams')        loadTeams();
  if (name === 'chat')         loadChat();
  if (name === 'profile')      loadProfile();
}

async function openTeamDetail(id) {
  currentTeamId = id;
  showPage('team-detail');
  await loadTeamDetail(id);
}

function goHome() {
  const user = getUser();
  showPage(user ? 'dashboard' : 'home');
}

function toggleSection(id) {
  document.getElementById(id).classList.toggle('hidden');
}

function closeDropdowns() {
  document.getElementById('notifPanel')?.classList.add('hidden');
  document.getElementById('userDropdown')?.classList.add('hidden');
}

document.addEventListener('click', e => {
  if (!e.target.closest('.notif-wrapper')) document.getElementById('notifPanel')?.classList.add('hidden');
  if (!e.target.closest('.user-menu')) document.getElementById('userDropdown')?.classList.add('hidden');
});

// ═══ Session ══════════════════════════════════════════════════════════════════
function initSession() {
  const user = getUser();
  if (user && getToken()) {
    updateNavLoggedIn(user);
    showPage('dashboard');
  } else {
    showPage('home');
  }
}

function updateNavLoggedIn(user) {
  document.querySelectorAll('.auth-only').forEach(el => el.classList.remove('hidden'));
  document.getElementById('btnLogin').classList.add('hidden');
  const initials = (user.prenom[0] + user.nom[0]).toUpperCase();
  document.getElementById('navAvatar').textContent = initials;
  document.getElementById('navUserName').textContent = user.prenom;
  loadNotifications();
}

function updateNavLoggedOut() {
  document.querySelectorAll('.auth-only').forEach(el => el.classList.add('hidden'));
  document.getElementById('btnLogin').classList.remove('hidden');
}

function logout() {
  clearToken();
  updateNavLoggedOut();
  showPage('home');
}

function toggleUserMenu() {
  document.getElementById('userDropdown').classList.toggle('hidden');
}

// ═══ Auth ══════════════════════════════════════════════════════════════════════
function switchAuthTab(tab) {
  document.getElementById('form-login').classList.toggle('hidden', tab !== 'login');
  document.getElementById('form-register').classList.toggle('hidden', tab !== 'register');
  document.getElementById('tab-login').classList.toggle('active', tab === 'login');
  document.getElementById('tab-register').classList.toggle('active', tab === 'register');
}

async function login() {
  const email = document.getElementById('loginEmail').value.trim();
  const password = document.getElementById('loginPassword').value;
  const errEl = document.getElementById('loginError');
  errEl.classList.add('hidden');
  if (!email || !password) { showErr(errEl, 'Veuillez remplir tous les champs.'); return; }

  setLoading('loginBtn', 'loginSpinner', true);
  try {
    const res = await AuthApi.login({ email, motDePasse: password });
    setToken(res.token);
    setUser(res.utilisateur);
    updateNavLoggedIn(res.utilisateur);
    showPage('dashboard');
  } catch (e) { showErr(errEl, e.message); }
  finally { setLoading('loginBtn', 'loginSpinner', false); }
}

async function register() {
  const prenom = document.getElementById('regPrenom').value.trim();
  const nom = document.getElementById('regNom').value.trim();
  const email = document.getElementById('regEmail').value.trim();
  const motDePasse = document.getElementById('regPassword').value;
  const profilInvestisseur = document.getElementById('regProfil').value;
  const errEl = document.getElementById('registerError');
  errEl.classList.add('hidden');

  if (!prenom || !nom || !email || !motDePasse) { showErr(errEl, 'Tous les champs sont requis.'); return; }
  if (motDePasse.length < 6) { showErr(errEl, 'Mot de passe minimum 6 caractères.'); return; }

  setLoading('registerBtn', 'registerSpinner', true);
  try {
    const res = await AuthApi.register({ nom, prenom, email, motDePasse, profilInvestisseur });
    setToken(res.token);
    setUser(res.utilisateur);
    updateNavLoggedIn(res.utilisateur);
    showPage('dashboard');
  } catch (e) { showErr(errEl, e.message); }
  finally { setLoading('registerBtn', 'registerSpinner', false); }
}

// ═══ Dashboard ════════════════════════════════════════════════════════════════
let tendanceChartInstance = null;

async function loadDashboard() {
  const user = getUser();
  if (user) {
    document.getElementById('dashTitle').textContent = `Bonjour, ${user.prenom} 👋`;
    document.getElementById('dashWelcome').textContent = `${user.profilInvestisseur} · Tableajfnjnu de bord personnel`;
  }
  try {
    const stats = await AuthApi.getDashboard();
    animateCount('statTotal', stats.totalAnalyses);
    animateCount('statHausse', stats.analysesEnHausse);
    animateCount('statBaisse', stats.analysesEnBaisse);
    animateCount('statStable', stats.analysesStables);

    // Chart
    const ctx = document.getElementById('tendanceChart').getContext('2d');
    if (tendanceChartInstance) tendanceChartInstance.destroy();
    const hasData = stats.totalAnalyses > 0;
    document.getElementById('chartEmpty').classList.toggle('hidden', hasData);
    document.getElementById('tendanceChart').style.display = hasData ? 'block' : 'none';

    if (hasData) {
      tendanceChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['Hausse', 'Baisse', 'Stable'],
          datasets: [{ data: [stats.analysesEnHausse, stats.analysesEnBaisse, stats.analysesStables],
            backgroundColor: ['rgba(16,185,129,.8)', 'rgba(239,68,68,.8)', 'rgba(100,116,139,.8)'],
            borderColor: ['#10b981', '#ef4444', '#475569'],
            borderWidth: 1.5, hoverOffset: 6 }]
        },
        options: {
          responsive: true, maintainAspectRatio: false, cutout: '70%',
          plugins: { legend: { position: 'bottom', labels: { color: '#94a3b8', font: { size: 12 }, padding: 12 } } }
        }
      });
    }

    // Dernières analyses
    const container = document.getElementById('lastAnalyses');
    if (stats.dernieresAnalyses?.length) {
      container.innerHTML = stats.dernieresAnalyses.map(a => `
        <div class="analyse-item">
          <div>
            <div class="analyse-item-name">Courbe #${a.courbeBoursiereId}</div>
            <div class="analyse-item-date">${formatDate(a.dateAnalyse)}</div>
          </div>
          <div style="display:flex;align-items:center;gap:.75rem">
            ${a.prixMoyen ? `<span style="font-weight:700;color:var(--primary)">${a.prixMoyen.toFixed(2)}</span>` : ''}
            <span class="badge ${tendanceBadge(a.tendance)}">${tendanceIcon(a.tendance)} ${a.tendance}</span>
          </div>
        </div>`).join('');
    } else {
      container.innerHTML = `<div class="empty-state">Aucune analyse. <a href="#" onclick="showPage('upload')">Uploader →</a></div>`;
    }
  } catch (e) { console.error(e); }

  // Invitations en attente
  await loadPendingInvitations();
}

async function loadPendingInvitations() {
  try {
    const teams = await TeamApi.getMesTeams();
    const pending = await getPendingInvitations();
    const card = document.getElementById('invitationsCard');
    if (pending.length > 0) {
      card.style.display = 'block';
      document.getElementById('invitCount').textContent = `${pending.length} en attente`;
      document.getElementById('invitationsList').innerHTML = pending.map(inv => `
        <div class="pending-invit">
          <div>
            <div class="pending-invit-info">Invitation : <strong>${inv.teamNom}</strong></div>
            <div class="pending-invit-sub">par ${inv.createurNom}</div>
          </div>
          <div class="pending-actions">
            <button class="btn btn-sm btn-primary" onclick="repondreInvitation(${inv.teamId}, true)">✅ Accepter</button>
            <button class="btn btn-sm btn-danger-soft" onclick="repondreInvitation(${inv.teamId}, false)">❌ Refuser</button>
          </div>
        </div>`).join('');
    } else {
      card.style.display = 'none';
    }
  } catch (e) {}
}

async function getPendingInvitations() {
  try {
    const teams = await apiFetch('/team');
    return [];
  } catch { return []; }
}

async function repondreInvitation(teamId, accepter) {
  try {
    await TeamApi.repondre({ teamId, accepter });
    await loadPendingInvitations();
    await loadTeams();
  } catch (e) { alert(e.message); }
}

// ═══ Upload ═══════════════════════════════════════════════════════════════════
let selectedFile = null;

function handleFile(input) {
  const file = input.files[0];
  if (!file) return;
  selectedFile = file;
  document.getElementById('previewImg').src = URL.createObjectURL(file);
  document.getElementById('fileName').textContent = `${file.name} (${(file.size/1024).toFixed(0)} Ko)`;
  document.getElementById('dropzoneContent').classList.add('hidden');
  document.getElementById('previewWrap').classList.remove('hidden');
  document.getElementById('btnUpload').disabled = false;
}

function resetFile() {
  selectedFile = null;
  document.getElementById('fileInput').value = '';
  document.getElementById('dropzoneContent').classList.remove('hidden');
  document.getElementById('previewWrap').classList.add('hidden');
  document.getElementById('btnUpload').disabled = true;
}

async function uploadCourbe() {
  if (!selectedFile) return;
  const typeAction = document.getElementById('typeAction').value.trim();
  const errEl = document.getElementById('uploadError');
  const sucEl = document.getElementById('uploadSuccess');
  errEl.classList.add('hidden');
  sucEl.classList.add('hidden');

  setLoading('btnUpload', 'uploadSpinner', true);
  showProgress(30, 'Upload en cours...');

  try {
    const courbe = await UploadApi.upload(selectedFile, typeAction || null);
    showProgress(65, 'Analyse IA en cours...');

    const resultat = await AnalyseApi.analyser(courbe.id);
    showProgress(100, 'Terminé !');

    setTimeout(() => document.getElementById('uploadProgress').classList.add('hidden'), 800);
    sucEl.textContent = `✅ Analyse terminée — Tendance : ${resultat.tendance}`;
    sucEl.classList.remove('hidden');

    afficherResultatAnalyse(resultat);
    resetFile();
    document.getElementById('typeAction').value = '';
    loadCourbes();
  } catch (e) {
    showErr(errEl, e.message);
    document.getElementById('uploadProgress').classList.add('hidden');
  } finally {
    setLoading('btnUpload', 'uploadSpinner', false);
  }
}

function showProgress(pct, label) {
  document.getElementById('uploadProgress').classList.remove('hidden');
  document.getElementById('progressFill').style.width = pct + '%';
  document.getElementById('progressLabel').textContent = label;
}

function afficherResultatAnalyse(r) {
  const tendanceClass = r.tendance === 'Hausse' ? 'tendance-hausse' : r.tendance === 'Baisse' ? 'tendance-baisse' : 'tendance-stable';
  const statut = document.getElementById('analyseStatut');
  statut.className = `badge ${tendanceBadge(r.tendance)}`;
  statut.textContent = `${tendanceIcon(r.tendance)} ${r.tendance}`;

  document.getElementById('analyseContent').innerHTML = `
    <div class="analyse-tendance ${tendanceClass}">${tendanceIcon(r.tendance)} Tendance détectée : <strong>${r.tendance}</strong></div>
    <div class="analyse-stat"><span class="analyse-stat-val">${r.prixMin?.toFixed(2) ?? '—'}</span><div class="analyse-stat-label">Prix minimum</div></div>
    <div class="analyse-stat"><span class="analyse-stat-val">${r.prixMoyen?.toFixed(2) ?? '—'}</span><div class="analyse-stat-label">Prix moyen</div></div>
    <div class="analyse-stat"><span class="analyse-stat-val">${r.prixMax?.toFixed(2) ?? '—'}</span><div class="analyse-stat-label">Prix maximum</div></div>
    ${r.ecartType ? `<div class="analyse-stat"><span class="analyse-stat-val">${r.ecartType.toFixed(2)}</span><div class="analyse-stat-label">Écart-type</div></div>` : ''}
    ${r.pointsCles ? `<div class="analyse-points">📌 <strong>Points clés :</strong> ${r.pointsCles}</div>` : ''}
  `;
  document.getElementById('analyseResult').classList.remove('hidden');
}

async function loadCourbes() {
  try {
    const courbes = await UploadApi.getMesCourbes();
    const count = document.getElementById('courbesCount');
    count.textContent = `${courbes.length} courbe(s)`;
    const container = document.getElementById('courbesList');
    if (!courbes.length) { container.innerHTML = '<div class="empty-state">Aucune courbe uploadée.</div>'; return; }
    container.innerHTML = courbes.map(c => `
      <div class="courbe-item">
        <div>
          <div class="courbe-name">${c.nomFichier}</div>
          <div class="courbe-meta">${c.typeAction ? `🏷️ ${c.typeAction} · ` : ''}${formatDate(c.dateUpload)}</div>
        </div>
        <div class="courbe-actions">
          <span class="badge ${statutBadge(c.statut)}">${c.statut}</span>
          ${c.statut === 'EnAttente' ? `<button class="btn btn-sm btn-primary" onclick="analyserCourbe(${c.id})">🤖</button>` : ''}
          <button class="btn btn-sm btn-danger-soft" onclick="supprimerCourbe(${c.id})">🗑</button>
        </div>
      </div>`).join('');
  } catch (e) { console.error(e); }
}

async function analyserCourbe(id) {
  try {
    const r = await AnalyseApi.analyser(id);
    afficherResultatAnalyse(r);
    loadCourbes();
  } catch (e) { alert(e.message); }
}

async function supprimerCourbe(id) {
  if (!confirm('Supprimer cette courbe ?')) return;
  try { await UploadApi.supprimer(id); loadCourbes(); }
  catch (e) { alert(e.message); }
}

// ═══ Idées ════════════════════════════════════════════════════════════════════
let currentIdeas = [];

function filterIdeas(type, btn) {
  document.querySelectorAll('.filter-tab').forEach(t => t.classList.remove('active'));
  btn.classList.add('active');
  loadIdeas(type);
}

function filterIdeaSearch() {
  const q = document.getElementById('ideaSearch').value.toLowerCase();
  renderIdeas(currentIdeas.filter(i =>
    i.titre.toLowerCase().includes(q) ||
    i.description.toLowerCase().includes(q) ||
    (i.actionConcernee || '').toLowerCase().includes(q)
  ));
}

async function loadIdeas(type) {
  try {
    currentIdeas = type === 'mes' ? await IdeaApi.getMesIdees() : await IdeaApi.getPubliques();
    renderIdeas(currentIdeas);
  } catch (e) { console.error(e); }
}

function renderIdeas(idees) {
  const container = document.getElementById('ideasList');
  if (!idees.length) { container.innerHTML = '<p class="empty-state" style="grid-column:1/-1">Aucune idée trouvée.</p>'; return; }
  container.innerHTML = idees.map(i => `
    <div class="idea-card">
      <div class="idea-header">
        <div class="idea-titre">${i.titre}</div>
        <span class="badge ${tendanceBadge(i.tendance)}">${tendanceIcon(i.tendance)} ${i.tendance}</span>
      </div>
      <div class="idea-desc">${i.description}</div>
      <div style="display:flex;gap:.5rem;flex-wrap:wrap">
        ${i.actionConcernee ? `<span class="idea-action-tag">📌 ${i.actionConcernee}</span>` : ''}
        ${i.estPublique ? `<span class="badge badge-info">🌐 Public</span>` : ''}
        ${i.nomTeam ? `<span class="badge badge-stable">👥 ${i.nomTeam}</span>` : ''}
      </div>
      <div class="idea-footer">
        <span>✍️ ${i.nomAuteur}</span>
        ${i.rentabiliteEstimee ? `<span class="idea-rentabilite">+${i.rentabiliteEstimee}%</span>` : ''}
        <span>${formatDate(i.dateCreation)}</span>
      </div>
    </div>`).join('');
}

async function createIdea() {
  const titre = document.getElementById('ideaTitre').value.trim();
  const description = document.getElementById('ideaDesc').value.trim();
  const actionConcernee = document.getElementById('ideaAction').value.trim();
  const tendance = document.getElementById('ideaTendance').value;
  const rentabiliteEstimee = parseFloat(document.getElementById('ideaRentabilite').value) || null;
  const estPublique = document.getElementById('ideaPublique').checked;
  const errEl = document.getElementById('ideaError');
  errEl.classList.add('hidden');

  if (!titre || !description) { showErr(errEl, 'Titre et description requis.'); return; }
  try {
    await IdeaApi.creer({ titre, description, actionConcernee, tendance, rentabiliteEstimee, estPublique });
    toggleSection('createIdeaForm');
    document.getElementById('ideaTitre').value = '';
    document.getElementById('ideaDesc').value = '';
    document.getElementById('ideaRentabilite').value = '';
    loadIdeas('mes');
  } catch (e) { showErr(errEl, e.message); }
}

// ═══ Équipes ══════════════════════════════════════════════════════════════════
async function loadTeams() {
  try {
    const teams = await TeamApi.getMesTeams();
    const container = document.getElementById('teamsList');
    if (!teams.length) {
      container.innerHTML = '<div class="empty-state" style="grid-column:1/-1;padding:3rem">Créez votre première équipe pour collaborer !</div>';
      return;
    }
    container.innerHTML = teams.map(t => `
      <div class="team-card" id="tcard-${t.id}">
        <div class="team-card-header">
          <div>
            <div class="team-name">👥 ${t.nom}</div>
            <div class="team-desc">${t.description || 'Aucune description'}</div>
          </div>
          <span class="badge ${t.monRole === 'Admin' ? 'badge-admin' : 'badge-stable'}">${t.monRole === 'Admin' ? '⭐ Admin' : '👤 Membre'}</span>
        </div>
        <div class="team-divider"></div>
        <div class="team-members-row">
          <span>👥 <strong>${t.nombreMembres}</strong> membre(s)</span>
          <span style="font-size:.75rem;color:var(--text3)">Créée ${formatDate(t.dateCreation)}</span>
        </div>
        <div class="team-actions">
          <button class="btn btn-primary btn-sm" onclick="openTeamDetail(${t.id})">🔍 Ouvrir l'équipe</button>
          ${t.monRole === 'Admin'
            ? `<button class="btn btn-sm btn-danger-soft" onclick="supprimerTeam(${t.id},'${escapeStr(t.nom)}')">🗑 Supprimer</button>`
            : `<button class="btn btn-sm btn-danger-soft" onclick="quitterTeam(${t.id},'${escapeStr(t.nom)}')">🚪 Quitter</button>`}
        </div>
      </div>`).join('');
  } catch (e) { console.error(e); }
}

function toggleInvite(teamId) {
  const el = document.getElementById(`invite-${teamId}`);
  const isHidden = el.style.display === 'none';
  el.style.display = isHidden ? 'block' : 'none';
  if (isHidden) document.getElementById(`inviteEmail-${teamId}`).focus();
}

async function inviterMembre(teamId) {
  const emailEl = document.getElementById(`inviteEmail-${teamId}`);
  const msgEl = document.getElementById(`inviteMsg-${teamId}`);
  const email = emailEl.value.trim();
  msgEl.className = 'invite-msg';
  if (!email) { msgEl.className = 'invite-msg error'; msgEl.textContent = 'Email requis.'; return; }
  try {
    await TeamApi.inviter({ teamId, emailInvite: email });
    msgEl.className = 'invite-msg success';
    msgEl.textContent = `✅ Invitation envoyée à ${email}`;
    emailEl.value = '';
    setTimeout(() => loadTeams(), 1500);
  } catch (e) { msgEl.className = 'invite-msg error'; msgEl.textContent = `❌ ${e.message}`; }
}

async function createTeam() {
  const nom = document.getElementById('teamNom').value.trim();
  const description = document.getElementById('teamDesc').value.trim();
  const errEl = document.getElementById('teamError');
  errEl.classList.add('hidden');
  if (!nom) { showErr(errEl, 'Nom de l\'équipe requis.'); return; }
  try {
    await TeamApi.creer({ nom, description });
    toggleSection('createTeamForm');
    document.getElementById('teamNom').value = '';
    document.getElementById('teamDesc').value = '';
    loadTeams();
  } catch (e) { showErr(errEl, e.message); }
}

async function supprimerTeam(id, nom) {
  if (!confirm(`Supprimer définitivement "${nom}" ?`)) return;
  try { await TeamApi.supprimer(id); loadTeams(); }
  catch (e) { alert(e.message); }
}

async function quitterTeam(id, nom) {
  if (!confirm(`Quitter l'équipe "${nom}" ?`)) return;
  try { await TeamApi.quitter(id); loadTeams(); }
  catch (e) { alert(e.message); }
}

// ═══ Détail Équipe ════════════════════════════════════════════════════════════
let teamDetail = null;

async function loadTeamDetail(teamId) {
  try {
    teamDetail = await TeamApi.getDetail(teamId);
    const me = getUser();
    const isAdmin = teamDetail.monRole === 'Admin';

    document.getElementById('tdNom').textContent = `👥 ${teamDetail.nom}`;
    document.getElementById('tdDesc').textContent = teamDetail.description || '';
    document.getElementById('tdMonRole').textContent = isAdmin ? '⭐ Admin' : '👤 Membre';
    document.getElementById('tdNbMembres').textContent = `${teamDetail.nombreMembres} membre(s)`;

    document.querySelectorAll('.admin-only').forEach(el =>
      el.classList.toggle('hidden', !isAdmin));
    if (isAdmin) document.getElementById('tdBanSection').classList.remove('hidden');

    renderTdMembres(teamDetail.membres, me?.id, isAdmin);

    // Load first tab content
    switchTeamTab('membres', document.querySelector('.td-tab'));
  } catch (e) { alert('Erreur chargement équipe : ' + e.message); showPage('teams'); }
}

function switchTeamTab(tab, btn) {
  document.querySelectorAll('.td-tab').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.td-tab-content').forEach(t => t.classList.add('hidden'));
  btn?.classList.add('active');
  document.getElementById(`td-tab-${tab}`)?.classList.remove('hidden');

  if (tab === 'chat') loadTdChat();
  if (tab === 'analyses') loadTdAnalyses();
}

function renderTdMembres(membres, myId, isAdmin) {
  const actifs = membres.filter(m => m.statut === 'Accepté');
  const bannis = membres.filter(m => m.statut === 'Banni');
  const enAttente = membres.filter(m => m.statut === 'EnAttente');

  const renderMembre = (m, showBanActions) => {
    const isSelf = m.utilisateurId === myId;
    const isCreateur = teamDetail && m.utilisateurId === teamDetail.createurId;
    return `
      <div class="membre-item">
        <div class="membre-avatar">${(m.prenom[0]+m.nom[0]).toUpperCase()}</div>
        <div class="membre-info">
          <div class="membre-name">${m.prenom} ${m.nom} ${isSelf ? '<span class="badge badge-stable">Vous</span>' : ''}</div>
          <div class="membre-email">${m.email}</div>
        </div>
        <div class="membre-badges">
          <span class="badge ${m.role === 'Admin' ? 'badge-admin' : 'badge-stable'}">${m.role === 'Admin' ? '⭐ Admin' : '👤 Membre'}</span>
          ${m.statut === 'EnAttente' ? '<span class="badge badge-warn">⏳ En attente</span>' : ''}
          ${m.statut === 'Banni' ? '<span class="badge badge-down">🚫 Banni</span>' : ''}
        </div>
        ${isAdmin && !isSelf && !isCreateur ? `
          <div class="membre-actions">
            ${showBanActions ? `
              <button class="btn btn-xs btn-outline" onclick="tdDebloquer(${m.utilisateurId})" title="Débloquer">✅ Débloquer</button>
            ` : `
              <button class="btn btn-xs btn-outline" onclick="tdKick(${m.utilisateurId},'${escapeStr(m.prenom+' '+m.nom)}')" title="Retirer">👢 Kick</button>
              <button class="btn btn-xs btn-danger-soft" onclick="tdBan(${m.utilisateurId},'${escapeStr(m.prenom+' '+m.nom)}')" title="Bannir">🚫 Ban</button>
              ${m.role !== 'Admin' ? `<button class="btn btn-xs btn-outline" onclick="tdPromouvoir(${m.utilisateurId},'${escapeStr(m.prenom+' '+m.nom)}')" title="Promouvoir admin">⭐</button>` : ''}
            `}
          </div>
        ` : ''}
      </div>`;
  };

  document.getElementById('tdMembresList').innerHTML = actifs.length
    ? actifs.map(m => renderMembre(m, false)).join('')
    + (enAttente.length ? `<div class="membres-section-title">⏳ Invitations en attente (${enAttente.length})</div>`
      + enAttente.map(m => renderMembre(m, false)).join('') : '')
    : '<div class="empty-state">Aucun membre actif.</div>';

  if (isAdmin) {
    document.getElementById('tdBannisList').innerHTML = bannis.length
      ? bannis.map(m => renderMembre(m, true)).join('')
      : '<div class="empty-state" style="padding:1rem">Aucun membre banni.</div>';
  }
}

async function tdInviterMembre() {
  const email = document.getElementById('tdInviteEmail').value.trim();
  const errEl = document.getElementById('tdInviteError');
  errEl.classList.add('hidden');
  if (!email) { showErr(errEl, 'Email requis.'); return; }
  try {
    await TeamApi.inviter({ teamId: currentTeamId, emailInvite: email });
    document.getElementById('tdInviteEmail').value = '';
    toggleSection('tdInviteForm');
    await loadTeamDetail(currentTeamId);
  } catch (e) { showErr(errEl, e.message); }
}

async function tdKick(membreId, nom) {
  if (!confirm(`Retirer "${nom}" de l'équipe ?`)) return;
  try {
    await TeamApi.kick(currentTeamId, membreId);
    await loadTeamDetail(currentTeamId);
  } catch (e) { alert(e.message); }
}

async function tdBan(membreId, nom) {
  if (!confirm(`Bannir "${nom}" de l'équipe ? Cette personne ne pourra plus rejoindre l'équipe.`)) return;
  try {
    await TeamApi.ban(currentTeamId, membreId);
    await loadTeamDetail(currentTeamId);
  } catch (e) { alert(e.message); }
}

async function tdDebloquer(membreId) {
  try {
    await TeamApi.debloquer(currentTeamId, membreId);
    await loadTeamDetail(currentTeamId);
  } catch (e) { alert(e.message); }
}

async function tdPromouvoir(membreId, nom) {
  if (!confirm(`Promouvoir "${nom}" comme Admin ? Il aura tous les droits.`)) return;
  try {
    await TeamApi.changerRole(currentTeamId, membreId, 'Admin');
    await loadTeamDetail(currentTeamId);
  } catch (e) { alert(e.message); }
}

// ── Chat Équipe (dans détail) ────────────────────────────────────────────────
async function loadTdChat() {
  try {
    const msgs = await ChatApi.getMessagesTeam(currentTeamId);
    renderTdMessages(msgs);
    await ChatApi.marquerLu(null, currentTeamId);
  } catch (e) { console.error(e); }
}

function renderTdMessages(msgs) {
  const me = getUser();
  const container = document.getElementById('tdChatMessages');
  if (!msgs.length) {
    container.innerHTML = '<p style="text-align:center;color:var(--text3);padding:2rem;font-size:.875rem">Aucun message. Soyez le premier !</p>';
    return;
  }
  container.innerHTML = msgs.map(m => {
    const isMine = m.expediteurId === me?.id;
    return `<div class="msg-wrap ${isMine ? 'mine' : ''}">
      ${!isMine ? `<span style="font-size:.72rem;color:var(--text3);margin-bottom:2px">${m.nomExpediteur}</span>` : ''}
      <div class="msg-bubble ${isMine ? 'mine' : 'theirs'}">${escapeHtml(m.contenu)}</div>
      <span class="msg-meta">${formatTime(m.dateEnvoi)}</span>
    </div>`;
  }).join('');
  container.scrollTop = container.scrollHeight;
}

async function tdSendMessage() {
  const input = document.getElementById('tdChatInput');
  const text = input.value.trim();
  if (!text || !currentTeamId) return;
  input.value = '';
  try {
    await ChatApi.envoyer({ contenu: text, teamId: currentTeamId });
    await loadTdChat();
  } catch (e) { console.error(e); }
}

// ── Analyses Partagées (dans détail) ────────────────────────────────────────
async function loadTdAnalyses() {
  try {
    const [courbes, mesCourbes] = await Promise.all([
      TeamApi.getCourbsEquipe(currentTeamId),
      UploadApi.getMesCourbes()
    ]);

    // Remplir le select de partage
    const sel = document.getElementById('tdCourbeSelect');
    sel.innerHTML = mesCourbes.length
      ? mesCourbes.map(c => `<option value="${c.id}">${c.nomFichier}${c.typeAction ? ` (${c.typeAction})` : ''} — ${c.statut}</option>`).join('')
      : '<option value="">Aucune courbe disponible</option>';

    // Afficher les courbes partagées
    const container = document.getElementById('tdAnalysesList');
    if (!courbes.length) {
      container.innerHTML = '<div class="empty-state">Aucune courbe partagée dans cette équipe.</div>';
      return;
    }
    const me = getUser();
    container.innerHTML = courbes.map(p => {
      const canDelete = me?.id === p.partageParId || teamDetail?.monRole === 'Admin';
      const a = p.analyse;
      return `
        <div class="td-analyse-card">
          <div class="td-analyse-header">
            <div>
              <div class="td-analyse-title">📊 ${p.nomFichier}</div>
              <div class="td-analyse-meta">
                Partagé par <strong>${p.nomPartage}</strong> · ${formatDate(p.datePartage)}
                ${p.typeAction ? ` · 🏷️ ${p.typeAction}` : ''}
              </div>
              ${p.commentaire ? `<div class="td-analyse-comment">💬 ${escapeHtml(p.commentaire)}</div>` : ''}
            </div>
            ${canDelete ? `<button class="btn btn-xs btn-danger-soft" onclick="tdRetirerPartage(${p.courbeId})">✖ Retirer</button>` : ''}
          </div>
          ${a ? `
            <div class="td-analyse-result">
              <span class="badge ${tendanceBadge(a.tendance)}">${tendanceIcon(a.tendance)} ${a.tendance}</span>
              ${a.prixMoyen ? `<span class="analyse-val">Moy: <strong>${a.prixMoyen.toFixed(2)}</strong></span>` : ''}
              ${a.prixMin ? `<span class="analyse-val">Min: ${a.prixMin.toFixed(2)}</span>` : ''}
              ${a.prixMax ? `<span class="analyse-val">Max: ${a.prixMax.toFixed(2)}</span>` : ''}
              ${a.ecartType ? `<span class="analyse-val">σ: ${a.ecartType.toFixed(2)}</span>` : ''}
            </div>` : `<div class="td-analyse-no-analyse">⚠️ Analyse non encore effectuée (statut: ${p.statut})</div>`}
        </div>`;
    }).join('');
  } catch (e) { console.error(e); }
}

async function tdPartagerCourbe() {
  const courbeId = parseInt(document.getElementById('tdCourbeSelect').value);
  const commentaire = document.getElementById('tdShareComment').value.trim();
  const errEl = document.getElementById('tdShareError');
  errEl.classList.add('hidden');
  if (!courbeId) { showErr(errEl, 'Sélectionnez une courbe.'); return; }
  try {
    await TeamApi.partager({ teamId: currentTeamId, courbeId, commentaire });
    document.getElementById('tdShareComment').value = '';
    toggleSection('tdShareForm');
    await loadTdAnalyses();
  } catch (e) { showErr(errEl, e.message); }
}

async function tdRetirerPartage(courbeId) {
  if (!confirm('Retirer cette courbe du partage ?')) return;
  try {
    await TeamApi.retirerPartage(currentTeamId, courbeId);
    await loadTdAnalyses();
  } catch (e) { alert(e.message); }
}

// ═══ Chat ═════════════════════════════════════════════════════════════════════
let activeChatId = null;
let activeChatType = 'private';

async function loadChat() {
  await switchChatTab('private', document.querySelector('.chat-tab'));
}

async function switchChatTab(type, btn) {
  activeChatType = type;
  document.querySelectorAll('.chat-tab').forEach(t => t.classList.remove('active'));
  btn?.classList.add('active');

  const container = document.getElementById('chatContactsList');
  container.innerHTML = '';

  if (type === 'private') {
    try {
      const convs = await ChatApi.getConversations();
      if (!convs.length) {
        container.innerHTML = '<p class="empty-state" style="padding:1.5rem">Aucune conversation</p>';
        return;
      }
      container.innerHTML = convs.map(c => `
        <div class="chat-contact" onclick="openPrivateChat(${c.utilisateurId},'${escapeStr(c.prenom+' '+c.nom)}')">
          <div class="chat-contact-avatar">${(c.prenom[0]+c.nom[0]).toUpperCase()}</div>
          <div class="chat-contact-info">
            <div class="chat-contact-name">${c.prenom} ${c.nom}</div>
            <div class="chat-contact-last">${c.dernierMessage || 'Démarrer une conversation'}</div>
          </div>
          ${c.messagesNonLus > 0 ? `<span class="chat-unread">${c.messagesNonLus}</span>` : ''}
        </div>`).join('');
    } catch (e) { console.error(e); }
  } else {
    try {
      const teams = await TeamApi.getMesTeams();
      if (!teams.length) {
        container.innerHTML = '<p class="empty-state" style="padding:1.5rem">Aucune équipe</p>';
        return;
      }
      container.innerHTML = teams.map(t => `
        <div class="chat-contact" onclick="openTeamChat(${t.id},'${escapeStr(t.nom)}')">
          <div class="chat-contact-avatar" style="background:linear-gradient(135deg,#10b981,#059669)">👥</div>
          <div class="chat-contact-info">
            <div class="chat-contact-name">${t.nom}</div>
            <div class="chat-contact-last">${t.nombreMembres} membres</div>
          </div>
        </div>`).join('');
    } catch (e) { console.error(e); }
  }
}

async function openPrivateChat(userId, name) {
  activeChatId = userId;
  activeChatType = 'private';
  document.querySelectorAll('.chat-contact').forEach(c => c.classList.remove('active'));
  document.getElementById('chatWelcome').classList.add('hidden');
  document.getElementById('chatWindow').classList.remove('hidden');
  document.getElementById('chatHeaderBar').innerHTML = `
    <div class="chat-contact-avatar">${name.substring(0,2).toUpperCase()}</div>
    <span>${name}</span>`;
  try {
    const msgs = await ChatApi.getConversationPrivee(userId);
    renderMessages(msgs);
    await ChatApi.marquerLu(userId, null);
  } catch (e) { console.error(e); }
}

async function openTeamChat(teamId, name) {
  activeChatId = teamId;
  activeChatType = 'team';
  document.querySelectorAll('.chat-contact').forEach(c => c.classList.remove('active'));
  document.getElementById('chatWelcome').classList.add('hidden');
  document.getElementById('chatWindow').classList.remove('hidden');
  document.getElementById('chatHeaderBar').innerHTML = `
    <div class="chat-contact-avatar" style="background:linear-gradient(135deg,#10b981,#059669)">👥</div>
    <span>${name}</span>`;
  try {
    const msgs = await ChatApi.getMessagesTeam(teamId);
    renderMessages(msgs);
  } catch (e) { console.error(e); }
}

function renderMessages(msgs) {
  const me = getUser();
  const container = document.getElementById('chatMessages');
  if (!msgs.length) { container.innerHTML = '<p style="text-align:center;color:var(--text3);padding:2rem;font-size:.875rem">Aucun message. Soyez le premier !</p>'; return; }
  container.innerHTML = msgs.map(m => {
    const isMine = m.expediteurId === me?.id;
    return `<div class="msg-wrap ${isMine ? 'mine' : ''}">
      ${!isMine ? `<span style="font-size:.72rem;color:var(--text3);margin-bottom:2px">${m.nomExpediteur}</span>` : ''}
      <div class="msg-bubble ${isMine ? 'mine' : 'theirs'}">${escapeHtml(m.contenu)}</div>
      <span class="msg-meta">${formatTime(m.dateEnvoi)}</span>
    </div>`;
  }).join('');
  container.scrollTop = container.scrollHeight;
}

async function sendMessage() {
  if (!activeChatId) return;
  const input = document.getElementById('chatInput');
  const text = input.value.trim();
  if (!text) return;
  input.value = '';
  try {
    const dto = activeChatType === 'team'
      ? { contenu: text, teamId: activeChatId }
      : { contenu: text, destinataireId: activeChatId };
    await ChatApi.envoyer(dto);
    if (activeChatType === 'team') {
      const msgs = await ChatApi.getMessagesTeam(activeChatId);
      renderMessages(msgs);
    } else {
      const msgs = await ChatApi.getConversationPrivee(activeChatId);
      renderMessages(msgs);
    }
  } catch (e) { console.error(e); }
}

// ═══ Notifications ════════════════════════════════════════════════════════════
function toggleNotifPanel() {
  document.getElementById('notifPanel').classList.toggle('hidden');
  loadNotifications();
}

async function loadNotifications() {
  try {
    const notifs = await NotifApi.getAll();
    const unread = notifs.filter(n => !n.estLue).length;
    const badge = document.getElementById('notifBadge');
    if (unread > 0) { badge.textContent = unread; badge.classList.remove('hidden'); }
    else { badge.classList.add('hidden'); }

    const list = document.getElementById('notifList');
    if (!notifs.length) {
      list.innerHTML = '<p class="empty-state" style="padding:1.25rem">Aucune notification</p>';
      return;
    }
    list.innerHTML = notifs.slice(0, 10).map(n => `
      <div class="notif-item ${n.estLue ? '' : 'unread'}">
        ${!n.estLue ? '<div class="notif-dot"></div>' : '<div style="width:8px"></div>'}
        <div class="notif-item-content">
          <div class="notif-item-msg">${n.message}</div>
          <div class="notif-item-time">${formatDate(n.dateCreation)}</div>
        </div>
      </div>`).join('');
  } catch (e) { console.error(e); }
}

async function marquerToutesLues() {
  try {
    await NotifApi.marquerToutesLues();
    loadNotifications();
  } catch (e) {}
}

// ═══ Profile ══════════════════════════════════════════════════════════════════
async function loadProfile() {
  try {
    const u = await AuthApi.getProfil();
    setUser(u);
    const initials = (u.prenom[0] + u.nom[0]).toUpperCase();
    document.getElementById('profileAvatarBig').textContent = initials;
    document.getElementById('navAvatar').textContent = initials;
    document.getElementById('profileName').textContent = `${u.prenom} ${u.nom}`;
    document.getElementById('profileEmail').textContent = u.email;
    document.getElementById('profileBadge').textContent = `🏅 ${u.profilInvestisseur}`;
    document.getElementById('ps-date').textContent = new Date(u.dateInscription).toLocaleDateString('fr-FR', {day:'2-digit',month:'long',year:'numeric'});
    document.getElementById('editPrenom').value = u.prenom;
    document.getElementById('editNom').value = u.nom;
    document.getElementById('editProfil').value = u.profilInvestisseur;
  } catch (e) { console.error(e); }
}

async function saveProfile() {
  const nom = document.getElementById('editNom').value.trim();
  const prenom = document.getElementById('editPrenom').value.trim();
  const profilInvestisseur = document.getElementById('editProfil').value;
  const sucEl = document.getElementById('profileSuccess');
  const errEl = document.getElementById('profileError');
  sucEl.classList.add('hidden');
  errEl.classList.add('hidden');
  try {
    await AuthApi.updateProfil({ nom, prenom, profilInvestisseur });
    const u = getUser();
    setUser({ ...u, nom, prenom, profilInvestisseur });
    sucEl.textContent = '✅ Profil mis à jour !';
    sucEl.classList.remove('hidden');
    loadProfile();
    setTimeout(() => sucEl.classList.add('hidden'), 3000);
  } catch (e) { showErr(errEl, e.message); }
}

// ═══ Drag & Drop ══════════════════════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', () => {
  initSession();
  const dz = document.getElementById('dropzone');
  if (!dz) return;
  dz.addEventListener('dragover', e => { e.preventDefault(); dz.classList.add('drag-over'); });
  dz.addEventListener('dragleave', () => dz.classList.remove('drag-over'));
  dz.addEventListener('drop', e => {
    e.preventDefault();
    dz.classList.remove('drag-over');
    const file = e.dataTransfer.files[0];
    if (file) handleFile({ files: [file] });
  });
});

// ═══ Helpers ══════════════════════════════════════════════════════════════════
function showErr(el, msg) { el.textContent = `⚠️ ${msg}`; el.classList.remove('hidden'); }
function setLoading(btnId, spinnerId, loading) {
  const btn = document.getElementById(btnId);
  const sp = document.getElementById(spinnerId);
  if (btn) btn.disabled = loading;
  sp?.classList.toggle('hidden', !loading);
}
function animateCount(id, target) {
  const el = document.getElementById(id);
  if (!el || target === 0) { el.textContent = target; return; }
  let current = 0;
  const step = Math.max(1, Math.floor(target / 20));
  const timer = setInterval(() => {
    current = Math.min(current + step, target);
    el.textContent = current;
    if (current >= target) clearInterval(timer);
  }, 50);
}
function formatDate(d) {
  if (!d) return '';
  return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
function formatTime(d) {
  if (!d) return '';
  return new Date(d).toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
}
function tendanceBadge(t) { return t === 'Hausse' ? 'badge-up' : t === 'Baisse' ? 'badge-down' : 'badge-stable'; }
function tendanceIcon(t) { return t === 'Hausse' ? '📈' : t === 'Baisse' ? '📉' : '➡️'; }
function statutBadge(s) { return s === 'Analysé' ? 'badge-up' : s === 'ErreurAnalyse' ? 'badge-down' : 'badge-stable'; }
function escapeHtml(str) { return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
function escapeStr(str) { return str.replace(/'/g,"\\'"); }
