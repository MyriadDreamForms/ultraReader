/**
 * Admin paneli için Chart.js grafik entegrasyonu
 * Dark mode desteği ile birlikte
 */

document.addEventListener('DOMContentLoaded', function() {
    // Tema bilgisini al
    const isDarkMode = document.documentElement.classList.contains('dark');
    
    // Ortak grafik ayarları
    const commonOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                labels: {
                    color: isDarkMode ? '#e5e7eb' : '#374151'
                }
            },
            tooltip: {
                backgroundColor: isDarkMode ? '#374151' : '#ffffff',
                titleColor: isDarkMode ? '#e5e7eb' : '#111827',
                bodyColor: isDarkMode ? '#e5e7eb' : '#374151',
                borderColor: isDarkMode ? '#4b5563' : '#e5e7eb',
                borderWidth: 1
            }
        },
        scales: {
            x: {
                ticks: {
                    color: isDarkMode ? '#9ca3af' : '#6b7280'
                },
                grid: {
                    color: isDarkMode ? 'rgba(75, 85, 99, 0.2)' : 'rgba(229, 231, 235, 0.5)'
                }
            },
            y: {
                beginAtZero: true,
                ticks: {
                    color: isDarkMode ? '#9ca3af' : '#6b7280',
                    precision: 0
                },
                grid: {
                    color: isDarkMode ? 'rgba(75, 85, 99, 0.2)' : 'rgba(229, 231, 235, 0.5)'
                }
            }
        }
    };

    // Koyu/açık tema renk paleti
    const colors = {
        primary: isDarkMode ? '#4da3ff' : '#1a73e8',
        success: isDarkMode ? '#22c55e' : '#16a34a',
        warning: isDarkMode ? '#eab308' : '#ca8a04',
        danger: isDarkMode ? '#ef4444' : '#dc2626',
        info: isDarkMode ? '#6366f1' : '#4f46e5',
        background: {
            primary: isDarkMode ? 'rgba(77, 163, 255, 0.2)' : 'rgba(26, 115, 232, 0.2)',
            success: isDarkMode ? 'rgba(34, 197, 94, 0.2)' : 'rgba(22, 163, 74, 0.2)',
            warning: isDarkMode ? 'rgba(234, 179, 8, 0.2)' : 'rgba(202, 138, 4, 0.2)',
            danger: isDarkMode ? 'rgba(239, 68, 68, 0.2)' : 'rgba(220, 38, 38, 0.2)',
            info: isDarkMode ? 'rgba(99, 102, 241, 0.2)' : 'rgba(79, 70, 229, 0.2)'
        }
    };

    // Farklı grafik türleri için renk ve arka plan dizileri
    const chartColors = [
        colors.primary, 
        colors.success, 
        colors.warning, 
        colors.danger, 
        colors.info, 
        '#6b7280', 
        '#8b5cf6', 
        '#ec4899', 
        '#14b8a6', 
        '#f59e0b'
    ];
    
    const chartBackgrounds = [
        colors.background.primary,
        colors.background.success,
        colors.background.warning,
        colors.background.danger,
        colors.background.info,
        'rgba(107, 114, 128, 0.2)',
        'rgba(139, 92, 246, 0.2)',
        'rgba(236, 72, 153, 0.2)',
        'rgba(20, 184, 166, 0.2)',
        'rgba(245, 158, 11, 0.2)'
    ];

    // Türlere Göre Webtoon Dağılımı Grafiği
    function createGenreChart(data) {
        const ctx = document.getElementById('genreChart');
        if (!ctx) return;
        
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: Object.keys(data),
                datasets: [{
                    data: Object.values(data),
                    backgroundColor: chartColors,
                    borderColor: isDarkMode ? '#1f2937' : '#ffffff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: isDarkMode ? '#e5e7eb' : '#374151',
                            padding: 15,
                            font: {
                                size: 11
                            }
                        }
                    }
                }
            }
        });
    }

    // Durumlara Göre Webtoon Dağılımı Grafiği
    function createStatusChart(data) {
        const ctx = document.getElementById('statusChart');
        if (!ctx) return;
        
        return new Chart(ctx, {
            type: 'bar',
            data: {
                labels: Object.keys(data),
                datasets: [{
                    label: 'Webtoon Sayısı',
                    data: Object.values(data),
                    backgroundColor: colors.success,
                    borderColor: isDarkMode ? '#059669' : '#047857',
                    borderWidth: 1
                }]
            },
            options: commonOptions
        });
    }

    // Son 7 Gün Yorum İstatistikleri
    function createCommentChart(data) {
        const ctx = document.getElementById('commentChart');
        if (!ctx) return;
        
        const dates = Object.keys(data).map(date => {
            const d = new Date(date);
            return d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit' });
        });
        
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Yorum Sayısı',
                    data: Object.values(data),
                    fill: false,
                    backgroundColor: colors.warning,
                    borderColor: colors.warning,
                    tension: 0.2
                }]
            },
            options: commonOptions
        });
    }

    // Son 7 Gün Kullanıcı Kayıt İstatistikleri
    function createUserChart(data) {
        const ctx = document.getElementById('userChart');
        if (!ctx) return;
        
        const dates = Object.keys(data).map(date => {
            const d = new Date(date);
            return d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit' });
        });
        
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Yeni Kullanıcı',
                    data: Object.values(data),
                    fill: true,
                    backgroundColor: colors.background.info,
                    borderColor: colors.info,
                    tension: 0.2
                }]
            },
            options: commonOptions
        });
    }

    // Webtoon Görüntülenme Analizi
    function createViewsChart(data) {
        const ctx = document.getElementById('viewsChart');
        if (!ctx) return;
        
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: Object.keys(data),
                datasets: [{
                    label: 'Görüntülenme',
                    data: Object.values(data),
                    fill: true,
                    backgroundColor: colors.background.primary,
                    borderColor: colors.primary,
                    tension: 0.3
                }]
            },
            options: commonOptions
        });
    }

    // Tema değişince grafikleri yeniden oluştur
    document.addEventListener('themeChanged', function() {
        location.reload(); // Sayfayı yenile
    });

    // AdminCharts nesnesini global olarak tanımla
    window.AdminCharts = {
        createGenreChart,
        createStatusChart,
        createCommentChart,
        createUserChart,
        createViewsChart
    };
}); 