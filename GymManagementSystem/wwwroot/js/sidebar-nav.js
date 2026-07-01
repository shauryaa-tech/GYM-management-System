(function () {
    const LAYOUT_SCRIPT_MARKERS = [
        'toggleSidebar',
        'loadNavbarStats',
        'renderNotifications',
        'getNotificationColor',
        'loadNavbarMessages',
        'runGlobalSearch'
    ];

    const MENU_PATHS = {
        masters: [
            '/membershipplans', '/membermasters', '/staffmasters', '/exercise',
            '/dietmaster', '/classmaster', '/equipmentmaster', '/products',
            '/vendors', '/expenseheads', '/paymentgateway', '/leadsources', '/usersroles'
        ],
        entries: [
            '/membershipmanagement', '/attendance', '/payments', '/leads',
            '/trainerassignments', '/workoutplans', '/dietplans', '/ptsessions',
            '/classbooking', '/stockpurchase', '/stockissue', '/equipmentmaintenance',
            '/expenses'
        ],
        payroll: [
            '/payroll', '/salaryprocessing', '/staffattendance',
            '/salaryrulemaster'
        ],
        reports: [
            '/members/index', '/reports/attendance', '/reports/membershipexpiry',
            '/reports/collections', '/reports/outstanding', '/reports/profitloss'
        ]
    };

    let activePageScripts = [];
    let isNavigating = false;

    function normalizePath(url) {
        try {
            const path = new URL(url, window.location.origin).pathname.toLowerCase();
            return path.endsWith('/') && path.length > 1 ? path.slice(0, -1) : path;
        } catch {
            return url.toLowerCase();
        }
    }

    function getMenuForPath(path) {
        for (const [menuId, paths] of Object.entries(MENU_PATHS)) {
            if (paths.some(p => path.startsWith(p))) {
                return menuId;
            }
        }
        return null;
    }

    function saveSidebarState() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        const openMenus = [];
        document.querySelectorAll('.submenu.show').forEach(menu => {
            if (menu.id) openMenus.push(menu.id);
        });

        sessionStorage.setItem('sidebarOpenMenus', JSON.stringify(openMenus));
        sessionStorage.setItem('sidebarScrollTop', String(sidebar.scrollTop));
        sessionStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed') ? '1' : '0');
    }

    function restoreSidebarState() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        let openMenus = [];
        try {
            openMenus = JSON.parse(sessionStorage.getItem('sidebarOpenMenus') || '[]');
        } catch {
            openMenus = [];
        }

        const currentMenu = getMenuForPath(normalizePath(window.location.pathname));
        if (currentMenu && !openMenus.includes(currentMenu)) {
            openMenus.push(currentMenu);
        }

        const path = normalizePath(window.location.pathname);
        if (path.startsWith('/attendance') && !openMenus.includes('entries')) {
            openMenus.push('entries');
        }
        if ((path.startsWith('/payroll') || path.startsWith('/salaryprocessing')
            || path.startsWith('/staffattendance') || path.startsWith('/salaryrulemaster'))
            && !openMenus.includes('payroll')) {
            openMenus.push('payroll');
        }

        openMenus.forEach(id => {
            const menu = document.getElementById(id);
            if (!menu) return;
            menu.classList.add('show');
            const toggle = menu.previousElementSibling;
            const arrow = toggle?.querySelector('.arrow');
            if (arrow) arrow.classList.add('rotate');
        });

        if (sessionStorage.getItem('sidebarCollapsed') === '1') {
            sidebar.classList.add('collapsed');
            document.getElementById('content')?.classList.add('expanded');
        }

        const scrollTop = parseInt(sessionStorage.getItem('sidebarScrollTop') || '0', 10);
        if (!Number.isNaN(scrollTop)) {
            sidebar.scrollTop = scrollTop;
        }
    }

    function highlightCurrentPage(url) {
        const current = normalizePath(url || window.location.pathname);

        document.querySelectorAll('.sidebar a').forEach(link => {
            link.classList.remove('current-page');
            const href = link.getAttribute('href');
            if (!href || href.startsWith('javascript')) return;

            if (normalizePath(href) === current) {
                link.classList.add('current-page');

                let parentSubMenu = link.closest('.submenu');
                while (parentSubMenu) {
                    parentSubMenu.classList.add('show');
                    const parentToggle = parentSubMenu.previousElementSibling;
                    parentToggle?.querySelector('.arrow')?.classList.add('rotate');
                    parentSubMenu = parentSubMenu.parentElement?.closest('.submenu') ?? null;
                }
            }
        });
    }

    function isLayoutScript(script) {
        if (script.src) {
            const src = script.src.toLowerCase();
            return src.includes('jquery') ||
                src.includes('bootstrap') ||
                src.includes('sweetalert') ||
                src.includes('toastr') ||
                src.includes('sidebar-nav.js') ||
                src.includes('navbar.js');
        }

        const text = script.textContent || '';
        return LAYOUT_SCRIPT_MARKERS.some(marker => text.includes(marker));
    }

    function removeActivePageScripts() {
        activePageScripts.forEach(script => script.remove());
        activePageScripts = [];
    }

    function runPageScripts(doc) {
        removeActivePageScripts();

        doc.body.querySelectorAll('script').forEach(script => {
            if (isLayoutScript(script)) return;

            const newScript = document.createElement('script');
            Array.from(script.attributes).forEach(attr => {
                newScript.setAttribute(attr.name, attr.value);
            });
            newScript.textContent = script.textContent;
            newScript.dataset.pageScript = 'true';
            document.body.appendChild(newScript);
            activePageScripts.push(newScript);
        });
    }

    function shouldUseFullNavigation(link, event) {
        if (!link) return true;
        if (event && (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey)) return true;

        const href = link.getAttribute('href');
        if (!href || href.startsWith('javascript') || href === '#') return true;
        if (link.target === '_blank') return true;
        if (link.hasAttribute('download')) return true;
        if (link.dataset.fullNav === 'true') return true;

        return false;
    }

    async function loadPage(url, pushState = true) {
        if (isNavigating) return;
        isNavigating = true;

        const content = document.getElementById('content');
        if (!content) {
            window.location.href = url;
            return;
        }

        content.classList.add('is-loading');
        saveSidebarState();

        try {
            const response = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                window.location.href = url;
                return;
            }

            const html = await response.text();
            const doc = new DOMParser().parseFromString(html, 'text/html');
            const newContent = doc.getElementById('content');

            if (!newContent) {
                window.location.href = url;
                return;
            }

            content.innerHTML = newContent.innerHTML;
            document.title = doc.title;
            runPageScripts(doc);
            highlightCurrentPage(url);

            if (pushState) {
                history.pushState({ ajaxUrl: url }, '', url);
            }

            restoreSidebarState();
            window.scrollTo(0, 0);

            if (typeof window.loadNavbarStats === 'function') {
                window.loadNavbarStats();
            }
            if (typeof window.loadNavbarMessages === 'function') {
                window.loadNavbarMessages();
            }
        } catch {
            window.location.href = url;
        } finally {
            content.classList.remove('is-loading');
            isNavigating = false;
        }
    }

    window.toggleSidebar = function toggleSidebar() {
        document.getElementById('sidebar')?.classList.toggle('collapsed');
        document.getElementById('content')?.classList.toggle('expanded');
        saveSidebarState();
    };

    window.toggleMenu = function toggleMenu(id, element) {
        const sidebar = document.getElementById('sidebar');
        const menu = document.getElementById(id);
        if (!menu) return;

        if (sidebar?.classList.contains('collapsed')) {
            sidebar.classList.remove('collapsed');
            document.getElementById('content')?.classList.remove('expanded');
            if (!menu.classList.contains('show')) {
                menu.classList.add('show');
                element.querySelector('.arrow')?.classList.add('rotate');
            }
            saveSidebarState();
            return;
        }

        menu.classList.toggle('show');
        element.querySelector('.arrow')?.classList.toggle('rotate');
        saveSidebarState();
    };

    function applySidebarTooltips() {
        document.querySelectorAll('#sidebar a').forEach(link => {
            const label = link.querySelector('.text')?.textContent?.trim();
            if (label) {
                link.setAttribute('title', label);
            }
        });
    }

    window.loadPage = loadPage;

    function bindSidebarNavigation() {
        document.getElementById('sidebar')?.addEventListener('click', function (event) {
            const link = event.target.closest('a[href]');
            if (!link || !link.closest('#sidebar')) return;
            if (shouldUseFullNavigation(link, event)) return;

            event.preventDefault();
            loadPage(link.href);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        applySidebarTooltips();
        restoreSidebarState();
        highlightCurrentPage(window.location.pathname);
        bindSidebarNavigation();

        history.replaceState({ ajaxUrl: window.location.href }, '', window.location.href);

        window.addEventListener('popstate', function (event) {
            if (event.state?.ajaxUrl) {
                loadPage(event.state.ajaxUrl, false);
            } else {
                window.location.reload();
            }
        });
    });

    window.addEventListener('beforeunload', saveSidebarState);
})();
