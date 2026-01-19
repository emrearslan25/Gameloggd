// Horizontal scroll for games
function scrollGames(direction) {
    const container = document.getElementById('gamesScroll');
    if (container) {
        const scrollAmount = 300;
        container.scrollBy({
            left: direction === 'left' ? -scrollAmount : scrollAmount,
            behavior: 'smooth'
        });
    }
}

// Navigation active state handling
document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('.nav-link');
    const currentPath = window.location.pathname;

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath ||
            (currentPath === '/' && link.getAttribute('href') === '/')) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
});

// Search functionality placeholder
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const searchQuery = this.value;
                console.log('Searching for:', searchQuery);
                // Add your search logic here
                // Example: window.location.href = '/search?q=' + encodeURIComponent(searchQuery);
            }
        });
    }
});

// Chat Functionality
const chatState = {
    isOpen: false,
    currentUser: null,
    pollInterval: null,
    meAvatar: null
};

// Toggle chat box visibility
function toggleChat() {
    const container = document.getElementById('chat-container');
    if (container) {
        // If we don't have a user, do not open, or prompt selection?
        // User said "select user falan diyo açarken gerek yok" -> implies it shouldn't show "select user" empty state.
        // We probably only want to open if we have a current user.
        if (!chatState.currentUser && !chatState.isOpen) {
            // Do nothing if trying to open without a user
            return;
        }

        chatState.isOpen = !chatState.isOpen;
        if (chatState.isOpen) {
            container.classList.add('open');
            setTimeout(() => document.getElementById('chat-input')?.focus(), 300);
            startPolling();
            // Close sidebar if opening chat? User said "sidebar açılınca sağ alttaki chat kapansın", 
            // but also "sidebar açıldığı zaman sağ alttaki mesaj kutusu kapansın" -> when SIDEBAR opens, chat closes.
            // But when CHAT opens (from sidebar click), sidebar probably stays or closes?
            // "select user falan diyo açarken gerek yok" -> likely refers to opening the sidebar shouldn't trigger chat to show "Select User".
        } else {
            container.classList.remove('open');
            stopPolling();
        }
    }
}

// Close chat explicitly
function closeChat() {
    const container = document.getElementById('chat-container');
    if (container) {
        chatState.isOpen = false;
        container.classList.remove('open');
        stopPolling();
    }

    // If chat is closed, show the Messages dock button again
    document.getElementById('chat-sidebar-toggle')?.classList.remove('hidden');
}

// Open chat with specific user
function showChat(username) {
    if (chatState.currentUser !== username) {
        chatState.currentUser = username;
        document.getElementById('chat-username').textContent = username;
        loadMessages();
    }

    if (!chatState.isOpen) {
        // Force open logic directly to avoid toggle issues
        const container = document.getElementById('chat-container');
        if (container) {
            chatState.isOpen = true;
            container.classList.add('open');
            startPolling();
            setTimeout(() => document.getElementById('chat-input')?.focus(), 300);
        }
    }

    // Avoid overlapping controls in bottom-right
    document.getElementById('chat-sidebar-toggle')?.classList.add('hidden');
}

// Load messages from API
async function loadMessages() {
    if (!chatState.currentUser) return;

    try {
        const response = await fetch(`/Chat/GetMessages?username=${chatState.currentUser}`);
        if (!response.ok) return;

        const messages = await response.json();
        renderMessages(messages);
    } catch (error) {
        console.error('Failed to load messages', error);
    }
}

// Render messages to DOM
function renderMessages(messages) {
    const body = document.getElementById('chat-body');
    body.innerHTML = ''; // Clear current

    if (messages.length === 0) {
        body.innerHTML = '<div class="text-center text-white/30 text-xs mt-4">Start of conversation</div>';
    } else {
        messages.forEach((msg, index) => {
            const type = msg.isSentByMe ? 'sent' : 'received';
            const next = messages[index + 1];
            const nextType = next ? (next.isSentByMe ? 'sent' : 'received') : null;
            const showAvatar = nextType !== type; // show once at end of consecutive block

            appendMessage({
                text: msg.content,
                type,
                avatarUrl: msg.senderProfilePicture || '/images/default-profile.png',
                showAvatar,
                shouldScroll: false
            });
        });
        scrollToBottom();
    }
}

// Poll for new messages
function startPolling() {
    if (chatState.pollInterval) clearInterval(chatState.pollInterval);
    chatState.pollInterval = setInterval(loadMessages, 3000); // Poll every 3s
}

function stopPolling() {
    if (chatState.pollInterval) {
        clearInterval(chatState.pollInterval);
        chatState.pollInterval = null;
    }
}

// Handle message sending
async function sendMessage() {
    const input = document.getElementById('chat-input');
    const content = input.value.trim();

    if (content && chatState.currentUser) {
        // Optimistic append
        appendMessage({
            text: content,
            type: 'sent',
            avatarUrl: chatState.meAvatar || '/images/default-profile.png',
            showAvatar: true,
            shouldScroll: true
        });
        input.value = '';

        try {
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    receiverUsername: chatState.currentUser,
                    content: content
                })
            });

            if (response.ok) {
                // Success - essentially redundant if we poll, but good for confirmation
                // Ideally we'd replace the optimistic message with the confirmed one, but for simple MVP this works
                loadMessages();
            } else {
                // Handle error (maybe show alert)
                console.error("Failed to send");
            }
        } catch (error) {
            console.error("Error sending message", error);
        }
    }
}

function handleChatInput(event) {
    if (event.key === 'Enter') {
        sendMessage();
    }
}

function appendMessage(text, type, shouldScroll = true) {
    const body = document.getElementById('chat-body');
    const payload = (typeof text === 'object' && text !== null)
        ? text
        : { text, type, avatarUrl: null, showAvatar: true, shouldScroll };

    const row = document.createElement('div');
    row.className = `chat-row ${payload.type}`;

    const avatar = document.createElement('img');
    avatar.className = `chat-avatar${payload.showAvatar ? '' : ' is-hidden'}`;
    avatar.alt = '';
    if (payload.avatarUrl) avatar.src = payload.avatarUrl;

    const bubble = document.createElement('div');
    bubble.className = `chat-message ${payload.type}`;
    bubble.textContent = payload.text;

    row.appendChild(avatar);
    row.appendChild(bubble);
    body.appendChild(row);

    if (payload.shouldScroll) scrollToBottom();
}

function scrollToBottom() {
    const body = document.getElementById('chat-body');
    body.scrollTop = body.scrollHeight;
}

// Event Listeners
// ... existing code ...

// Sidebar Functionality
const sidebarState = {
    isOpen: false,
    pollInterval: null
};

function toggleSidebar() {
    const sidebar = document.getElementById('chat-sidebar');
    const dockBtn = document.getElementById('chat-sidebar-toggle');
    if (sidebar) {
        sidebarState.isOpen = !sidebarState.isOpen;
        if (sidebarState.isOpen) {
            sidebar.classList.remove('translate-y-2', 'opacity-0', 'pointer-events-none');
            dockBtn?.classList.add('rounded-t-none');

            // Close chat if open to prevent overlap
            closeChat();

            loadSidebarData();
            startSidebarPolling();
        } else {
            sidebar.classList.add('translate-y-2', 'opacity-0', 'pointer-events-none');
            dockBtn?.classList.remove('rounded-t-none');
            stopSidebarPolling();
        }
    }
}

async function loadSidebarData() {
    // Parallel fetch
    const [convRes, friendsRes] = await Promise.all([
        fetch('/Chat/GetConversations'),
        fetch('/Chat/GetFriends')
    ]);

    if (convRes.ok) {
        const conversations = await convRes.json();
        renderConversations(conversations);
    }

    if (friendsRes.ok) {
        const friends = await friendsRes.json();
        renderFriends(friends);
    }
}

function renderConversations(list) {
    const container = document.getElementById('sidebar-recent-list');
    if (!container) return;

    if (list.length === 0) {
        container.innerHTML = '<div class="text-white/20 text-sm pl-2">No recent messages</div>';
        return;
    }

    container.innerHTML = list.map(item => `
        <div class="sidebar-user-entry flex items-center gap-3 p-2 rounded-lg cursor-pointer hover:bg-white/5 transition-colors group" data-username="${item.username}">
            <div class="relative">
                <img src="${item.profilePicture}" class="w-10 h-10 rounded-full object-cover border border-white/10 group-hover:border-primary/50 transition-colors">
                ${item.unreadCount > 0 ? `<div class="w-3 h-3 bg-primary rounded-full absolute -top-1 -right-1 border border-background-dark"></div>` : ''}
            </div>
            <div class="flex-1 min-w-0">
                <div class="flex justify-between items-baseline mb-0.5">
                    <span class="text-sm font-medium text-white truncate group-hover:text-primary transition-colors">${item.username}</span>
                    <span class="text-[10px] text-white/30">${item.timeAgo}</span>
                </div>
                <div class="text-xs text-white/40 truncate group-hover:text-white/60 transition-colors">${item.lastMessage}</div>
            </div>
        </div>
    `).join('');
}

function renderFriends(list) {
    const container = document.getElementById('sidebar-friends-list');
    if (!container) return;

    if (list.length === 0) {
        container.innerHTML = '<div class="text-white/20 text-sm pl-2">No friends found</div>';
        return;
    }

    container.innerHTML = list.map(item => `
        <div class="sidebar-user-entry flex items-center gap-3 p-2 rounded-lg cursor-pointer hover:bg-white/5 transition-colors group" data-username="${item.username}">
            <img src="${item.profilePicture}" class="w-8 h-8 rounded-full object-cover border border-white/10 group-hover:border-primary/50 transition-colors">
            <span class="text-sm text-white/70 group-hover:text-white transition-colors">${item.username}</span>
        </div>
    `).join('');
}

function startSidebarPolling() {
    if (sidebarState.pollInterval) clearInterval(sidebarState.pollInterval);
    sidebarState.pollInterval = setInterval(loadSidebarData, 5000);
}

function stopSidebarPolling() {
    if (sidebarState.pollInterval) {
        clearInterval(sidebarState.pollInterval);
        sidebarState.pollInterval = null;
    }
}

// Ensure Chat updates refresh the sidebar list if open
// Overriding previous showChat to trigger updates if needed
const originalShowChat = showChat;
showChat = function (username) {
    originalShowChat(username);
    // If we send a message, we might want to refresh the sidebar list soon
    setTimeout(loadSidebarData, 1000);
};

document.addEventListener('DOMContentLoaded', function () {
    chatState.meAvatar = document.body?.dataset?.currentUserAvatar || '/images/default-profile.png';

    // Event Delegation for all clicks
    document.addEventListener('click', function (e) {
        // 1. Sidebar Toggle (Navbar Message Button)
        const toggleBtn = e.target.closest('#chat-sidebar-toggle') || e.target.closest('#sidebar-close-btn');
        if (toggleBtn) {
            toggleSidebar();
            return;
        }

        // 2. Profile Dropdown Trigger
        const profileTrigger = e.target.closest('#profile-dropdown-trigger');
        if (profileTrigger) {
            const menu = document.getElementById('profile-dropdown-menu');
            if (menu) {
                const isHidden = menu.classList.contains('hidden');
                if (isHidden) {
                    // Open
                    menu.classList.remove('hidden');
                    requestAnimationFrame(() => {
                        menu.classList.remove('opacity-0', 'scale-95');
                    });
                } else {
                    // Close
                    menu.classList.add('opacity-0', 'scale-95');
                    setTimeout(() => menu.classList.add('hidden'), 200);
                }
            }
            return;
        }

        // 3. Click OUTSIDE Profile Dropdown -> Close it
        const profileMenu = document.getElementById('profile-dropdown-menu');
        if (profileMenu && !profileMenu.classList.contains('hidden') && !e.target.closest('#profile-dropdown-menu')) {
            profileMenu.classList.add('opacity-0', 'scale-95');
            setTimeout(() => profileMenu.classList.add('hidden'), 200);
        }

        // 4. Sidebar User Entry -> Open Chat
        const sidebarUser = e.target.closest('.sidebar-user-entry');
        if (sidebarUser) {
            const username = sidebarUser.getAttribute('data-username');
            showChat(username);
            toggleSidebar();
            return;
        }

        // 5. Profile Page "Message" Button
        const msgBtn = e.target.closest('#btn-message-user');
        if (msgBtn) {
            const username = msgBtn.getAttribute('data-username');
            showChat(username);
            return;
        }
    });

    // Optional: close the sidebar on Escape for accessibility
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;

        const sidebar = document.getElementById('chat-sidebar');
        if (sidebar && sidebarState.isOpen) {
            toggleSidebar();
        }

        const profileMenu = document.getElementById('profile-dropdown-menu');
        if (profileMenu && !profileMenu.classList.contains('hidden')) {
            profileMenu.classList.add('opacity-0', 'scale-95');
            setTimeout(() => profileMenu.classList.add('hidden'), 200);
        }
    });
});
