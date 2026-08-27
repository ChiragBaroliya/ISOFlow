// ISOFlow Dashboard Visualizations Script

function initComplianceChart() {
    const canvas = document.getElementById('complianceChart');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
            datasets: [{
                label: 'Compliance Score %',
                data: [72.0, 74.5, 76.0, 78.0, 79.5, 81.0, 82.5],
                borderColor: '#2563eb',
                backgroundColor: 'rgba(37, 99, 235, 0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { min: 60, max: 100 } }
        }
    });
}

document.addEventListener('DOMContentLoaded', initComplianceChart);
