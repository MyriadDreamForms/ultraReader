document.addEventListener('DOMContentLoaded', function() {
    // Tema seçim düğmesini bulalım
    const themeSwitcher = document.getElementById('theme-switcher');
    if (!themeSwitcher) return;
    
    // Tema durumunu kontrol et
    const currentTheme = localStorage.getItem('theme') || 'light';
    
    // Sayfa yüklendiğinde tema uygula
    applyTheme(currentTheme);
    
    // Tema değiştirme butonunun metin ve ikonunu ayarla
    updateThemeSwitcherUI(currentTheme);
    
    // Tema değiştirme olayını dinle
    themeSwitcher.addEventListener('click', function() {
        const currentTheme = document.body.classList.contains('dark-theme') ? 'dark' : 'light';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        // Temayı değiştir
        applyTheme(newTheme);
        
        // Tema durumunu kaydet
        localStorage.setItem('theme', newTheme);
        
        // Tema değiştirme butonunun metin ve ikonunu güncelle
        updateThemeSwitcherUI(newTheme);
    });
});

function applyTheme(theme) {
    const body = document.body;
    const themeContainer = document.getElementById('theme-container');
    
    if (theme === 'dark') {
        body.classList.add('dark-theme');
        if (themeContainer) themeContainer.classList.add('dark-theme');
        
        // Root elementine dark-theme class ekle (tailwind için)
        document.documentElement.classList.add('dark');
        
        // Meta teması ayarla (mobil cihazlar için durum çubuğu)
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', '#111827'); // gray-900
        } else {
            const meta = document.createElement('meta');
            meta.name = 'theme-color';
            meta.content = '#111827'; // gray-900
            document.head.appendChild(meta);
        }
    } else {
        body.classList.remove('dark-theme');
        if (themeContainer) themeContainer.classList.remove('dark-theme');
        
        // Root elementinden dark-theme class kaldır (tailwind için)
        document.documentElement.classList.remove('dark');
        
        // Meta teması ayarla (mobil cihazlar için durum çubuğu)
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', '#f9fafb'); // gray-50
        } else {
            const meta = document.createElement('meta');
            meta.name = 'theme-color';
            meta.content = '#f9fafb'; // gray-50
            document.head.appendChild(meta);
        }
    }
    
    // Tema değişikliği olayını bildir
    document.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
}

function updateThemeSwitcherUI(theme) {
    const themeSwitcher = document.getElementById('theme-switcher');
    if (!themeSwitcher) return;
    
    const themeIcon = document.getElementById('theme-icon');
    const themeText = document.getElementById('theme-text');
    
    if (theme === 'dark') {
        themeIcon.classList.remove('bi-moon-fill');
        themeIcon.classList.add('bi-sun-fill');
        if (themeText) themeText.textContent = 'Aydınlık Tema';
    } else {
        themeIcon.classList.remove('bi-sun-fill');
        themeIcon.classList.add('bi-moon-fill');
        if (themeText) themeText.textContent = 'Karanlık Tema';
    }
} 