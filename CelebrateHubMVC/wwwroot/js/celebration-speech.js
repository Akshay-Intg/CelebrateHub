// ============================================================
// celebration-speech.js
// Place: CelebrateHubMVC → wwwroot → js
// ============================================================
// Reads birthday/anniversary flags from the DOM (set by Razor),
// shows the celebration banner, then fetches the MP3 from
// /Speech/Generate (MVC proxy → API → FreeTTS) and plays it.
//
// localStorage prevents the audio from auto-playing more than
// once per day per person per occasion type.
// The play button always lets the user replay on demand.
// ============================================================

(function () {
    'use strict';

    // ── Read context from hidden DOM element set by Razor ─────────────────
    const ctx = document.getElementById('speechContext');
    if (!ctx) return;

    const name = ctx.dataset.name || 'Friend';
    const firstName = name.split(' ')[0];
    const isBday = ctx.dataset.isBirthday === 'true';
    const isAnn = ctx.dataset.isAnniversary === 'true';
    const age = parseInt(ctx.dataset.age || '0', 10);
    const years = parseInt(ctx.dataset.years || '0', 10);
    const dept = ctx.dataset.dept || '';

    // ── Nothing to celebrate today ─────────────────────────────────────────
    if (!isBday && !isAnn) return;

    // ── Determine occasion type (birthday takes priority) ─────────────────
    const type = isBday ? 'birthday' : 'anniversary';

    // ── localStorage dedup key — resets every calendar day ────────────────
    const todayStr = new Date().toISOString().split('T')[0]; // YYYY-MM-DD
    const storageKey = `celebSpoken_${name}_${type}_${todayStr}`;
    const alreadyPlayed = localStorage.getItem(storageKey) === '1';

    // ── Audio instance (reused for replay) ────────────────────────────────
    let audioUrl = null;   // object URL of fetched MP3
    let audioEl = null;   // HTML5 Audio element

    // ── Expose public API ──────────────────────────────────────────────────
    window.CelebrateSpeech = { play: playAudio };

    // ── Boot: show banner, fetch audio ────────────────────────────────────
    showBanner();
    fetchAndPrepareAudio();

    // ─────────────────────────────────────────────────────────────────────

    function showBanner() {
        const banner = document.getElementById('celebrationBanner');
        const emoji = document.getElementById('bannerEmoji');
        const title = document.getElementById('bannerTitle');
        const sub = document.getElementById('bannerSub');
        const status = document.getElementById('speechStatus');

        if (!banner) return;

        if (type === 'birthday') {
            banner.classList.add('is-birthday');
            emoji.textContent = '🎂';
            title.textContent = `Happy Birthday, ${firstName}! 🎉`;
            sub.innerHTML = age
                ? `Turning ${age} today! <span id="speechStatus">Loading your wish...</span>`
                : `Wishing you an amazing day! <span id="speechStatus">Loading your wish...</span>`;
        } else {
            banner.classList.add('is-anniversary');
            emoji.textContent = '💐';
            title.textContent = `Happy Work Anniversary, ${firstName}! 🌟`;
            sub.innerHTML = years
                ? `${years} incredible year${years !== 1 ? 's' : ''} with us! <span id="speechStatus">Loading your wish...</span>`
                : `Thank you for your dedication! <span id="speechStatus">Loading your wish...</span>`;
        }

        createParticles();
        banner.style.display = 'block';
    }

    async function fetchAndPrepareAudio() {
        const playBtn = document.getElementById('playBtn');
        const playIcon = document.getElementById('playIcon');

        try {
            setStatus('Preparing your message...');

            // Build query string for MVC proxy
            const params = new URLSearchParams({
                type,
                name,
                age: age.toString(),
                years: years.toString(),
                dept: dept
            });

            const resp = await fetch(`/Speech/Generate?${params.toString()}`);

            if (!resp.ok) {
                setStatus('Could not load audio.');
                return;
            }

            // Convert response to a blob URL the Audio element can use
            const blob = await resp.blob();
            audioUrl = URL.createObjectURL(blob);
            audioEl = new Audio(audioUrl);

            // Wire up audio events
            audioEl.onplay = () => {
                setStatus('Playing...');
                if (playIcon) {
                    playIcon.className = 'bi bi-pause-circle-fill';
                }
                if (playBtn) playBtn.classList.add('playing');
            };

            audioEl.onended = () => {
                setStatus('Click ▶ to replay');
                if (playIcon) playIcon.className = 'bi bi-play-circle-fill';
                if (playBtn) playBtn.classList.remove('playing');
            };

            audioEl.onerror = () => {
                setStatus('Playback error. Try again.');
                if (playIcon) playIcon.className = 'bi bi-play-circle-fill';
                if (playBtn) playBtn.classList.remove('playing');
            };

            setStatus('Click ▶ to hear your wish');

            // Auto-play only once per day (after a short delay)
            if (!alreadyPlayed) {
                setTimeout(playAudio, 1200);
                localStorage.setItem(storageKey, '1');
            }

        } catch (err) {
            console.warn('CelebrateHub TTS fetch failed:', err);
            setStatus('Audio unavailable.');
        }
    }

    function playAudio() {
        if (!audioEl) {
            setStatus('Audio still loading...');
            return;
        }

        const playBtn = document.getElementById('playBtn');
        const playIcon = document.getElementById('playIcon');

        // Toggle: if already playing, pause instead
        if (!audioEl.paused) {
            audioEl.pause();
            setStatus('Paused. Click ▶ to resume.');
            if (playIcon) playIcon.className = 'bi bi-play-circle-fill';
            if (playBtn) playBtn.classList.remove('playing');
            return;
        }

        // Reset to start if finished
        if (audioEl.ended) audioEl.currentTime = 0;

        audioEl.play().catch(err => {
            // Browsers block autoplay without user gesture —
            // this only triggers if play() is called without a click.
            // Manual replays via button always work.
            console.warn('Audio play blocked:', err);
            setStatus('Click ▶ to hear your wish');
        });
    }

    function setStatus(msg) {
        const el = document.getElementById('speechStatus');
        if (el) el.textContent = msg;
    }

    // ── Floating confetti particles ────────────────────────────────────────
    function createParticles() {
        const container = document.getElementById('particles');
        if (!container) return;

        const colors = type === 'birthday'
            ? ['#fb7185', '#fbbf24', '#a78bfa', '#34d399', '#60a5fa', '#f472b6']
            : ['#34d399', '#60a5fa', '#a78bfa', '#fbbf24', '#6366f1', '#10b981'];

        for (let i = 0; i < 20; i++) {
            const p = document.createElement('div');
            p.className = 'particle';
            const size = Math.random() * 14 + 5;
            const color = colors[Math.floor(Math.random() * colors.length)];
            const left = Math.random() * 100;
            const dur = Math.random() * 7 + 5;
            const delay = Math.random() * 5;

            p.style.cssText = `
                width:${size}px; height:${size}px;
                background:${color};
                left:${left}%; bottom:-10px;
                animation-duration:${dur}s;
                animation-delay:${delay}s;
                border-radius:${Math.random() > 0.5 ? '50%' : '4px'};
            `;
            container.appendChild(p);
        }
    }

})();