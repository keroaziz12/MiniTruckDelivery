(() => {
    const menu = document.getElementById("tripNotifications");
    if (!menu) return;

    let audioContext = null;
    let audioUnlocked = false;
    const reminders = [...menu.querySelectorAll("[data-trip-id][data-trip-date][data-reminder-date]")].map(item => {
        const [year, month, day] = item.dataset.reminderDate.split("-").map(Number);
        return {
            id: item.dataset.tripId,
            tripDate: item.dataset.tripDate,
            reminderAt: new Date(year, month - 1, day, 8, 0, 0, 0)
        };
    });

    const storageKey = reminder => `mtd-trip-reminder-${reminder.id}-${reminder.tripDate}`;
    const wasPlayed = reminder => {
        try { return localStorage.getItem(storageKey(reminder)) === "played"; } catch { return false; }
    };
    const markPlayed = reminder => {
        try { localStorage.setItem(storageKey(reminder), "played"); } catch { }
    };

    const playChime = () => {
        if (!audioContext || !audioUnlocked) return false;
        const startAt = audioContext.currentTime;
        [[880, 0], [1174.66, 0.16]].forEach(([frequency, delay]) => {
            const oscillator = audioContext.createOscillator();
            const gain = audioContext.createGain();
            oscillator.type = "sine";
            oscillator.frequency.value = frequency;
            gain.gain.setValueAtTime(0.0001, startAt + delay);
            gain.gain.exponentialRampToValueAtTime(0.15, startAt + delay + 0.025);
            gain.gain.exponentialRampToValueAtTime(0.0001, startAt + delay + 0.3);
            oscillator.connect(gain);
            gain.connect(audioContext.destination);
            oscillator.start(startAt + delay);
            oscillator.stop(startAt + delay + 0.32);
        });
        return true;
    };

    const checkDueReminders = () => {
        if (!audioUnlocked) return;
        const now = new Date();
        const due = reminders.filter(reminder => reminder.reminderAt <= now && !wasPlayed(reminder));
        if (due.length && playChime()) due.forEach(markPlayed);
    };

    const unlockAudio = async () => {
        const AudioContextType = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextType) return;
        try {
            audioContext ??= new AudioContextType();
            if (audioContext.state === "suspended") await audioContext.resume();
            audioUnlocked = audioContext.state === "running";
            checkDueReminders();
        } catch { audioUnlocked = false; }
    };

    reminders.forEach(reminder => {
        const delay = reminder.reminderAt.getTime() - Date.now();
        if (delay > 0 && delay < 2147480000) {
            window.setTimeout(checkDueReminders, delay + 1000);
        }
    });

    // Browsers require a user gesture before a page can play audio.
    document.addEventListener("pointerdown", unlockAudio, { once: true, passive: true });
    document.addEventListener("keydown", unlockAudio, { once: true });
})();
