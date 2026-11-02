// Global Site Scripts (Theme Toggle Helper)
window.toggleTheme = function () {
    try {
        var current = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        var next = current === 'dark' ? 'light' : 'dark';
        localStorage.setItem('hc_theme', next);
        document.documentElement.setAttribute('data-theme', next);
        if (next === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        window.dispatchEvent(new CustomEvent('theme-changed', { detail: { theme: next } }));
        return next;
    } catch (e) {
        return 'light';
    }
};
