// wwwroot/js/sidebar.js
(function () {
    // 避免重複初始化（Offcanvas + Desktop 都會載入）
    if (window.__sidebarInited) return;
    window.__sidebarInited = true;

    // 啟用所有 tooltip
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el, { container: 'body' });
    });

    // 桌機縮欄/展欄
    const KEY = 'sidebar-collapsed';
    const mqLg = window.matchMedia('(min-width: 992px)');
    const root = document.documentElement;

    function applyState() {
        const collapsed = localStorage.getItem(KEY) === '1';
        if (mqLg.matches) root.classList.toggle('sidebar-collapsed', collapsed);
        else root.classList.remove('sidebar-collapsed');
    }

    function toggle(e) {
        if (e) e.preventDefault();
        const next = !(localStorage.getItem(KEY) === '1');
        localStorage.setItem(KEY, next ? '1' : '0');
        applyState();
    }

    // 事件代理：只要點到 .js-sidebar-toggle 就切換
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('.js-sidebar-toggle');
        if (!btn) return;
        if (!mqLg.matches) return; // 行動版不縮欄；用 Offcanvas
        toggle(e);
    });

    // 快捷鍵 [
    window.addEventListener('keydown', (e) => {
        if (e.key === '[' && mqLg.matches) { e.preventDefault(); toggle(e); }
    });

    mqLg.addEventListener('change', applyState);

    // 第一次載入預設縮欄
    if (localStorage.getItem(KEY) === null) localStorage.setItem(KEY, '1');
    applyState();
})();
