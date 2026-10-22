"""Original layered pixel art and ASE sources, exported to Unity textures.

Run with Python + Pillow. ASE layout follows Aseprite's official specification:
https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md
No raster AI generation is used by this deterministic native-asset authoring tool.
"""
from pathlib import Path
import struct as st
import zlib
import json
import random
from PIL import Image, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
EXPORT = ROOT / 'Assets/Resources/Backfranz/Art'
EXPORT.mkdir(parents=True, exist_ok=True)
INK = '#192430'
GOLD = '#efc077'
LIGHT = '#f5e7bc'
TEAL = '#57aaa1'
manifest = []


def string(value):
    data = value.encode('utf8')
    return st.pack('<H', len(data)) + data


def chunk(kind, data):
    return st.pack('<IH', len(data) + 6, kind) + data


def write_ase(path, frames, names, tags=()):
    w, h = frames[0][0].size
    data = b''
    for index, layers in enumerate(frames):
        chunks = []
        if index == 0:
            for name in names:
                chunks.append(chunk(0x2004, st.pack('<6HB3x', 3, 0, 0, 0, 0, 0, 255) + string(name)))
            if tags:
                payload = st.pack('<H8x', len(tags))
                for first, last, name in tags:
                    payload += st.pack('<HHBH6x3BB', first, last, 0, 0, 239, 192, 119, 0) + string(name)
                chunks.append(chunk(0x2018, payload))
        for layer, image in enumerate(layers):
            raw = zlib.compress(image.convert('RGBA').tobytes(), 9)
            payload = st.pack('<HhhBHh5xHH', layer, 0, 0, 255, 2, 0, w, h) + raw
            chunks.append(chunk(0x2005, payload))
        payload = b''.join(chunks)
        data += st.pack('<IHHH2xI', 16 + len(payload), 0xF1FA, len(chunks), 125, 0) + payload
    header = bytearray(128)
    st.pack_into('<I5HIH', header, 0, 128 + len(data), 0xA5E0, len(frames), w, h, 32, 1, 125)
    st.pack_into('<BBhhHH', header, 34, 1, 1, 0, 0, 32, 32)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(bytes(header) + data)


class Art:
    def __init__(self, size=(32, 48)):
        self.layers = [Image.new('RGBA', size) for _ in range(3)]
        self.draws = [ImageDraw.Draw(x) for x in self.layers]
    def rect(self, xy, fill, layer=1, outline=None):
        self.draws[layer].rectangle(xy, fill=fill, outline=outline)
    def line(self, xy, fill, layer=2, width=1):
        self.draws[layer].line(xy, fill=fill, width=width)
    def ellipse(self, xy, fill, layer=1, outline=None):
        self.draws[layer].ellipse(xy, fill=fill, outline=outline)
    def poly(self, xy, fill, layer=1):
        self.draws[layer].polygon(xy, fill=fill)
    def shadow(self, xy):
        self.ellipse(xy, (4, 12, 23, 90), 0)


def flatten(layers):
    result = Image.new('RGBA', layers[0].size)
    for layer in layers:
        result = Image.alpha_composite(result, layer)
    return result


def save(name, art, frames=None, tags=()):
    frames = frames or [art.layers]
    ase = SOURCE / 'Editables' / (name + '.ase')
    write_ase(ase, frames, ['Sombras', 'Volumen y color', 'Detalles y luces'], tags)
    files = []
    for i, layers in enumerate(frames):
        filename = name + (f'-{i:02d}' if len(frames) > 1 else '') + '.png'
        flatten(layers).save(EXPORT / filename)
        files.append((EXPORT / filename).relative_to(ROOT).as_posix())
    manifest.append({'source': ase.relative_to(ROOT).as_posix(), 'exports': files, 'layers': 3, 'frames': len(frames)})


def tiles():
    rng = random.Random(64)
    for name, base, mid, seam in [
        ('floor-clean', '#617b7c', '#6c8685', '#425e66'),
        ('floor-dark', '#6c6244', '#776b48', '#494534'),
        ('floor-classroom', '#76807c', '#818b82', '#586861'),
        ('floor-carpet', '#3d5365', '#445c6b', '#354b5b')]:
        a = Art((32, 32)); a.rect((0, 0, 31, 31), base)
        a.line((0, 0, 31, 0), seam); a.line((0, 0, 0, 31), seam)
        a.line((1, 1, 31, 1), mid); a.line((1, 1, 1, 31), mid)
        a.line((16, 1, 16, 31), seam); a.line((1, 16, 31, 16), seam)
        for _ in range(9):
            x, y = rng.randrange(2, 31), rng.randrange(2, 31)
            a.line((x, y, min(x+1, 31), y), mid)
        save(name, a)
    for dark in (False, True):
        a = Art((32, 48)); a.shadow((0, 36, 31, 47))
        a.rect((0, 2, 31, 39), '#353f46' if not dark else '#494333', outline=INK)
        a.rect((1, 3, 30, 9), '#c9c5aa' if not dark else '#9d8d55')
        a.rect((1, 10, 30, 33), '#a3b0a6' if not dark else '#7b754c')
        a.rect((1, 27, 30, 33), '#526f70' if not dark else '#5b573c')
        a.line((1, 3, 30, 3), '#e3dcc0' if not dark else '#bab075')
        a.rect((0, 34, 31, 39), '#293d44')
        for x in (7, 23):
            a.line((x, 11, x, 25), '#bbc6b5' if not dark else '#958958')
        if dark:
            a.line((21, 13, 19, 17, 22, 20, 20, 25), '#454833')
        save('wall-dark' if dark else 'wall-clean', a)
    a = Art((32, 32));a.rect((0, 0, 31, 31), '#ffffff');save('solid', a)


def props():
    # Reception desk: shaped top, front panels, monitor and papers.
    a=Art((64,48));a.shadow((2,35,62,47));a.rect((2,12,61,39),INK)
    a.rect((4,19,59,36),'#527a78');a.rect((5,21,58,23),'#7ca093')
    for x in (7,24,41): a.rect((x,26,x+13,34),'#345a62');a.line((x,26,x+13,26),'#88a89c')
    a.rect((1,11,62,20),'#b18c62',outline=INK);a.line((3,12,60,12),'#e1bb86')
    a.rect((35,2,52,13),INK);a.rect((37,4,50,10),'#6ea5a0');a.rect((42,14,46,16),'#314652')
    a.rect((10,13,22,16),LIGHT);a.line((12,14,19,14),'#a2a992');save('reception-desk',a)
    a=Art((48,48));a.shadow((2,35,46,45));a.rect((5,17,42,37),INK);a.rect((7,21,40,34),'#715e53')
    a.rect((3,11,44,24),INK);a.rect((5,12,42,21),'#b59365');a.line((6,13,41,13),'#e6c08c')
    a.rect((11,9,23,16),LIGHT);a.line((13,11,20,11),'#8ba49a');a.rect((31,10,35,15),TEAL);save('desk',a)
    a=Art();a.shadow((5,34,27,44));a.rect((7,8,24,29),INK);a.rect((9,10,22,24),'#39566b')
    a.rect((6,27,25,34),'#243f54');a.rect((8,28,23,31),'#5a8290');a.rect((8,34,10,41),INK);a.rect((21,34,23,41),INK);save('chair',a)
    a=Art((48,64));a.shadow((1,50,47,63));a.rect((3,4,44,56),INK);a.rect((5,5,42,54),'#776453')
    rng=random.Random(9)
    for y in (9,23,37):
        a.rect((7,y,40,y+11),'#263d44')
        x=8
        while x<39:
            width=rng.choice((3,4,5));color=rng.choice(('#aa655a','#69948b','#d0b779','#7c8ca4'))
            a.rect((x,y+2,x+width-1,y+10),color);a.line((x+1,y+3,x+1,y+8),'#e0caa0');x+=width+1
        a.line((6,y+12,41,y+12),'#b9976c')
    a.line((4,5,4,53),'#baa17c');save('bookshelf',a)
    a=Art((48,64));a.shadow((2,51,45,62));a.rect((3,4,44,57),INK)
    for x in (5,25):
        a.rect((x,6,x+17,54),'#55717c');a.line((x+1,7,x+15,7),'#96b0b1');a.rect((x+13,29,x+14,34),GOLD)
        for y in (12,15,18):a.line((x+3,y,x+12,y),'#263f50')
        a.rect((x+3,39,x+13,45),'#425966');a.line((x+5,40,x+10,40),LIGHT)
    save('lockers',a)
    a=Art((32,64));a.shadow((3,51,29,62));a.rect((3,6,28,55),INK);a.rect((5,8,26,52),'#84544f')
    a.rect((7,10,24,38),'#213d4c');a.rect((9,12,20,15),'#c09a65')
    for x in (9,15,21):
        for y in (21,29):a.rect((x,y,x+2,y+5),'#7cae9c');a.line((x,y,x+2,y),LIGHT)
    a.rect((8,43,22,48),INK);a.rect((24,26,26,31),GOLD);a.line((4,7,27,7),'#d19b78');save('vending',a)
    a=Art((32,48));a.shadow((4,35,28,45));a.poly([(8,28),(24,28),(21,40),(11,40)],'#b16f53')
    a.rect((7,27,24,30),'#dda271');a.line((16,10,16,27),'#52765a',1,2)
    for box,c in [((2,13,15,23),'#356c62'),((14,6,27,18),'#4b8974'),((5,3,19,15),'#66a17e'),((13,17,29,27),'#3d7e69')]:
        a.ellipse(box,INK);a.ellipse(tuple(v+1 if i<2 else v-1 for i,v in enumerate(box)),c)
    a.line((9,9,14,7),'#9cc19a');save('plant',a)
    for name,green in [('elevator',False),('exit',True)]:
        a=Art((64,80));a.shadow((0,65,63,79));a.rect((4,8,59,70),INK);a.rect((7,12,56,65),'#718789')
        a.rect((10,16,53,64),'#263d48');a.rect((12,18,31,62),'#769496');a.rect((33,18,51,62),'#627c86')
        a.line((14,20,29,20),'#acc1b3');a.line((35,20,49,20),'#8eaaa5');a.line((30,18,30,62),'#3d5663')
        a.rect((19,7,44,13),'#457f6d' if green else '#293f4c');a.rect((28,8,32,11),LIGHT if green else GOLD)
        a.rect((55,36,59,47),INK);a.rect((56,38,57,40),TEAL);a.rect((56,43,57,45),GOLD)
        a.rect((8,66,55,69),'#bed2bb');save(name,a)
    a=Art((32,32));a.shadow((7,23,26,30));a.rect((9,3,24,25),INK);a.rect((10,4,23,24),LIGHT)
    a.rect((10,5,12,23),'#ae655a');a.poly([(19,4),(23,8),(19,8)],'#c1b98b')
    for y in (10,14,18):a.line((15,y,21,y),'#668b8b')
    save('note',a)
    a=Art((32,32));a.shadow((4,23,28,29));a.rect((4,9,27,24),INK);a.rect((5,10,26,23),'#b4cab6')
    a.rect((5,11,26,14),TEAL);a.rect((8,17,12,21),'#dfb565');a.line((16,17,23,17),'#506f77');a.line((16,20,21,20),'#506f77');save('card',a)
    for on in (False,True):
        a=Art((32,48));a.shadow((3,35,29,46));a.rect((4,5,27,40),INK);a.rect((6,7,25,38),'#49626c')
        a.rect((8,9,23,17),'#183d42');a.line((10,12,20,12),TEAL if on else '#8b5d55')
        a.rect((10,21,21,33),INK);a.rect((12,22 if on else 28,19,26 if on else 32),TEAL if on else GOLD)
        for x in (7,24):a.rect((x,35,x+1,36),'#9db3ad')
        save('switch-on' if on else 'switch',a)
    a=Art((32,48));a.shadow((0,34,31,45));a.rect((0,5,31,11),'#394e59');a.rect((0,32,31,38),'#394e59')
    for x in range(3,32,7):a.rect((x,8,x+2,36),'#91a39a');a.line((x,9,x,34),'#d4d2ad')
    save('gate',a)
    a=Art((32,48));a.shadow((6,37,26,45));a.rect((10,4,22,36),INK);a.rect((12,6,20,33),'#8b9c8c')
    a.rect((6,4,26,10),'#3d5665');a.rect((8,5,24,8),LIGHT);a.rect((10,19,22,23),'#354f5e');save('lamp',a)
    a=Art((32,48));a.rect((3,5,28,35),INK);a.rect((5,7,26,33),'#cbb98d');a.rect((7,9,24,17),'#5c8b8a')
    for y in (21,25,29):a.line((8,y,22,y),'#829488')
    save('poster',a)
    a=Art((32,48));a.shadow((2,35,30,45));a.rect((4,16,28,36),'#2e4e58',outline=INK)
    a.rect((2,13,29,19),'#6c9b92',outline=INK);a.rect((6,21,26,27),'#25444e');a.rect((15,28,17,34),GOLD)
    a.rect((7,4,24,14),INK);a.rect((9,6,22,11),'#5ba2a1');a.line((10,7,19,7),'#a6d0b3');save('computer',a)


def character():
    frames=[]
    for direction in ('down','left','right','up'):
        for frame in range(4):
            a=Art((32,48));a.shadow((6,38,26,45));bob=1 if frame in (1,3) else 0
            # Shoes and trousers, each leg has its own step silhouette.
            left=2 if frame==1 else -1 if frame==3 else 0
            right=2 if frame==3 else -1 if frame==1 else 0
            for x,step in ((10,left),(18,right)):
                a.rect((x,29+bob,x+4,39+step),'#263a54',outline=INK)
                a.rect((x+1,31+bob,x+2,37+step),'#49637c')
                a.rect((x-1,39+step,x+5,42+step),INK);a.line((x,41+step,x+5,41+step),'#babda8')
            a.rect((8,19+bob,24,31+bob),INK);a.rect((9,20+bob,23,29+bob),'#aa4b49')
            a.rect((10,21+bob,14,28+bob),'#cb6559');a.rect((21,21+bob,23,29+bob),'#743e42')
            for x,step in ((5,right//2),(25,left//2)):
                a.rect((x,22+bob+step,x+2,30+bob+step),INK)
                a.rect((x,24+bob+step,x+2,28+bob+step),'#d5a078')
            a.rect((13,16+bob,19,21+bob),'#b67c5d')
            a.ellipse((7,2+bob,25,21+bob),INK)
            a.rect((9,9+bob,23,17+bob),'#dda97d');a.rect((11,18+bob,21,19+bob),'#c88e69')
            a.poly([(8,10+bob),(8,6+bob),(11,3+bob),(20,2+bob),(25,6+bob),(24,11+bob),(21,9+bob),(17,10+bob),(13,8+bob),(10,11+bob)],'#27303b')
            a.line((11,5+bob,20,4+bob),'#43505b');a.line((12,6+bob,17,5+bob),'#35434e')
            if direction=='up':
                a.rect((9,9+bob,23,17+bob),'#27303b');a.line((11,17+bob,21,17+bob),'#18232e')
                a.rect((11,21+bob,21,32+bob),INK);a.rect((12,22+bob,20,30+bob),'#49626c');a.rect((13,26+bob,19,29+bob),'#739087')
                a.line((10,21+bob,10,28+bob),'#d3b77f');a.line((22,21+bob,22,28+bob),'#d3b77f')
            elif direction in ('left','right'):
                a.rect((9,12+bob,11,14+bob),INK);a.rect((8,14+bob,10,16+bob),'#e5b58a')
                a.rect((20,21+bob,25,31+bob),INK);a.rect((21,22+bob,24,29+bob),'#647e83')
                if direction=='right':a.layers=[ImageOps.mirror(x) for x in a.layers]
            else:
                for x in (11,20):a.rect((x,12+bob,x+1,14+bob),INK);a.rect((x,12+bob,x,12+bob),LIGHT)
                a.line((14,17+bob,18,17+bob),'#9b6255')
            frames.append(a.layers)
    save('yamil',a,frames,[(i*4,i*4+3,d) for i,d in enumerate(('Caminar abajo','Caminar izquierda','Caminar derecha','Caminar arriba'))])
    frames=[]
    for f in range(4):
        a=Art((32,48));a.shadow((2,34,30,47));a.ellipse((8,4,25,23),'#101a26')
        a.poly([(10,18),(23,17),(29,38),(23,36),(20,42),(16,38),(12,43),(7,39),(3,40)],'#152330')
        a.poly([(11,21),(16,20),(13,35),(8,39)],'#20323d');a.line((19,23,23,36),'#273a41')
        for x in (12,20):a.rect((x,12+(f%2),x+2,14+(f%2)),'#e3936a');a.rect((x+1,12+(f%2),x+1,13+(f%2)),LIGHT)
        frames.append(a.layers)
    save('shadow',a,frames,[(0,3,'Flotar')])


def ui():
    # Native pixel panels support Unity nine-slicing with an 8px border.
    for name,base in [('panel','#172c3c'),('button','#2c4957'),('button-gold','#8c6545')]:
        a=Art((48,48));a.rect((1,1,46,46),INK);a.rect((3,3,44,44),'#526e75');a.rect((5,5,42,42),base)
        a.line((5,5,42,5),'#a3b4a0');a.line((5,5,5,42),'#718f91');a.line((6,43,43,43),'#101e2d');a.rect((3,3,5,5),GOLD)
        save(name,a)
    for name,points in [('arrow-up',[(16,7),(25,19),(20,19),(20,26),(12,26),(12,19),(7,19)]),
                        ('interact',[(10,7),(22,7),(25,10),(25,21),(20,26),(13,26),(7,20),(7,15),(10,15)])]:
        a=Art((32,32));a.poly(points,LIGHT);save(name,a)
    a=Art((32,32));a.rect((8,7,13,25),LIGHT);a.rect((19,7,24,25),LIGHT);save('pause',a)
    a=Art((32,32));a.poly([(4,12),(16,4),(28,12),(26,14),(24,12),(24,27),(8,27),(8,12),(6,14)],LIGHT);a.rect((14,18,19,27),INK);save('home',a)
    a=Art((32,32));a.line((7,17,15,24,27,8),LIGHT,width=3);save('check',a)
    a=Art((256,144))
    for y in range(144):
        for x in range(256):
            distance=((x-127.5)/150)**2+((y-71.5)/85)**2
            alpha=int(min(155,max(0,distance-.22)*100))
            a.layers[0].putpixel((x,y),(7,15,25,alpha))
    save('vignette',a)
    a=Art((96,96))
    for y in range(96):
        for x in range(96):
            distance=((x-47.5)/48)**2+((y-47.5)/48)**2
            alpha=int(max(0,1-distance)**2*32)
            a.layers[2].putpixel((x,y),(250,220,151,alpha))
    save('lightpool',a)


def legacy_sources():
    # Existing raster art remains flattened: importing cannot recover lost layers.
    for png in (ROOT/'Assets/Art').rglob('*.png'):
        image=Image.open(png).convert('RGBA')
        relative=png.relative_to(ROOT/'Assets/Art').with_suffix('.ase')
        ase=SOURCE/'Importados'/relative
        write_ase(ase,[[image]],['Imagen importada (una capa)'])
        manifest.append({'source':ase.relative_to(ROOT).as_posix(),'exports':[png.relative_to(ROOT).as_posix()],'layers':1,'frames':1,'origin':'Existing flattened raster, preserved without invented layers'})


def main():
    tiles();props();character();ui();legacy_sources()
    (SOURCE/'assets.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf8')
    thumbs=[]
    for item in manifest:
        if 'Importados' in item['source']:continue
        image=Image.open(ROOT/item['exports'][0]);image.thumbnail((64,72),Image.Resampling.NEAREST)
        thumbs.append((Path(item['source']).stem,image))
    contact=Image.new('RGB',(640,((len(thumbs)+7)//8)*100),'#172530');draw=ImageDraw.Draw(contact)
    for i,(name,image) in enumerate(thumbs):
        x=(i%8)*80;y=(i//8)*100;contact.paste(image,(x+(80-image.width)//2,y),image);draw.text((x+2,y+76),name[:12],fill='#dfd3b1')
    contact.save(SOURCE/'catalogo.png')
    print(f'{len(manifest)} fuentes ASE y exportaciones generadas.')


if __name__=='__main__':main()
