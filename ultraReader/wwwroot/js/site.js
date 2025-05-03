// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Mobil menü işlemleri
document.addEventListener("DOMContentLoaded", function() {
    const mobileMenuButton = document.getElementById('mobile-menu-button');
    const navbarMenu = document.getElementById('navbar-menu');
    
    if (mobileMenuButton && navbarMenu) {
        mobileMenuButton.addEventListener('click', function() {
            if (navbarMenu.classList.contains('hidden')) {
                // Menüyü aç
                navbarMenu.classList.remove('hidden');
                navbarMenu.classList.add('fade-in');
                navbarMenu.classList.add('absolute', 'top-16', 'right-4', 'z-50', 'bg-gray-800', 'dark:bg-gray-900', 'p-4', 'rounded-md', 'shadow-lg', 'flex', 'flex-col', 'w-48');
                mobileMenuButton.innerHTML = '<i class="bi bi-x-lg text-2xl"></i>';
            } else {
                // Menüyü kapat
                navbarMenu.classList.add('hidden');
                navbarMenu.classList.remove('fade-in');
                navbarMenu.classList.remove('absolute', 'top-16', 'right-4', 'z-50', 'bg-gray-800', 'dark:bg-gray-900', 'p-4', 'rounded-md', 'shadow-lg', 'flex', 'flex-col', 'w-48');
                mobileMenuButton.innerHTML = '<i class="bi bi-list text-2xl"></i>';
            }
        });
        
        // Sayfa dışına tıklanınca menüyü kapat
        document.addEventListener('click', function(event) {
            const isClickInside = navbarMenu.contains(event.target) || mobileMenuButton.contains(event.target);
            
            if (!isClickInside && !navbarMenu.classList.contains('hidden') && window.innerWidth < 768) {
                navbarMenu.classList.add('hidden');
                navbarMenu.classList.remove('fade-in');
                navbarMenu.classList.remove('absolute', 'top-16', 'right-4', 'z-50', 'bg-gray-800', 'dark:bg-gray-900', 'p-4', 'rounded-md', 'shadow-lg', 'flex', 'flex-col', 'w-48');
                mobileMenuButton.innerHTML = '<i class="bi bi-list text-2xl"></i>';
            }
        });
        
        // Pencere boyutuna göre düzenleme
        window.addEventListener('resize', function() {
            if (window.innerWidth >= 768) {
                // Masaüstü görünümüne geç
                navbarMenu.classList.remove('hidden');
                navbarMenu.classList.remove('absolute', 'top-16', 'right-4', 'z-50', 'bg-gray-800', 'dark:bg-gray-900', 'p-4', 'rounded-md', 'shadow-lg', 'flex', 'flex-col', 'w-48');
                mobileMenuButton.innerHTML = '<i class="bi bi-list text-2xl"></i>';
            } else {
                // Mobil görünümde menüyü kapat
                navbarMenu.classList.add('hidden');
                mobileMenuButton.innerHTML = '<i class="bi bi-list text-2xl"></i>';
            }
        });
    }
    
    // Responsive tablo işlemleri
    const tables = document.querySelectorAll('table');
    tables.forEach(table => {
        if (!table.parentElement.classList.contains('overflow-x-auto')) {
            const wrapper = document.createElement('div');
            wrapper.classList.add('overflow-x-auto');
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }
    });
    
    // Uzun açıklamaları kısaltma
    const truncateElements = document.querySelectorAll('[data-truncate]');
    truncateElements.forEach(element => {
        const originalText = element.textContent;
        const maxLength = parseInt(element.getAttribute('data-truncate'));
        
        if (originalText.length > maxLength) {
            element.textContent = originalText.substring(0, maxLength) + '...';
            
            // Üzerine gelindiğinde tam metni göster
            element.setAttribute('title', originalText);
            
            if (element.getAttribute('data-truncate-toggle') === 'true') {
                element.style.cursor = 'pointer';
                element.addEventListener('click', function() {
                    if (element.textContent.endsWith('...')) {
                        element.textContent = originalText;
                    } else {
                        element.textContent = originalText.substring(0, maxLength) + '...';
                    }
                });
            }
        }
    });
    
    // Animasyonlu sayfa geçişleri için
    const contentArea = document.querySelector('main[role="main"]');
    if (contentArea) {
        contentArea.classList.add('fade-in');
    }
});

// Görünüm Değiştirici (Light/Dark Mode)
function setTheme(themeName) {
    localStorage.setItem('theme', themeName);
    
    const themeIcon = document.getElementById('theme-icon');
    const themeText = document.getElementById('theme-text');
    
    if (themeName === 'dark') {
        document.documentElement.classList.add('dark');
        if (themeIcon && themeText) {
            themeIcon.classList.remove('bi-moon-fill');
            themeIcon.classList.add('bi-sun-fill');
            themeText.textContent = 'Aydınlık Tema';
        }
    } else {
        document.documentElement.classList.remove('dark');
        if (themeIcon && themeText) {
            themeIcon.classList.remove('bi-sun-fill');
            themeIcon.classList.add('bi-moon-fill');
            themeText.textContent = 'Karanlık Tema';
        }
    }
}

// Tema tercihi kontrolü
(function() {
    // Yerel depolamadan tema tercihi veya sistem teması kontrolü
    if (localStorage.getItem('theme') === 'dark' || 
        (!localStorage.getItem('theme') && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
        setTheme('dark');
    } else {
        setTheme('light');
    }
    
    // Sistem teması değişikliğini dinle
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        if (!localStorage.getItem('theme')) {
            setTheme(e.matches ? 'dark' : 'light');
        }
    });
    
    const themeSwitcher = document.getElementById('theme-switcher');
    if (themeSwitcher) {
        themeSwitcher.addEventListener('click', function() {
            if (document.documentElement.classList.contains('dark')) {
                setTheme('light');
            } else {
                setTheme('dark');
            }
        });
    }
})();
