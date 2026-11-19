# Generates the chiptune sound effects and music loops as WAV files.
# Run from the repo root:  python tools/generate_audio.py
# Needs NumPy:  pip install numpy
# AudioManager plays any file in Resources/Audio by name, e.g. AudioManager.Play("knock").
import os, sys, wave
import numpy as np

OUT = sys.argv[1] if len(sys.argv) > 1 else "LastDelivery/Assets/Resources/Audio"
os.makedirs(OUT, exist_ok=True)
SR = 22050
rng = np.random.default_rng(7)

def t(sec): return np.arange(int(SR * sec)) / SR

def square(f, sec, duty=0.5):
    ph = np.cumsum(np.full(int(SR * sec), f) if np.isscalar(f) else f) / SR
    return np.where((ph % 1.0) < duty, 1.0, -1.0)

def tri(f, sec):
    ph = (np.arange(int(SR * sec)) * f / SR) % 1.0
    return 4 * np.abs(ph - 0.5) - 1

def env(n, a=0.005, r=0.08):
    e = np.ones(n)
    na, nr = int(SR * a), int(SR * r)
    if na: e[:na] = np.linspace(0, 1, na)
    if nr: e[-nr:] *= np.linspace(1, 0, nr)
    return e

def decay(n, k=6.0): return np.exp(-k * np.arange(n) / n)

def noise(sec): return rng.uniform(-1, 1, int(SR * sec))

def lowpass(x, a=0.15):
    y = np.zeros_like(x); acc = 0.0
    for i, v in enumerate(x):
        acc += a * (v - acc); y[i] = acc
    return y

def save(name, x, vol=0.6):
    x = x / (np.max(np.abs(x)) + 1e-9) * vol
    data = (x * 32767).astype(np.int16)
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data.tobytes())

def note(f, sec, kind="sq", duty=0.5, k=4.0):
    n = int(SR * sec)
    w = square(f, sec, duty) if kind == "sq" else tri(f, sec)
    return w * decay(n, k) * env(n, 0.003, 0.02)

def cat(*xs): return np.concatenate(xs)
def silence(sec): return np.zeros(int(SR * sec))
def hz(m): return 440.0 * 2 ** ((m - 69) / 12)

# ---- SFX ----
save("pickup", square(np.linspace(600, 1300, int(SR * 0.14)), 0.14, 0.25) * env(int(SR * 0.14), 0.002, 0.05), 0.45)

thump = lambda: lowpass(noise(0.09), 0.08) * decay(int(SR * 0.09), 8) + tri(90, 0.09) * decay(int(SR * 0.09), 10) * 0.8
save("knock", cat(thump(), silence(0.09), thump(), silence(0.05)), 0.8)

save("deliver_good", cat(*[note(hz(m), 0.09, "sq", 0.25, 3) for m in (72, 76, 79, 84)], note(hz(88), 0.3, "sq", 0.25, 4)), 0.4)
save("deliver_bad", cat(*[note(hz(m), 0.18, "tri", k=2) for m in (67, 63, 60)], note(hz(55), 0.45, "tri", k=3)), 0.55)
save("wrong", cat(note(140, 0.12, "sq", 0.5, 1), silence(0.05), note(120, 0.18, "sq", 0.5, 1)), 0.35)
save("note", lowpass(noise(0.32), 0.35) * np.sin(np.linspace(0, np.pi, int(SR * 0.32))) ** 2, 0.4)
save("clockout", cat(note(hz(79), 0.18, "tri", k=2), note(hz(84), 0.5, "tri", k=2.5)), 0.5)
crash_n = int(SR * 0.6)
save("crash", noise(0.6) * decay(crash_n, 5) + tri(70, 0.6) * decay(crash_n, 7), 0.7)
wob = np.sin(2 * np.pi * np.cumsum(200 + 40 * np.sin(2 * np.pi * 9 * t(0.45))) / SR)
save("wobble", wob * env(len(wob), 0.02, 0.1), 0.4)
save("start", cat(*[note(hz(m), 0.1, "sq", 0.25, 2) for m in (60, 64, 67)], note(hz(72), 0.35, "sq", 0.25, 3)), 0.4)

# ---- music ----
def song(bpm, chords, melody, bass_oct=36, lead_duty=0.25, detune=0.0, vol=0.5, hats=True):
    beat = 60 / bpm
    step = beat / 2                      # eighth notes
    bars = len(chords)
    total = int(SR * beat * 4 * bars)
    mix = np.zeros(total)
    def put(x, start):
        s = int(SR * start); e = min(total, s + len(x)); mix[s:e] += x[:e - s]
    for b, (root, third, fifth) in enumerate(chords):
        bar0 = b * beat * 4
        for q in range(4):               # bass on every beat: root, fifth, root, fifth
            m = bass_oct + (root if q % 2 == 0 else fifth)
            put(note(hz(m), beat * 0.9, "tri", k=2) * 0.7, bar0 + q * beat)
        for q in range(8):               # soft chord arpeggio
            m = 60 + [root, third, fifth, third][q % 4]
            put(note(hz(m), step * 0.9, "sq", 0.125, 5) * 0.12, bar0 + q * step)
        if hats:
            for q in range(8):
                h = lowpass(noise(0.03), 0.9) - lowpass(noise(0.03), 0.3)
                put(h * decay(len(h), 9) * (0.12 if q % 2 else 0.2), bar0 + q * step)
    for i, m in enumerate(melody):       # one melody note per eighth (None = rest, negative = hold)
        if m is None: continue
        length = step * (2 if m < 0 else 1)
        m = abs(m)
        x = note(hz(m), length * 0.95, "sq", lead_duty, 2.5) * 0.35
        if detune:
            x = x + note(hz(m) * (1 + detune), length * 0.95, "sq", lead_duty, 2.5) * 0.25
        put(x, i * step)
    return mix, vol

C, Am, F, G, Dm, E = (0, 4, 7), (9, 12, 16), (5, 9, 12), (7, 11, 14), (2, 5, 9), (4, 8, 11)
# Monday: bright, C major
calm_chords = [C, Am, F, G, C, Am, F, G]
calm_mel = [72, None, 76, 79, -81, None, 79, 76,   76, None, 72, 74, -76, None, None, None,
            77, None, 76, 74, -72, None, 69, 72,   74, None, 76, 74, -79, None, None, None,
            72, None, 76, 79, -84, None, 81, 79,   76, None, 79, 76, -72, None, 69, None,
            72, None, 74, 76, 77, None, 76, 74,    -72, None, None, None, None, None, None, None]
mix, v = song(118, calm_chords, calm_mel)
save("music_calm", mix, v)

# Tuesday onward: slower, minor, slightly detuned lead (the memory going wrong)
uneasy_chords = [Am, F, Dm, E, Am, F, Dm, E]
uneasy_mel = [69, None, 72, None, -76, None, 74, 72,   -72, None, 71, None, -69, None, None, None,
              65, None, 69, None, -72, None, 71, 69,   -68, None, None, None, -64, None, None, None,
              69, None, 72, 76, -77, None, 76, 74,     72, None, 74, 72, -71, None, 68, None,
              -69, None, None, None, None, None, None, None,  None, None, None, None, None, None, None, None]
mix, v = song(92, uneasy_chords, uneasy_mel, lead_duty=0.5, detune=0.012, vol=0.45, hats=False)
save("music_uneasy", mix, v)

print(sorted(os.listdir(OUT)))
