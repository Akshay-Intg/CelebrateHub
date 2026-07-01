// BirthdayPortal.MVC/wwwroot/js/portal.js
// Place in: BirthdayPortal.MVC → wwwroot → js folder


(function () {
    'use strict';

    // ── Sidebar toggle ───────────────────────────────────────────────────────
    const sidebar = document.getElementById('sidebar');
    const toggleBtn = document.getElementById('sidebarToggle');

    // Create a backdrop for mobile sidebar overlay
    let backdrop = document.getElementById('sidebarBackdrop');
    if (!backdrop) {
        backdrop = document.createElement('div');
        backdrop.id = 'sidebarBackdrop';
        backdrop.style.cssText = 'position:fixed;inset:0;background:rgba(15,18,34,0.45);z-index:1040;opacity:0;pointer-events:none;transition:opacity .25s ease;';
        document.body.appendChild(backdrop);
    }

    function openMobileSidebar() {
        sidebar.classList.add('mobile-open');
        backdrop.style.opacity = '1';
        backdrop.style.pointerEvents = 'auto';
    }
    function closeMobileSidebar() {
        sidebar.classList.remove('mobile-open');
        backdrop.style.opacity = '0';
        backdrop.style.pointerEvents = 'none';
    }

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                sidebar.classList.contains('mobile-open') ? closeMobileSidebar() : openMobileSidebar();
            } else {
                sidebar.classList.toggle('collapsed');
            }
        });

        backdrop.addEventListener('click', closeMobileSidebar);

        // Close sidebar after navigating (mobile)
        sidebar.querySelectorAll('.sidebar-link').forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth <= 768) closeMobileSidebar();
            });
        });
    }

    // ── Auto-dismiss alerts after 5s ─────────────────────────────────────────
    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (el) {
            if (bootstrap && bootstrap.Alert) {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
                bsAlert.close();
            }
        });
    }, 5000);

    // ── Employee table live search (client-side filter) ───────────────────────
    const searchInput = document.querySelector('input[name="search"]');
    const tableBody = document.querySelector('#employeesTable tbody');

    if (searchInput && tableBody) {
        // The form submits to server-side; this is just a UX enhancement
        // to give instant feedback while the user types (debounced)
        let debounceTimer;
        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(function () {
                const term = searchInput.value.toLowerCase().trim();
                if (!term) {
                    // Show all rows
                    tableBody.querySelectorAll('tr').forEach(r => r.style.display = '');
                    return;
                }
                tableBody.querySelectorAll('tr').forEach(function (row) {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(term) ? '' : 'none';
                });
            }, 200);
        });
    }

})();