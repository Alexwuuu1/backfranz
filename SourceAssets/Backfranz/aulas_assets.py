"""Small layered props for the active classrooms and campus services."""

from bloque_a_assets import dark_layers

INK = "#24303c"
PAPER = "#eee1b6"
TEAL = "#69b4ae"
AMBER = "#dca966"


def draw_classroom_props(Art, save):
    def export(name, art):
        asset = "aula-" + name
        save(asset, art)
        altered = Art(art.layers[0].size)
        altered.layers = dark_layers(art.layers)
        save("backfranz-" + asset, altered)

    art = Art((38,22))
    for x, y, color in ((3,3,"#a6734d"),(12,7,"#516d87"),(22,4,"#648478")):
        art.rect((x,y,x+12,y+14),INK)
        art.rect((x+1,y+1,x+10,y+11),color)
        art.rect((x+4,y+3,x+8,y+4),PAPER,layer=2)
        art.line((x+2,y+12,x+10,y+12),PAPER)
    export("carpetas",art)

    art = Art((36,25))
    art.rect((0,2,35,24),INK)
    art.rect((2,4,33,21),"#506676")
    for x, color in ((4,TEAL),(15,AMBER),(26,"#b98aa3")):
        art.rect((x,7,x+6,12),color)
        for y in (15,18):art.line((x,y,x+5,y),PAPER)
    export("horarios",art)

    art = Art((34,20))
    art.rect((0,6,33,18),INK)
    art.rect((2,8,31,16),"#829494")
    for x, color in ((6,"#c57c5c"),(15,"#91ae69"),(24,PAPER)):
        art.ellipse((x,6,x+5,13),color,layer=2)
    art.rect((28,0,29,7),PAPER)
    export("bandeja-gastro",art)

    art = Art((32,22))
    art.rect((0,4,31,20),INK)
    art.rect((2,6,29,17),"#668288")
    art.rect((5,8,16,15),PAPER)
    art.rect((9,9,11,14),"#b96669",layer=2)
    art.rect((7,11,13,12),"#b96669",layer=2)
    art.line((21,8,26,14),"#d1dbc6",width=2)
    export("instrumental",art)

    art = Art((32,20))
    art.rect((0,13,31,19),INK)
    for x, color in ((3,"#ac789b"),(13,TEAL),(22,AMBER)):
        art.rect((x,3,x+6,15),PAPER)
        art.rect((x+1,7,x+5,13),color,layer=2)
        art.line((x,2,x+6,2),INK,width=2)
    export("muestras",art)

    art = Art((42,23))
    art.rect((0,3,41,22),INK)
    art.rect((2,5,39,19),"#394c5f")
    for x in (5,11,17,23):
        art.line((x,7,x,16),"#1a2732",width=2)
        y=8+(x*2)%8
        art.rect((x-1,y,x+1,y+2),PAPER,layer=2)
    for y, color in ((8,TEAL),(12,AMBER),(16,"#d17879")):art.rect((31,y,36,y+1),color,layer=2)
    export("mezcladora",art)

    art = Art((32,34))
    art.line((15,12,15,31),INK,width=3)
    art.line((15,25,7,33),INK,width=2)
    art.line((15,25,25,33),INK,width=2)
    art.poly([(3,4),(26,0),(30,11),(7,17)],INK)
    art.poly([(6,5),(24,2),(27,10),(8,14)],PAPER,layer=2)
    export("luz-estudio",art)

    art = Art((35,22))
    art.poly([(0,5),(16,3),(22,5),(33,3),(34,17),(21,21),(15,18),(1,20)],INK)
    art.poly([(2,5),(15,4),(20,6),(32,4),(32,15),(21,18),(15,16),(3,18)],PAPER)
    for y in (8,11,14):
        art.line((5,y,13,y-1),"#9c8c78")
        art.line((23,y+1,29,y),"#9c8c78")
    art.line((20,6,21,17),"#aa835f")
    export("cuaderno",art)

    art = Art((38,23))
    art.rect((0,4,37,22),INK)
    art.rect((2,6,35,19),"#497063")
    art.line((7,10,14,10,14,16,27,16,27,9,33,9),AMBER)
    art.rect((16,8,23,14),INK)
    for x in (6,10,28,32):art.rect((x,12,x+2,14),TEAL,layer=2)
    export("placa-electronica",art)

    art = Art((29,19))
    art.rect((1,4,27,17),INK)
    art.rect((3,6,25,16),"#314d59")
    art.rect((5,7,21,11),TEAL)
    for x in (6,10,14,18):art.rect((x,13,x+2,14),PAPER,layer=2)
    export("lector",art)

    art = Art((24,26))
    art.rect((2,2,21,24),INK)
    art.rect((4,4,19,22),"#428170")
    art.rect((6,6,17,15),PAPER)
    art.line((7,18,16,18),TEAL,width=2)
    export("dispensador",art)

    art = Art((30,19))
    art.ellipse((2,8,26,18),"#967d68")
    art.ellipse((4,8,24,15),PAPER,layer=2)
    art.ellipse((8,4,19,12),INK,layer=2)
    art.ellipse((9,5,18,9),"#986249",layer=2)
    art.line((18,7,23,8,23,11,18,11),PAPER,width=2)
    export("taza",art)
