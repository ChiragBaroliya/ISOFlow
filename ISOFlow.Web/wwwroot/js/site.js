// ISOFlow Global Client Scripts

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar Mobile Toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }

    // Global Search: jump straight to an entity by its code prefix (e.g. CTRL-007, RISK-012, AUD-2026-002).
    // Unknown codes land on the detail page's "not found" response.
    const globalSearchRoutes = [
        { prefix: 'CTRL-', path: '/Controls/Detail' },
        { prefix: 'RISK-', path: '/Risks/Detail' },
        { prefix: 'FIND-', path: '/Findings/Detail' },
        { prefix: 'CAPA-', path: '/Capa/Detail' },
        { prefix: 'AUD-', path: '/Audits/Detail' },
        { prefix: 'ISO-', path: '/Standards/Detail' }
    ];
    const globalSearchInput = document.getElementById('globalSearchInput');
    if (globalSearchInput) {
        globalSearchInput.addEventListener('keypress', function (e) {
            if (e.key !== 'Enter') return;
            const query = this.value.trim().toUpperCase();
            if (!query) return;

            const route = globalSearchRoutes.find(r => query.startsWith(r.prefix));
            if (route) {
                window.location.href = route.path + '?id=' + encodeURIComponent(query);
            } else if (typeof showToast === 'function') {
                showToast('Search', 'Enter an entity code starting with ' + globalSearchRoutes.map(r => r.prefix).join(', '));
            }
        });
    }

    // Role Switcher Simulation Toast
    const roleSwitcher = document.getElementById('roleSwitcher');
    if (roleSwitcher) {
        roleSwitcher.addEventListener('change', function () {
            showToast('Role Switched', 'Active context permissions updated for: ' + this.value);
        });
    }
});

function showToast(title, message) {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) return;

    const toastHtml = `
        <div class="toast align-items-center text-white bg-primary border-0 show mb-2" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong>: ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
}
