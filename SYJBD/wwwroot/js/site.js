/* =====================================================================
   site.js – Sidebar estable, acordeón confiable, ruta activa y off-canvas
   + UX de tabla: hint de overflow y drag-to-scroll (mouse/táctil)
   ===================================================================== */
(function () {
    "use strict";

    // ---------- helpers ----------
    const $ = (s, r) => (r || document).querySelector(s);
    const $$ = (s, r) => Array.from((r || document).querySelectorAll(s));
    const on = (el, ev, fn, opts) => el && el.addEventListener(ev, fn, opts || false);
    const isMobile = () => matchMedia("(max-width:1024px)").matches;

    // ---------- acordeón ----------
    function openSub(section) {
        const btn = $(".nav-btn", section);
        const sub = $(".nav-sub", section);
        if (!btn || !sub) return;

        btn.classList.add("open");
        btn.setAttribute("aria-expanded", "true");

        sub.style.display = "block";
        sub.style.overflow = "hidden";
        sub.style.maxHeight = "0px";

        const target = sub.scrollHeight;
        requestAnimationFrame(() => { sub.style.maxHeight = target + "px"; });

        const done = () => {
            if (btn.classList.contains("open")) {
                sub.style.maxHeight = "none";
                sub.style.overflow = "visible";
            }
            sub.removeEventListener("transitionend", done);
        };
        sub.addEventListener("transitionend", done);
    }

    function closeSub(section) {
        const btn = $(".nav-btn", section);
        const sub = $(".nav-sub", section);
        if (!btn || !sub) return;

        btn.classList.remove("open");
        btn.setAttribute("aria-expanded", "false");

        if (sub.style.maxHeight === "none" || !sub.style.maxHeight) {
            sub.style.maxHeight = sub.scrollHeight + "px";
        }
        sub.style.overflow = "hidden";
        requestAnimationFrame(() => { sub.style.maxHeight = "0px"; });
    }

    function closeAll() { $$("#sidebar .nav-section[data-section]").forEach(closeSub); }
    function openOnly(s) { closeAll(); openSub(s); }

    // ---------- resalta activo por URL ----------
    function markActiveByPath() {
        const path = location.pathname.toLowerCase();
        let active = null;

        $$("#sidebar a.nav-link").forEach(a => {
            a.classList.remove("active");
            const linkPath = (a.pathname || "").toLowerCase();
            if (linkPath && path.startsWith(linkPath) && linkPath !== "/") active = a;
        });

        if (active) {
            active.classList.add("active");
            const section = active.closest(".nav-section[data-section]");
            if (section) openOnly(section);
        } else {
            closeAll();
        }
    }

    // ---------- init sidebar ----------
    function initSidebar() {
        const sections = $$("#sidebar .nav-section[data-section]");

        // acordeón (click + teclado)
        sections.forEach(section => {
            const btn = $(".nav-btn", section);
            on(btn, "click", (e) => {
                e.preventDefault();
                const opened = btn.classList.contains("open");
                opened ? closeSub(section) : openOnly(section);
            });
            on(btn, "keydown", (e) => {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    const opened = btn.classList.contains("open");
                    opened ? closeSub(section) : openOnly(section);
                }
            });
        });

        // hamburguesa (off-canvas móvil)
        const burger = $("#sidebarToggle");
        on(burger, "click", () => {
            const sb = $("#sidebar");
            const now = !sb.classList.contains("open");
            sb.classList.toggle("open", now);
            document.body.classList.toggle("menu-open", now);
        });

        // cerrar al tocar fuera (solo móvil)
        on(document, "click", (ev) => {
            const sb = $("#sidebar");
            if (!isMobile() || !sb?.classList.contains("open")) return;
            const inside = sb.contains(ev.target);
            const onBurger = $("#sidebarToggle")?.contains(ev.target);
            if (!inside && !onBurger) {
                sb.classList.remove("open");
                document.body.classList.remove("menu-open");
            }
        });

        // estado inicial
        closeAll();
        markActiveByPath();
    }

    // ---------- UX tabla: hint de overflow + drag-to-scroll ----------
    function initTableUX() {
        const wraps = $$(".table-wrap");
        if (!wraps.length) return;

        wraps.forEach(wrap => {
            // 1) activar/desactivar hint de overflow (sombras laterales)
            const toggleHint = () => {
                const hasOverflow = wrap.scrollWidth > wrap.clientWidth + 1;
                wrap.classList.toggle("is-overflowing", hasOverflow);
            };
            toggleHint();
            on(wrap, "scroll", toggleHint, { passive: true });
            try { new ResizeObserver(toggleHint).observe(wrap); } catch { /* IE/antiguos */ }
            on(window, "resize", toggleHint, { passive: true });

            // 2) drag-to-scroll (mouse y táctil) con heurística por eje
            let isDown = false, startX = 0, startY = 0, sl = 0, st = 0, moved = false;

            const hasH = () => wrap.scrollWidth > wrap.clientWidth + 1;
            const hasV = () => wrap.scrollHeight > wrap.clientHeight + 1;

            const down = (e) => {
                isDown = true; moved = false;
                wrap.classList.add("dragging");
                const p = e.touches ? e.touches[0] : e;
                startX = p.clientX; startY = p.clientY;
                sl = wrap.scrollLeft; st = wrap.scrollTop;
            };

            const move = (e) => {
                if (!isDown) return;
                const p = e.touches ? e.touches[0] : e;
                const dx = p.clientX - startX;
                const dy = p.clientY - startY;

                // Si hay overflow en el eje correspondiente, permitimos arrastrar ese eje
                let consumed = false;

                if (hasH() && Math.abs(dx) > 2) {
                    wrap.scrollLeft = sl - dx;
                    consumed = true;
                }
                if (hasV() && Math.abs(dy) > 2) {
                    wrap.scrollTop = st - dy;
                    consumed = true;
                }

                if (consumed) {
                    moved = true;
                    // Solo evitamos el comportamiento nativo si realmente estamos desplazando la tabla.
                    e.preventDefault();
                }
            };

            const up = () => {
                isDown = false;
                wrap.classList.remove("dragging");
            };

            on(wrap, "mousedown", down, { passive: true });
            on(wrap, "mousemove", move, { passive: false });
            on(window, "mouseup", up, { passive: true });

            on(wrap, "touchstart", down, { passive: true });
            on(wrap, "touchmove", move, { passive: false });
            on(wrap, "touchend", up, { passive: true });

            // Evita click “fantasma” tras arrastrar
            on(wrap, "click", (e) => { if (moved) e.preventDefault(); }, true);

        });
    }

    // ---------- boot ----------
    document.addEventListener("DOMContentLoaded", () => {
        initSidebar();
        initTableUX();

        // ---------- MODAL LIGERO (fetch + partials) ----------
        (() => {
            const modal = document.getElementById('app-modal');
            if (!modal) return;
            const body = document.getElementById('app-modal-body');

            const open = (html) => {
                body.innerHTML = html;
                modal.classList.remove('hidden');
                modal.querySelector('.app-modal__box').focus();
            };

            const close = () => {
                modal.classList.add('hidden');
                body.innerHTML = '';
            };

            modal.addEventListener('click', (e) => {
                if (e.target.closest('[data-modal-close]')) close();
            });

            // Abrir modal desde cualquier enlace con data-modal-url
            document.addEventListener('click', async (e) => {
                const a = e.target.closest('[data-modal-url]');
                if (!a) return;
                e.preventDefault();
                const url = a.getAttribute('data-modal-url');
                const r = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                open(await r.text());
            });

            // Interceptar forms dentro del modal y enviar por fetch
            modal.addEventListener('submit', async (e) => {
                const f = e.target.closest('form');
                if (!f) return;
                e.preventDefault();

                const r = await fetch(f.action, {
                    method: f.method || 'POST',
                    body: new FormData(f),
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                // Si el servidor devuelve 200 con HTML => re-pinta el modal (errores de validación)
                // Si devuelve 204 (NoContent) o 302 (redirección) => cierra y recarga
                if (r.status === 204 || r.redirected) {
                    close();
                    location.reload();
                    return;
                }

                const ct = r.headers.get('content-type') || '';
                if (ct.includes('text/html')) {
                    open(await r.text());
                } else {
                    close(); location.reload();
                }
            });
        })();
    });
})();
