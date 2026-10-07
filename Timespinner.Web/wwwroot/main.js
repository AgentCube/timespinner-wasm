// main.js — Timespinner WASM bootstrap
//
// Key design principles:
//   1. Strict single-handle requestAnimationFrame loop with isRunning gate.
//   2. Zero DOM measurements inside frame step().
//   3. Monospace loading progress indicator (Loading: X / 1087 (Y%)).
//   4. WebAudio unlock on user gesture (capture-phase, passive).
//   5. MEMFS preload: /Content from Content/manifest.json with browser Cache Storage.
//   6. /save mounted to IDBFS + direct IndexedDB redundancy under 'timespinner-save-db'.

import { dotnet } from './_framework/dotnet.js';

const canvas     = document.getElementById('canvas');
const progressEl = document.getElementById('loading-progress');
const overlayEl  = document.getElementById('loading');
const fpsEl      = document.getElementById('fps-counter');

// Set native back-buffer dimensions
if (canvas) {
    canvas.width  = 1280;
    canvas.height = 720;
}

// Neutralize forced fullscreen triggers
if (canvas) {
    canvas.requestFullscreen = () => Promise.resolve();
    canvas.webkitRequestFullscreen = () => Promise.resolve();
    canvas.mozRequestFullScreen = () => Promise.resolve();
    canvas.msRequestFullscreen = () => Promise.resolve();
}
document.documentElement.requestFullscreen = () => Promise.resolve();
document.body.requestFullscreen = () => Promise.resolve();
if (globalThis.Module) {
    globalThis.Module.requestFullscreen = () => {};
}

function setStatus(msg) {
    console.log('[Timespinner] ' + msg);
    if (progressEl) progressEl.textContent = msg;
}

setStatus('Initialising .NET WebAssembly runtime…');

globalThis.Module = globalThis.Module || {};
if (canvas) globalThis.Module.canvas = canvas;

// ---------------------------------------------------------------------------
// 1. WebAudio autoplay unlock + ScriptProcessor power-of-2 clamp
// ---------------------------------------------------------------------------
const activeAudioContexts = new Set();
const OrigAudioContext = window.AudioContext || window.webkitAudioContext;

if (OrigAudioContext) {
    const WrappedAudioContext = function (...args) {
        let opts = args[0];
        if (!opts || typeof opts !== 'object') opts = {};
        else opts = Object.assign({}, opts);
        if (!opts.sampleRate) opts.sampleRate = 48000;
        const ctx = new OrigAudioContext(opts);
        activeAudioContexts.add(ctx);
        console.log('[Timespinner Audio] AudioContext created. Rate:', ctx.sampleRate, 'State:', ctx.state);
        ctx.addEventListener('statechange', () =>
            console.log('[Timespinner Audio] AudioContext state:', ctx.state));
        return ctx;
    };
    WrappedAudioContext.prototype = OrigAudioContext.prototype;

    const origCSP = OrigAudioContext.prototype.createScriptProcessor;
    if (origCSP) {
        OrigAudioContext.prototype.createScriptProcessor = function (bufferSize, inCh, outCh) {
            const valid = [256, 512, 1024, 2048, 4096, 8192, 16384];
            let clamped = bufferSize;
            if (!valid.includes(bufferSize)) {
                clamped = valid.reduce((best, p) =>
                    Math.abs(p - bufferSize) < Math.abs(best - bufferSize) ? p : best, 4096);
            }
            if (clamped < 4096) {
                clamped = 4096;
            }
            console.log('[Timespinner Audio] ScriptProcessor bufferSize:', bufferSize, '-> clamped to:', clamped);
            return origCSP.call(this, clamped, inCh, outCh);
        };
    }

    window.AudioContext = WrappedAudioContext;
    if (window.webkitAudioContext) window.webkitAudioContext = WrappedAudioContext;
}

function resumeAllAudio() {
    for (const ctx of activeAudioContexts) {
        if (ctx.state === 'suspended') {
            ctx.resume().catch(err => console.warn('[Timespinner Audio] Resume error:', err));
        }
    }
    const sdl2 = globalThis.Module?.SDL2 || globalThis.SDL2 || window.SDL2;
    if (sdl2?.audioContext && sdl2.audioContext.state === 'suspended') {
        sdl2.audioContext.resume().catch(() => {});
    }
}

const unlockAudio = () => resumeAllAudio();
['click','keydown','keyup','mousedown','mouseup','pointerdown','touchstart','touchend'].forEach(evt => {
    window.addEventListener(evt, unlockAudio, { capture: true, passive: true });
    document.addEventListener(evt, unlockAudio, { capture: true, passive: true });
    if (canvas) canvas.addEventListener(evt, unlockAudio, { capture: true, passive: true });
});

// ---------------------------------------------------------------------------
// 2. FPS counter — 500 ms setInterval sampling integer counter
// ---------------------------------------------------------------------------
let _fpsFrameCount = 0;

setInterval(() => {
    const fps = _fpsFrameCount * 2;
    _fpsFrameCount = 0;
    if (fpsEl) {
        fpsEl.textContent = fps + ' FPS';
        if (fps >= 50) {
            fpsEl.style.color = '#4ade80';
            fpsEl.style.borderColor = 'rgba(74,222,128,0.35)';
        } else if (fps >= 28) {
            fpsEl.style.color = '#facc15';
            fpsEl.style.borderColor = 'rgba(250,204,21,0.35)';
        } else {
            fpsEl.style.color = '#f87171';
            fpsEl.style.borderColor = 'rgba(248,113,113,0.35)';
        }
    }
    if (navigator.userActivation && navigator.userActivation.hasBeenActive) {
        resumeAllAudio();
    }
}, 500);

// ---------------------------------------------------------------------------
// 3. Emscripten filesystem utilities
// ---------------------------------------------------------------------------
function getDirname(filePath) {
    const idx = filePath.lastIndexOf('/');
    return idx === -1 ? '' : filePath.substring(0, idx);
}

function ensureDirectoryExists(fs, dirPath) {
    if (!dirPath || dirPath === '/' || dirPath === '.') return;
    const parts = dirPath.split('/').filter(p => p.length > 0);
    let current = '';
    for (const part of parts) {
        current += '/' + part;
        try {
            if (typeof fs.analyzePath === 'function') {
                if (!fs.analyzePath(current).exists) fs.mkdir(current);
            } else {
                fs.mkdir(current);
            }
        } catch (_) { }
    }
}

// ---------------------------------------------------------------------------
// 4. Asset Preloading with Browser Cache Storage
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
// 4. Asset Preloading with Binary Blob Streaming & Extraction
// ---------------------------------------------------------------------------
const ASSET_CACHE_NAME = 'timespinner-assets-v2';

async function fetchCached(cache, url) {
    if (cache) {
        try {
            const hit = await cache.match(url);
            if (hit) {
                const buf = await hit.arrayBuffer();
                if (buf.byteLength > 0) return buf;
            }
        } catch (_) {}
    }
    const resp = await fetch(url);
    if (!resp.ok) throw new Error('HTTP ' + resp.status + ' for ' + url);
    const buf = await resp.arrayBuffer();
    if (cache && buf.byteLength > 0) {
        try { await cache.put(url, new Response(buf)); } catch (_) {}
    }
    return buf;
}

async function preloadAssets(FS) {
    if (!FS) {
        console.warn('[Timespinner] FS not available, skipping asset preload.');
        return;
    }

    let cache = null;
    if (typeof caches !== 'undefined') {
        try { cache = await caches.open(ASSET_CACHE_NAME); } catch (_) {}
    }

    // 1. Try to load packed binary blobs (assets/content.pack.json or assets/content.pack)
    let packManifest = null;
    try {
        const r = await fetch('assets/content.pack.json', { cache: 'no-store' });
        if (r.ok) packManifest = await r.json();
    } catch (_) {}

    if (packManifest && packManifest.parts && packManifest.parts.length > 0) {
        const parts = packManifest.parts;
        const totalParts = parts.length;
        console.log(`[Timespinner] Downloading ${totalParts} packed asset parts (${(packManifest.totalSize / (1024*1024)).toFixed(1)} MB)...`);
        setStatus(`Downloading asset pack: 0 / ${totalParts} parts (0%)`);

        let downloaded = 0;
        const chunks = await Promise.all(parts.map(async (part) => {
            const url = 'assets/' + part;
            const buf = await fetchCached(cache, url);
            downloaded++;
            const pct = Math.round((downloaded / totalParts) * 100);
            setStatus(`Downloading asset pack: ${downloaded} / ${totalParts} (${pct}%)`);
            return buf;
        }));

        setStatus('Assembling asset archive…');
        await new Promise(r => setTimeout(r, 0));

        let totalLen = 0;
        for (const c of chunks) totalLen += c.byteLength;
        const archive = new Uint8Array(totalLen);
        let offset = 0;
        for (const c of chunks) {
            archive.set(new Uint8Array(c), offset);
            offset += c.byteLength;
        }

        // Verify TSPK header
        const view = new DataView(archive.buffer, archive.byteOffset, archive.byteLength);
        const magic = String.fromCharCode(view.getUint8(0), view.getUint8(1), view.getUint8(2), view.getUint8(3));
        if (magic !== 'TSPK') {
            throw new Error('Invalid pack magic: ' + magic);
        }

        const entryCount = view.getUint32(4, true);
        let pos = 8;
        const decoder = new TextDecoder('utf-8');
        const entries = [];

        for (let i = 0; i < entryCount; i++) {
            const pathLen = view.getUint16(pos, true);
            pos += 2;
            const pathBytes = new Uint8Array(archive.buffer, archive.byteOffset + pos, pathLen);
            const relPath = decoder.decode(pathBytes);
            pos += pathLen;
            const payloadOffset = view.getUint32(pos, true);
            pos += 4;
            const dataLen = view.getUint32(pos, true);
            pos += 4;
            entries.push({ relPath, payloadOffset, dataLen });
        }

        const payloadBase = pos;
        const createdDirs = new Set(['/Content']);
        ensureDirectoryExists(FS, '/Content');
        setStatus(`Extracting assets: 0 / ${entries.length} (0%)`);

        for (let i = 0; i < entries.length; i++) {
            const entry = entries[i];
            const fileSlice = new Uint8Array(archive.buffer, archive.byteOffset + payloadBase + entry.payloadOffset, entry.dataLen);
            const virtPath = '/Content/' + entry.relPath;
            const dir = getDirname(virtPath);
            if (!createdDirs.has(dir)) {
                if (typeof FS.mkdirTree === 'function') {
                    try { FS.mkdirTree(dir); } catch (_) {}
                } else {
                    ensureDirectoryExists(FS, dir);
                }
                createdDirs.add(dir);
            }
            FS.writeFile(virtPath, fileSlice);

            if (i % 100 === 0 || i === entries.length - 1) {
                const pct = Math.round(((i + 1) / entries.length) * 100);
                setStatus(`Extracting assets: ${i + 1} / ${entries.length} (${pct}%)`);
                await new Promise(r => setTimeout(r, 0));
            }
        }

        console.log(`[Timespinner] Preloaded and extracted ${entries.length} Content assets into MEMFS.`);
        return;
    }

    // 2. Fallback: single content.pack without json manifest
    let singlePackBuf = null;
    try {
        singlePackBuf = await fetchCached(cache, 'assets/content.pack');
    } catch (_) {}

    if (singlePackBuf && singlePackBuf.byteLength >= 8) {
        setStatus('Extracting asset pack…');
        const archive = new Uint8Array(singlePackBuf);
        const view = new DataView(archive.buffer, archive.byteOffset, archive.byteLength);
        const magic = String.fromCharCode(view.getUint8(0), view.getUint8(1), view.getUint8(2), view.getUint8(3));
        if (magic === 'TSPK') {
            const entryCount = view.getUint32(4, true);
            let pos = 8;
            const decoder = new TextDecoder('utf-8');
            const entries = [];
            for (let i = 0; i < entryCount; i++) {
                const pathLen = view.getUint16(pos, true);
                pos += 2;
                const pathBytes = new Uint8Array(archive.buffer, archive.byteOffset + pos, pathLen);
                const relPath = decoder.decode(pathBytes);
                pos += pathLen;
                const payloadOffset = view.getUint32(pos, true);
                pos += 4;
                const dataLen = view.getUint32(pos, true);
                pos += 4;
                entries.push({ relPath, payloadOffset, dataLen });
            }
            const payloadBase = pos;
            ensureDirectoryExists(FS, '/Content');
            for (let i = 0; i < entries.length; i++) {
                const entry = entries[i];
                const fileSlice = new Uint8Array(archive.buffer, archive.byteOffset + payloadBase + entry.payloadOffset, entry.dataLen);
                const virtPath = '/Content/' + entry.relPath;
                const dir = getDirname(virtPath);
                if (typeof FS.mkdirTree === 'function') {
                    try { FS.mkdirTree(dir); } catch (_) {}
                } else {
                    ensureDirectoryExists(FS, dir);
                }
                FS.writeFile(virtPath, fileSlice);
            }
            console.log(`[Timespinner] Extracted ${entries.length} assets from single content.pack into MEMFS.`);
            return;
        }
    }

    // 3. Fallback: Legacy loose files from Content/manifest.json
    let contentFiles = [];
    try {
        const r = await fetch('Content/manifest.json', { cache: 'no-store' });
        if (r.ok) contentFiles = await r.json();
    } catch (_) {}

    if (contentFiles.length > 0) {
        ensureDirectoryExists(FS, '/Content');
        const total = contentFiles.length;
        setStatus(`Loading: 0 / ${total} (0%)`);
        let count = 0;
        const CONC = 8;
        let nextIdx = 0;

        async function worker() {
            while (nextIdx < contentFiles.length) {
                const idx = nextIdx++;
                const rel = contentFiles[idx];
                const url = 'Content/' + rel;
                const virt = '/Content/' + rel;
                ensureDirectoryExists(FS, getDirname(virt));
                try {
                    const buf = await fetchCached(cache, url);
                    FS.writeFile(virt, new Uint8Array(buf));
                } catch (e) {
                    console.warn('[Timespinner] Asset load failed:', url, e);
                }
                count++;
                if (count % 10 === 0 || count === total) {
                    const pct = Math.round((count / total) * 100);
                    setStatus(`Loading: ${count} / ${total} (${pct}%)`);
                    await new Promise(r => setTimeout(r, 0));
                }
            }
        }

        const workers = [];
        for (let i = 0; i < Math.min(CONC, contentFiles.length); i++) workers.push(worker());
        await Promise.all(workers);
        console.log('[Timespinner] Preloaded', contentFiles.length, 'Content assets into MEMFS.');
    } else {
        console.log('[Timespinner] No asset pack or Content/manifest.json found.');
    }
}

// ---------------------------------------------------------------------------
// 5. Save Data Management (IDBFS + IndexedDB Direct Redundancy)
// ---------------------------------------------------------------------------
const SAVE_DB_NAME    = 'timespinner-save-db';
const SAVE_STORE_NAME = 'saves';
let isIDBFSMounted  = false;
let isSyncing       = false;
let saveIsDirty     = false;

function openSaveDB() {
    return new Promise(resolve => {
        if (typeof indexedDB === 'undefined') { resolve(null); return; }
        try {
            const req = indexedDB.open(SAVE_DB_NAME, 1);
            req.onupgradeneeded = e => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(SAVE_STORE_NAME))
                    db.createObjectStore(SAVE_STORE_NAME);
            };
            req.onsuccess  = () => resolve(req.result);
            req.onerror    = () => resolve(null);
        } catch (_) { resolve(null); }
    });
}

async function restoreSavesFromIDB(FS) {
    const db = await openSaveDB();
    if (!db) return;
    return new Promise(resolve => {
        try {
            const tx    = db.transaction(SAVE_STORE_NAME, 'readonly');
            const store = tx.objectStore(SAVE_STORE_NAME);
            const req   = store.openCursor();
            req.onsuccess = e => {
                const cursor = e.target.result;
                if (cursor) {
                    const path = cursor.key;
                    // Prevent cross-port save contamination
                    if (path.includes('Phobia') || path.includes('Carrion') || path.includes('SuperBloodHockey')) {
                        cursor.continue();
                        return;
                    }
                    const data = cursor.value;
                    try {
                        ensureDirectoryExists(FS, getDirname(path));
                        FS.writeFile(path, data);
                        console.log('[Timespinner] Restored save:', path);
                    } catch (err) {
                        console.warn('[Timespinner] Restore save failed:', path, err);
                    }
                    cursor.continue();
                } else {
                    resolve();
                }
            };
            req.onerror = () => resolve();
        } catch (err) {
            console.warn('[Timespinner] IDB restore error:', err);
            resolve();
        }
    });
}

function collectFiles(FS, dir) {
    let results = [];
    try {
        if (typeof FS.analyzePath === 'function' && !FS.analyzePath(dir).exists) return results;
        const entries = FS.readdir(dir);
        for (const entry of entries) {
            if (entry === '.' || entry === '..') continue;
            const full = (dir === '/' ? '/' : dir + '/') + entry;
            try {
                const stat = FS.stat(full);
                if (FS.isDir(stat.mode)) {
                    results = results.concat(collectFiles(FS, full));
                } else if (FS.isFile(stat.mode)) {
                    results.push(full);
                }
            } catch (_) {}
        }
    } catch (_) {}
    return results;
}

async function persistSavesToIDB(FS, force = false) {
    if (!FS || isSyncing) return;
    if (!saveIsDirty && !force) return;
    isSyncing = true;
    try {
        if (isIDBFSMounted && typeof FS?.syncfs === 'function' && FS?.filesystems?.IDBFS) {
            await new Promise(resolve => {
                FS.syncfs(false, err => {
                    if (err) console.warn('[Timespinner] IDBFS flush error:', err);
                    resolve();
                });
            });
        }

        const db = await openSaveDB();
        if (!db) { saveIsDirty = false; return; }

        let saveFiles = collectFiles(FS, '/save');
        saveFiles = saveFiles.filter(p => !p.includes('Phobia') && !p.includes('Carrion') && !p.includes('SuperBloodHockey'));
        if (saveFiles.length === 0) { saveIsDirty = false; return; }

        await new Promise(resolve => {
            try {
                const tx    = db.transaction(SAVE_STORE_NAME, 'readwrite');
                const store = tx.objectStore(SAVE_STORE_NAME);
                for (const filePath of saveFiles) {
                    try {
                        const data = FS.readFile(filePath);
                        store.put(data, filePath);
                    } catch (err) {
                        console.warn('[Timespinner] Save write failed:', filePath, err);
                    }
                }
                tx.oncomplete = () => resolve();
                tx.onerror    = () => resolve();
            } catch (err) {
                console.warn('[Timespinner] IDB persist error:', err);
                resolve();
            }
        });
        saveIsDirty = false;
    } catch (err) {
        console.warn('[Timespinner] persistSavesToIDB error:', err);
    } finally {
        isSyncing = false;
    }
}

async function mountSave(FS) {
    if (!FS) {
        console.warn('[Timespinner] FS not available, skipping save mount.');
        return;
    }

    ensureDirectoryExists(FS, '/save');
    ensureDirectoryExists(FS, '/save/Timespinner');

    const origWrite = FS.writeFile;
    if (origWrite && !FS._timespinnerSaveHookInstalled) {
        FS._timespinnerSaveHookInstalled = true;
        FS.writeFile = function (path, data, options) {
            if (typeof path === 'string' && path.startsWith('/save')) {
                saveIsDirty = true;
            }
            return origWrite.call(this, path, data, options);
        };
    }

    if (typeof FS.mount === 'function' && FS.filesystems?.IDBFS) {
        try {
            FS.mount(FS.filesystems.IDBFS, {}, '/save');
            await new Promise(resolve => {
                FS.syncfs(true, err => {
                    if (err) console.warn('[Timespinner] IDBFS initial sync error:', err);
                    resolve();
                });
            });
            isIDBFSMounted = true;
            console.log('[Timespinner] /save mounted to IDBFS.');
        } catch (e) {
            console.warn('[Timespinner] IDBFS mount failed, using IDB direct mode:', e);
        }
    }

    console.log('[Timespinner] Restoring saves from IndexedDB…');
    await restoreSavesFromIDB(FS);
}

// ---------------------------------------------------------------------------
// 6. Runtime Bootstrap
// ---------------------------------------------------------------------------
let currentRafId = null;
let isRunning    = false;

window.notifyGameReady = () => {
    const loader = document.getElementById('loading');
    if (loader) {
        loader.classList.add('hidden');
        loader.style.opacity = '0';
        loader.style.pointerEvents = 'none';
        loader.style.display = 'none';
    }
};

try {
    const runtime = await dotnet
        .withEnvironmentVariable('FNA_PLATFORM_BACKEND', 'SDL2')
        .withDiagnosticTracing(false)
        .withModuleConfig({ canvas: canvas })
        .create();

    if (runtime.Module && canvas) runtime.Module.canvas = canvas;
    if (canvas) globalThis.Module.canvas = canvas;

    if (typeof runtime.setModuleImports === 'function') {
        runtime.setModuleImports('main.js', {
            setMainLoop: (cb) => {
                if (isRunning) return;
                isRunning = true;
                console.log('[Timespinner JS] setMainLoop: Starting single rAF game loop.');
                if (currentRafId !== null) cancelAnimationFrame(currentRafId);

                function step() {
                    try {
                        cb();
                    } catch (err) {
                        console.error('[Timespinner JS loop frame error]', err);
                    }
                    _fpsFrameCount++;
                    currentRafId = requestAnimationFrame(step);
                }
                currentRafId = requestAnimationFrame(step);
            },

            notifyGameReady: () => {
                console.log('[Timespinner JS] notifyGameReady: First frame rendered!');
                window._gameReady = true;
                const loader = document.getElementById('loading');
                if (loader) {
                    loader.classList.add('hidden');
                    loader.style.opacity = '0';
                    loader.style.pointerEvents = 'none';
                    loader.style.display = 'none';
                }
                if (canvas) {
                    canvas.focus();
                }
            },
        });

        window.addEventListener('keydown', () => {
            if (document.activeElement !== canvas && canvas) {
                canvas.focus();
            }
        });
    }

    const FS = runtime.Module?.FS || runtime.FS || globalThis.Module?.FS;

    setStatus('Mounting save storage…');
    await mountSave(FS);

    setStatus('Pre-loading game assets…');
    await preloadAssets(FS);

    const flushSaves = (force = false) => {
        try { persistSavesToIDB(FS, force); } catch (_) {}
    };
    setInterval(() => flushSaves(false), 10000);
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'hidden') flushSaves(true);
    });
    window.addEventListener('beforeunload', () => flushSaves(true));

    setStatus('Starting game engine…');
    await new Promise(r => setTimeout(r, 50));

    await dotnet.run();

} catch (err) {
    console.error('[Timespinner Fatal Error]', err);
    if (progressEl) progressEl.textContent = 'Fatal Error: ' + (err.message || err);
}
