// UltraReader - Reader JavaScript Module

let dotNetRef = null;
let scrollHandler = null;
let keyHandler = null;
let lastScrollTop = 0;
let headerVisible = true;

export function initReader(objRef) {
    dotNetRef = objRef;
    lastScrollTop = 0;
    headerVisible = true;
    
    // Scroll handler for progress tracking and header visibility
    scrollHandler = () => {
        const scrollTop = window.scrollY;
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        const progress = docHeight > 0 ? Math.round((scrollTop / docHeight) * 100) : 0;
        
        // Calculate current page
        const pages = document.querySelectorAll('.reader-page');
        let currentPage = 1;
        const viewportMiddle = window.innerHeight / 2;
        
        pages.forEach((page, index) => {
            const rect = page.getBoundingClientRect();
            if (rect.top < viewportMiddle && rect.bottom > 0) {
                currentPage = index + 1;
            }
        });
        
        // Header visibility based on scroll direction
        const scrollDelta = scrollTop - lastScrollTop;
        const header = document.querySelector('.reader-header');
        
        if (header) {
            // Hide header when scrolling down, show when scrolling up
            if (scrollDelta > 10 && scrollTop > 100) {
                // Scrolling down
                if (headerVisible) {
                    header.classList.add('hidden');
                    headerVisible = false;
                }
            } else if (scrollDelta < -10 || scrollTop < 50) {
                // Scrolling up or near top
                if (!headerVisible) {
                    header.classList.remove('hidden');
                    headerVisible = true;
                }
            }
        }
        
        lastScrollTop = scrollTop;
        
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('UpdateScrollProgress', progress, currentPage);
        }
    };
    
    // Keyboard navigation
    keyHandler = (e) => {
        // Don't trigger if user is typing in an input
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return;
        }
        
        if (e.key === 'ArrowLeft') {
            e.preventDefault();
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('NavigateToChapter', 'prev');
            }
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('NavigateToChapter', 'next');
            }
        } else if (e.key === 'Home') {
            e.preventDefault();
            scrollToTop();
        } else if (e.key === 'End') {
            e.preventDefault();
            window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
        }
    };
    
    window.addEventListener('scroll', scrollHandler, { passive: true });
    window.addEventListener('keydown', keyHandler);
    
    // Initial call
    scrollHandler();
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function scrollToPage(pageNumber) {
    const page = document.getElementById(`page-${pageNumber}`);
    if (page) {
        page.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

export function disposeReader() {
    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler);
    }
    if (keyHandler) {
        window.removeEventListener('keydown', keyHandler);
    }
    dotNetRef = null;
    lastScrollTop = 0;
    headerVisible = true;
}
