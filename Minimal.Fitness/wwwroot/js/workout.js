// State
let currentIndex = 0;
let currentSetNum = 0;
let timerInterval = null;
let timerSecondsLeft = 0;
let restInterval = null;
let audioCtx = null;

const CIRCUMFERENCE = 2 * Math.PI * 54; // SVG circle r=54

function getPlanExercise(exerciseId) {
    return planExercises.find(pe => pe.exerciseId === exerciseId);
}

function showExercise(index) {
    currentIndex = index;
    const se = sessionExercises[index];
    const pe = getPlanExercise(se.exerciseId);
    currentSetNum = se.setsCompleted;

    // Progress
    document.getElementById('progress-label').textContent = `Exercise ${index + 1} of ${sessionExercises.length}`;
    const pct = Math.round((index / sessionExercises.length) * 100);
    document.getElementById('progress-bar').style.width = pct + '%';
    document.getElementById('progress-pct').textContent = pct + '%';
    updateDifficultyDots();

    // Section & name
    document.getElementById('section-label').textContent = pe?.section || '';
    document.getElementById('exercise-name').textContent = se.name;
    document.getElementById('set-label').textContent = `Set ${currentSetNum + 1} of ${pe?.defaultSets || 3}`;

    // Animation
    const animArea = document.getElementById('animation-area');
    if (se.animationPath) {
        animArea.innerHTML = `<dotlottie-player src="/animations/${se.animationPath}" autoplay loop style="height:160px;width:160px;margin:0 auto;"></dotlottie-player>`;
    } else {
        const emoji = { Chest: '🫁', Back: '🔙', Shoulders: '💪', Biceps: '💪', Triceps: '💪', Legs: '🦵', Core: '🎯', Glutes: '🍑', FullBody: '🏋️' };
        animArea.innerHTML = `<div class="exercise-placeholder display-1">${emoji[se.muscleGroup] || '🏋️'}</div>`;
    }

    // Show correct input mode
    const repsInput = document.getElementById('reps-input');
    const timerInput = document.getElementById('timer-input');
    const diffPrompt = document.getElementById('difficulty-prompt');
    const completeArea = document.getElementById('complete-area');

    repsInput.classList.add('d-none');
    timerInput.classList.add('d-none');
    diffPrompt.classList.add('d-none');
    completeArea.classList.add('d-none');
    document.getElementById('exercise-area').classList.remove('d-none');

    if (se.trackingType === 'Timer') {
        timerInput.classList.remove('d-none');
        timerSecondsLeft = pe?.defaultDurationSeconds || 60;
        updateTimerDisplay();
        document.getElementById('btn-timer-start').classList.remove('d-none');
        document.getElementById('btn-timer-stop').classList.add('d-none');
        document.getElementById('btn-timer-done').classList.add('d-none');
    } else {
        repsInput.classList.remove('d-none');
        document.getElementById('input-reps').value = pe?.defaultReps || '';
        document.getElementById('input-weight').value = pe?.defaultWeight || '';
    }
}

// Reps mode
function completeSet() {
    const se = sessionExercises[currentIndex];
    const pe = getPlanExercise(se.exerciseId);
    const reps = parseInt(document.getElementById('input-reps').value) || null;
    const weight = parseFloat(document.getElementById('input-weight').value) || null;

    fetch('/WorkoutSession/CompleteSet', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiForgeryToken },
        body: JSON.stringify({ sessionExerciseId: se.id, reps, weight, durationSeconds: null })
    })
    .then(r => r.json())
    .then(data => {
        currentSetNum = data.setNumber;
        se.setsCompleted = currentSetNum;

        if (currentSetNum >= (pe?.defaultSets || 3)) {
            showDifficultyPrompt();
        } else {
            document.getElementById('set-label').textContent = `Set ${currentSetNum + 1} of ${pe?.defaultSets || 3}`;
            showRestTimer();
        }
    });
}

// Timer mode
function startTimer() {
    document.getElementById('btn-timer-start').classList.add('d-none');
    document.getElementById('btn-timer-stop').classList.remove('d-none');

    timerInterval = setInterval(() => {
        timerSecondsLeft--;
        updateTimerDisplay();
        if (timerSecondsLeft <= 0) {
            clearInterval(timerInterval);
            timerInterval = null;
            playBeep();
            finishTimerSet();
        }
    }, 1000);
}

function stopTimer() {
    clearInterval(timerInterval);
    timerInterval = null;
    document.getElementById('btn-timer-stop').classList.add('d-none');
    document.getElementById('btn-timer-start').classList.remove('d-none');
    document.getElementById('btn-timer-done').classList.remove('d-none');
}

function finishTimerSet() {
    clearInterval(timerInterval);
    timerInterval = null;

    const se = sessionExercises[currentIndex];
    const pe = getPlanExercise(se.exerciseId);
    const actualDuration = (pe?.defaultDurationSeconds || 60) - timerSecondsLeft;

    fetch('/WorkoutSession/CompleteSet', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiForgeryToken },
        body: JSON.stringify({ sessionExerciseId: se.id, reps: null, weight: null, durationSeconds: actualDuration })
    })
    .then(r => r.json())
    .then(data => {
        currentSetNum = data.setNumber;
        se.setsCompleted = currentSetNum;

        if (currentSetNum >= (pe?.defaultSets || 3)) {
            showDifficultyPrompt();
        } else {
            document.getElementById('set-label').textContent = `Set ${currentSetNum + 1} of ${pe?.defaultSets || 3}`;
            timerSecondsLeft = pe?.defaultDurationSeconds || 60;
            updateTimerDisplay();
            document.getElementById('btn-timer-start').classList.remove('d-none');
            document.getElementById('btn-timer-stop').classList.add('d-none');
            document.getElementById('btn-timer-done').classList.add('d-none');
            showRestTimer();
        }
    });
}

function updateTimerDisplay() {
    const mins = Math.floor(timerSecondsLeft / 60);
    const secs = timerSecondsLeft % 60;
    document.getElementById('timer-display').textContent = `${mins}:${secs.toString().padStart(2, '0')}`;
}

// Difficulty rating
function showDifficultyPrompt() {
    document.getElementById('reps-input').classList.add('d-none');
    document.getElementById('timer-input').classList.add('d-none');
    document.getElementById('difficulty-prompt').classList.remove('d-none');
    document.getElementById('set-label').textContent = 'All sets complete!';
}

function rateExercise(difficulty) {
    const se = sessionExercises[currentIndex];

    fetch('/WorkoutSession/RateExercise', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiForgeryToken },
        body: JSON.stringify({ sessionExerciseId: se.id, difficulty })
    })
    .then(() => {
        se.difficulty = difficulty;
        if (currentIndex + 1 < sessionExercises.length) {
            showExercise(currentIndex + 1);
        } else {
            showWorkoutComplete();
        }
    });
}

function showWorkoutComplete() {
    document.getElementById('exercise-area').classList.add('d-none');
    document.getElementById('complete-area').classList.remove('d-none');

    // Full progress
    document.getElementById('progress-bar').style.width = '100%';
    document.getElementById('progress-pct').textContent = '100%';
    document.getElementById('progress-label').textContent = 'Complete!';
    updateDifficultyDots();
}

// Rest timer
function showRestTimer() {
    const overlay = document.getElementById('rest-overlay');
    overlay.classList.remove('d-none');

    let remaining = restSeconds;
    const ring = document.getElementById('rest-ring');
    const label = document.getElementById('rest-seconds');

    label.textContent = remaining;
    ring.style.strokeDashoffset = '0';

    restInterval = setInterval(() => {
        remaining--;
        label.textContent = remaining;
        const offset = CIRCUMFERENCE * (1 - remaining / restSeconds);
        ring.style.strokeDashoffset = offset;

        if (remaining <= 0) {
            clearInterval(restInterval);
            restInterval = null;
            playBeep();
            setTimeout(() => skipRest(), 500);
        }
    }, 1000);
}

function skipRest() {
    clearInterval(restInterval);
    restInterval = null;
    document.getElementById('rest-overlay').classList.add('d-none');
}

// Audio
function playBeep() {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    osc.connect(gain);
    gain.connect(audioCtx.destination);
    osc.frequency.value = 880;
    gain.gain.value = 0.3;
    osc.start();
    osc.stop(audioCtx.currentTime + 0.2);
}

// Difficulty dots
function updateDifficultyDots() {
    const container = document.getElementById('difficulty-dots');
    container.innerHTML = sessionExercises.map((se, i) => {
        const colors = { TooEasy: '#22c55e', GotIt: '#3b82f6', Struggled: '#eab308' };
        const color = se.difficulty ? colors[se.difficulty] : (i < currentIndex ? '#9ca3af' : '#e5e7eb');
        const active = i === currentIndex ? 'border: 2px solid #000; border-radius: 50%;' : '';
        return `<span style="color:${color}; font-size:1rem; ${active}" title="${se.name}">●</span>`;
    }).join('');
}

// Init
document.addEventListener('DOMContentLoaded', () => showExercise(0));
