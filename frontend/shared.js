// shared.js — navigation, role helpers, modal utilities

// ── ROLE MANAGEMENT ──
const ROLES = { STUDENT: 'student', TEACHER: 'teacher', ASSISTANT: 'assistant' };

function getCurrentUser() {
  const stored = sessionStorage.getItem('cms_user');
  if (stored) return JSON.parse(stored);
  // Default demo user
  return { id: 1, fname: 'Alex', lname: 'Johnson', email: 'alex@example.com', role: 'student' };
}

function setCurrentUser(user) {
  sessionStorage.setItem('cms_user', JSON.stringify(user));
}

function getUserInitials(user) {
  return ((user.fname?.[0] || '') + (user.lname?.[0] || '')).toUpperCase();
}

// ── SIDEBAR NAV ITEMS ──
const NAV = {
  student: [
    { label: 'Main', items: [
      { href: 'my-courses.html', icon: iconHome, text: 'Dashboard' },
      { href: 'all-courses.html', icon: iconGrid, text: 'Browse Courses' },
    ]},
    { label: 'Account', items: [
      { href: 'profile.html', icon: iconUser, text: 'My Profile' },
    ]},
  ],
  teacher: [
    { label: 'Main', items: [
      { href: 'my-courses.html', icon: iconHome, text: 'Dashboard' },
      { href: 'course-management.html', icon: iconBook, text: 'My Courses' },
    ]},
    { label: 'Create', items: [
      { href: 'editor-course.html', icon: iconPlus, text: 'New Course' },
      { href: 'editor-class.html', icon: iconPlus, text: 'New Class' },
      { href: 'editor-exam.html', icon: iconPlus, text: 'New Exam/Quiz' },
    ]},
    { label: 'Account', items: [
      { href: 'profile.html', icon: iconUser, text: 'My Profile' },
    ]},
  ],
  assistant: [
    { label: 'Main', items: [
      { href: 'my-courses.html', icon: iconHome, text: 'Dashboard' },
      { href: 'course-management.html', icon: iconBook, text: 'Assigned Courses' },
    ]},
    { label: 'Manage', items: [
      { href: 'editor-class.html', icon: iconPlus, text: 'New Class' },
      { href: 'editor-exam.html', icon: iconPlus, text: 'New Exam/Quiz' },
    ]},
    { label: 'Account', items: [
      { href: 'profile.html', icon: iconUser, text: 'My Profile' },
    ]},
  ],
};

// ── SVG ICONS ──
function iconHome() { return `<svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>`; }
function iconGrid() { return `<svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>`; }
function iconBook() { return `<svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>`; }
function iconUser() { return `<svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`; }
function iconPlus() { return `<svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>`; }

// ── BUILD TOPNAV ──
function buildTopNav(user) {
  const nav = document.getElementById('topnav');
  if (!nav) return;
  const roleLabel = { student: 'Student', teacher: 'Teacher', assistant: 'Assistant' }[user.role] || '';
  nav.innerHTML = `
    <button class="sidebar-toggle btn-icon" onclick="toggleSidebar()" style="background:none;border:none;cursor:pointer;color:white;display:none" id="sidebarToggle">
      <svg width="22" height="22" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/></svg>
    </button>
    <a href="my-courses.html" class="topnav-brand">Edu<span>Arc</span></a>
    <div class="topnav-spacer"></div>
    <span class="topnav-role">${roleLabel}</span>
    <a href="profile.html" class="topnav-avatar">${getUserInitials(user)}</a>
  `;
}

// ── BUILD SIDEBAR ──
function buildSidebar(user, currentPage) {
  const sidebar = document.getElementById('sidebar');
  if (!sidebar) return;
  const sections = NAV[user.role] || NAV.student;
  let html = '';
  for (const section of sections) {
    html += `<div class="sidebar-section"><div class="sidebar-label">${section.label}</div>`;
    for (const item of section.items) {
      const active = currentPage === item.href ? 'active' : '';
      html += `<a href="${item.href}" class="sidebar-link ${active}">${item.icon()}${item.text}</a>`;
    }
    html += `</div>`;
  }
  html += `<div class="sidebar-section" style="margin-top:auto">
    <div class="sidebar-label">Session</div>
    <a href="login.html" class="sidebar-link" onclick="sessionStorage.clear()">
      <svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
      Logout
    </a>
  </div>`;
  sidebar.innerHTML = html;
}

function toggleSidebar() {
  document.getElementById('sidebar')?.classList.toggle('open');
}

// ── MODAL HELPERS ──
function openModal(id) { document.getElementById(id)?.classList.add('open'); }
function closeModal(id) { document.getElementById(id)?.classList.remove('open'); }

// Close modal on overlay click
document.addEventListener('click', (e) => {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('open');
  }
});

// ── INIT ──
document.addEventListener('DOMContentLoaded', () => {
  const user = getCurrentUser();
  buildTopNav(user);
  const page = window.location.pathname.split('/').pop() || 'index.html';
  buildSidebar(user, page);

  // Show hamburger on mobile
  const toggle = document.getElementById('sidebarToggle');
  if (toggle) {
    const mq = window.matchMedia('(max-width: 768px)');
    toggle.style.display = mq.matches ? 'block' : 'none';
    mq.addEventListener('change', e => { toggle.style.display = e.matches ? 'block' : 'none'; });
  }
});
