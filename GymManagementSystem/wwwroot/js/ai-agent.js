(function () {
    'use strict';

    const MOODS = { happy: 'smile', sad: 'sad', neutral: 'neutral', think: 'neutral' };

    const RESPONSES = [
        {
            keys: ['hello', 'hi', 'hey', 'namaste', 'help'],
            mood: 'happy',
            text: 'Hello! Main CloudMex AI hoon. Members, payments, attendance ya WhatsApp bot ke baare mein poochho!'
        },
        {
            keys: ['member', 'members', 'add member'],
            mood: 'happy',
            text: 'Naya member add karne ke liye: Masters → Member Master → Add. Wahan details fill karke save karein.'
        },
        {
            keys: ['payment', 'pay', 'fees', 'online'],
            mood: 'happy',
            text: 'Payments collect karne ke liye Entries → Payments. Online pay ke liye pehle Masters → Payment Gateway configure karein.'
        },
        {
            keys: ['attendance', 'check in', 'checkin'],
            mood: 'happy',
            text: 'Member attendance Entries → Attendance se mark hoti hai. Report ke liye Reports → Attendance Report dekhein.'
        },
        {
            keys: ['whatsapp', 'bot', 'lead'],
            mood: 'happy',
            text: 'WhatsApp setup: Masters → WhatsApp API Setup. Form link share: Entries → WhatsApp Bot. User /join form fill karke bot flow start karta hai.'
        },
        {
            keys: ['report', 'reports', 'collection', 'profit', 'expiry'],
            mood: 'happy',
            text: 'Reports sidebar mein hain: Attendance, Expiry, Collections, Outstanding, Profit/Loss. Date range select karke filter karein.'
        },
        {
            keys: ['membership', 'plan', 'renew'],
            mood: 'happy',
            text: 'Plans Masters → Membership Plans mein banate hain. Assign/renew Entries → Membership Management se hota hai.'
        },
        {
            keys: ['error', 'problem', 'issue', 'fail', 'not working'],
            mood: 'sad',
            text: 'Sorry, kuch issue lag raha hai! Page refresh karein. Payment/WhatsApp ke liye gateway credentials aur webhook check karein.'
        },
        {
            keys: ['sad', 'bad', 'worst'],
            mood: 'sad',
            text: 'Oh no! Main yahan hoon help ke liye. Batao kya problem hai — main guide karunga.'
        },
        {
            keys: ['thank', 'thanks', 'shukriya', 'dhanyavad'],
            mood: 'happy',
            text: 'You\'re welcome! Aur kuch chahiye ho to poochho — main hamesha yahan hoon.'
        },
        {
            keys: ['dashboard', 'home'],
            mood: 'happy',
            text: 'Dashboard par aapko members, revenue, attendance charts aur recent activity dikhti hai. Navbar se global search bhi use kar sakte ho.'
        }
    ];

    const DEFAULT_REPLY = {
        mood: 'neutral',
        text: 'Main samjha nahi — thoda detail mein likho. Try: "payment kaise karein", "whatsapp setup", "attendance report".'
    };

    let root, launcher, panel, messagesEl, inputEl, sendBtn, closeBtn;
    let launcherFace, panelFace;
    let blinkTimer, lookTimer, moodTimer;
    let isOpen = false;

    function init() {
        root = document.getElementById('aiAgentRoot');
        if (!root) return;

        launcher = document.getElementById('aiAgentLauncher');
        panel = document.getElementById('aiAgentPanel');
        messagesEl = document.getElementById('aiAgentMessages');
        inputEl = document.getElementById('aiAgentInput');
        sendBtn = document.getElementById('aiAgentSend');
        closeBtn = document.getElementById('aiAgentClose');

        launcherFace = launcher?.querySelector('.ai-face');
        panelFace = document.getElementById('aiPanelFace');

        bindEvents();
        startBlinkLoop();
        startLookLoop();
        setMood('happy', launcherFace);
        setMood('happy', panelFace);
    }

    function bindEvents() {
        launcher?.addEventListener('click', openPanel);
        closeBtn?.addEventListener('click', closePanel);
        sendBtn?.addEventListener('click', sendMessage);
        inputEl?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') sendMessage();
        });

        document.querySelectorAll('.ai-chip').forEach((chip) => {
            chip.addEventListener('click', () => {
                if (!inputEl) return;
                inputEl.value = chip.dataset.msg || chip.textContent.trim();
                sendMessage();
            });
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && isOpen) closePanel();
        });
    }

    function openPanel() {
        isOpen = true;
        root.classList.add('is-open');
        setMood('happy', panelFace);
        inputEl?.focus();

        if (messagesEl && messagesEl.children.length === 0) {
            appendBotMessage(
                'Namaste! Main CloudMex AI assistant hoon. Gym software mein help chahiye? Neeche chips try karein ya kuch poochho!',
                'happy',
                false
            );
        }
    }

    function closePanel() {
        isOpen = false;
        root.classList.remove('is-open');
        setMood('happy', launcherFace);
    }

    function startBlinkLoop() {
        const blink = () => {
            blinkEyes(launcherFace);
            if (isOpen) blinkEyes(panelFace);
            const delay = 2500 + Math.random() * 3500;
            blinkTimer = setTimeout(blink, delay);
        };
        blinkTimer = setTimeout(blink, 2000);
    }

    function blinkEyes(faceEl) {
        if (!faceEl) return;
        faceEl.querySelectorAll('.ai-eye').forEach((eye) => {
            eye.classList.add('is-blinking');
            setTimeout(() => eye.classList.remove('is-blinking'), 200);
        });
    }

    function startLookLoop() {
        const look = () => {
            if (!isOpen && launcherFace) {
                const dirs = ['', 'look-left', 'look-right', 'look-up', ''];
                const dir = dirs[Math.floor(Math.random() * dirs.length)];
                launcherFace.classList.remove('look-left', 'look-right', 'look-up');
                if (dir) launcherFace.classList.add(dir);
            }
            lookTimer = setTimeout(look, 4000 + Math.random() * 3000);
        };
        lookTimer = setTimeout(look, 3500);
    }

    function setMood(mood, faceEl) {
        if (!faceEl) return;

        const mouth = faceEl.querySelector('.ai-mouth');
        if (!mouth) return;

        mouth.classList.remove('ai-mouth--smile', 'ai-mouth--sad', 'ai-mouth--neutral');
        mouth.classList.add('ai-mouth--' + (MOODS[mood] || 'neutral'));

        faceEl.classList.remove('mood-happy', 'mood-sad');
        if (mood === 'happy') faceEl.classList.add('mood-happy');
        if (mood === 'sad') faceEl.classList.add('mood-sad');

        clearTimeout(moodTimer);
        if (mood !== 'happy') {
            moodTimer = setTimeout(() => setMood('happy', faceEl), 4000);
        }
    }

    function syncMood(mood) {
        setMood(mood, panelFace);
        if (!isOpen) setMood(mood, launcherFace);
    }

    function sendMessage() {
        const text = inputEl?.value?.trim();
        if (!text) return;

        appendUserMessage(text);
        inputEl.value = '';

        syncMood('neutral');
        blinkEyes(panelFace);

        const typingEl = showTyping();

        setTimeout(() => {
            typingEl?.remove();
            const reply = findReply(text);
            appendBotMessage(reply.text, reply.mood, true);
        }, 600 + Math.random() * 500);
    }

    function findReply(text) {
        const lower = text.toLowerCase();
        for (const item of RESPONSES) {
            if (item.keys.some((k) => lower.includes(k))) {
                return item;
            }
        }
        return DEFAULT_REPLY;
    }

    function appendUserMessage(text) {
        if (!messagesEl) return;
        const el = document.createElement('div');
        el.className = 'ai-msg ai-msg--user';
        el.textContent = text;
        messagesEl.appendChild(el);
        scrollMessages();
    }

    function appendBotMessage(text, mood, animateMood) {
        if (!messagesEl) return;
        const el = document.createElement('div');
        el.className = 'ai-msg ai-msg--bot';
        el.textContent = text;
        messagesEl.appendChild(el);
        scrollMessages();

        if (animateMood) syncMood(mood || 'happy');
        blinkEyes(panelFace);
    }

    function showTyping() {
        if (!messagesEl) return null;
        const el = document.createElement('div');
        el.className = 'ai-msg ai-msg--typing';
        el.innerHTML = '<div class="ai-typing-dots"><span></span><span></span><span></span></div>';
        messagesEl.appendChild(el);
        scrollMessages();
        return el;
    }

    function scrollMessages() {
        if (messagesEl) messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
