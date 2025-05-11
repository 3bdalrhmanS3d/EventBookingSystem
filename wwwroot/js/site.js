// Theme management
document.addEventListener('DOMContentLoaded', function () {
    // Get theme toggle button
    const themeToggleBtn = document.getElementById('theme-toggle-btn');

    // Function to set theme
    function setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);

        // Update any theme-specific elements
        updateThemeSpecificElements(theme);
    }

    // Function to update theme-specific elements
    function updateThemeSpecificElements(theme) {
        // Update logo if needed
        const logo = document.querySelector('.navbar-brand img');
        if (logo) {
            if (theme === 'dark') {
                // If you have a dark mode specific logo
                if (logo.src.indexOf('logo.png') !== -1) {
                    logo.src = logo.src.replace('logo.png', 'logo-dark.png');
                }
            } else {
                // Switch back to light mode logo
                if (logo.src.indexOf('logo-dark.png') !== -1) {
                    logo.src = logo.src.replace('logo-dark.png', 'logo.png');
                }
            }
        }
    }

    // Function to toggle theme
    function toggleTheme() {
        const currentTheme = localStorage.getItem('theme') || 'light';
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
    }

    // Add click event to theme toggle button
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', toggleTheme);
    }

    // Initialize theme based on:
    // 1. User's previous preference from localStorage
    // 2. System preference if no localStorage value
    // 3. Default to light theme if neither is available
    function initializeTheme() {
        const savedTheme = localStorage.getItem('theme');

        if (savedTheme) {
            // Use saved theme
            setTheme(savedTheme);
        } else {
            // Check for system preference
            const prefersDarkMode = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            setTheme(prefersDarkMode ? 'dark' : 'light');

            // Listen for system preference changes
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                if (!localStorage.getItem('theme')) {
                    setTheme(e.matches ? 'dark' : 'light');
                }
            });
        }
    }

    // Initialize theme on page load
    initializeTheme();
});