// ISOFlow Dashboard Visualizations Script

function initComplianceChart() {
    const canvas = document.getElementById('complianceChart');
    const dataEl = document.getElementById('complianceTrendData');
    if (!canvas || !dataEl) return;

    // Trend data is rendered server-side from the API's monthly compliance snapshots.
    const trend = JSON.parse(dataEl.textContent);

    new Chart(canvas.getContext('2d'), {
        type: 'line',
        data: {
            labels: trend.labels,
            datasets: [{
                label: 'Compliance Score %',
                data: trend.scores,
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
            scales: { y: { min: 0, max: 100 } }
        }
    });
}

document.addEventListener('DOMContentLoaded', initComplianceChart);
