(() => {
    const button = document.getElementById("themeToggle");
    if (!button) return;
    const icon = button.querySelector(".theme-icon");
    const label = button.querySelector(".theme-label");
    const logos = document.querySelectorAll("[data-logo-light][data-logo-dark]");
    const update = () => {
        const dark = document.documentElement.dataset.theme === "dark";
        icon.textContent = dark ? "☀" : "☾";
        label.textContent = dark ? "Light mode" : "Dark mode";
        button.setAttribute("aria-label", dark ? "Switch to light mode" : "Switch to dark mode");
        logos.forEach(logo => { logo.src = dark ? logo.dataset.logoDark : logo.dataset.logoLight; });
    };
    update();
    button.addEventListener("click", () => {
        const theme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
        document.documentElement.dataset.theme = theme;
        try { localStorage.setItem("mtd-theme", theme); } catch { /* Keep the in-page selection. */ }
        update();
    });
})();

