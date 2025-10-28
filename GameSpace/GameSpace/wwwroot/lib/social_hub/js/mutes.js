(function () {
    document.addEventListener('DOMContentLoaded', function () {

        // ==== 跨頁搜尋：輸入後 300ms 自動送出（伺服器端搜尋），並重設到第 1 頁 ====
        var form = document.getElementById('mutes-search-form'); // ← Index.cshtml 的 GET 表單 id
        var input = document.getElementById('mutes-search');      // ← 搜尋輸入框 id
        if (input) {
            var timer = null;

            function submitWithPageReset() {
                var q = (input.value || '').trim();

                if (form) {
                    // 確保有 page=1 的 hidden 欄位
                    var pg = form.querySelector('input[name="page"]');
                    if (!pg) {
                        pg = document.createElement('input');
                        pg.type = 'hidden';
                        pg.name = 'page';
                        form.appendChild(pg);
                    }
                    pg.value = '1';

                    // 空字串時讓 q 也送空（Controller 會視為不搜尋）
                    // 若你想「清空時改用超連結」，可在 View 用「清除」按鈕處理
                    if (form.requestSubmit) form.requestSubmit();
                    else form.submit();
                } else {
                    // 沒有表單也能運作：用 URL 參數直接導向
                    var params = new URLSearchParams(window.location.search);
                    if (q === '') params.delete('q'); else params.set('q', q);
                    params.set('page', '1');
                    var url = window.location.pathname + '?' + params.toString();
                    window.location.assign(url);
                }
            }

            // Enter 立即送出
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    submitWithPageReset();
                }
            });

            // 停 300ms 自動送出
            input.addEventListener('input', function () {
                clearTimeout(timer);
                timer = setTimeout(submitWithPageReset, 300);
            });
        }

        // ==== 刪除確認 ====
        document.querySelectorAll('form.js-delete-form').forEach(function (f) {
            f.addEventListener('submit', function (e) {
                if (!confirm('確定要刪除這個禁詞嗎？')) e.preventDefault();
            });
        });

        // ==== 防重複送出（Create/Edit） ====
        document.querySelectorAll('form.js-prevent-double-submit').forEach(function (form) {
            var submitting = false;
            form.addEventListener('submit', function (e) {
                if (submitting) { e.preventDefault(); return; }
                submitting = true;
                var btn = form.querySelector('button[type=submit]');
                if (btn) { btn.disabled = true; btn.innerText = '處理中…'; }
            });
        });

        // ==== 點擊複製 ====
        document.querySelectorAll('[data-copy]').forEach(function (el) {
            el.classList.add('copyable');
            el.addEventListener('click', async function () {
                try { await navigator.clipboard.writeText(el.getAttribute('data-copy')); } catch (e) { }
            });
        });

        // ==== Bootstrap tooltip（若有載入） ====
        if (window.bootstrap) {
            document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
                new bootstrap.Tooltip(el);
            });
        }
    });
})();
