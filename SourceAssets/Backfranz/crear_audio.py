"""Render the Backfranz sound bank from the CC0 sources listed in AUDIO.md."""

import argparse
import math
from pathlib import Path

import numpy as np
from scipy import signal
import soundfile as sf


RATE = 48_000
OUT = Path(__file__).resolve().parents[2] / "Assets/Resources/Backfranz/Audio"
RNG = np.random.default_rng(7719)


def seconds(value):
    return round(value * RATE)


def load(path, stereo=False):
    data, rate = sf.read(path, always_2d=True)
    if rate != RATE:
        divisor = math.gcd(rate, RATE)
        data = signal.resample_poly(data, RATE // divisor, rate // divisor, axis=0)
    if stereo:
        return data[:, :2] if data.shape[1] >= 2 else np.repeat(data, 2, axis=1)
    return data.mean(axis=1)


def filter_band(data, low, high):
    band = signal.butter(2, [low, high], fs=RATE, btype="bandpass", output="sos")
    return signal.sosfiltfilt(band, data, axis=0)


def level(data, db):
    return data * (10 ** (db / 20) / max(1e-8, np.max(np.abs(data))))


def trim(data, duration, start=0):
    return data[seconds(start):seconds(start + duration)].copy()


def blank(duration, stereo=False):
    return np.zeros((seconds(duration), 2) if stereo else seconds(duration))


def place(track, sound, start, gain=1):
    offset = seconds(start)
    count = min(len(sound), len(track) - offset)
    track[offset:offset + count] += sound[:count] * gain


def soft_edges(data, attack=.006, release=.065):
    begin = min(seconds(attack), len(data) // 4)
    end = min(seconds(release), len(data) // 4)
    data[:begin] *= np.linspace(0, 1, begin).reshape((-1,) + (1,) * (data.ndim - 1))
    data[-end:] *= np.linspace(1, 0, end).reshape((-1,) + (1,) * (data.ndim - 1))
    return data


def emit(name, track, peak, loop=False, ogg=False, rms=None):
    track = track - track.mean(axis=0)
    if not loop:
        track = soft_edges(track)
    if rms is None:
        track = level(track, peak)
    else:
        ceiling = 10 ** (peak / 20)
        energy = np.sqrt(np.mean(track ** 2))
        track = ceiling * np.tanh(track * 10 ** (rms / 20) / max(1e-8, energy * ceiling))
    path = OUT / (name + (".ogg" if ogg else ".wav"))
    channels = 1 if track.ndim == 1 else track.shape[1]
    with sf.SoundFile(path, "w", RATE, channels, "VORBIS" if ogg else "PCM_16") as output:
        for offset in range(0, len(track), 4096):
            output.write(track[offset:offset + 4096].astype(np.float32))


def room_echo(mono, wet=.15):
    stereo = np.repeat(mono[:, None], 2, axis=1)
    for index, delay in enumerate((.037, .053)):
        offset = seconds(delay)
        stereo[offset:, index] += mono[:-offset] * wet
        stereo[offset * 2:, 1 - index] += mono[:-offset * 2] * wet * .47
    return stereo


def stretch(data, amount):
    numerator = round(amount * 100)
    return signal.resample_poly(data, numerator, 100, axis=0)


def loop_join(data, duration, overlap=1.5):
    length, fade = seconds(duration), seconds(overlap)
    repeated = np.concatenate([data] * math.ceil((length + fade) / len(data)))
    result = repeated[:length].copy()
    mix = np.linspace(0, 1, fade)[:, None]
    result[:fade] = result[:fade] * mix + repeated[length:length + fade] * (1 - mix)
    return result


def bell(hz, duration, decay=.21, attack=.014):
    t = np.arange(seconds(duration)) / RATE
    harmonic = .79 * np.sin(2 * np.pi * hz * t)
    harmonic += .22 * np.sin(2 * np.pi * hz * 2.004 * t) * np.exp(-t / .18)
    harmonic += .10 * np.sin(2 * np.pi * hz * 3.01 * t) * np.exp(-t / .08)
    return harmonic * (1 - np.exp(-t / attack)) * np.exp(-t / decay)


def voice_pulse(hz, formant, duration):
    t = np.arange(seconds(duration)) / RATE
    phase = 2 * np.pi * hz * (t - .11 * t*t / duration)
    pulse = np.zeros_like(t)
    for harmonic in range(1, 10):
        frequency = harmonic * hz
        resonance = np.exp(-.5 * ((frequency - formant) / 185) ** 2)
        pulse += (.19 + resonance) * np.sin(harmonic * phase) / harmonic ** .7
    envelope = np.sin(np.pi * t / duration) ** 1.4
    return pulse * envelope


def dialogue_voices():
    emit("dialogo-yamil", voice_pulse(173, 570, .051), -12)
    emit("dialogo-persona", voice_pulse(228, 770, .048), -13)


def auditorium_recording():
    tape = blank(1.55)
    for start, hz, formant, duration in (
        (.16, 182, 960, .19), (.34, 176, 710, .20), (.55, 162, 530, .22),
        (.86, 169, 1080, .16), (1.02, 152, 630, .25),
    ):
        place(tape, voice_pulse(hz, formant, duration), start)
    hiss = np.random.default_rng(412).normal(size=len(tape))
    tape += .14 * filter_band(hiss, 780, 3200)
    distorted = np.tanh(filter_band(tape, 260, 2800) * 2.5)
    emit("auditorio-grabadora", room_echo(distorted, .28), -12)


def footsteps(cache):
    for i, metal in enumerate((1, 4, 5, 8, 9)):
        for target, material, cutoff, peak in (
            ("baldosa", "concrete", 7700, -12),
            ("madera", "wood", 4400, -13),
            ("alfombra", "carpet", 2200, -16),
            ("adoquin", "concrete", 6000, -12),
        ):
            step = load(cache / "impact/Audio" / f"footstep_{material}_{i:03}.ogg")
            step = level(filter_band(step, 86, cutoff), -10)
            if target == "adoquin":
                gravel = load(cache / "surface/gravel.ogg")
                grain = trim(gravel, .085, (.05, .11, .18, .25, .32)[i])
                place(step, soft_edges(level(grain, -19), .009, .026), .034)
            emit(f"paso-{target}-{i+1}", step, peak, rms=-34 if target=="alfombra" else -30)
        steel = filter_band(load(cache / "metal" / f"metal_steps_{metal:02}.wav"), 95, 8200)
        emit(f"paso-metal-{i+1}", steel, -12.5, rms=-31)


def effects(cache):
    def foley(name):
        return load(cache / "foley" / (name + ".ogg"))

    move = trim(foley("switch_01"), .095)
    emit("ui-mover", filter_band(move, 220, 4100), -18)
    emit("ui-volver", filter_band(stretch(move, 1.35), 185, 3600), -17)
    confirm = blank(.43)
    place(confirm, level(move, -17), 0)
    place(confirm, bell(493.88, .35, .095) * .52, .027)
    place(confirm, bell(659.25, .27, .09) * .28, .085)
    emit("ui-confirmar", confirm, -14)
    dialogue_voices()

    for name, source, peak, cutoff in (
        ("puerta-vidrio", "door_open", -9, 6200),
        ("puerta-madera", "wooded_box_open", -10, 4600),
        ("cerrojo", "key_open_02", -10, 5800),
        ("objeto-recoger", "paper_02", -13, 4800),
        ("diario-abrir", "paper_01", -14, 5800),
        ("diario-cerrar", "paper_03", -14, 4800),
        ("cafe-taza-1", "dishes_01", -21, 4700),
        ("cafe-taza-2", "dishes_03", -21, 4900),
        ("tablero-interruptor", "switch_02", -11, 7200),
        ("ascensor-campana", "bell_01", -11, 5500),
        ("placa-paso", "spring_03", -14, 4200),
    ):
        emit(name, filter_band(foley(source), 80, cutoff), peak)

    elevator = load(cache / "elevator.wav", stereo=True)
    emit("ascensor-abre", filter_band(stretch(trim(elevator, 1.72, .24), .78), 55, 6500), -10)
    close = blank(1.05, stereo=True)
    place(close, level(filter_band(trim(elevator, .63, .93)[::-1], 65, 4500), -16), 0)
    place(close, level(room_echo(foley("door_close_02")), -9), .34)
    emit("ascensor-cierra", close, -9)
    machinery = load(cache / "old-lift.mp3", stereo=True)
    emit("puerta-metal", filter_band(trim(machinery, 1.65, .23), 60, 5100), -8)
    emit("ascensor-motor", filter_band(stretch(trim(machinery, 1.38, .45), 1.18), 45, 360), -12)

    clank = load(cache / "clank.wav")
    emit("mecanismo-liberado", room_echo(filter_band(stretch(clank, 1.20), 60, 5200)), -10)
    for name, notes in (
        ("lector-escaneo", (466.16, 698.46)),
        ("lector-permitido", (659.25, 987.77)),
        ("lector-rechazado", (246.94, 207.65)),
        ("partida-guardada", (392, 523.25, 659.25)),
        ("rescate-reencuentro", (293.66, 440, 587.33)),
    ):
        cue = blank(.81)
        for i, hz in enumerate(notes):
            place(cue, bell(hz, .50, .13), i * .087)
        emit(name, room_echo(cue, .12), -14)

    static = filter_band(RNG.normal(size=seconds(.46)), 260, 2600)
    emit("archivo-reproduccion", soft_edges(static, .03, .11), -15)
    emit("placa-interferencia", room_echo(filter_band(stretch(foley("spring_06"), 1.4), 80, 3400)), -12)
    emit("terror-interferencia", room_echo(stretch(soft_edges(static, .018, .14), 1.17), .25), -18)
    emit("terror-gota", room_echo(filter_band(foley("plop_01"), 110, 4500), .24), -18)
    squeak = filter_band(stretch(load(cache / "squeak.wav"), 2.8), 70, 2400)
    emit("terror-raspado", room_echo(squeak, .26), -16)
    emit("terror-paso-lejano", room_echo(filter_band(stretch(foley("slam_02"), 1.7), 65, 580), .28), -18)
    knock = blank(1.65)
    hit = level(filter_band(stretch(clank, 2.25), 44, 330), -10)
    place(knock, hit, .04)
    place(knock, hit, .42, .62)
    emit("golpe-tras-muro", room_echo(knock, .31), -12)

    pulse = filter_band(load(cache / "heartbeat.flac"), 36, 460)
    emit("sombra-alerta", room_echo(trim(pulse, 1.53, .3), .21), -10)
    caught = blank(1.72)
    place(caught, hit, .023)
    place(caught, trim(pulse, 1.1, .15), .09)
    place(caught, squeak[::-1], .11, .58)
    emit("sombra-retorno", room_echo(caught, .28), -8)

    descent = blank(2.65, stereo=True)
    place(descent, level(filter_band(stretch(trim(machinery, 1.4, .48), 1.65), 45, 470), -14), 0)
    place(descent, level(room_echo(hit, .31), -10), 1.84)
    emit("descenso-umbral", descent, -8)


def rooms(cache):
    street = load(cache / "street.ogg", stereo=True)
    coffee = load(cache / "coffee.ogg", stereo=True)
    air = load(cache / "outside.ogg", stereo=True)
    fridge_path = cache / "rooms" / "TheShopCollection_convenience_store_drinks_fridge_drone_2.wav"
    # Equal-duration forward and reverse passes provide enough material for a crossfaded loop.
    fridge = load(fridge_path, stereo=True)
    plant = np.concatenate((fridge, fridge[::-1]))
    for name, source, low, high, length, peak, ogg in (
        ("landaeta", trim(street, 25.5, 15), 43, 5900, 24, -16, False),
        ("campus", trim(coffee, 22, 42), 130, 775, 20, -23, False),
        ("backfranz", stretch(air[::-1], 1.18), 35, 405, 22, -22, False),
        ("mantenimiento", stretch(plant, 1.28), 43, 1680, 22, -19, False),
        ("galeria", stretch(air[::-1], 1.58), 32, 780, 24, -21, False),
        ("gradas", trim(street, 25.5, 68), 80, 470, 24, -25, True),
        ("cabina", plant, 72, 890, 22, -23, True),
        ("cabina-alterada", stretch(plant[::-1], 1.71), 39, 1110, 24, -21, True),
        ("oficinas", trim(coffee, 21.5, 109), 170, 1110, 20, -27, True),
        ("auditorio", air, 36, 330, 24, -26, True),
        ("juegos", plant, 110, 2900, 22, -24, True),
        ("cafe", trim(coffee, 29.5, 66), 110, 5600, 28, -18, True),
        ("servicio", plant, 85, 1850, 22, -24, True),
        ("aula", trim(coffee, 21.5, 130), 154, 625, 20, -26, True),
        ("silencio", air, 50, 375, 24, -29, True),
        ("computo", plant[::-1], 115, 1470, 22, -25, True),
        ("electricidad-alterada", stretch(plant[::-1], .94), 75, 3100, 22, -21, True),
        ("servicio-alterado", stretch(plant, 1.45), 46, 1150, 24, -23, True),
        ("silencio-alterado", stretch(air[::-1], 2.0), 38, 520, 26, -25, True),
    ):
        bed = loop_join(filter_band(source, low, high), length)
        density = -32 if name in ("landaeta", "cafe", "mantenimiento") else min(-33, peak-11)
        emit("ambiente-" + name, bed, peak, loop=True, ogg=ogg, rms=density)


def themes():
    # The menu motif resolves into three different cadences in the endings.
    score = blank(32, stereo=True)
    notes = (220, 261.63, 329.63, 293.66, 207.65, 246.94, 311.13, 293.66)
    for i, hz in enumerate(notes):
        at = i * 4 + .22
        place(score, room_echo(bell(hz / 2, 3.5, 1.0, .075)), at, .60)
        place(score, room_echo(bell(hz, 2.9, .68, .036), .25), at+.46, .37)
        place(score, room_echo(bell(hz * 1.4983, 2.2, .76, .041), .21), at+1.49, .25)
    emit("musica-menu", score, -17, loop=True, ogg=True)
    for name, sequence in (
        ("amanecer", (261.63, 329.63, 392, 523.25)),
        ("eco", (220, 293.66, 349.23, 261.63)),
        ("umbral", (220, 207.65, 185, 146.83)),
    ):
        result = blank(4.4, stereo=True)
        for i, hz in enumerate(sequence):
            place(result, room_echo(bell(hz, 2.1, .49, .045), .25), .05+i*.56)
        emit("final-"+name, result, -13)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("sources", type=Path, help="Carpeta de grabaciones descrita en AUDIO.md")
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    footsteps(args.sources)
    effects(args.sources)
    auditorium_recording()
    rooms(args.sources)
    themes()


if __name__ == "__main__":
    main()
