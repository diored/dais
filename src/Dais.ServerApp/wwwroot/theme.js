window.themeManager = {
    init: () => {
        const saved = localStorage.getItem("theme") || "light";
        document.documentElement.setAttribute("data-theme", saved);
        const btn = document.querySelector("#theme-toggle");
        if (btn) btn.textContent = saved === "dark" ? "🌙" : "☀️";
    },
    toggle: () => {
        const current = document.documentElement.getAttribute("data-theme");
        const next = current === "light" ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", next);
        localStorage.setItem("theme", next);
        const btn = document.querySelector("#theme-toggle");
        if (btn) btn.textContent = next === "dark" ? "🌙" : "☀️";
    }
};
