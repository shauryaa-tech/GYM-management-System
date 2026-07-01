(function () {
    'use strict';

    var chartInstances = [];

    function destroyCharts() {
        chartInstances.forEach(function (c) {
            try { c.destroy(); } catch (e) { /* ignore */ }
        });
        chartInstances = [];
    }

    function track(chart) {
        if (chart) chartInstances.push(chart);
        return chart;
    }

    function resizeCharts() {
        chartInstances.forEach(function (c) {
            try { c.resize(); } catch (e) { /* ignore */ }
        });
    }

    function sum(arr) {
        return (arr || []).reduce(function (a, b) { return a + Number(b || 0); }, 0);
    }

    function showEmptyOverlay(wrapEl, message) {
        if (!wrapEl || wrapEl.querySelector('.dash-chart-empty')) return;
        var el = document.createElement('div');
        el.className = 'dash-chart-empty';
        el.textContent = message;
        wrapEl.appendChild(el);
    }

    function initDashboardCharts(config) {
        if (typeof Chart === 'undefined') return;

        destroyCharts();

        Chart.defaults.font.family = "'Segoe UI', system-ui, sans-serif";
        Chart.defaults.color = '#64748b';

        const gradient = (ctx, c1, c2, h) => {
            const g = ctx.createLinearGradient(0, 0, 0, h || 200);
            g.addColorStop(0, c1);
            g.addColorStop(1, c2);
            return g;
        };

        const months = config.months && config.months.length ? config.months : ['—'];
        const revenues = config.revenues && config.revenues.length ? config.revenues : [0];
        const attendanceDates = config.attendanceDates && config.attendanceDates.length
            ? config.attendanceDates
            : ['—'];
        const memberAttendance = config.memberAttendance && config.memberAttendance.length
            ? config.memberAttendance
            : [0];
        const trainerAttendance = config.trainerAttendance && config.trainerAttendance.length
            ? config.trainerAttendance
            : [0];

        const revEl = document.getElementById('dashRevenueChart');
        if (revEl) {
            const wrap = revEl.closest('.dash-chart-wrap');
            const ctx = revEl.getContext('2d');
            if (sum(revenues) === 0) {
                showEmptyOverlay(wrap, 'No revenue data for this period');
            }
            track(new Chart(ctx, {
                type: 'line',
                data: {
                    labels: months,
                    datasets: [{
                        label: 'Revenue (₹)',
                        data: revenues,
                        borderColor: '#6366f1',
                        backgroundColor: gradient(ctx, 'rgba(99, 102, 241, 0.35)', 'rgba(99, 102, 241, 0.02)'),
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 5,
                        pointHoverRadius: 8,
                        pointBackgroundColor: '#fff',
                        pointBorderColor: '#6366f1',
                        pointBorderWidth: 2
                    }]
                },
                options: chartOptions({
                    yPrefix: '₹',
                    tooltipCallback: (v) => '₹' + Number(v).toLocaleString('en-IN')
                })
            }));
        }

        const attEl = document.getElementById('dashAttendanceChart');
        if (attEl) {
            track(new Chart(attEl.getContext('2d'), {
                type: 'bar',
                data: {
                    labels: attendanceDates,
                    datasets: [
                        {
                            label: 'Members',
                            data: memberAttendance,
                            backgroundColor: 'rgba(59, 130, 246, 0.75)',
                            borderRadius: 8,
                            borderSkipped: false
                        },
                        {
                            label: 'Staff',
                            data: trainerAttendance,
                            backgroundColor: 'rgba(245, 158, 11, 0.75)',
                            borderRadius: 8,
                            borderSkipped: false
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    interaction: { mode: 'index', intersect: false },
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: { usePointStyle: true, padding: 12, font: { weight: '600', size: 11 } }
                        },
                        tooltip: {
                            backgroundColor: '#0f172a',
                            padding: 12,
                            cornerRadius: 10
                        }
                    },
                    scales: {
                        x: { grid: { display: false }, ticks: { font: { size: 11 } } },
                        y: { beginAtZero: true, ticks: { stepSize: 1, font: { size: 11 } }, grid: { color: '#f1f5f9' } }
                    },
                    animation: { duration: 600 }
                }
            }));
        }

        const memberEl = document.getElementById('dashMemberChart');
        if (memberEl) {
            const inactive = Math.max(0, config.totalMembers - config.activeMembers);
            track(new Chart(memberEl.getContext('2d'), {
                type: 'doughnut',
                data: {
                    labels: ['Active', 'Inactive'],
                    datasets: [{
                        data: config.totalMembers > 0 ? [config.activeMembers, inactive] : [1, 0],
                        backgroundColor: config.totalMembers > 0 ? ['#10b981', '#e2e8f0'] : ['#e2e8f0', '#e2e8f0'],
                        borderWidth: 0,
                        hoverOffset: 8
                    }]
                },
                options: doughnutOptions(true)
            }));
        }

        const payEl = document.getElementById('dashPaymentChart');
        if (payEl && config.paymentModes.length > 0) {
            const colors = ['#6366f1', '#10b981', '#f59e0b', '#f43f5e', '#06b6d4', '#8b5cf6'];
            track(new Chart(payEl.getContext('2d'), {
                type: 'doughnut',
                data: {
                    labels: config.paymentModes,
                    datasets: [{
                        data: config.paymentModeAmounts,
                        backgroundColor: config.paymentModes.map((_, i) => colors[i % colors.length]),
                        borderWidth: 0,
                        hoverOffset: 8
                    }]
                },
                options: doughnutOptions(false)
            }));
        }

        const trendEl = document.getElementById('dashTrendChart');
        if (trendEl) {
            const ctx = trendEl.getContext('2d');
            track(new Chart(ctx, {
                type: 'line',
                data: {
                    labels: attendanceDates,
                    datasets: [{
                        label: 'Member Check-ins',
                        data: memberAttendance,
                        borderColor: '#3b82f6',
                        backgroundColor: gradient(ctx, 'rgba(59, 130, 246, 0.2)', 'rgba(59, 130, 246, 0)'),
                        borderWidth: 2.5,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 4,
                        pointBackgroundColor: '#3b82f6'
                    }]
                },
                options: chartOptions({})
            }));
        }

        requestAnimationFrame(resizeCharts);
        setTimeout(resizeCharts, 150);
    }

    function chartOptions(opts) {
        return {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#0f172a',
                    padding: 12,
                    cornerRadius: 10,
                    callbacks: opts.tooltipCallback ? {
                        label: (ctx) => opts.tooltipCallback(ctx.parsed.y)
                    } : undefined
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { font: { size: 11 } } },
                y: {
                    beginAtZero: true,
                    grid: { color: '#f1f5f9' },
                    ticks: {
                        font: { size: 11 },
                        callback: opts.yPrefix ? (v) => opts.yPrefix + Number(v).toLocaleString('en-IN') : undefined
                    }
                }
            },
            animation: { duration: 600 }
        };
    }

    function doughnutOptions(isMemberChart) {
        return {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '68%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { usePointStyle: true, padding: 8, font: { size: 11, weight: '600' } }
                },
                tooltip: {
                    backgroundColor: '#0f172a',
                    padding: 10,
                    cornerRadius: 8,
                    callbacks: {
                        label: (ctx) => {
                            const val = ctx.parsed;
                            return isMemberChart
                                ? ctx.label + ': ' + Number(val).toLocaleString('en-IN')
                                : ctx.label + ': ₹' + Number(val).toLocaleString('en-IN');
                        }
                    }
                }
            },
            animation: { animateRotate: true, duration: 600 }
        };
    }

    window.initDashboardCharts = initDashboardCharts;

    function initDashboardDatePicker() {
        const form = document.getElementById('dashDateForm');
        const input = document.getElementById('dashDateInput');
        const prev = document.getElementById('dashDatePrev');
        const next = document.getElementById('dashDateNext');
        if (!form || !input) return;

        const todayStr = input.max;

        input.addEventListener('change', function () {
            if (input.value) form.submit();
        });

        prev?.addEventListener('click', function () {
            if (!input.value) return;
            const d = new Date(input.value + 'T12:00:00');
            d.setDate(d.getDate() - 1);
            input.value = d.toISOString().slice(0, 10);
            form.submit();
        });

        next?.addEventListener('click', function () {
            if (!input.value || next.disabled) return;
            const d = new Date(input.value + 'T12:00:00');
            d.setDate(d.getDate() + 1);
            const nextVal = d.toISOString().slice(0, 10);
            if (nextVal > todayStr) return;
            input.value = nextVal;
            form.submit();
        });
    }

    window.initDashboardDatePicker = initDashboardDatePicker;

    window.addEventListener('load', resizeCharts);
    window.addEventListener('resize', function () {
        clearTimeout(window._dashResizeTimer);
        window._dashResizeTimer = setTimeout(resizeCharts, 120);
    });
})();
