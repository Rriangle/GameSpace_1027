// 即時搜尋 + 手打 ID 檢查
(function () {
    const box = document.getElementById('reassign-filter');
    const sel = document.getElementById('reassign-select');
    const warn = document.getElementById('reassign-warning');
    const wt = document.getElementById('reassign-warning-text');
    const submit = document.getElementById('reassign-submit');
    if (!box || !sel) return;

    const all = Array.from(sel.options).map(o => ({
        value: o.value, text: o.text, selected: o.selected, disabled: o.disabled
    }));
    const validSet = new Set(all.filter(o => !o.disabled).map(o => o.value));
    const currentId = document.querySelector('span.badge')?.textContent?.trim() || '';

    function checkSelectedValidity() {
        const sel = document.getElementById('reassign-select');
        const warn = document.getElementById('reassign-warning');
        const wt = document.getElementById('reassign-warning-text');

        const currentId = document.querySelector('[data-current-id]')?.getAttribute('data-current-id')
            || document.querySelector('.meta-card .badge')?.textContent?.trim()
            || '';

        const submit = document.getElementById('reassign-submit');
        if (!sel) return;

        const opt = sel.selectedOptions && sel.selectedOptions[0];
        if (!opt) { warn.classList.remove('show'); submit?.removeAttribute('disabled'); return; }

        if (opt.disabled) {
            wt.textContent = '此對象不可指派。';
            warn.classList.add('show'); submit?.setAttribute('disabled', 'disabled'); return;
        }
        if (currentId && opt.value === currentId) {
            wt.textContent = '選擇的對象與目前指派相同。';
            warn.classList.add('show'); submit?.setAttribute('disabled', 'disabled'); return;
        }
        warn.classList.remove('show'); submit?.removeAttribute('disabled');
    }

    window.checkSelectedValidity = checkSelectedValidity;

    function apply() {
        const q = (box.value || '').trim();
        sel.innerHTML = '';
        const items = q ? all.filter(o =>
            o.value.toLowerCase().includes(q.toLowerCase()) ||
            o.text.toLowerCase().includes(q.toLowerCase())
        ) : all;

        for (const it of items) {
            const opt = document.createElement('option');
            opt.value = it.value; opt.textContent = it.text;
            if (it.disabled) opt.disabled = true;
            if (it.selected) opt.selected = true;
            sel.appendChild(opt);
        }

        const justDigits = /^\d+$/.test(q);
        if (q && justDigits && !validSet.has(q)) {
            wt.textContent = '輸入的 ID 不在可指派名單（未具客服權限或不存在）。';
            warn.classList.add('show'); submit?.setAttribute('disabled', 'disabled');
        } else {
            if (validSet.has(q)) {
                const opt = Array.from(sel.options).find(o => o.value === q);
                if (opt) opt.selected = true;
            }
            warn.classList.remove('show'); submit?.removeAttribute('disabled');
        }

        checkSelectedValidity();
    }

    let t = null;
    box.addEventListener('input', () => { if (t) clearTimeout(t); t = setTimeout(apply, 100); });
    box.addEventListener('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); apply(); } });
    sel.addEventListener('change', checkSelectedValidity);
    apply();
})();

// 備註字數
(function () {
    const note = document.getElementById('reassign-note');
    const cnt = document.getElementById('reassign-note-count');
    if (!note || !cnt) return;
    const upd = () => cnt.textContent = (note.value || '').length;
    note.addEventListener('input', upd); upd();
})();

// 取消：如果沒用 data-parent-close，也保一手
document.getElementById('btn-cancel')?.addEventListener('click', () => {
    window.parent?.postMessage({ type: 'reassign-cancelled' }, '*');
});
window.addEventListener('keydown', e => {
    if (e.key === 'Escape') window.parent?.postMessage({ type: 'reassign-cancelled' }, '*');
});

// AJAX 送出
(function () {
    const form = document.getElementById('reassignForm');
    const btn = document.getElementById('reassign-submit');
    if (!form || !btn) return;

    form.addEventListener('submit', async e => {
        e.preventDefault();

        if (typeof window.checkSelectedValidity === 'function') {
            window.checkSelectedValidity();
            if (document.getElementById('reassign-warning')?.classList.contains('show')) return;
        }

        btn.disabled = true; btn.classList.add('disabled');
        try {
            const r = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (r.ok) {
                window.parent?.postMessage({ type: 'reassign-done' }, '*');
            } else {
                alert((await r.text()) || '轉單失敗'); btn.disabled = false; btn.classList.remove('disabled');
            }
        } catch {
            alert('連線失敗，請稍後再試'); btn.disabled = false; btn.classList.remove('disabled');
        }
    });
})();
