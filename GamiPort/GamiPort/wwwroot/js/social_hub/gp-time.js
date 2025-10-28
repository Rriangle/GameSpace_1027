/* ============================================================
   gp-time.js － 穩定版（SQL/ISO 都吃，強制轉台灣時間）
   - ensureIsoUtc(iso): 把各種字串正規化成 UTC ISO
   - hm(iso):         以「數學 +08:00」回傳 HH:mm
   - dateKey(iso):    以「數學 +08:00」回傳 yyyy-MM-dd
   - prettyKey(key):  今天/昨天/或 yyyy/MM/dd（以台灣時區判斷）
   ============================================================ */
(function (w) {
    'use strict';

    // ---- 正規化：把 DB/ISO 風格全部轉為「YYYY-MM-DDTHH:mm:ss.sssZ」（UTC）
    function normalizeToUtcIso(input) {
        if (!input) return new Date().toISOString();              // now(UTC)
        var s = String(input).trim();

        // 1) 把空白換成 'T'（SQL Server 常見）
        if (/^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}/.test(s)) {
            s = s.replace(' ', 'T');
        }

        // 2) 去掉多於 3 位的小數秒
        //    例如 ".4054304" -> ".405"
        s = s.replace(/(\.\d{1,3})\d+/, '$1');

        // 3) 若缺少時區（沒有 Z 或 ±HH:MM）→ 視為 UTC，補 'Z'
        if (!/[zZ]|[+\-]\d{2}:\d{2}$/.test(s)) s += 'Z';

        // 4) 若還是解析失敗，就退回現在時間（避免 NaN）
        var d = new Date(s);
        if (isNaN(d.getTime())) return new Date().toISOString();
        return d.toISOString(); // 標準化
    }

    // ---- 將 UTC ISO 轉為「台灣時間」的 Date（不用 Intl，固定 +08:00）
    function utcIsoToTpeDate(isoUtc) {
        var d = new Date(isoUtc);                // UTC
        var t = new Date(d.getTime() + 8 * 3600 * 1000); // +08:00
        return t; // 注意：這個 t 的「UTC 欄位」就是台灣當地時間
    }

    function ensureIsoUtc(iso) { return normalizeToUtcIso(iso); }

    // ---- 格式化：HH:mm（台灣）
    function hm(iso) {
        var t = utcIsoToTpeDate(ensureIsoUtc(iso));
        var hh = String(t.getUTCHours()).padStart(2, '0');     // 用 UTC 欄位當地時間
        var mm = String(t.getUTCMinutes()).padStart(2, '0');
        return hh + ':' + mm;
    }

    // ---- 格式化：yyyy-MM-dd（台灣）
    function dateKey(iso) {
        var t = utcIsoToTpeDate(ensureIsoUtc(iso));
        var y = t.getUTCFullYear();
        var m = String(t.getUTCMonth() + 1).padStart(2, '0');
        var dd = String(t.getUTCDate()).padStart(2, '0');
        return y + '-' + m + '-' + dd;
    }

    // ---- 「今天/昨天/或 yyyy/MM/dd」（以台灣時區判斷）
    function prettyKey(key) {
        var todayK = dateKey(new Date().toISOString());
        var yK = dateKey(new Date(Date.now() - 86400000).toISOString());
        if (key === todayK) return '今天';
        if (key === yK) return '昨天';
        return key.replace(/-/g, '/');
    }

    // 保留 parseUtc 以相容舊呼叫（仍會回標準 Date 物件）
    function parseUtc(iso) { return new Date(ensureIsoUtc(iso)); }

    w.GPTime = { ensureIsoUtc, parseUtc, hm, dateKey, prettyKey };
})(window);
