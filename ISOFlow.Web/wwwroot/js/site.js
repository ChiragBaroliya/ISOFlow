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

    // Global Search Redirect Engine
    const globalSearchInput = document.getElementById('globalSearchInput');
    if (globalSearchInput) {
        globalSearchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const query = this.value.trim().toUpperCase();
                if (query.includes('CTRL-') || query.includes('001')) {
                    window.location.href = '/Controls/Detail?id=CTRL-001';
                } else if (query.includes('RISK-')) {
                    window.location.href = '/Risks/Detail?id=RISK-001';
                } else if (query.includes('AUD-') || query.includes('FIND-')) {
                    window.location.href = '/Findings/Detail?id=FIND-001';
                } else if (query.includes('CAPA-')) {
                    window.location.href = '/Capa/Detail?id=CAPA-001';
                } else if (query.includes('27001') || query.includes('A.5')) {
                    window.location.href = '/Standards/Detail?id=ISO-27001-2022';
                } else {
                    alert('Global Search indexed entities: CTRL-001, RISK-001, FIND-001, CAPA-001, ISO 27001, A.5.18, POL-001');
                }
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
