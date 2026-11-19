"""Layered room-scale character sprites, drawn on the same grid as Yamil."""
import unicodedata
from PIL import Image, ImageOps

# hair silhouette, clothes, accessory and skin tone, in character order.
DESIGNS = {
    "Camila": ("bob", "vermilion", "lanyard", 1),
    "Iván": ("crop", "pine", "gloves", 2),
    "Paula": ("pony", "salmon", "medic", 0),
    "Dalia": ("braids", "lilac", "zip", 2),
    "Lázaro": ("waves", "navy", "camera", 1),
    "Nuria": ("bun", "ink", "scarf", 1),
    "Lina": ("undercut", "berry", "headphones", 0),
    "Diego": ("spiky", "sand", "camera", 2),
    "Hugo": ("waves", "teal", "headphones", 1),
    "Iris": ("bob", "ink", "glasses", 0),
    "Renata": ("braids", "vermilion", "folio", 3),
    "Andrés": ("cap", "ochre", "apron", 2),
    "Mateo": ("crop", "slate", "glasses", 2),
    "Eva": ("pony", "ochre", "work", 1),
    "Alma": ("braids", "pine", "scarf", 2),
    "Vera": ("bun", "berry", "lanyard", 3),
    "Mónica": ("bob", "sand", "folio", 1),
    "Sara": ("long", "salmon", "zip", 0),
    "Raúl": ("spiky", "teal", "scarf", 3),
    "Mara": ("undercut", "ochre", "work", 2),
    "Felipe": ("cap", "pine", "work", 1),
    "Celia": ("crop", "denim", "work", 3),
    "Inés": ("long", "lilac", "folio", 1),
    "Marcos": ("waves", "pine", "glasses", 2),
    "Elena": ("braids", "ochre", "medic", 2),
    "Bibliotecaria": ("bun", "vermilion", "glasses", 2),
    "Guardia": ("cap", "slate", "badge", 2),
    "Recepcionista": ("long", "teal", "lanyard", 1),
    "Mauro": ("spiky", "berry", "scarf", 1),
    "Rita": ("bun", "sand", "apron", 0),
    "Nico": ("blue", "ink", "headphones", 1),
    "Elías": ("crop", "sand", "badge", 3),
    "Mariela": ("pony", "berry", "lanyard", 2),
    "Lucía": ("bob", "salmon", "folio", 2),
    "Samuel": ("waves", "slate", "zip", 1),
    "Irene": ("bun", "teal", "coat", 2),
    "David": ("undercut", "navy", "headphones", 3),
    "Belén": ("braids", "denim", "glasses", 1),
    "Pablo": ("cap", "vermilion", "camera", 1),
    "Clara": ("long", "pine", "glasses", 0),
    "Valeria": ("spiky", "denim", "gloves", 1),
    "Joel": ("waves", "ochre", "folio", 3),
    "Noelia": ("bob", "navy", "work", 0),
    "Omar": ("blue", "vermilion", "headphones", 2),
    "Sofía": ("pony", "lilac", "scarf", 2),
    "Mica": ("undercut", "salmon", "zip", 3),
    "Estudiante": ("beanie", "vermilion", "scarf", 2),
    "Personal administrativo": ("crop", "navy", "lanyard", 3),
    "Profesora": ("bun", "lilac", "coat", 0),
    "Estudiante de Derecho": ("waves", "pine", "folio", 0),
    "Personal de cafetería": ("cap", "teal", "apron", 2),
    "Estudiante de audiovisual": ("undercut", "berry", "camera", 3),
    "Técnico": ("cap", "ochre", "work", 3),
    "Delegada": ("braids", "slate", "lanyard", 1),
    "Encargada de cocina": ("bun", "pine", "apron", 2),
    "Aylin": ("braids", "sand", "scarf", 3),
    "Damián": ("beanie", "denim", "zip", 1),
    "Docente de Computación": ("crop", "teal", "glasses", 3),
    "Docente de Derecho": ("bob", "vermilion", "folio", 2),
    "Docente del Taller": ("pony", "ochre", "work", 3),
}
PALETTES = {
    "vermilion": ("#aa4942", "#db7660", "#713c40"), "pine": ("#52724b", "#8e9a5b", "#2e534c"),
    "salmon": ("#c56e73", "#f0ada2", "#8e4e63"), "lilac": ("#846d9a", "#bda3cc", "#5a5077"),
    "navy": ("#3b536e", "#708ca7", "#293b55"), "ink": ("#45414c", "#727080", "#282a39"),
    "berry": ("#903d64", "#c06592", "#5d3659"), "sand": ("#be986d", "#ead6a7", "#86664d"),
    "ochre": ("#bb7739", "#e8ad55", "#815137"), "teal": ("#388072", "#74b3a1", "#295759"),
    "slate": ("#566a70", "#8a9a99", "#394e57"), "denim": ("#446e97", "#75a3bf", "#304a6f"),
}
SKIN = (("#e5ad82", "#f6c99c", "#ad725d"), ("#c68b60", "#e6a978", "#975f45"),
        ("#ae7353", "#d7996d", "#7d5142"), ("#86573f", "#b9835b", "#623e36"))
MOBILE = {"Inés", "Marcos", "Elena", "Damián", "Estudiante de Derecho"}
WARDROBE = {
    "Iván":"cargo", "Paula":"short", "Dalia":"puffer", "Lázaro":"tall", "Nuria":"skirt",
    "Lina":"short", "Diego":"cargo", "Hugo":"tall", "Iris":"longcoat", "Renata":"skirt",
    "Andrés":"broad", "Mateo":"tall", "Eva":"cargo", "Alma":"skirt", "Vera":"longcoat",
    "Mónica":"skirt", "Sara":"puffer", "Raúl":"cargo", "Mara":"broad", "Felipe":"tall",
    "Celia":"cargo", "Inés":"short", "Marcos":"tall", "Elena":"puffer",
    "Bibliotecaria":"skirt", "Guardia":"broad", "Recepcionista":"longcoat", "Mauro":"tall",
    "Rita":"short", "Nico":"puffer", "Elías":"broad", "Mariela":"longcoat", "Lucía":"short",
    "Samuel":"tall", "Irene":"tall", "David":"puffer", "Belén":"puffer", "Pablo":"cargo",
    "Clara":"skirt", "Valeria":"cargo", "Joel":"broad", "Noelia":"longcoat", "Omar":"tall",
    "Sofía":"puffer", "Mica":"short", "Estudiante":"cargo", "Personal administrativo":"longcoat",
    "Profesora":"short", "Estudiante de Derecho":"tall", "Personal de cafetería":"broad",
    "Estudiante de audiovisual":"puffer", "Técnico":"broad", "Delegada":"short",
    "Encargada de cocina":"broad", "Aylin":"skirt", "Damián":"puffer",
    "Docente de Computación":"tall", "Docente de Derecho":"skirt", "Docente del Taller":"cargo",
}
INK = "#182132"


def slug(name):
    return "-".join("".join(c for c in unicodedata.normalize("NFD", name.lower())
                           if not unicodedata.combining(c)).split())


def draw_frame(Art, design, direction, step, wardrobe, seated=False):
    hair, palette, gear, skin_index = design
    cloth, upper, darker = PALETTES[palette]
    skin, cheek, shade = SKIN[skin_index]
    a = Art((32, 48))
    a.shadow((4, 39, 28, 46))
    side = direction in (1, 2)
    rear = direction == 3
    stride = (0, 1, 0, -1)[step]
    lift = abs(stride)
    broad = gear in ("work", "badge", "coat") or wardrobe in ("broad","puffer")
    face_y = lift + (3 if seated else 0)
    coat = gear == "coat"
    dark_hair = "#342735" if skin_index % 2 == 0 else "#252835"
    hair_light = "#69444d" if skin_index % 2 == 0 else "#505064"
    if hair == "blue":
        dark_hair, hair_light = "#39689b", "#76aacc"

    # A long jacket, apron, uniform or backpack changes the silhouette below the head.
    for i, x in enumerate((9, 17) if not side else (11-2*stride, 16+2*stride)):
        foot = 41 + stride*(1 if i == 0 else -1)
        a.rect((x, 29 if not seated else 33, x+5, foot), INK)
        trousers=("#68796b","#475a55") if wardrobe=="cargo" else ("#405367","#304053")
        a.rect((x+1, 32 if not seated else 34, x+4, foot-2), trousers[i])
        a.rect((x-1, foot-2, x+5, foot+1), INK)
        a.line((x, foot, x+4, foot), "#d1c3a2" if broad else upper)
        if wardrobe=="cargo" and foot>38:a.rect((x,34,x+4,36),trousers[1],outline=INK)
    if rear or side:
        a.rect((21, 21, 28, 32) if side else (8, 22, 23, 33), "#4d4851", outline=INK)
    left, right = (5, 26) if broad else (7, 24)
    bottom = 36 if coat or wardrobe=="longcoat" else 32
    if wardrobe=="skirt":
        a.poly([(10,28),(21,28),(25,38),(7,38)],INK)
        a.poly([(11,29),(20,29),(23,37),(9,37)],darker)
        for x in (12,17,21):a.line((x,33,x-1,36),upper)
    a.poly([(left+4, 19+face_y), (right-4, 19+face_y), (right, 24), (right-2,bottom),
            (left+2,bottom), (left,24)], INK)
    a.poly([(left+4, 21+face_y),(right-4,21+face_y),(right-2,25),
            (right-3,bottom-1),(left+3,bottom-1),(left+2,25)], "#cbd2d4" if coat else cloth)
    a.line((left+4,24,left+3,bottom-3), "#ece6cc" if coat else upper)
    a.line((right-4,25,right-4,bottom-2), "#8798a1" if coat else darker)
    if wardrobe=="puffer":
        for y in (25,28):a.line((left+3,y,right-3,y),darker)
    if side:
        a.rect((13+stride, 25, 17+stride,31), cloth, outline=INK)
        a.rect((14+stride,29,16+stride,31),skin)
    else:
        for x in (left, right-2):
            a.rect((x,26,x+2,31),INK)
            a.line((x+1,27,x+1,30), "#eeeecc" if gear=="gloves" else skin)

    hx = 13 if side else 16
    def poly(points, colour, layer=1):
        a.poly([(hx+x, y+face_y) for x,y in points], colour, layer)
    def box(x0,y0,x1,y1,colour,layer=2):
        a.rect((hx+x0,y0+face_y,hx+x1,y1+face_y),colour,layer)

    if hair=="long":
        poly([(3,6),(8,8),(10,13),(11,20),(13,24),(7,27),(4,23),(-9,24),(-10,18),(-7,8)],INK)
        poly([(5,8),(8,13),(8,18),(11,23),(7,25),(4,21),(-8,22),(-8,15),(-5,8)],dark_hair)
        box(8,20,9,23,hair_light)
    if hair=="pony":
        poly([(4,5),(9,3),(14,6),(14,13),(11,19),(8,21),(7,17),(10,11),(8,8)],INK)
        poly([(8,5),(12,6),(13,12),(9,18),(9,15),(11,11),(9,7)],dark_hair)
        box(9,6,11,8,upper)
    if hair == "bun":
        a.ellipse((hx+2, 1+face_y, hx+11, 9+face_y), INK)
        a.ellipse((hx+3, 2+face_y, hx+10, 8+face_y), dark_hair)
        box(4,3,7,4,hair_light)
    poly([(-9,9),(-5,4),(1,3),(7,5),(10,9),(10,14),(7,18),(4,21),(-4,21),(-8,18),(-10,15)],INK)
    poly([(-7,10),(-4,6),(1,5),(6,7),(8,11),(8,15),(5,18),(2,19),(-4,19),(-7,16),(-8,14)],
         dark_hair if rear else skin)
    if rear:
        box(-5,7,4,9,hair_light)
        box(-3,19,3,19,shade)
    else:
        if side:
            box(-9,13,-6,15,skin)
        eyes=(-5,) if side else (-5,3)
        for x in eyes:
            box(x,12,x+1,14,INK);box(x,12,x,12,"#fcf0d1")
            box(x-1,15,x+1,15,cheek)
        box(-2,17,0,17,shade)
        if gear=="glasses":
            for x in eyes:
                a.rect((hx+x-1,11+face_y,hx+x+3,15+face_y),None,outline="#574344",layer=2)
            if not side:box(-1,13,2,13,"#574344")
    crowns = {
        "spiky": [(-8,11),(-10,6),(-5,7),(-5,2),(-1,4),(3,1),(4,4),(9,3),(8,6),(11,9),(7,12),(3,9),(-2,10),(-5,9)],
        "blue": [(-10,6),(-6,6),(-4,2),(1,4),(6,2),(8,6),(11,8),(8,13),(4,10),(0,12),(-2,9),(-7,13)],
        "waves": [(-9,10),(-9,6),(-6,5),(-4,2),(0,3),(4,2),(6,5),(9,5),(10,11),(7,11),(3,8),(-2,10),(-4,8)],
        "crop": [(-8,10),(-8,7),(-5,4),(5,4),(8,7),(8,10),(4,8),(-4,8)],
        "undercut": [(-8,9),(-6,4),(4,3),(9,5),(10,9),(8,14),(5,12),(6,8),(-2,10)],
    }
    poly(crowns.get(hair,[(-9,12),(-8,7),(-4,4),(3,4),(8,6),(9,11),(7,15),(5,9),(1,8),(-3,9),(-6,12)]),dark_hair,2)
    box(-5,6,1,6,hair_light)
    if hair in ("cap", "beanie"):
        poly([(-10,9),(-9,5),(-5,2),(4,2),(8,5),(10,10),(-10,10)], cloth,2)
        box(-8,8,9,10,INK)
        box(-6,5,2,5,upper)
        if hair=="beanie":box(0,0,3,2,upper)
        elif not rear:box(-11,10,3,11,darker)
    if hair == "braids":
        for x in (-9,7):
            box(x-1,15,x+3,24,INK)
            box(x,15,x+2,23,dark_hair)
            for y in (16,19,21):box(x,y,x+1,y,hair_light)
            box(x,22,x+2,22,upper)
    if gear in ("apron","medic","work","lanyard","scarf","badge","zip","camera","folio","headphones") and not rear:
        if gear=="apron":
            a.rect((11,24,21,34),darker,outline=INK)
            a.rect((13,29,19,32),upper,outline=darker)
        elif gear=="medic":
            a.rect((12,24,20,31),"#e7c8ac",outline=INK)
            a.line((16,25,16,30),"#aa4942",width=2);a.line((14,27,18,27),"#aa4942",width=2)
        elif gear=="work":
            a.line((9,26,23,26),"#ebcca2")
            a.rect((16,29,22,34),darker,outline=INK)
        elif gear in ("lanyard","camera"):
            a.line((11,22,16,29,21,22),"#e5caa5" if gear=="lanyard" else INK)
            a.rect((13,27,19,32), "#ebe1bb" if gear=="lanyard" else "#4c5972",outline=INK)
            a.rect((16,29,17,30),"#5faab6")
        elif gear=="folio":
            a.poly([(17,24),(24,22),(24,30),(17,33)], INK)
            a.poly([(18,24),(23,23),(23,29),(18,31)], "#bd825a")
            a.line((18,25,22,24),"#ead4ac")
        elif gear=="scarf":
            a.line((10,22,21,22,19,28),upper,width=3)
        elif gear=="headphones":
            box(-10,11,-8,15,upper)
            if not side:box(8,11,10,15,upper)
        else:
            a.line((16,23,16,31),upper)
            if gear=="badge":a.rect((18,24,20,26),"#efc077")
    if direction == 2:
        a.layers = [ImageOps.mirror(layer) for layer in a.layers]
    if wardrobe in ("short","tall","broad"):
        width,height={"short":(30,43),"tall":(28,48),"broad":(32,46)}[wardrobe]
        for i in (1,2):
            fitted=Image.new("RGBA",(32,48))
            fitted.paste(a.layers[i].resize((width,height),Image.Resampling.NEAREST),
                         ((32-width)//2,48-height))
            a.layers[i]=fitted
    return a


CAMPUS_CROWD = (
    (("long","sand","folio",2),"longcoat"), (("crop","teal","glasses",3),"tall"),
    (("braids","berry","zip",3),"puffer"), (("beanie","ochre","headphones",1),"cargo"),
    (("bob","pine","scarf",0),"skirt"), (("undercut","salmon","zip",2),"broad"),
    (("pony","denim","folio",1),"short"), (("waves","lilac","lanyard",0),"regular"),
    (("cap","slate","work",2),"cargo"), (("bun","vermilion","lanyard",1),"regular"),
    (("spiky","salmon","glasses",1),"regular"), (("long","lilac","coat",3),"short"),
    (("bun","teal","medic",0),"longcoat"), (("crop","sand","apron",2),"broad"),
    (("blue","denim","headphones",1),"puffer"), (("braids","navy","camera",2),"cargo"),
    (("pony","berry","folio",3),"skirt"), (("waves","ochre","work",0),"tall"),
)


def draw_campus_crowd(Art, save):
    tags = [(i*4,i*4+3,"Caminar "+d) for i,d in enumerate(("abajo","izquierda","derecha","arriba"))]
    for index,(design,wardrobe) in enumerate(CAMPUS_CROWD,1):
        key = f"estudiante-campus-{index:02}"
        standing = draw_frame(Art,design,0,0,wardrobe)
        frames = [draw_frame(Art,design,d,f,wardrobe).layers for d in range(4) for f in range(4)]
        save(key,standing,frames,tags)
        save(key+"-sentado",draw_frame(Art,design,0,0,wardrobe,seated=True))
        save(key+"-sentado-atras",draw_frame(Art,design,3,0,wardrobe,seated=True))


def draw_people(Art, save):
    for name, design in DESIGNS.items():
        key = "npc-" + slug(name)
        cut=WARDROBE.get(name,"regular")
        first = draw_frame(Art, design, 0, 0,cut)
        if name in MOBILE:
            frames = [draw_frame(Art, design, d, f,cut).layers for d in range(4) for f in range(4)]
            tags = [(i*4,i*4+3,"Caminar "+d) for i,d in enumerate(("abajo","izquierda","derecha","arriba"))]
            save(key,first,frames,tags)
        else:
            save(key,first)
        if name in ("Guardia","Aylin"):
            save(key+"-sentado",draw_frame(Art,design,0,0,cut,seated=True))
    draw_campus_crowd(Art,save)
