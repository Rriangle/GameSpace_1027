/* [Chat 1-to-1 client for GameSpace social_hub]
   [雙模] 有 window.signalR → 即時；否則 → HTTP 輪詢
   [HTML 結構需求]
     - <div id="chatBox">                                [訊息容器]
       (server 可預先渲染 #chatBox > .message.mine|.other[data-at|data-time-iso])
     - <form id="chatSendForm">                          [送訊息表單]
         ├─ <input id="messageInput" ...>               [輸入框]
         ├─ <input type="hidden" id="otherId" name="otherId" value="對方ID">
         ├─ <input type="hidden" id="isManagerDm" name="isManagerDm" value="true|false">
         └─ <input name="__RequestVerificationToken" ...>  [Anti-Forgery]
   [可選] <meta name="me-id"> / <meta name="me-name">
*/
(function () {
    if (window.__gsChatLoaded1to1) return;   // ← 已載入就直接退出
    window.__gsChatLoaded1to1 = true;
    // ... 原本內容從這裡開始


    (function () {
        // ================== 設定 ==================
        const CONFIG = {
            hubUrl: "/social_hub/chatHub",            // SignalR Hub
            sendUrl: "/social_hub/Chat/Send",         // POST 送訊息
            markReadUrl: "/social_hub/Chat/MarkRead", // POST 標記已讀
            historyUrl: "/social_hub/Chat/History",   // GET 增量
            pollMs: 2500                              // 輪詢間隔（ms）
        };

        // ================== 工具 ==================
        const $ = (sel) => document.querySelector(sel);
        const esc = (s) =>
            String(s ?? "").replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
        const isNearBottom = (el, th = 36) => (el.scrollTop + el.clientHeight) >= (el.scrollHeight - th);
        const hhmm = (iso) => {
            try {
                const d = new Date(iso);
                if (isNaN(d.getTime())) return "";
                return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
            } catch { return ""; }
        };

        // ================== DOM 取得 ==================
        const chatBox = $("#chatBox");
        const form = $("#chatSendForm");
        const input = $("#messageInput");
        const otherId = parseInt($("#otherId")?.value || "0", 10) || 0;
        const isManagerDm = (String($("#isManagerDm")?.value || "false").toLowerCase() === "true");
        const csrf = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

        if (!chatBox || !form || !input || !otherId || !csrf) {
            console.warn("[chat-1to1] 缺少必要 DOM 或 Anti-Forgery/otherId。腳本不啟動。");
            return;
        }

        // ================== 身分（可選） ==================
        const meId = parseInt(document.querySelector('meta[name="me-id"]')?.content || "0", 10) || 0;
        const meName = document.querySelector('meta[name="me-name"]')?.content || "";

        // ================== 狀態 ==================
        let lastIso = null;        // 目前已載入的最後時間（ISO）
        let lastTs = 0;            // 對應時間戳（ms），避免字串比較
        let conn = null;           // SignalR 連線
        let pollTimer = 0;         // 輪詢計時器
        const seenIds = new Set(); // 去重（僅對有 id 的訊息）
        const hasSignalR = !!window.signalR;

        // ================== DOM：建立泡泡 ==================
        function buildBubble({ mine, text, iso, isRead, id }) {
            const wrap = document.createElement("div");
            wrap.className = `message ${mine ? "mine" : "other"}`;
            if (iso) wrap.setAttribute("data-at", iso);
            if (id) wrap.setAttribute("data-id", String(id));

            const content = document.createElement("div");
            content.className = "content";
            content.textContent = text ?? "";

            const meta = document.createElement("div");
            meta.className = "meta";

            const timeSpan = document.createElement("span");
            timeSpan.className = "time";
            timeSpan.textContent = hhmm(iso);

            meta.appendChild(timeSpan);

            if (mine) {
                const readSpan = document.createElement("span");
                readSpan.className = `readmark ${isRead ? "on" : ""}`;
                // 用 ✓ 與 CSS 著色；也加上 data-read 供 JS 快速選取
                readSpan.setAttribute("data-read", "badge");
                readSpan.textContent = "✓";
                meta.appendChild(document.createTextNode(" "));
                meta.appendChild(readSpan);
            }

            wrap.appendChild(content);
            wrap.appendChild(meta);
            return wrap;
        }

        function appendMessage({ id, mine, text, iso, isRead }) {
            if (id) {
                // 先查 DOM，再查記憶集合，任何一邊命中都不重插
                if (chatBox.querySelector(`.message[data-id="${id}"]`)) return;
                if (seenIds.has(id)) return;
                seenIds.add(id);
            }
            // ...後面維持原邏輯（wasBottom、buildBubble、markRead、更新游標）

            const wasBottom = isNearBottom(chatBox);
            chatBox.appendChild(buildBubble({ id, mine, text: esc(text), iso, isRead }));

            if (wasBottom) chatBox.scrollTop = chatBox.scrollHeight;

            // 對方訊息顯示後 → 標記已讀（到這一則）
            if (!mine && iso) {
                const upTo = normalizeIso(iso);
                if (upTo) markRead(upTo);
            }

            // 更新游標
            const t = Date.parse(iso || "");
            if (!isNaN(t) && t > lastTs) {
                lastTs = t;
                lastIso = new Date(t).toISOString();
            }
        }

        // ================== 初始化：吃預先渲染 ==================
        (function initFromServerRendered() {
            // 先把所有已渲染的 data-id 收進去重集合
            chatBox.querySelectorAll(".message[data-id]").forEach(el => {
                const raw = el.getAttribute("data-id");
                const id = raw ? parseInt(raw, 10) : 0;
                if (id) seenIds.add(id);
            });

            // 支援 data-at 或 data-time-iso，找出最大的當游標
            const nodes = chatBox.querySelectorAll(".message[data-at], .message[data-time-iso]");
            if (!nodes.length) return;

            const last = Array.from(nodes).reduce((acc, el) => {
                const iso = el.getAttribute("data-at") || el.getAttribute("data-time-iso") || "";
                const t = Date.parse(iso || "");
                return (!isNaN(t) && t > acc.t) ? { iso, t } : acc;
            }, { iso: null, t: 0 });

            if (last.t > 0) {
                lastTs = last.t;
                lastIso = new Date(last.t).toISOString();
            }
        })();


        // ================== ISO 正規化 ==================
        function normalizeIso(iso) {
            try {
                const d = new Date(iso);
                return isNaN(d.getTime()) ? null : d.toISOString();
            } catch { return null; }
        }

        // ================== HTTP：抓增量 ==================
        async function fetchDelta() {
            try {
                const url = new URL(CONFIG.historyUrl, window.location.origin);
                url.searchParams.set("otherId", String(otherId));
                url.searchParams.set("isManagerDm", String(isManagerDm));
                if (lastIso) url.searchParams.set("after", lastIso);

                const r = await fetch(url, { method: "GET", headers: { "Accept": "application/json" }, credentials: "same-origin" });
                if (!r.ok) return;
                const arr = await r.json();
                if (!Array.isArray(arr)) return;

                for (const m of arr) {
                    // 支援 VM 屬性大小寫不同
                    const id = m.messageId ?? m.MessageId ?? 0;
                    const mine = (m.isMine ?? m.IsMine) === true;
                    const text = m.content ?? m.text ?? m.Content ?? m.Text ?? "";
                    const iso = m.time ?? m.at ?? m.Time ?? m.At ?? null;
                    const isRead = (m.isRead ?? m.IsRead) === true;

                    appendMessage({ id, mine, text, iso, isRead });
                }
            } catch (_) { /* ignore */ }
        }

        // ================== HTTP：標記已讀 ==================
        async function markRead(upToIso) {
            if (!upToIso) return;
            try {
                await fetch(CONFIG.markReadUrl, {
                    method: "POST",
                    credentials: "same-origin",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "RequestVerificationToken": csrf
                    },
                    body: new URLSearchParams({
                        otherId: String(otherId),
                        isManagerDm: String(isManagerDm),
                        upToIso
                    })
                });
            } catch (_) { /* ignore */ }

            // 即時模式可回報已讀給對方（若 Hub 有實作）
            if (conn && conn.invoke) {
                try { await conn.invoke("NotifyRead", otherId, upToIso); } catch (_) { /* ignore */ }
            }
        }

        // ================== 送出 ==================
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            const txt = (input.value || "").trim();
            if (!txt) return;

            // 優先走 Hub
            if (conn && conn.invoke) {
                try {
                    await conn.invoke("SendMessageTo", otherId, txt);
                    input.value = "";
                    return; // Hub 會觸發 ReceiveDirect 再插入
                } catch (err) {
                    console.warn("[Hub 送出失敗，改用 HTTP]", err);
                }
            }

            // 回退 HTTP
            try {
                const fd = new FormData(form); // 內含 Anti-Forgery
                const r = await fetch(CONFIG.sendUrl, { method: "POST", body: fd, credentials: "same-origin" });
                if (r.ok) {
                    input.value = "";
                    // 後端回傳 { messageId, at, mine, text }（你控制器就是這格式）
                    const res = await r.json().catch(() => ({}));
                    appendMessage({
                        id: res.messageId ?? 0,
                        mine: true,
                        text: txt,                         // 以送出內容為準
                        iso: res.at ?? new Date().toISOString(),
                        isRead: false
                    });
                }
            } catch (err) {
                console.error("[HTTP 送出失敗]", err);
            }
        });

        // ================== 即時模式 ==================
        function setupSignalR() {
            if (!hasSignalR) return false;

            conn = new signalR.HubConnectionBuilder()
                .withUrl(CONFIG.hubUrl)
                .withAutomaticReconnect()
                .build();

            // 新訊息（Hub 需傳：messageId, senderId, receiverId, content, sentAtIso）
            conn.on("ReceiveDirect", (p) => {
                if (!p) return;
                const from = p.senderId ?? 0;
                const to = p.receiverId ?? 0;

                // 嚴格配對：需要 (meId, otherId) 成對；若 meId 不明，退而求其次
                const isThisConversation = meId > 0
                    ? ((from === otherId && to === meId) || (from === meId && to === otherId))
                    : (from === otherId || to === otherId);

                if (!isThisConversation) return;

                const mine = meId > 0 ? (from === meId) : (from !== otherId);
                appendMessage({
                    id: p.messageId ?? 0,
                    mine,
                    text: p.content ?? "",
                    iso: p.sentAtIso ?? null,
                    isRead: mine ? false : true
                });
            });

            // 已讀回執（Hub 需傳：fromUserId, upToIso）
            conn.on("ReadReceipt", (info) => {
                if (!info || info.fromUserId !== otherId) return;
                const upTo = info.upToIso ? new Date(info.upToIso) : null;
                if (!upTo) return;

                // 把我方 <= upTo 的 readmark 全部打開
                chatBox.querySelectorAll(".message.mine").forEach((msg) => {
                    const iso = msg.getAttribute("data-at") || msg.getAttribute("data-time-iso") || "";
                    const t = Date.parse(iso || "");
                    if (!isNaN(t) && t <= +upTo) {
                        const badge = msg.querySelector('[data-read="badge"], .readmark');
                        if (badge) badge.classList.add("on");
                    }
                });
            });

            conn.on("Error", (code) => {
                if (code === "NOT_LOGGED_IN") alert("請先登入後再使用聊天功能。");
                else if (code === "NO_PEER") alert("尚未選擇對象。");
                else if (code === "RATE_LIMIT") alert("發送過快，請稍後再試。");
                else console.warn("[Hub Error]", code);
            });

            conn.start().then(async () => {
                if (meName) { try { await conn.invoke("RegisterUser", meName); } catch (_) { } }
                await fetchDelta(); // 初次補漏
            }).catch((err) => {
                console.error("[SignalR 連線失敗，退回輪詢]", err);
                pollTimer = window.setInterval(fetchDelta, CONFIG.pollMs);
            });

            // 頁面回到可見 → 標記已讀
            document.addEventListener("visibilitychange", () => {
                if (document.visibilityState === "visible" && lastIso) markRead(lastIso);
            });

            return true;
        }

        // ================== 啟動流程 ==================
        (async () => {
            // 先抓一次增量，避免空白
            await fetchDelta();

            // 即時 or 輪詢
            const ok = setupSignalR();
            if (!ok) pollTimer = window.setInterval(fetchDelta, CONFIG.pollMs);
        })();
    })();
