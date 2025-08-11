
    window.addEventListener('load', function () {
        const loader = document.getElementById('preloader');
    loader.style.opacity = '0';
    loader.style.visibility = 'hidden';
    loader.style.transition = 'opacity 0.5s ease';
        setTimeout(() => loader.remove(), 500);
    });

