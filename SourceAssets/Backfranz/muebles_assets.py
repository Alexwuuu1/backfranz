"""Furniture sized for the existing CampusWorld footprints."""
from random import Random

INK = "#1b2633"
CREAM = "#eee1bf"
OAK = ("#dbad71", "#ac784b", "#6e4b41")
BIRCH = ("#ead2a3", "#be9e73", "#75604f")
STEEL = ("#a0b4b8", "#728990", "#435962")
TEAL = ("#83afa6", "#4d817f", "#375963")
RED = ("#e99676", "#ac6157", "#6f454c")
BLUE = ("#81a8c3", "#547a9b", "#384e70")


def shelf(Art, material, seed, levels, archive=False, display=False):
    rng=Random(seed)
    a=Art((48,52))
    a.shadow((2,39,46,51))
    top,face,side=material
    y=6 if levels==3 else 17
    a.poly([(3,y+7),(10,y),(45,y),(45,44),(37,49),(3,49)],INK)
    a.poly([(4,y+7),(11,y+1),(44,y+1),(37,y+7)],top)
    a.poly([(38,y+8),(44,y+2),(44,43),(38,47)],side)
    a.rect((5,y+8,36,47),face)
    for row in range(levels):
        floor=46-(levels-1-row)*11
        a.rect((7,floor-9,34,floor-1),INK)
        x=8
        while x<32:
            width=rng.randint(2,5) if not archive else rng.randint(5,8)
            height=rng.randint(4,8)
            color=rng.choice(("#d58a64","#5c9189","#d4b67d","#8f758d","#6d91ad","#a5ad79"))
            a.rect((x,floor-height,min(34,x+width-1),floor-1),color)
            if archive:
                a.rect((x+1,floor-height+2,min(34,x+3),floor-height+3),CREAM,2)
            else:
                a.line((x,floor-3,min(34,x+width-1),floor-3),CREAM)
            x+=width+1
        a.line((5,floor,36,floor),top,layer=2)
    a.line((5,y+8,5,46),top)
    a.line((36,y+8,36,46),side)
    if display:
        a.rect((12,y+2,27,y+5),CREAM,outline=side)
        a.line((15,y+3,23,y+3),TEAL[2])
    return a


def table(Art, material, style):
    a=Art((64,46))
    a.shadow((4,32,59,45))
    top,face,side=material
    narrow=style in ("pupitre","auxiliar")
    left,right=(13,51) if narrow else (5,59)
    seats=style in ("cafe-redonda","cafe-cuadrada","grupo")
    if seats:
        for x in (12,43):
            a.rect((x,3,x+9,14),TEAL[1],outline=INK)
            a.line((x+1,4,x+8,4),TEAL[0])
    for x in (left+3,right-6):
        a.rect((x,22,x+4,39),side,outline=INK)
        a.line((x+1,26,x+1,36),face)
    if style in ("cafe-redonda","auxiliar"):
        a.ellipse((left,9,right,30),INK)
        a.ellipse((left+1,9,right-1,27),face)
        a.ellipse((left+1,7,right-1,24),top)
    else:
        a.poly([(left,16),(left+9,7),(right,7),(right,24),(right-7,30),(left,30)],INK)
        a.poly([(left+1,17),(left+9,8),(right-1,8),(right-8,22),(left+1,22)],top)
        a.rect((left+1,23,right-8,28),face)
        a.poly([(right-7,22),(right-1,9),(right-1,23),(right-7,28)],side)
        a.line((left+3,20,right-10,20),CREAM)
    if style in ("lectura-lampara","lectura-doble","grupo","docente","pupitre","juridica"):
        for x in ((19,39) if style in ("lectura-doble","grupo") else (29,)):
            a.poly([(x-5,13),(x+4,12),(x+6,17),(x-3,18)],CREAM,2)
            a.line((x-2,14,x+3,14),TEAL[2])
            a.line((x-1,16,x+4,15),face)
    if style=="lectura-lampara":
        a.poly([(8,16),(13,9),(13,2),(16,2),(17,12),(15,17)],side,2)
        a.poly([(13,2),(25,3),(23,8),(13,8)],INK,2)
        a.line((15,7,22,7),"#f6d48f")
    if style in ("cafe-redonda","cafe-cuadrada"):
        a.ellipse((28,12,39,18),face,2)
        a.ellipse((30,10,36,16),CREAM,2)
        a.line((35,11,37,12,36,14),CREAM)
    if style=="taller":
        a.rect((14,11,27,17),side,outline=INK,layer=2)
        a.line((31,18,43,12),STEEL[0],width=2)
        a.rect((43,11,47,13),RED[1],layer=2)
    if style=="juridica":
        for x in (15,41):
            a.rect((x-2,16,x+3,18),INK,layer=2)
            a.line((x,16,x+3,11,x+5,11),STEEL[2])
        a.rect((29,23,36,25),STEEL[2],layer=2)
    if style=="docente":
        a.rect((39,11,49,15),TEAL[2],outline=INK,layer=2)
        a.line((41,13,48,13),CREAM)
    if seats:
        for x in (12,42):
            a.rect((x+1,34,x+3,42),STEEL[2],outline=INK,layer=2)
            a.rect((x+8,34,x+10,42),STEEL[2],outline=INK,layer=2)
            a.ellipse((x,29,x+11,37),INK,layer=2)
            a.ellipse((x+1,29,x+10,34),TEAL[0],layer=2)
    return a


def seat(Art, material, style):
    wide=style in ("banca-espera","banca-pasillo","sofa-modular")
    a=Art((64,40) if wide else (34,40))
    w=a.layers[0].width
    a.shadow((1,29,w-2,39))
    top,face,side=material
    back=5 if not wide else 10
    a.poly([(4,back+4),(8,back),(w-5,back),(w-3,back+4),(w-3,27),(w-8,32),(3,32),(2,25)],INK)
    a.rect((6,back+3,w-6,19),face)
    a.rect((7,back+3,w-7,back+5),top)
    a.poly([(6,20),(w-7,20),(w-4,25),(w-8,28),(4,28),(3,25)],top)
    a.rect((5,28,w-8,30),face)
    if wide:
        for x in (w//3,w*2//3):
            a.line((x,back+3,x,25),side)
    if style in ("butaca","lateral","sofa-modular"):
        for x in (3,w-7):
            a.rect((x,17,x+4,29),side,outline=INK)
            a.line((x+1,18,x+3,18),top)
    for x in (6,w-11):
        a.rect((x,30,x+3,35),side,outline=INK)
    if style=="aula":
        a.line((8,35,25,35),STEEL[0])
    if style=="visita":
        a.line((8,29,8,34,23,34,23,30),STEEL[0])
    if style=="banca-pasillo":
        a.line((7,17,w-8,17),STEEL[0])
    return a


def cart(Art):
    a=Art((48,46))
    a.shadow((3,35,45,45))
    for x in (8,36):
        a.ellipse((x,35,x+6,42),INK)
        a.rect((x+2,38,x+3,39),STEEL[0],layer=2)
    for y in (12,26):
        a.poly([(4,y+6),(11,y),(44,y),(44,y+7),(38,y+12),(4,y+12)],INK)
        a.poly([(6,y+6),(12,y+1),(43,y+1),(38,y+7)],OAK[0])
        a.line((6,y+9,38,y+9),STEEL[0])
    for x in (5,40):
        a.line((x,9,x,38),STEEL[1],width=2)
    a.rect((13,9,23,16),TEAL[1],outline=INK)
    a.rect((23,6,31,14),RED[1],outline=INK)
    a.rect((20,26,33,31),BIRCH[1],outline=INK)
    return a


def draw_furniture(Art, save, dark_layers):
    def variants(name, art):
        save("mob-"+name, art)
        dark=Art(art.layers[0].size)
        dark.layers=dark_layers(art.layers)
        save("backfranz-mob-"+name, dark)
    for i,(name,mat,rows,archive,display) in enumerate((
        ("estante-roble", OAK,3,False,False), ("estante-acero",STEEL,3,False,False),
        ("estante-bajo",BIRCH,2,False,False), ("estante-archivo",TEAL,3,True,False),
        ("estante-exhibidor",OAK,2,False,True),
    )):
        variants(name,shelf(Art,mat,91+i,rows,archive,display))
    for name,mat,style in (
        ("mesa-lectura-lampara",OAK,"lectura-lampara"), ("mesa-lectura-doble",BIRCH,"lectura-doble"),
        ("mesa-grupo",TEAL,"grupo"), ("mesa-aula",BIRCH,"pupitre"), ("mesa-profesor",OAK,"docente"),
        ("mesa-juridica",OAK,"juridica"), ("mesa-cafe-redonda",OAK,"cafe-redonda"),
        ("mesa-cafe-cuadrada",BIRCH,"cafe-cuadrada"), ("mesa-auxiliar",TEAL,"auxiliar"),
        ("mesa-taller",STEEL,"taller"),
    ):
        variants(name,table(Art,mat,style))
    for name,mat,style in (
        ("butaca-auditorio",BLUE,"butaca"), ("butaca-lateral",RED,"lateral"), ("silla-aula",TEAL,"aula"),
        ("silla-visita",BIRCH,"visita"), ("banca-espera",TEAL,"banca-espera"),
        ("banca-pasillo",STEEL,"banca-pasillo"), ("sofa-modular",RED,"sofa-modular"),
    ):
        variants(name,seat(Art,mat,style))
    variants("carro-libros",cart(Art))
