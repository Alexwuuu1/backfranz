"""Native 16 px surfaces and wall-mounted props for the campus maps."""
from random import Random


MATERIALS = (
    ('vereda', '#aa9d90', '#97897d', '#b8ac9d', 'stone'),
    ('terrazo', '#c7b8a4', '#b2a18f', '#dacbb8', 'terrazzo'),
    ('oficina', '#b9afa3', '#a6998c', '#cbbfb1', 'stone'),
    ('laboratorio', '#a6b9b7', '#909f9e', '#bfd0cb', 'seam'),
    ('computo', '#879397', '#6e7d84', '#a0adb1', 'raised'),
    ('linoleo', '#bbab8d', '#a9997d', '#d0bc9b', 'seam'),
    ('taller', '#a29c8f', '#8a8275', '#b6ad97', 'concrete'),
    ('roble', '#a67e59', '#8f684b', '#b9936b', 'wood'),
    ('cocina', '#cbc3aa', '#aaa48f', '#e0d5b9', 'ceramic'),
    ('bano', '#a3b8b0', '#819b96', '#c1d1c8', 'ceramic'),
    ('cafe', '#c3a687', '#987a62', '#d8bd9f', 'encaustic'),
    ('auditorio', '#754c52', '#664148', '#86565d', 'weave'),
    ('juegos', '#61546f', '#554a65', '#776685', 'fleck'),
    ('acustico', '#5e6a76', '#535e69', '#707c87', 'weave'),
    ('metal', '#777f80', '#626e71', '#929c9b', 'metal'),
    ('subsuelo', '#787c70', '#5e645c', '#8c8d7e', 'concrete'),
    ('rejilla', '#535b59', '#394244', '#7b8581', 'grate'),
)


def draw_surface(art, name, base, seam, light, pattern):
    art.rect((0, 0, 15, 15), base)
    rng = Random(name)
    if pattern in ('stone', 'terrazzo', 'raised', 'seam'):
        art.line((0, 15, 15, 15), seam)
        if pattern != 'seam':
            art.line((15, 0, 15, 14), seam)
        if pattern in ('stone', 'raised'):
            art.line((1, 0, 13, 0), light)
        if pattern == 'raised':
            for x, y in ((1, 2), (13, 2), (1, 13), (13, 13)):
                art.rect((x, y, x, y), seam, layer=2)
    if pattern in ('stone', 'seam', 'terrazzo', 'concrete', 'fleck'):
        count = 23 if pattern == 'terrazzo' else 10
        for _ in range(count):
            x, y = rng.randrange(1, 14), rng.randrange(1, 14)
            art.line((x, y, x + rng.randrange(2), y), rng.choice((seam, light)), layer=2)
    if pattern == 'wood':
        for y, joints in ((0, 11), (5, 3), (11, 7)):
            art.line((0, y, 15, y), seam)
            art.line((0, y + 1, 15, y + 1), light)
            art.line((joints, y + 2, joints, min(y + 4, 15)), seam)
            for _ in range(3):
                x = rng.randrange(1, 12)
                art.line((x, y + 3, x + rng.randrange(2, 5), y + 3), seam, layer=2)
    if pattern in ('ceramic', 'encaustic'):
        step = 8 if pattern == 'ceramic' else 16
        for t in range(step - 1, 16, step):
            art.line((t, 0, t, 15), seam)
            art.line((0, t, 15, t), seam)
        if pattern == 'encaustic':
            art.poly(((7, 2), (12, 7), (7, 12), (2, 7)), light, layer=2)
            art.poly(((7, 5), (9, 7), (7, 9), (5, 7)), seam, layer=2)
        else:
            for x, y in ((1, 1), (9, 1), (1, 9), (9, 9)):
                art.line((x, y, x + 3, y), light)
    if pattern == 'weave':
        for y in range(1, 16, 3):
            for x in range(y % 2, 15, 4):
                art.line((x, y, x + 1, y), light if y % 2 else seam, layer=2)
    if pattern == 'metal':
        for y in (2, 7, 12):
            for x in (2, 7, 12):
                tilt = -1 if (x + y) % 2 else 1
                art.line((x, y, x + 2, y + tilt), light)
                art.line((x, y + 1, x + 2, y + tilt + 1), seam)
    if pattern == 'grate':
        for y in range(2, 15, 3):
            art.line((2, y, 13, y), seam)
            art.line((2, y + 1, 13, y + 1), light)
        for x in (0, 15):
            art.line((x, 0, x, 15), light)


def draw_scenery(Art, save, dark_layers):
    def save_pair(prefix, name, art):
        save(prefix + 'escena-' + name, art)
        dark = Art(art.layers[0].size)
        dark.layers = dark_layers(art.layers)
        save(prefix + 'backfranz-escena-' + name, dark)

    for name, base, seam, light, pattern in MATERIALS:
        art = Art((16, 16))
        draw_surface(art, name, base, seam, light, pattern)
        save_pair('tile16-', 'suelo-' + name, art)

    art = Art((32, 40))
    art.rect((0, 3, 31, 39), '#263037')
    art.rect((0, 6, 31, 33), '#73766c')
    art.rect((0, 4, 31, 6), '#a2a391')
    art.rect((0, 32, 31, 36), '#555b54')
    art.line((0, 18, 31, 18), '#5c625b')
    art.line((0, 1, 31, 1), '#262d30')
    for x in (5, 26):
        art.line((x, 7, x, 28), '#676e62')
    save_pair('', 'muro-servicio', art)

    art = Art((36, 42))
    art.shadow((1, 34, 34, 41))
    art.rect((3, 3, 32, 36), '#293842')
    art.rect((5, 5, 30, 30), '#9aaba8')
    art.rect((5, 30, 30, 34), '#607a7b')
    art.line((6, 6, 29, 6), '#c8d5cc')
    art.rect((24, 17, 26, 23), '#425961')
    for y in (33, 35):
        art.line((6, y, 29, y), '#516365')
    save_pair('', 'division-bano', art)

    art = Art((64, 34))
    art.rect((2, 4, 61, 29), '#28353d')
    art.rect((4, 6, 59, 27), '#836a56')
    for x in range(7, 58, 5):
        for y in (9, 15, 21):
            art.rect((x, y, x + 1, y), '#593f38', layer=2)
    for x, y in ((10, 11), (22, 9), (47, 12)):
        art.line((x, y, x + 6, y + 8), '#b0b9ad', width=2)
        art.line((x + 4, y + 5, x + 8, y + 10), '#b55642', width=2)
    art.line((32, 10, 42, 10), '#c5bc9e', width=2)
    save_pair('', 'panel-herramientas', art)
