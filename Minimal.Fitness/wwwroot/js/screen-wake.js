/**
 * Screen Wake Lock utility.
 *
 * Usage:
 *   ScreenWake.enable('PageName');  // acquire wake lock
 *   ScreenWake.disable();           // release wake lock
 *
 * Strategy:
 *  1. Native Wake Lock API (navigator.wakeLock)
 *  2. Canvas-video fallback: creates a 1x1 <canvas>, captures it as a
 *     MediaStream, feeds it to a looping <video>. Android Chrome treats
 *     a playing video as "active media" and keeps the screen on.
 */
var ScreenWake = (function () {
    var _wakeLock = null;
    var _enabled = false;
    var _videoEl = null;
    var _canvasEl = null;
    var _canvasTimer = null;
    var _page = '';

    function log(event, detail) {
        var msg = '[screen-wake:' + _page + '] ' + event + (detail ? ' – ' + detail : '');
        console.log(msg);
    }

    // ── Native Wake Lock API ──
    async function nativeAcquire() {
        if (!('wakeLock' in navigator)) {
            log('native-not-available', 'navigator.wakeLock is ' + typeof navigator.wakeLock);
            return false;
        }
        try {
            _wakeLock = await navigator.wakeLock.request('screen');
            _wakeLock.addEventListener('release', function () {
                log('native-released-by-system');
                _wakeLock = null;
                if (_enabled) videoAcquire();
            });
            log('native-acquired');
            return true;
        } catch (e) {
            log('native-failed', e.name + ': ' + e.message);
            _wakeLock = null;
            return false;
        }
    }

    async function nativeRelease() {
        if (_wakeLock) {
            try { await _wakeLock.release(); } catch (_) {}
            _wakeLock = null;
            log('native-released');
        }
    }

    // ── Canvas-video fallback (keeps Android Chrome awake) ──
    function videoAcquire() {
        if (_videoEl) return;
        try {
            _canvasEl = document.createElement('canvas');
            _canvasEl.width = 1;
            _canvasEl.height = 1;
            var ctx = _canvasEl.getContext('2d');
            ctx.fillRect(0, 0, 1, 1);

            var stream;
            if (_canvasEl.captureStream) {
                stream = _canvasEl.captureStream(0);
            } else if (_canvasEl.mozCaptureStream) {
                stream = _canvasEl.mozCaptureStream(0);
            } else {
                log('video-fallback-no-captureStream');
                return;
            }

            _videoEl = document.createElement('video');
            _videoEl.setAttribute('playsinline', '');
            _videoEl.setAttribute('muted', '');
            _videoEl.muted = true;
            _videoEl.style.cssText = 'position:fixed;top:-1px;left:-1px;width:1px;height:1px;opacity:0.01;pointer-events:none;';
            _videoEl.srcObject = stream;
            document.body.appendChild(_videoEl);

            var track = stream.getVideoTracks()[0];
            _canvasTimer = setInterval(function () {
                ctx.fillRect(0, 0, 1, 1);
                if (track && track.requestFrame) track.requestFrame();
            }, 1000);

            _videoEl.play().then(function () {
                log('video-fallback-acquired');
            }).catch(function (e) {
                log('video-fallback-play-failed', e.message);
                videoRelease();
            });
        } catch (e) {
            log('video-fallback-error', e.message);
            videoRelease();
        }
    }

    function videoRelease() {
        if (_canvasTimer) { clearInterval(_canvasTimer); _canvasTimer = null; }
        if (_videoEl) {
            try { _videoEl.pause(); } catch (_) {}
            if (_videoEl.srcObject) {
                try { _videoEl.srcObject.getTracks().forEach(function (t) { t.stop(); }); } catch (_) {}
            }
            try { _videoEl.remove(); } catch (_) {}
            _videoEl = null;
        }
        if (_canvasEl) { try { _canvasEl.remove(); } catch (_) {} _canvasEl = null; }
        log('video-fallback-released');
    }

    // ── Visibility change handler ──
    function onVisibility() {
        if (document.visibilityState === 'visible' && _enabled) {
            log('visibility-visible-reacquire');
            acquire();
        }
    }

    // ── Core acquire/release ──
    async function acquire() {
        var ok = await nativeAcquire();
        if (!ok) videoAcquire();
    }

    // ── Public API ──
    return {
        enable: function (page) {
            _page = page || 'unknown';
            _enabled = true;
            log('enable', 'secure=' + (location.protocol === 'https:') + ' hasNativeApi=' + ('wakeLock' in navigator));
            document.addEventListener('visibilitychange', onVisibility);
            acquire();
        },

        disable: function () {
            _enabled = false;
            log('disable');
            document.removeEventListener('visibilitychange', onVisibility);
            nativeRelease();
            videoRelease();
        }
    };
})();
