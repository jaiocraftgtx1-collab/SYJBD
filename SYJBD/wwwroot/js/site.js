/* =====================================================================
   site.js – Sidebar estable, UX de tabla, Modal ligero y flujos de Ventas
   ===================================================================== */
(function () {
    "use strict";

    // ---------- Helpers base ----------
    const $ = (s, r) => (r || document).querySelector(s);
    const $$ = (s, r) => Array.from((r || document).querySelectorAll(s));
    const on = (el, ev, fn, opts) => el && el.addEventListener(ev, fn, opts || false);
    const isMobile = () => matchMedia("(max-width:1024px)").matches;

    // Fallback de formato a 2 decimales si no existe global
    window.fmt2 = window.fmt2 || ((x) => Number(x || 0).toFixed(2));

    /* =====================================================================
       SIDEBAR (acordeón estable + ruta activa + off-canvas móvil)
       ===================================================================== */
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

    function initSidebar() {
        const sections = $$("#sidebar .nav-section[data-section]");

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

        const burger = $("#sidebarToggle");
        on(burger, "click", () => {
            const sb = $("#sidebar");
            const now = !sb.classList.contains("open");
            sb.classList.toggle("open", now);
            document.body.classList.toggle("menu-open", now);
        });

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

        closeAll();
        markActiveByPath();
    }

    /* =====================================================================
       UX TABLA: hint de overflow + drag-to-scroll
       ===================================================================== */
    function initTableUX() {
        const wraps = $$(".table-wrap");
        if (!wraps.length) return;

        wraps.forEach(wrap => {
            const toggleHint = () => {
                const hasOverflow = wrap.scrollWidth > wrap.clientWidth + 1;
                wrap.classList.toggle("is-overflowing", hasOverflow);
            };
            toggleHint();
            on(wrap, "scroll", toggleHint, { passive: true });
            try { new ResizeObserver(toggleHint).observe(wrap); } catch { }
            on(window, "resize", toggleHint, { passive: true });

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
                let consumed = false;

                if (hasH() && Math.abs(dx) > 2) { wrap.scrollLeft = sl - dx; consumed = true; }
                if (hasV() && Math.abs(dy) > 2) { wrap.scrollTop = st - dy; consumed = true; }

                if (consumed) { moved = true; e.preventDefault(); }
            };
            const up = () => { isDown = false; wrap.classList.remove("dragging"); };

            on(wrap, "mousedown", down, { passive: true });
            on(wrap, "mousemove", move, { passive: false });
            on(window, "mouseup", up, { passive: true });
            on(wrap, "touchstart", down, { passive: true });
            on(wrap, "touchmove", move, { passive: false });
            on(wrap, "touchend", up, { passive: true });

            on(wrap, "click", (e) => { if (moved) e.preventDefault(); }, true);
        });
    }

    /* =====================================================================
       LOADER GLOBAL (API)
       Requiere en DOM:
         <div id="app-loader" class="app-loader app-loader_hidden">
           <strong id="app-loader-text">Procesando…</strong>
           <span class="app-loader__dots"></span>
         </div>
       ===================================================================== */
    (function () {
        const host = document.getElementById('app-loader');
        const txtEl = document.getElementById('app-loader-text');
        const dotsEl = host ? host.querySelector('.app-loader__dots') : null;
        let dotsTimer = null;

        function startDots() {
            if (!dotsEl) return;
            clearInterval(dotsTimer);
            let i = 0;
            dotsTimer = setInterval(() => {
                i = (i + 1) % 4;
                dotsEl.textContent = '.'.repeat(i);
            }, 400);
        }
        function stopDots() {
            if (!dotsEl) return;
            clearInterval(dotsTimer);
            dotsEl.textContent = '';
        }

        window.showLoader = function (message = 'Procesando…') {
            if (!host) return;
            if (txtEl) txtEl.textContent = message;
            host.classList.remove('app-loader_hidden');
            host.setAttribute('aria-hidden', 'false');
            startDots();
        };
        window.hideLoader = function () {
            if (!host) return;
            stopDots();
            host.classList.add('app-loader_hidden');
            host.setAttribute('aria-hidden', 'true');
        };

        window.fetchWithLoader = async function (url, options = {}, message = 'Procesando…') {
            showLoader(message);
            try { return await fetch(url, options); }
            finally { hideLoader(); }
        };
    })();

    /* =====================================================================
       MODAL LIGERO (partials AJAX)
       ===================================================================== */
    function initModal() {
        const modal = document.getElementById("app-modal");
        if (!modal) return;

        const box = modal.querySelector(".app-modal__box");
        const body = document.getElementById("app-modal-body");

        const open = (html) => {
            body.innerHTML = html;
            modal.classList.remove("app-modal_hidden");
            box && box.focus();
            modal.__cajaWired = false;      // permite recablear en cada partial
            wireCajaCerrarInModal(modal);   // si existe #frmCajaCerrar, se cablea
            wirePreciosPorTalla(modal);
        };
        const close = () => {
            modal.classList.add("app-modal_hidden");
            body.innerHTML = "";
        };

        // Cerrar por botón / backdrop
        on(modal, "click", (e) => { if (e.target.closest("[data-modal-close]")) close(); });

        // Inputs decimales con máximo 2 decimales, sin signo negativo
        on(modal, "keypress", (e) => {
            const inp = e.target.closest('input[data-decimal="2"]');
            if (!inp) return;
            const ch = e.key;
            if (ch.length !== 1) return;
            if (!/[0-9.]/.test(ch)) { e.preventDefault(); return; }
            if (ch === "-") { e.preventDefault(); return; }
            const v = inp.value || "";
            if (ch === "." && v.includes(".")) { e.preventDefault(); return; }
            const dot = v.indexOf(".");
            if (dot >= 0) {
                const selStart = inp.selectionStart ?? v.length;
                const isAfterDot = selStart > dot;
                const decimals = v.substring(dot + 1);
                if (isAfterDot && decimals.length >= 2) { e.preventDefault(); return; }
            }
        });

        // Interceptor de links que abren partial en el modal
        on(document, "click", async (e) => {
            if (e.button !== 0) return;                       // solo click izquierdo
            if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

            const a = e.target.closest("a");
            if (!a || a.matches("[data-modal-ignore]")) return;

            let url = a.getAttribute("data-modal-url");
            if (!url) {
                const href = a.getAttribute("href") || "";
                if (/\/Ventas\/Ver\/|^\/$/.test(new URL(href, location.origin).pathname)) url = href;
            }
            if (!url) return;

            e.preventDefault();
            try {
                const r = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
                if (!r.ok) throw new Error(r.statusText);
                const html = await r.text();
                open(html);
            } catch (err) {
                console.error("Modal fetch error:", err);
                const fallback = a.getAttribute("href") || url;
                if (fallback) location.href = fallback;
            }
        });

        // Botones dentro del modal que hacen POST simple
        on(modal, "click", async (e) => {
            const btn = e.target.closest("[data-post-url]");
            if (!btn) return;

            const url = btn.getAttribute("data-post-url") || "";
            const msg = btn.getAttribute("data-confirm") || "";
            if (msg && !confirm(msg)) return;

            try {
                const r = await fetchWithLoader(url, {
                    method: "POST",
                    headers: { "X-Requested-With": "XMLHttpRequest" }
                }, "Procesando...");
                if (!r.ok) {
                    const t = await r.text().catch(() => "");
                    alert("No se pudo completar la acción. " + (t || r.statusText));
                    return;
                }
                modal.querySelector("[data-modal-close]")?.click();
                location.reload();
            } catch (err) {
                alert("Error: " + err);
            }
        });

        // Apertura de caja (form dedicado)
        on(modal, "submit", async (e) => {
            const f = e.target.closest("#frmCajaAbrir");
            if (!f) return;
            e.preventDefault();

            const selTienda = f.querySelector('[name="IdTienda"]');
            const inpMonto = f.querySelector('[name="MontoApertura"]');

            if (!selTienda || !selTienda.value) {
                alert("Selecciona una tienda.");
                selTienda?.focus(); return;
            }
            const val = (inpMonto?.value || "").trim();
            if (!/^\d+(\.\d{1,2})?$/.test(val)) {
                alert("Ingresa un monto válido (solo números y hasta 2 decimales).");
                inpMonto?.focus(); return;
            }
            if (!confirm("¿Deseas aperturar la caja con estos datos?")) return;

            let r;
            try {
                r = await fetchWithLoader(f.action, {
                    method: "POST",
                    body: new FormData(f),
                    headers: { "X-Requested-With": "XMLHttpRequest" }
                }, "Registrando apertura...");
            } catch {
                alert("Error de conexión al registrar la apertura."); return;
            }

            const ct = r.headers.get("content-type") || "";
            if (!ct.includes("application/json")) {
                if (r.status === 204 || r.redirected) { close(); location.reload(); return; }
                const html = await r.text();
                document.getElementById("app-modal-body").innerHTML = html;
                alert("Respuesta inesperada del servidor.");
                return;
            }

            const data = await r.json().catch(() => ({}));
            if (!data.ok) { alert(data.msg || "No se pudo aperturar la caja."); return; }

            modal.querySelector("[data-modal-close]")?.click();
            const go = data.url
                || `/Ventas/NuevaVenta?idCaja=${encodeURIComponent(data.idCaja)}&idTienda=${encodeURIComponent(data.idTienda)}&idUsuario=${encodeURIComponent(data.idUsuario)}`;
            location.href = go;
        });

        // Interceptor genérico de forms — IGNORA abrir y cerrar caja
        on(modal, "submit", async (e) => {
            const f = e.target.closest("form");
            if (!f || f.matches("#frmCajaAbrir") || f.matches("#frmCajaCerrar")) return; // <- FIX clave
            e.preventDefault();

            const r = await fetch(f.action, {
                method: (f.method || "POST").toUpperCase(),
                body: new FormData(f),
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (r.status === 204 || r.redirected) { close(); location.reload(); return; }
            const ct = r.headers.get("content-type") || "";
            if (ct.includes("text/html")) {
                const html = await r.text();
                open(html);
            } else {
                close(); location.reload();
            }
        });
    }

    /* =====================================================================
       CIERRE DE CAJA – Confirmación INLINE (para modal y para página suelta)
       ===================================================================== */
    function attachCajaCerrar(frm) {
        if (!frm || frm.__wired) return;
        frm.__wired = true;

        const inpReal = frm.querySelector('#inpReal');
        const hidEsperado = frm.querySelector('#hidEsperado');
        const hidPend = frm.querySelector('#hidPend');
        const txtEstado = frm.querySelector('#txtEstado');
        let box = frm.querySelector('#cajaConfirm');

        // si falta el contenedor de confirmación, lo creamos después de .form-actions o al final
        if (!box) {
            box = document.createElement('div');
            box.id = 'cajaConfirm';
            const btnRow = frm.querySelector('.form-actions') || frm.lastElementChild;
            (btnRow?.parentElement || frm).appendChild(box);
        }

        // Hidden que la acción del controller espera
        let hidReal = frm.querySelector('input[name="EfectivoReal"]');
        if (!hidReal) {
            hidReal = document.createElement('input');
            hidReal.type = 'hidden';
            hidReal.name = 'EfectivoReal';
            frm.appendChild(hidReal);
        }

        const onlyDec = (v) => {
            v = (v || '').toString().replace(/,/g, '.').replace(/[^\d.]/g, '');
            const parts = v.split('.');
            if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
            const [ent, dec = ''] = v.split('.');
            return ent.replace(/^0+(\d)/, '$1') + (v.includes('.') ? '.' + dec.slice(0, 2) : '');
        };
        const calcEstado = (esp, real) => {
            const dif = +(esp - real).toFixed(2);
            if (dif === 0) return { texto: 'CIERRE CORRECTO', dif };
            if (dif > 0) return { texto: `OBSERVADO (FALTA ${dif.toFixed(2)})`, dif };
            return { texto: `OBSERVADO (EXCEDE ${Math.abs(dif).toFixed(2)})`, dif };
        };
        const resetConfirm = () => { box.innerHTML = ''; frm.dataset.confirmed = '0'; };

        // Estado automático en vivo
        if (inpReal) {
            inpReal.addEventListener('input', () => {
                inpReal.value = onlyDec(inpReal.value);
                const esp = Number(hidEsperado?.value || 0);
                const real = Number(inpReal.value || 0);
                const est = calcEstado(esp, real);
                if (txtEstado) { txtEstado.value = est.texto; txtEstado.dataset.diff = String(est.dif); }
                resetConfirm();
            }, { passive: true });
            inpReal.dispatchEvent(new Event('input'));
        }

        function renderConfirm(htmlMsg, tone /* primary|warning|danger */) {
            box.innerHTML = `
        <div class="alert alert-${tone} d-flex justify-content-between align-items-center gap-2 mt-2">
          <div>${htmlMsg}</div>
          <div class="d-flex gap-2">
            <button type="button" class="btn btn-secondary btn-sm" data-cf="cancel">Cancelar</button>
            <button type="button" class="btn btn-primary btn-sm" data-cf="ok">Confirmar cierre</button>
          </div>
        </div>`;
            const btnOk = box.querySelector('[data-cf="ok"]');
            const btnCan = box.querySelector('[data-cf="cancel"]');

            btnCan?.addEventListener('click', resetConfirm);

            // Enviar por AJAX al confirmar (para evitar que otro listener intercepte)
            btnOk?.addEventListener('click', async () => {
                const vNorm = onlyDec(inpReal?.value || '');
                if (!vNorm) { alert('Ingresa el efectivo contado (formato 0.00).'); inpReal?.focus(); return; }
                hidReal.value = vNorm;

                if (frm.dataset.sending === '1') return;
                frm.dataset.sending = '1';

                try {
                    const r = await (window.fetchWithLoader
                        ? fetchWithLoader(frm.action, { method: 'POST', body: new FormData(frm), headers: { 'X-Requested-With': 'XMLHttpRequest' } }, 'Cerrando caja...')
                        : fetch(frm.action, { method: 'POST', body: new FormData(frm), headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                    );

                    if (!r.ok) {
                        const t = await r.text().catch(() => '');
                        alert(t || ('Error HTTP ' + r.status));
                        return;
                    }

                    const ct = r.headers.get('content-type') || '';
                    if (ct.includes('application/json')) {
                        const data = await r.json().catch(() => ({}));
                        if (data?.msg) alert(data.msg);
                    }

                    // Si estaba en modal, ciérralo; si no, igual recarga
                    document.querySelector('#app-modal [data-modal-close]')?.click();
                    location.reload();
                } catch (err) {
                    console.error(err);
                    alert(err?.message || 'No se pudo completar la operación.');
                } finally {
                    frm.dataset.sending = '0';
                }
            });
        }

        // Interceptar submit del botón “Cerrar caja” -> mostrar confirmación inline
        frm.addEventListener('submit', (e) => {
            e.preventDefault();
            e.stopImmediatePropagation(); // crítico: evita que otro listener AJAX lo capture

            const pend = Number(hidPend?.value || 0);
            if (pend > 0) {
                renderConfirm(
                    `<strong>No puedes cerrar la caja.</strong><br>Hay egresos en <b>EFECTIVO</b> pendientes de autorización.`,
                    'danger'
                );
                return;
            }

            const v = onlyDec(inpReal?.value || '');
            if (!v) { alert('Ingresa el efectivo contado (formato 0.00).'); inpReal?.focus(); return; }
            const real = Number(v);
            if (real < 0) { alert('El efectivo contado no puede ser negativo.'); inpReal?.focus(); return; }

            const esp = Number(hidEsperado?.value || 0);
            const dif = +(esp - real).toFixed(2);
            const msg = (dif === 0)
                ? '¿Confirmas el cierre de caja?'
                : `${dif > 0 ? `OBSERVADO (FALTA ${dif.toFixed(2)})` : `OBSERVADO (EXCEDE ${Math.abs(dif).toFixed(2)})`}<br>¿Deseas cerrar la caja de todas formas?`;

            renderConfirm(msg, dif === 0 ? 'primary' : 'warning');
        });
    }

    // En modal
    function wireCajaCerrarInModal(modal) {
        if (modal.__cajaWired) return;
        const frm = modal.querySelector('#frmCajaCerrar');
        if (!frm) return;
        modal.__cajaWired = true;
        attachCajaCerrar(frm);

        // Si cierran el modal, limpia confirmación visual
        on(modal, 'click', (ev) => {
            if (ev.target.closest('[data-modal-close]')) {
                const box = frm.querySelector('#cajaConfirm');
                if (box) box.innerHTML = '';
            }
        });
    }

    // En página suelta (por si /Ventas/CajaCerrar abre vista directa)
    function wireCajaCerrarInPage() {
        const frm = document.getElementById('frmCajaCerrar');
        if (frm) attachCajaCerrar(frm);
    }

    /* =====================================================================
       BLOQUE VENTAS: dropdown filtrable + validación de fechas
       ===================================================================== */
    function initVentas() {
        // Dropdowns filtrables
        $$(".fd-wrap[data-fd]").forEach(wrap => {
            const btn = $(".fd-btn", wrap);
            const caption = $(".fd-caption", wrap);
            const menu = $(".fd-menu", wrap);
            const inputHidden = wrap.querySelector('input[type="hidden"]');
            const search = $(".fd-search", menu);
            const items = $$(".fd-list li", menu);

            btn && btn.addEventListener("click", () => {
                menu.classList.toggle("open");
                if (menu.classList.contains("open")) {
                    search.value = "";
                    items.forEach(li => li.style.display = "");
                    search.focus();
                }
            });

            document.addEventListener("click", (e) => {
                if (!wrap.contains(e.target)) menu.classList.remove("open");
            });

            search && search.addEventListener("input", (e) => {
                const q = (e.target.value || "").toLowerCase();
                items.forEach(li => {
                    const t = li.textContent.toLowerCase();
                    li.style.display = t.includes(q) ? "" : "none";
                });
            });

            items.forEach(li => {
                li.addEventListener("click", () => {
                    const val = li.getAttribute("data-value");
                    inputHidden.value = val;
                    caption.textContent = val;
                    menu.classList.remove("open");
                });
            });

            const current = wrap.getAttribute("data-current");
            if (current) caption.textContent = current;
        });

        // Filtro por fechas: ambas o ninguna
        const frm = document.getElementById("frmVentas");
        if (frm) {
            frm.addEventListener("submit", function (e) {
                const d = frm.querySelector('input[name="Desde"]').value;
                const h = frm.querySelector('input[name="Hasta"]').value;
                if ((d && !h) || (!d && h)) {
                    e.preventDefault();
                    const msg = document.createElement("div");
                    msg.className = "alert alert-warning";
                    msg.style.margin = "8px 16px";
                    msg.innerHTML = '<i class="bi bi-exclamation-triangle"></i> Debes seleccionar ambas fechas (Desde y Hasta).';
                    frm.parentElement.insertBefore(msg, frm);
                    setTimeout(() => msg.remove(), 4000);
                }
            });
        }

        // Fechas dd/MM/yyyy -> hidden ISO
        (function () {
            const parseUI = (s) => {
                const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec((s || "").trim());
                if (!m) return null;
                const d = +m[1], M = +m[2], Y = +m[3];
                const dt = new Date(Y, M - 1, d);
                if (dt.getFullYear() !== Y || dt.getMonth() !== (M - 1) || dt.getDate() !== d) return null;
                const mm = String(M).padStart(2, "0");
                const dd = String(d).padStart(2, "0");
                return `${Y}-${mm}-${dd}`;
            };
            const formatUI = (iso) => {
                if (!iso) return "";
                const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso);
                if (!m) return "";
                return `${m[3]}/${m[2]}/${m[1]}`;
            };

            document.querySelectorAll(".date-ddmmyyyy").forEach(inp => {
                const hiddenSel = inp.getAttribute("data-hidden");
                const hid = hiddenSel ? document.querySelector(hiddenSel) : null;
                if (!hid) return;

                if (!inp.value && hid.value) inp.value = formatUI(hid.value);

                inp.addEventListener("blur", () => {
                    const iso = parseUI(inp.value);
                    hid.value = iso || "";
                    if (!iso && inp.value.trim() !== "") {
                        inp.classList.add("is-invalid");
                    } else {
                        inp.classList.remove("is-invalid");
                    }
                });
            });
        })();
    }


    // ======== Precios por talla: validación + guardar + UX ========

    // ======== Precios por talla: validación + guardar + UX ========
function wirePreciosPorTalla(modal){
  const scope = modal || document;

  // Normalizador a NN[.DD] (punto, máx 2 dec) -> número >= 0 o null
  const normPrecio = (raw) => {
    if (raw == null) return null;
    let v = String(raw).trim().replace(/,/g,'.').replace(/[^0-9.]/g,'');
    const parts = v.split('.');
    if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
    const [ent, dec=''] = v.split('.');
    v = (ent.replace(/^0+(\d)/,'$1') || (v.includes('.') ? '0' : '')) + (v.includes('.') ? '.' + dec.slice(0,2) : '');
    if (v === '' || v === '.') return null;
    const n = Number(v);
    if (!Number.isFinite(n) || n < 0) return null;
    return Math.round(n * 100) / 100;
  };

  // Al abrir el modal: formatea visualmente a 2 decimales (0.00)
  scope.querySelectorAll('.js-precio').forEach(inp=>{
    inp.value = (window.fmt2 ? window.fmt2(inp.value) : Number(inp.value||0).toFixed(2));
  });

  // Teclado: solo dígitos, un punto y teclas de edición/navegación
  const ALLOW = new Set(['Backspace','Delete','Tab','Enter','Escape','Home','End','ArrowLeft','ArrowRight','ArrowUp','ArrowDown','.']);
  scope.addEventListener('keydown', (ev) => {
    const inp = ev.target;
    if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
    const k = ev.key;
    if (ALLOW.has(k)) { if (k === '.' && inp.value.includes('.')) ev.preventDefault(); return; }
    if (k >= '0' && k <= '9') return;
    ev.preventDefault();
  });

  // Paste/typing: normalizar en vivo
  const normalizeInput = (el)=>{
    const before = el.value;
    let v = before.replace(/,/g,'.').replace(/[^0-9.]/g,'');
    const parts = v.split('.');
    if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
    const [ent, dec=''] = v.split('.');
    v = (ent.replace(/^0+(\d)/,'$1') || (v.includes('.') ? '0' : '')) + (v.includes('.') ? '.' + dec.slice(0,2) : '');
    if (before !== v) el.value = v;
  };
  scope.addEventListener('input', (ev) => {
    const inp = ev.target;
    if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
    normalizeInput(inp);
  });

  // Seleccionar todo al enfocar (un toque/click)
  scope.addEventListener('focusin', (ev) => {
    const el = ev.target;
    if (el instanceof HTMLInputElement && el.classList.contains('sel-all')) {
      setTimeout(()=>{ try{ el.select(); }catch{} }, 0);
    }
  });
  scope.addEventListener('mouseup', (ev) => {
    if (ev.target?.classList?.contains('sel-all')) ev.preventDefault();
  }, true);

  // Blur: fija a 2 decimales
  scope.addEventListener('blur', (ev) => {
    const inp = ev.target;
    if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
    const n = normPrecio(inp.value);
    if (n != null) inp.value = n.toFixed(2);
  }, true);

  // Guardar (delegado en el modal)
  scope.addEventListener('click', async (ev) => {
    const btn = ev.target.closest?.('.js-save');
    if (!btn) return;

    const row   = btn.closest('tr');
    const talla = row?.dataset.talla?.trim() || '';
    const inp   = row?.querySelector('.js-precio');
    const precio = normPrecio(inp?.value ?? '');

    if (!talla){ alert('Talla inválida.'); return; }
    // Si tu backend exige >= 1.00, mantenlo:
    if (precio == null || precio < 1){ alert('Precio inválido (use punto y 2 decimales, mínimo 1.00).'); inp?.focus(); return; }

    const token = document.querySelector('#js-anti input[name="__RequestVerificationToken"]')?.value || '';
    const pid   = Number(document.getElementById('js-id-producto')?.value || 0);
    if (!token || !pid){ alert('No se pudo obtener datos del formulario.'); return; }

    btn.disabled = true;
    const old = btn.textContent;
    try{
      const r = await fetch('/Productos/ActualizarPrecio', {
        method:'POST',
        headers:{ 'Content-Type':'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ idProducto: pid, idTalla: talla, precio }),
        credentials:'same-origin'
      });
      const data = await r.json().catch(()=>({ok:false,msg:'Error'}));
      if (!data?.ok){ alert(data?.msg || 'No se pudo actualizar.'); btn.disabled=false; return; }

      if (inp) inp.value = (+precio).toFixed(2);
      btn.textContent = 'OK';
      setTimeout(()=>{ btn.textContent = old; btn.disabled = false; }, 900);
    }catch(e){
      alert('Error de red: ' + (e?.message || e));
      btn.disabled = false;
    }
  });
}

    function wirePreciosPorTalla(modal) {
        const scope = modal || document;

        // Normalizador a NN[.DD] (punto, máx 2 dec) -> número >= 0 o null
        const normPrecio = (raw) => {
            if (raw == null) return null;
            let v = String(raw).trim().replace(/,/g, '.').replace(/[^0-9.]/g, '');
            const parts = v.split('.');
            if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
            const [ent, dec = ''] = v.split('.');
            v = (ent.replace(/^0+(\d)/, '$1') || (v.includes('.') ? '0' : '')) + (v.includes('.') ? '.' + dec.slice(0, 2) : '');
            if (v === '' || v === '.') return null;
            const n = Number(v);
            if (!Number.isFinite(n) || n < 0) return null;
            return Math.round(n * 100) / 100;
        };

        // Al abrir el modal: formatea visualmente a 2 decimales (0.00)
        scope.querySelectorAll('.js-precio').forEach(inp => {
            inp.value = (window.fmt2 ? window.fmt2(inp.value) : Number(inp.value || 0).toFixed(2));
        });

        // Teclado: solo dígitos, un punto y teclas de edición/navegación
        const ALLOW = new Set(['Backspace', 'Delete', 'Tab', 'Enter', 'Escape', 'Home', 'End', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', '.']);
        scope.addEventListener('keydown', (ev) => {
            const inp = ev.target;
            if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
            const k = ev.key;
            if (ALLOW.has(k)) { if (k === '.' && inp.value.includes('.')) ev.preventDefault(); return; }
            if (k >= '0' && k <= '9') return;
            ev.preventDefault();
        });

        // Paste/typing: normalizar en vivo
        const normalizeInput = (el) => {
            const before = el.value;
            let v = before.replace(/,/g, '.').replace(/[^0-9.]/g, '');
            const parts = v.split('.');
            if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
            const [ent, dec = ''] = v.split('.');
            v = (ent.replace(/^0+(\d)/, '$1') || (v.includes('.') ? '0' : '')) + (v.includes('.') ? '.' + dec.slice(0, 2) : '');
            if (before !== v) el.value = v;
        };
        scope.addEventListener('input', (ev) => {
            const inp = ev.target;
            if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
            normalizeInput(inp);
        });

        // Seleccionar todo al enfocar (un toque/click)
        scope.addEventListener('focusin', (ev) => {
            const el = ev.target;
            if (el instanceof HTMLInputElement && el.classList.contains('sel-all')) {
                setTimeout(() => { try { el.select(); } catch { } }, 0);
            }
        });
        scope.addEventListener('mouseup', (ev) => {
            if (ev.target?.classList?.contains('sel-all')) ev.preventDefault();
        }, true);

        // Blur: fija a 2 decimales
        scope.addEventListener('blur', (ev) => {
            const inp = ev.target;
            if (!(inp instanceof HTMLInputElement) || !inp.classList.contains('js-precio')) return;
            const n = normPrecio(inp.value);
            if (n != null) inp.value = n.toFixed(2);
        }, true);

        // Guardar (delegado en el modal)
        scope.addEventListener('click', async (ev) => {
            const btn = ev.target.closest?.('.js-save');
            if (!btn) return;

            const row = btn.closest('tr');
            const talla = row?.dataset.talla?.trim() || '';
            const inp = row?.querySelector('.js-precio');
            const precio = normPrecio(inp?.value ?? '');

            if (!talla) { alert('Talla inválida.'); return; }
            // Si tu backend exige >= 1.00, mantenlo:
            if (precio == null || precio < 1) { alert('Precio inválido (use punto y 2 decimales, mínimo 1.00).'); inp?.focus(); return; }

            const token = document.querySelector('#js-anti input[name="__RequestVerificationToken"]')?.value || '';
            const pid = Number(document.getElementById('js-id-producto')?.value || 0);
            if (!token || !pid) { alert('No se pudo obtener datos del formulario.'); return; }

            btn.disabled = true;
            const old = btn.textContent;
            try {
                const r = await fetch('/Productos/ActualizarPrecio', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                    body: JSON.stringify({ idProducto: pid, idTalla: talla, precio }),
                    credentials: 'same-origin'
                });
                const data = await r.json().catch(() => ({ ok: false, msg: 'Error' }));
                if (!data?.ok) { alert(data?.msg || 'No se pudo actualizar.'); btn.disabled = false; return; }

                if (inp) inp.value = (+precio).toFixed(2);
                btn.textContent = 'OK';
                setTimeout(() => { btn.textContent = old; btn.disabled = false; }, 900);
            } catch (e) {
                alert('Error de red: ' + (e?.message || e));
                btn.disabled = false;
            }
        });
    }

    (() => {
        const form = document.querySelector('form.search');
        const input = form?.querySelector('input[name="q"]');
        if (!form || !input) return;

        const body = document.getElementById('prod-body');
        const pager = document.getElementById('prod-pager');
        const meta = document.getElementById('prod-meta');

        async function load(url) {
            const html = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } }).then(r => r.text());
            const tmp = document.createElement('div');
            tmp.innerHTML = html;

            body.innerHTML = tmp.querySelector('#prod-body')?.innerHTML ?? body.innerHTML;
            pager.innerHTML = tmp.querySelector('#prod-pager')?.innerHTML ?? '';
            meta.innerHTML = tmp.querySelector('#prod-meta')?.innerHTML ?? meta.innerHTML;
        }

        // al teclear: debounce + AJAX
        let t = null;
        input.addEventListener('input', () => {
            clearTimeout(t);
            t = setTimeout(() => {
                const base = form.getAttribute('action') || location.pathname;
                const url = `${base}?q=${encodeURIComponent(input.value)}&page=1`;
                load(url);
            }, 300);
        });

        // paginación AJAX
        document.addEventListener('click', (e) => {
            const a = e.target.closest('#prod-pager a');
            if (!a) return;
            e.preventDefault();
            load(a.href);
        });
    })();


    /* =====================================================================
       BOOT
       ===================================================================== */
    document.addEventListener("DOMContentLoaded", () => {
        initSidebar();
        initTableUX();
        initModal();
        wireCajaCerrarInPage();  // por si la vista de cierre se abre como página
        initVentas();
    });
})();
