(function () {
    let searchTimer = null;
    let lastSearchResults = [];
    let seenNotificationIds = new Set();
    let seenMessageIds = new Set();
    let notificationsInitialized = false;
    let messagesInitialized = false;
    const TOAST_DURATION = 5500;
    const MAX_TOASTS = 4;
    const STORAGE_KEY = 'cm_seen_notifications';
    const MSG_STORAGE_KEY = 'cm_seen_messages';

    function loadSeenIds(key) {
        try {
            const raw = sessionStorage.getItem(key);
            if (!raw) return new Set();
            return new Set(JSON.parse(raw));
        } catch {
            return new Set();
        }
    }

    function saveSeenIds(key, set) {
        try {
            const arr = Array.from(set).slice(-100);
            sessionStorage.setItem(key, JSON.stringify(arr));
        } catch { /* ignore */ }
    }

    seenNotificationIds = loadSeenIds(STORAGE_KEY);
    seenMessageIds = loadSeenIds(MSG_STORAGE_KEY);

    function getToastVariant(n) {
        const map = {
            'text-success': 'success',
            'text-warning': 'warning',
            'text-danger': 'danger',
            'text-info': 'info',
            'text-primary': 'primary'
        };
        return map[n.color] || 'primary';
    }

    function removeToast(el) {
        if (!el || el.classList.contains('cm-toast-exit')) return;
        el.classList.add('cm-toast-exit');
        setTimeout(() => el.remove(), 350);
    }

    function showToastPopup(item) {
        const container = document.getElementById('cm-toast-container');
        if (!container) return;

        while (container.children.length >= MAX_TOASTS) {
            removeToast(container.firstElementChild);
        }

        const variant = item.variant || getToastVariant(item);
        const toast = document.createElement('div');
        toast.className = `cm-toast cm-toast--${variant}`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="cm-toast__icon"><i class="${item.icon || 'fa-solid fa-bell'}"></i></div>
            <div class="cm-toast__body">
                <div class="cm-toast__title">${escapeHtml(item.title)}</div>
                <p class="cm-toast__msg">${escapeHtml(item.message)}</p>
                ${item.time ? `<div class="cm-toast__time">${escapeHtml(item.time)}</div>` : ''}
            </div>
            <button type="button" class="cm-toast__close" aria-label="Close">&times;</button>
            <div class="cm-toast__progress"></div>
        `;

        const url = item.url;
        if (url) {
            toast.addEventListener('click', function (e) {
                if (e.target.closest('.cm-toast__close')) return;
                removeToast(toast);
                navigateTo(url);
            });
        }

        toast.querySelector('.cm-toast__close').addEventListener('click', function (e) {
            e.stopPropagation();
            removeToast(toast);
        });

        container.appendChild(toast);
        setTimeout(() => removeToast(toast), TOAST_DURATION);
    }

    function ringBell() {
        const bell = document.querySelector('.nav-icon.dropdown[title="Notifications"]');
        if (!bell) return;
        bell.classList.remove('bell-ring');
        void bell.offsetWidth;
        bell.classList.add('bell-ring');
        setTimeout(() => bell.classList.remove('bell-ring'), 800);
    }

    function processNotificationPopups(notifications) {
        if (!notifications?.length) return;

        const fresh = [];
        notifications.forEach(n => {
            const id = n.id || `${n.type}|${n.title}|${n.message}|${n.time}`;
            if (!seenNotificationIds.has(id)) {
                seenNotificationIds.add(id);
                fresh.push(n);
            }
        });

        if (!notificationsInitialized) {
            notificationsInitialized = true;
            saveSeenIds(STORAGE_KEY, seenNotificationIds);
            if (fresh.length > 0) {
                fresh.slice(0, 2).forEach((n, i) => {
                    setTimeout(() => showToastPopup(n), i * 350);
                });
                ringBell();
            }
            return;
        }

        if (fresh.length === 0) return;

        saveSeenIds(STORAGE_KEY, seenNotificationIds);
        ringBell();
        fresh.slice(0, 3).forEach((n, i) => {
            setTimeout(() => showToastPopup(n), i * 400);
        });
    }

    function processMessagePopups(messages) {
        if (!messages?.length) return;

        const fresh = [];
        messages.forEach(m => {
            const id = `followup-${m.id}`;
            if (!seenMessageIds.has(id)) {
                seenMessageIds.add(id);
                fresh.push(m);
            }
        });

        if (fresh.length === 0) return;

        saveSeenIds(MSG_STORAGE_KEY, seenMessageIds);

        if (!messagesInitialized) {
            messagesInitialized = true;
            const m = fresh[0];
            showToastPopup({
                title: m.isOverdue ? 'Overdue Follow-up' : 'Follow-up Due',
                message: `${m.title} — ${m.message || m.status || 'Call today'}`,
                icon: 'fa-solid fa-envelope',
                color: m.isOverdue ? 'text-danger' : 'text-warning',
                variant: m.isOverdue ? 'danger' : 'followup',
                url: m.url || '/Leads/Index',
                time: m.date
            });
            return;
        }

        fresh.slice(0, 2).forEach((m, i) => {
            setTimeout(() => showToastPopup({
                title: m.isOverdue ? 'Overdue Follow-up' : 'Follow-up Due',
                message: `${m.title} — ${m.message || m.status || 'Call today'}`,
                icon: 'fa-solid fa-envelope',
                color: m.isOverdue ? 'text-danger' : 'text-warning',
                variant: m.isOverdue ? 'danger' : 'followup',
                url: m.url || '/Leads/Index',
                time: m.date
            }), i * 500);
        });
    }

    function navigateTo(url) {
        if (!url || url === '#') return;

        if (typeof window.loadPage === 'function' && !url.toLowerCase().includes('/account/logout')) {
            window.loadPage(url);
            return;
        }

        window.location.href = url;
    }

    function shouldFullNavigate(link) {
        if (!link) return false;
        if (link.dataset.fullNav === 'true') return true;
        const href = (link.getAttribute('href') || '').toLowerCase();
        return href.includes('/profile/settings') || href.includes('/settings/index');
    }

    function getNotificationColor(colorClass) {
        const colors = {
            'text-primary': 'text-primary',
            'text-success': 'text-success',
            'text-warning': 'text-warning',
            'text-info': 'text-info',
            'text-danger': 'text-danger'
        };
        return colors[colorClass] || 'text-primary';
    }

    function renderNotifications(notifications) {
        const container = document.getElementById('notificationList');
        if (!container) return;

        if (!notifications || notifications.length === 0) {
            container.innerHTML = '<div class="text-center text-muted py-3">No new notifications</div>';
            return;
        }

        container.innerHTML = notifications.map(n => `
            <a href="${n.url || '/Dashboard/Index'}" class="dropdown-item notification-item d-flex align-items-start gap-3 p-2 rounded navbar-nav-link" style="border-left: 3px solid; border-color: var(--bs-${getNotificationColor(n.color).replace('text-', '')}, #0d6efd);">
                <div class="notification-icon bg-light rounded-circle d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; flex-shrink: 0;">
                    <i class="${n.icon} ${getNotificationColor(n.color)}" style="font-size: 0.9rem;"></i>
                </div>
                <div class="flex-grow-1 min-w-0">
                    <div class="d-flex justify-content-between align-items-start">
                        <h6 class="mb-1 fw-semibold small">${escapeHtml(n.title)}</h6>
                        <small class="text-muted">${escapeHtml(n.time || '')}</small>
                    </div>
                    <p class="mb-0 small text-muted">${escapeHtml(n.message)}</p>
                </div>
            </a>
        `).join('');

        bindNavbarLinks(container);
    }

    function renderMessages(messages) {
        const container = document.getElementById('messageList');
        if (!container) return;

        if (!messages || messages.length === 0) {
            container.innerHTML = '<div class="text-center text-muted py-3">No follow-ups due today</div>';
            return;
        }

        container.innerHTML = messages.map(m => `
            <a href="${m.url || '/Leads/Index'}" class="dropdown-item d-flex align-items-start gap-3 p-2 rounded navbar-nav-link">
                <div class="rounded-circle d-flex align-items-center justify-content-center ${m.isOverdue ? 'bg-danger-subtle' : 'bg-warning-subtle'}" style="width: 36px; height: 36px; flex-shrink: 0;">
                    <i class="fa-solid fa-envelope ${m.isOverdue ? 'text-danger' : 'text-warning'}"></i>
                </div>
                <div class="flex-grow-1 min-w-0">
                    <div class="d-flex justify-content-between align-items-start">
                        <h6 class="mb-1 fw-semibold small">${escapeHtml(m.title)}</h6>
                        <small class="text-muted">${escapeHtml(m.date || '')}</small>
                    </div>
                    <p class="mb-0 small text-muted">${escapeHtml(m.message || m.status || 'Follow-up due')}</p>
                </div>
            </a>
        `).join('');

        bindNavbarLinks(container);
    }

    function escapeHtml(text) {
        if (!text) return '';
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function bindNavbarLinks(container) {
        container?.querySelectorAll('.navbar-nav-link').forEach(link => {
            link.addEventListener('click', function (event) {
                if (shouldFullNavigate(this)) return;
                event.preventDefault();
                const url = this.getAttribute('href');
                navigateTo(url);
            });
        });
    }

    async function loadNavbarStats() {
        try {
            const response = await fetch('/Dashboard/GetNavbarStats');
            if (!response.ok) return;

            const data = await response.json();
            if (data.error) return;

            const totalEl = document.getElementById('navTotalMembers');
            const activeEl = document.getElementById('navActiveMembers');
            const revenueEl = document.getElementById('navTodayRevenue');
            const countEl = document.getElementById('notificationCount');
            const badgeEl = document.getElementById('notificationBadgeCount');

            if (totalEl) totalEl.textContent = data.totalMembers?.toLocaleString() || '0';
            if (activeEl) activeEl.textContent = data.activeMembers?.toLocaleString() || '0';
            if (revenueEl) revenueEl.textContent = '₹' + (data.todayRevenue?.toLocaleString() || '0');
            if (countEl) {
                countEl.textContent = data.pendingNotifications || '0';
                countEl.style.display = data.pendingNotifications > 0 ? 'flex' : 'none';
            }
            if (badgeEl) badgeEl.textContent = data.pendingNotifications || '0';

            renderNotifications(data.notifications);
            processNotificationPopups(data.notifications);
        } catch (e) {
            console.log('Navbar stats load failed:', e);
        }
    }

    async function loadNavbarMessages() {
        try {
            const response = await fetch('/Dashboard/GetNavbarMessages');
            if (!response.ok) return;

            const data = await response.json();
            if (data.error) return;

            const countEl = document.getElementById('messageCount');
            const badgeEl = document.getElementById('messageBadgeCount');
            if (countEl) {
                countEl.textContent = data.count || '0';
                countEl.style.display = data.count > 0 ? 'flex' : 'none';
            }
            if (badgeEl) {
                badgeEl.textContent = data.count > 0 ? String(data.count) : 'Due';
            }

            renderMessages(data.messages);
            processMessagePopups(data.messages);
        } catch (e) {
            console.log('Navbar messages load failed:', e);
        }
    }

    function hideSearchResults() {
        const panel = document.getElementById('searchResults');
        if (panel) panel.style.display = 'none';
    }

    function showSearchResults() {
        const panel = document.getElementById('searchResults');
        if (panel) panel.style.display = 'block';
    }

    function renderSearchResults(results) {
        const panel = document.getElementById('searchResults');
        if (!panel) return;

        lastSearchResults = results || [];

        if (!lastSearchResults.length) {
            panel.innerHTML = '<div class="search-result-empty">No results found</div>';
            showSearchResults();
            return;
        }

        panel.innerHTML = lastSearchResults.map(r => `
            <button type="button" class="search-result-item" data-url="${r.url}">
                <span class="search-result-icon"><i class="${r.icon}"></i></span>
                <span class="search-result-body">
                    <span class="search-result-title">${escapeHtml(r.title)}</span>
                    <span class="search-result-sub">${escapeHtml(r.subtitle)} · ${escapeHtml(r.type)}</span>
                </span>
            </button>
        `).join('');

        panel.querySelectorAll('.search-result-item').forEach(item => {
            item.addEventListener('click', function () {
                const url = this.getAttribute('data-url');
                hideSearchResults();
                document.getElementById('globalSearch').value = '';
                navigateTo(url);
            });
        });

        showSearchResults();
    }

    async function runGlobalSearch(query) {
        if (query.length < 2) {
            hideSearchResults();
            lastSearchResults = [];
            return;
        }

        try {
            const response = await fetch('/Dashboard/GlobalSearch?q=' + encodeURIComponent(query));
            if (!response.ok) return;

            const data = await response.json();
            renderSearchResults(data.results);
        } catch (e) {
            console.log('Global search failed:', e);
        }
    }

    function initGlobalSearch() {
        const input = document.getElementById('globalSearch');
        if (!input) return;

        input.addEventListener('input', function (e) {
            const query = e.target.value.trim();
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => runGlobalSearch(query), 300);
        });

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && lastSearchResults.length > 0) {
                e.preventDefault();
                hideSearchResults();
                navigateTo(lastSearchResults[0].url);
                input.value = '';
            } else if (e.key === 'Escape') {
                hideSearchResults();
            }
        });

        document.addEventListener('click', function (e) {
            if (!e.target.closest('.search-container')) {
                hideSearchResults();
            }
        });
    }

    function initSettingsLinks() {
        document.querySelectorAll('.settings-nav-link').forEach(link => {
            link.addEventListener('click', function (event) {
                if (this.getAttribute('href')?.includes('/Account/Logout')) return;
                event.preventDefault();
                navigateTo(this.getAttribute('href'));
            });
        });
    }

    window.loadNavbarStats = loadNavbarStats;
    window.loadNavbarMessages = loadNavbarMessages;
    window.renderNotifications = renderNotifications;
    window.getNotificationColor = getNotificationColor;

    document.addEventListener('DOMContentLoaded', function () {
        loadNavbarStats();
        loadNavbarMessages();
        initGlobalSearch();
        initSettingsLinks();
        bindNavbarLinks(document);

        setInterval(loadNavbarStats, 30000);
        setInterval(loadNavbarMessages, 60000);
    });
})();
