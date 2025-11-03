(function () {
    const qs = (s, r = document) => r.querySelector(s);
    const qsa = (s, r = document) => Array.from(r.querySelectorAll(s));

    const drawer = qs('#syj-drawer');
    const backdrop = qs('#syj-backdrop');
    const burger = qs('#syj-burger');
    const btnClose = qs('#syj-drawer-close');

    function openDrawer() {
        drawer.classList.add('is-open');
        backdrop.classList.add('is-open');
        burger?.setAttribute('aria-expanded', 'true');
        drawer?.setAttribute('aria-hidden', 'false');
    }
    function closeDrawer() {
        drawer.classList.remove('is-open');
        backdrop.classList.remove('is-open');
        burger?.setAttribute('aria-expanded', 'false');
        drawer?.setAttribute('aria-hidden', 'true');
    }

    burger?.addEventListener('click', openDrawer);
    btnClose?.addEventListener('click', closeDrawer);
    backdrop?.addEventListener('click', closeDrawer);
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closeDrawer(); });

    // Acordeón (solo uno abierto)
    const accButtons = qsa('.syj-acc__btn');
    function closeAllPanels() {
        accButtons.forEach(b => b.setAttribute('aria-expanded', 'false'));
        qsa('.syj-acc__panel').forEach(p => p.classList.remove('is-open'));
    }
    accButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const key = btn.getAttribute('data-acc');
            const panel = qs(`.syj-acc__panel[data-panel="${key}"]`);
            const willOpen = btn.getAttribute('aria-expanded') !== 'true';
            closeAllPanels();
            if (willOpen) {
                btn.setAttribute('aria-expanded', 'true');
                panel?.classList.add('is-open');
            }
        });
    });
})();
