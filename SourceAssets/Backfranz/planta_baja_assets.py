"""Layered pixel-art materials and landmarks for Landaeta and planta baja."""
from random import Random
from PIL import ImageFont


INK = '#27303c'
CREAM = '#fff0d8'
STEEL = '#77858c'
WOOD = '#b87850'


def draw_ground(Art, save):
    for name, base, joints, highlight in (
        ('piedra', '#cbbfb4', '#b9aaa0', '#dbd1c6'),
        ('porcelanato', '#ddd0bb', '#cabca9', '#efe3ce'),
        ('adoquin', '#72777a', '#575e64', '#818688'),
        ('madera', '#95684f', '#79513f', '#ab7d5a'),
        ('alfombra', '#605468', '#5a4b60', '#6b5d71'),
    ):
        art = Art((32, 32))
        art.rect((0, 0, 31, 31), base)
        step = 8 if name in ('adoquin', 'madera') else 16 if name == 'piedra' else 32
        for y in range(0, 32, step):
            art.line((0, y, 31, y), joints)
            art.line((0, y + 1, 31, y + 1), highlight)
            for x in range(0 if y % (step * 2) == 0 else step // 2, 32, step if name != 'madera' else 32):
                art.line((x, y, x, y + step - 1), joints)
        rng = Random(name)
        for _ in range(32):
            x, y = rng.randrange(32), rng.randrange(32)
            art.line((x, y, min(x + rng.randrange(1, 4), 31), y), rng.choice((joints, highlight)), layer=2)
        save('suelo-' + name, art, mirrored=name not in ('piedra', 'adoquin'))

    for name, wall, top, base in (
        ('yeso', '#d5c8b5', '#fff0d8', '#7f7877'),
        ('vidrio', '#547c90', '#c4e1e1', '#364f60'),
        ('acustico', '#654b54', '#aa7b73', '#3c303d'),
        ('calle', '#c9c6ba', '#e6e2d3', '#65727c'),
    ):
        art = Art((32, 40))
        art.rect((0, 2, 31, 39), INK)
        art.rect((0, 5, 31, 34), wall)
        art.rect((0, 3, 31, 8), top)
        art.rect((0, 30, 31, 36), base)
        art.line((0, 9, 31, 9), base)
        art.line((0, 30, 31, 30), top)
        art.line((0, 10, 0, 29), base)
        if name == 'vidrio':
            art.poly(((2, 17), (15, 10), (20, 10), (2, 21)), '#739db0', layer=2)
            art.poly(((14, 27), (30, 19), (30, 23), (21, 27)), '#6b96a7', layer=2)
        if name == 'acustico':
            for x in range(3, 32, 4):
                art.line((x, 12, x, 29), base)
        if name == 'calle':
            art.line((0, 23, 31, 23), STEEL)
            art.line((16, 23, 16, 35), STEEL)
        save('muro-' + name, art, mirrored=name != 'calle')

    def slab(art, box, fill, edge):
        x, y, right, bottom = box
        art.rect(box, INK)
        art.rect((x+1, y+1, right-1, bottom-1), fill)
        art.line((x+1, y+1, right-1, y+1), edge)

    art = Art((112, 48))
    art.shadow((7, 27, 105, 46))
    front = ((4, 12), (15, 22), (34, 27), (77, 27), (96, 21), (107, 12),
             (107, 24), (98, 34), (77, 41), (34, 41), (14, 35), (4, 25))
    art.poly(front, INK)
    art.poly(((5, 13), (16, 23), (34, 29), (77, 29), (95, 23), (106, 14),
              (106, 24), (97, 33), (77, 39), (34, 39), (15, 33), (5, 24)), '#b5a797')
    art.poly(((4, 12), (12, 5), (24, 13), (39, 17), (72, 17), (87, 13), (100, 5),
              (107, 12), (96, 22), (77, 29), (34, 29), (15, 23)), CREAM)
    art.line((6, 23, 16, 32, 34, 38, 77, 38, 96, 32, 105, 23), '#e49852', width=2)
    slab(art, (69, 6, 85, 16), '#466877', '#a1b9ba')
    art.rect((75, 16, 79, 18), STEEL, layer=2)
    art.rect((25, 19, 35, 22), '#eadbc4', layer=2)
    art.line((27, 20, 32, 20), STEEL)
    save('recepcion', art)

    art = Art((112, 42))
    art.rect((1, 1, 110, 40), '#472f2d')
    for x in range(4, 110, 6):
        art.rect((x, 2, x+3, 38), WOOD)
        art.line((x, 3, x, 38), '#da9c65')
    slab(art, (22, 11, 89, 30), '#f5e2c6', '#fff3dc')
    art.draws[2].text((55, 20), 'UNIFRANZ', fill=INK, anchor='mm', font=ImageFont.load_default())
    art.line((42, 26, 68, 26), '#bd623d')
    save('listones', art)

    for name, surface, screen, paper in (
        ('ventanilla', '#96674f', '#71a49d', '#ebb878'),
        ('servicios', '#ba865b', '#518b9f', '#eef4d8'),
        ('admisiones', '#d2c3ad', '#5587b1', '#fff0d2'),
    ):
        art = Art((78, 47))
        art.shadow((4, 32, 73, 45))
        slab(art, (6, 23, 71, 41), surface, CREAM)
        slab(art, (3, 20, 74, 31), surface, CREAM)
        slab(art, (43, 7, 65, 20), '#334254', STEEL)
        art.rect((46, 9, 62, 17), screen, layer=2)
        art.line((47, 11, 58, 11), CREAM)
        art.rect((52, 20, 56, 23), STEEL, layer=2)
        art.rect((45, 25, 62, 26), '#546471', layer=2)
        art.rect((12, 23, 31, 27), paper, layer=2)
        art.line((14, 24, 23, 24), STEEL)
        if name == 'ventanilla':
            art.rect((7, 4, 69, 19), (128, 190, 192, 55), layer=2)
            art.line((7, 19, 7, 4, 69, 4, 69, 19), '#a0c8c6')
            art.line((13, 15, 31, 6), CREAM)
        save(name, art)

    art = Art((100, 64))
    art.shadow((4, 43, 95, 62))
    slab(art, (9, 9, 91, 55), '#834b36', '#d39b69')
    slab(art, (14, 12, 86, 44), '#396348', '#88ab73')
    art.line((49, 14, 49, 42), '#c5d5a5')
    art.ellipse((43, 23, 55, 34), None, layer=2, outline='#c5d5a5')
    for x in (21, 35, 63, 78):
        art.line((x, 4, x, 51), '#a7bac0')
        for y in (19, 28, 37):
            art.rect((x-2, y, x+2, y+3), '#e09465' if x in (21, 63) else '#6eadd2', layer=2)
    save('futbolin', art)

    art = Art((84, 44))
    art.ellipse((0, 0, 83, 43), (186, 91, 239, 30), layer=0)
    art.ellipse((7, 5, 76, 39), (206, 92, 246, 55), layer=0)
    slab(art, (12, 9, 71, 34), '#392948', '#a16baa')
    shape = (24, 27, 20, 23, 24, 16, 29, 14, 55, 14, 60, 16, 64, 23,
             60, 27, 51, 23, 32, 23, 24, 27)
    art.line(shape, '#c353c6', width=4)
    art.line(shape, '#ffb9ef')
    art.line((28, 18, 28, 22), '#c4f5f6')
    art.line((26, 20, 30, 20), '#c4f5f6')
    art.rect((53, 18, 54, 19), '#c4f5f6', layer=2)
    art.rect((57, 20, 58, 21), '#c4f5f6', layer=2)
    save('neon', art)

    art = Art((73, 41))
    art.shadow((4, 29, 67, 39))
    slab(art, (6, 9, 18, 33), '#7c8b93', '#c9cdd1')
    slab(art, (53, 9, 66, 33), '#7c8b93', '#c9cdd1')
    art.poly(((18, 14), (35, 12), (53, 16), (53, 25), (35, 29), (18, 24)), '#57828d')
    art.line((18, 14, 35, 12, 53, 16), '#b5dee0')
    slab(art, (48, 5, 69, 16), '#2b4947', '#98ad9e')
    for inset in (0, 2, 4):
        art.draws[2].arc((52+inset, 7+inset, 64-inset, 17), 180, 355, fill='#84dfa1')
    save('molinete', art)

    art = Art((72, 52))
    art.shadow((5, 35, 66, 50))
    slab(art, (4, 7, 67, 43), '#9c503c', '#f9ae62')
    for x in (9, 27, 45):
        slab(art, (x, 10, x+17, 28), '#cf7044', '#f4a268')
        slab(art, (x, 27, x+17, 38), '#e18b52', '#f8b575')
    save('sofa', art)

    art = Art((88, 59))
    art.shadow((4, 42, 82, 57))
    for x in (7, 67):
        slab(art, (x, 23, x+13, 44), '#284f65', '#6d9aa7')
    art.ellipse((17, 13, 70, 43), '#795039', outline=INK)
    art.ellipse((17, 9, 70, 35), '#cf925b', outline=INK)
    art.line((28, 25, 57, 25), '#bc7e4e')
    art.ellipse((37, 15, 49, 23), CREAM, layer=2, outline='#a67151')
    art.ellipse((41, 16, 46, 20), '#6e4638', layer=2)
    save('mesa-cafe', art)

    art = Art((110, 36))
    art.rect((0, 2, 109, 32), '#4d3641')
    for x in range(2, 110, 4):
        art.line((x, 3, x, 30), '#674456')
    slab(art, (10, 2, 99, 28), '#293441', '#a18d93')
    art.rect((15, 6, 94, 23), '#dfd9c4')
    art.line((19, 20, 91, 20), '#c7bcac')
    save('pantalla', art)

    art = Art((123, 45))
    art.rect((0, 3, 122, 41), '#4c5862')
    art.rect((0, 3, 122, 31), '#d6d4c9')
    art.rect((0, 3, 122, 6), '#f7eee0')
    for x in (5, 28, 51, 74, 97):
        slab(art, (x, 10, x+20, 28), '#3c5968', '#949da0')
        art.rect((x+3, 13, x+17, 17), '#506f7c')
        art.line((x+3, 22, x+16, 14), '#7e9ca7')
        art.line((x+7, 11, x+7, 27), '#34404a')
        art.line((x, 33, x, 40), STEEL)
    save('fachada', art, mirrored=False)

    art = Art((96, 58))
    art.shadow((2, 36, 93, 56))
    for i in range(6):
        y = 6+i*7
        inset = 9-i
        art.rect((inset, y, 95-inset, y+6), '#a8a39a')
        art.line((inset, y, 95-inset, y), '#dfd6c7')
        art.rect((inset, y+4, 95-inset, y+6), '#716e6b')
    for x in (8, 86):
        art.line((x, 8, x, 45), '#556775', width=2)
        art.line((x+1, 6, x+1, 43), '#c2c9c6')
    save('gradas', art)

    art = Art((83, 92))
    art.shadow((8, 69, 78, 89))
    art.poly(((38, 86), (41, 42), (47, 42), (47, 85), (54, 88), (33, 88)), '#5b4c42')
    rng = Random(23)
    for i, (x, y, r) in enumerate(((16, 42, 15), (63, 40, 16), (47, 51, 18),
                                     (40, 21, 19), (22, 24, 15), (58, 22, 17), (47, 13, 12))):
        art.ellipse((x-r, y-r, x+r, y+r), ('#344e40', '#415d46', '#58744d')[i % 3], outline='#2b4036')
        for _ in range(19):
            dx, dy = rng.randint(-r, r), rng.randint(-r, r)
            if dx*dx+dy*dy < (r-3)**2:
                art.line((x+dx, y+dy, x+dx+2, y+dy), rng.choice(('#688056', '#758c5f', '#4b684b')))
    save('arbol', art, mirrored=False)

    art = Art((89, 49))
    art.shadow((3, 26, 85, 46))
    for x in (15, 62):
        art.rect((x, 4, x+13, 42), INK)
    art.poly(((5, 15), (17, 12), (68, 12), (83, 20), (83, 31), (69, 38), (14, 38), (4, 32)), '#466775')
    art.poly(((18, 14), (28, 8), (53, 8), (67, 15), (60, 34), (24, 34)), '#9fb2ad')
    art.poly(((55, 12), (65, 16), (59, 31), (51, 27)), '#324956')
    art.poly(((17, 16), (25, 12), (28, 17), (25, 30), (18, 33)), '#3b525e')
    art.line((30, 11, 50, 11), '#d4ded1')
    art.line((5, 18, 5, 24), '#d57f63', width=2)
    art.line((81, 23, 81, 29), '#f9e4a8', width=2)
    save('auto', art, mirrored=False)

    art = Art((32, 44))
    art.shadow((4, 35, 27, 43))
    slab(art, (5, 4, 26, 29), '#e7e4d8', '#fff5df')
    slab(art, (4, 25, 27, 33), '#cecfc9', '#f4ece2')
    for x in (6, 23):
        art.rect((x, 32, x+2, 40), '#9ca7a6')
    save('silla-blanca', art, mirrored=False)

    for name, hair, skin, cloth, accent in (
        ('estudiante', '#352d35', '#c78965', '#3e797b', '#a5c9ad'),
        ('recepcionista', '#392a32', '#e1aa83', '#4d566d', '#f5d5a1'),
        ('guardia', '#263543', '#c28e68', '#405970', '#f1bb66'),
    ):
        art = Art((32, 44))
        art.shadow((5, 36, 26, 42))
        slab(art, (8, 28, 23, 37), '#48505b', '#67747b')
        art.rect((7, 36, 13, 40), INK)
        art.rect((18, 36, 24, 40), INK)
        art.poly(((8, 17), (23, 17), (27, 28), (23, 32), (8, 32), (4, 28)), INK)
        art.poly(((9, 18), (22, 18), (25, 27), (22, 30), (9, 30), (6, 27)), cloth)
        art.line((10, 21, 12, 25, 19, 25, 21, 21), accent)
        art.rect((14, 25, 17, 28), CREAM, layer=2)
        art.ellipse((6, 2, 25, 23), hair, outline=INK)
        art.ellipse((8, 7, 23, 21), skin)
        art.poly(((7, 5), (16, 3), (24, 6), (24, 12), (20, 8), (13, 10), (7, 13)), hair, layer=2)
        for x in (11, 19):
            art.rect((x, 13, x+1, 14), INK, layer=2)
        art.line((14, 18, 17, 18), '#8b594e')
        if name == 'guardia':
            slab(art, (6, 3, 25, 10), '#344f63', '#7d99a3')
            art.line((6, 10, 27, 10), INK, width=2)
            art.rect((9, 31, 15, 37), '#576d7e')
            art.rect((17, 31, 23, 37), '#576d7e')
        save(name, art, mirrored=False)
