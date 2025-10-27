/* ===================================================================
    前端腳本說明
    1) 監聽整頁 click：凡是點擊到 [data-notify] 的按鈕都會觸發送出
    2) 從按鈕的 dataset 讀 data-* 值 → 組 payload（自動把數字字串轉成 Number）
    3) 檢查至少有一個收件人（toUserId 或 toManagerId）
    4) fetch POST：
         - headers:
             - Content-Type: application/json
             - Accept: application/json（伺服器回 HTML 時可快速分辨）
             - RequestVerificationToken: <token>（若控制器有 [ValidateAntiForgeryToken]）
         - credentials: 'same-origin'（帶上同站 Cookie）
    5) 回應處理：
         - 非 2xx → alert 前 200 字（快速辨識錯誤頁/登入頁等 HTML）
         - Content-Type 非 JSON → alert 提示（多半是被導轉或打到 View）
         - JSON → 解析後讀 data.notificationId（控制器以小駝峰回傳）
    =================================================================== */
(function () {
    const $ = (sel) => document.querySelector(sel);

    // 將「字串 / 空字串 / 數字字串」轉成合適型別（空值→null；可數字→Number）
    function toMaybeNumber(v) {
        if (v == null || v === "") return null;
        return isFinite(v) ? Number(v) : v;
    }

    // 從 dataset 建立送給 API 的 payload
    function buildPayload(ds) {
        // data-aaa-bbb → ds.aaaBbb
        const p = {
            sourceId: toMaybeNumber(ds.sourceId),
            actionId: toMaybeNumber(ds.actionId),
            toUserId: toMaybeNumber(ds.toUserId),
            toManagerId: toMaybeNumber(ds.toManagerId),
            groupId: toMaybeNumber(ds.groupId),
            senderUserId: toMaybeNumber(ds.senderUserId),
            senderManagerId: toMaybeNumber(ds.senderManagerId),
            title: ds.title ?? "",
            message: ds.message ?? null
        };
        // 移除 null/空字串鍵，避免把空值送到後端
        Object.keys(p).forEach(k => { if (p[k] === null || p[k] === "") delete p[k]; });
        return p;
    }

    // 事件代理：點到任何 [data-notify] 按鈕就送
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-notify]');
        if (!btn) return;

        const url = btn.dataset.url;           // 後端 API 位址（建議用 Url.Action 產出）
        const body = buildPayload(btn.dataset); // 組 payload JSON

        // 最小前端驗證：至少要有一個收件人
        if (!('toUserId' in body) && !('toManagerId' in body)) {
            alert('請在按鈕上至少設定一個收件人（data-to-user-id 或 data-to-manager-id）。');
            return;
        }

        $('#status').textContent = '送出中…';

        // 讀 Anti-Forgery Token（若控制器未使用 [ValidateAntiForgeryToken]，可略）
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : null;

        // 組 headers（小駝峰 Accept：請求伺服器回 JSON）
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        // 若後端有啟用 Anti-Forgery，就把 token 放在自訂 Header
        if (token) headers['RequestVerificationToken'] = token;

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'same-origin',
                body: JSON.stringify(body)
            });

            // 先以純文字拿回（方便在錯誤時印出 HTML 片段）
            const contentType = res.headers.get('content-type') || '';
            const text = await res.text();

            if (!res.ok) {
                // 例如 400/404/405/500，直接把前 200 字顯示方便排錯
                alert(`失敗 (HTTP ${res.status})：${text.slice(0, 200)}`);
                $('#status').textContent = '';
                return;
            }

            if (!contentType.includes('application/json')) {
                // 多半是被 302 導去登入頁或打到 View（HTML）
                alert('伺服器回應不是 JSON（可能被轉址到登入頁或打錯路由）。');
                $('#status').textContent = '';
                return;
            }

            // 解析 JSON；控制器回小駝峰：{ "notificationId": 123 }
            const data = JSON.parse(text);
            const id = data.notificationId; // 若你之後改回大駝峰，這裡要改成 data.NotificationId

            alert(`OK！NotificationId=${id}`);
            $('#status').textContent = '完成';
        } catch (err) {
            alert('例外：' + err.message);
            $('#status').textContent = '';
        }
    });
})();