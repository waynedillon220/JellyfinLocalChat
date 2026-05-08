(function () {
    if (document.getElementById("jf-chat-bubble")) return;

    // 💬 Chat bubble
    const bubble = document.createElement("div");
    bubble.id = "jf-chat-bubble";
    bubble.innerText = "💬";

    Object.assign(bubble.style, {
        position: "fixed",
        bottom: "20px",
        right: "20px",
        width: "50px",
        height: "50px",
        background: "#222",
        color: "white",
        borderRadius: "50%",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        cursor: "pointer",
        zIndex: 9999,
        border: "2px solid #0A7EA4"
    });

    // 🪟 Chat window
    const chat = document.createElement("div");
    chat.id = "jf-chat-window";

    Object.assign(chat.style, {
        position: "fixed",
        bottom: "80px",
        right: "20px",
        width: "300px",
        height: "400px",
        background: "#111",
        color: "white",
        display: "none",
        flexDirection: "column",
        zIndex: 9999,
        borderRadius: "10px",
        overflow: "hidden",
        boxShadow: "0 0 10px rgba(10,126,164,0.5)"
    });

    chat.innerHTML = `
        <div style="padding:10px;background:#1a1a1a;border-bottom:1px solid #0A7EA4;">Local Chat</div>
        <div id="jf-chat-messages" style="flex:1;overflow:auto;padding:10px;"></div>
        <input id="jf-chat-input" placeholder="Type message..." style="border:none;border-top:1px solid #333;padding:10px;width:calc(100%-20px);background:#222;color:#fff;" />
    `;

    document.body.appendChild(bubble);
    document.body.appendChild(chat);

    bubble.onclick = () => {
        chat.style.display = chat.style.display === "none" ? "flex" : "none";
    };

    // Get user (from Jellyfin window object or use default)
    const getUsername = () => {
        if (window.ApiClient && window.ApiClient.userId) {
            return "User";
        }
        return "Anonymous";
    };

    // Load and display messages from localStorage
    const loadMessages = () => {
        const messages = JSON.parse(localStorage.getItem("jellyfinChatMessages") || "[]");
        const msgContainer = document.getElementById("jf-chat-messages");
        msgContainer.innerHTML = "";
        messages.forEach(msg => {
            const el = document.createElement("div");
            el.style.marginBottom = "5px";
            el.style.padding = "5px";
            el.style.backgroundColor = "#222";
            el.style.borderRadius = "5px";
            el.innerHTML = `<b style="color:#0A7EA4;">${msg.user}:</b> <span style="color:#fff;">${msg.text}</span>`;
            msgContainer.appendChild(el);
        });
        msgContainer.scrollTop = msgContainer.scrollHeight;
    };

    // Save message to localStorage
    const saveMessage = (user, text) => {
        const messages = JSON.parse(localStorage.getItem("jellyfinChatMessages") || "[]");
        messages.push({
            user: user,
            text: text,
            timestamp: new Date().toISOString()
        });
        // Keep only last 100 messages
        if (messages.length > 100) {
            messages.shift();
        }
        localStorage.setItem("jellyfinChatMessages", JSON.stringify(messages));
    };

    // Load initial messages
    loadMessages();

    // Handle input
    document.getElementById("jf-chat-input").addEventListener("keydown", (e) => {
        if (e.key === "Enter" && e.target.value.trim()) {
            const user = getUsername();
            const text = e.target.value.trim();
            saveMessage(user, text);
            loadMessages();
            e.target.value = "";
        }
    });

    // Refresh messages when storage changes (from other tabs)
    window.addEventListener("storage", (e) => {
        if (e.key === "jellyfinChatMessages") {
            loadMessages();
        }
    });

    // Hide during playback UI fade
    const observer = new MutationObserver(() => {
        const videoControls = document.querySelector(".videoOsdBottom");
        if (videoControls && videoControls.style.opacity === "0") {
            bubble.style.opacity = "0.3";
            chat.style.opacity = "0";
            chat.style.pointerEvents = "none";
        } else {
            bubble.style.opacity = "1";
            chat.style.opacity = "1";
            chat.style.pointerEvents = "auto";
        }
    });

    observer.observe(document.body, {
        attributes: true,
        subtree: true
    });

})();