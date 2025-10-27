/* -------------------------------------------------------------------
   chat-date-divider.js (safe + robust)
   功能：在聊天訊息串中自動插入「日期分隔線」（今天/昨天/yyyy/MM/dd）

   整合方式：
     ChatDivider.init({ container: '#cdBody', renderer: renderFn });
     ChatDivider.appendMessages(list, renderFn);  // 批次（舊→新）
     ChatDivider.appendMessage(item, renderFn);   // 單筆（新訊）
     ChatDivider.prependMessages(list, renderFn); // 前插（舊→新）

   說明：
     - renderer(item) 必須回傳 1 個 DOM Element（你現有的 createRow 結果）
     - 若專案提供 gp-time.js，會優先使用 GPTime.dateKey / GPTime.prettyKey 以台灣時區計算
--------------------------------------------------------------------*/
(function (global) {
    'use strict';
    var win = global || window;

    // ---- UTC ISO 工具：沒有時區 → 加 'Z' 視為 UTC -----------------------------
    function hasTz(s) { return /[zZ]|[+\-]\d{2}:\d{2}$/.test(s || ''); }
    function ensureIsoUtc(i) { return (i && hasTz(i)) ? i : (i ? (i + 'Z') : new Date().toISOString()); }

    // 從訊息物件抽一個「最合理的時間欄位」
    function pickIsoField(m) {
        // 常見欄位依優先序排列（snake_case / camelCase / PascalCase 全包）
        var cands = [
            'SentAtIso', 'sentAtIso', 'sent_at_iso',
            'SentAt', 'sentAt', 'sent_at',
            'EditedAt', 'editedAt', 'edited_at',
            'CreatedAt', 'createdAt', 'created_at',
            'ReadAt', 'readAt', 'read_at',
            'Time', 'time', 'timestamp', 'ts'
        ];
        for (var i = 0; i < cands.length; i++) {
            var k = cands[i];
            if (m && m[k] != null && String(m[k]).length > 0) return String(m[k]);
        }
        return null;
    }

    // ---- 日期鍵與顯示文字（優先用 GPTime，否則 fallback） ---------------------
    var DateKey = {
        key: function (iso) {
            if (win.GPTime && typeof win.GPTime.dateKey === 'function') return win.GPTime.dateKey(iso);
            // fallback：以本地 Date 解析（iso 已補 Z）
            var d = new Date(ensureIsoUtc(iso));
            var y = d.getFullYear();
            var m = ('0' + (d.getMonth() + 1)).slice(-2);
            var day = ('0' + d.getDate()).slice(-2);
            return y + '-' + m + '-' + day;   // yyyy-MM-dd
        },
        pretty: function (key) {
            if (win.GPTime && typeof win.GPTime.prettyKey === 'function') return win.GPTime.prettyKey(key);
            // fallback：今天/昨天 判斷（以本地時間）
            var now = new Date();
            function k(d) {
                var y = d.getFullYear(), m = ('0' + (d.getMonth() + 1)).slice(-2), day = ('0' + d.getDate()).slice(-2);
                return y + '-' + m + '-' + day;
            }
            var today = k(now);
            var yes = new Date(now); yes.setDate(now.getDate() - 1);
            var yesterday = k(yes);
            if (key === today) return '今天';
            if (key === yesterday) return '昨天';
            return key.replace(/-/g, '/'); // yyyy/MM/dd
        }
    };

    // ---- DOM 小工具 -----------------------------------------------------------
    function $(selOrEl) { return (typeof selOrEl === 'string') ? document.querySelector(selOrEl) : selOrEl; }

    function createDivider(key) {
        var div = document.createElement('div');
        div.className = 'chat-divider';
        div.setAttribute('data-date-key', key);
        var span = document.createElement('span');
        span.textContent = DateKey.pretty(key);
        div.appendChild(span);
        return div;
    }

    function getLastDateKey(container) {
        for (var i = container.children.length - 1; i >= 0; i--) {
            var el = container.children[i];
            if (el && el.dataset && el.dataset.dateKey) return el.dataset.dateKey;
        }
        return null;
    }

    function getFirstDateKey(container) {
        for (var i = 0; i < container.children.length; i++) {
            var el = container.children[i];
            if (el && el.dataset && el.dataset.dateKey) return el.dataset.dateKey;
        }
        return null;
    }

    function ensureAppendDivider(container, nextKey) {
        var tailKey = getLastDateKey(container);
        if (tailKey !== nextKey) {
            container.appendChild(createDivider(nextKey));
        }
    }

    // ---- 主物件：ChatDivider --------------------------------------------------
    var ChatDivider = {
        _container: null,
        _renderer: null,

        init: function (opts) {
            opts = opts || {};
            this._container = $(opts.container);
            if (!this._container) throw new Error('[ChatDivider] container not found');
            this._container.classList.add('chat-log'); // 語義性 class，不影響你的排版
            this._renderer = opts.renderer || this._defaultRenderer;
            return this;
        },

        setRenderer: function (renderer) {
            this._renderer = renderer || this._defaultRenderer;
        },

        // 批次追加（歷史：舊→新）
        appendMessages: function (items, renderer) {
            if (!items || !items.length) return;
            var render = renderer || this._renderer;
            for (var i = 0; i < items.length; i++) {
                var m = items[i];
                var iso = pickIsoField(m);
                var key = DateKey.key(iso);
                ensureAppendDivider(this._container, key);     // 新日期 → 先插分隔線
                var node = render(m);
                if (node && node.nodeType === 1) {
                    node.setAttribute('data-date-key', key);
                    this._container.appendChild(node);
                }
            }
        },

        // 單筆（新訊息）
        appendMessage: function (m, renderer) {
            var render = renderer || this._renderer;
            var iso = pickIsoField(m);
            var key = DateKey.key(iso);
            ensureAppendDivider(this._container, key);
            var node = render(m);
            if (node && node.nodeType === 1) {
                node.setAttribute('data-date-key', key);
                this._container.appendChild(node);
            }
        },

        // 前插（上捲載舊：舊→新）
        prependMessages: function (items, renderer) {
            if (!items || !items.length) return;
            var render = renderer || this._renderer;
            var batch = document.createDocumentFragment();
            var prevKey = null;

            for (var i = 0; i < items.length; i++) {
                var m = items[i];
                var iso = pickIsoField(m);
                var key = DateKey.key(iso);
                if (prevKey !== key) { batch.appendChild(createDivider(key)); prevKey = key; }
                var node = render(m);
                if (node && node.nodeType === 1) {
                    node.setAttribute('data-date-key', key);
                    batch.appendChild(node);
                }
            }

            // 交界處去重（新批最後日期 == 既有最前日期）
            var firstExistingKey = getFirstDateKey(this._container);
            var lastNewKey = prevKey;
            if (firstExistingKey && lastNewKey === firstExistingKey) {
                for (var j = batch.childNodes.length - 1; j >= 0; j--) {
                    var el = batch.childNodes[j];
                    if (el && el.nodeType === 1 && el.classList && el.classList.contains('chat-divider')) {
                        batch.removeChild(el); break;
                    }
                    if (el && el.nodeType === 1) break;
                }
            }

            this._container.insertBefore(batch, this._container.firstChild);
        },

        _defaultRenderer: function (m) {
            var wrap = document.createElement('div');
            wrap.textContent = m.Content || m.content || '';
            return wrap;
        }
    };

    win.ChatDivider = ChatDivider;
})(window);
