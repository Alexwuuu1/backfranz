"""Editable pixel-art landmarks observed in the La Paz tour, not video textures."""
from random import Random
from PIL import ImageFont


def draw_tour(Art, save):
    ink, white, silver, glass = '#293743', '#f4f4ee', '#a4b0b3', '#a4c3c7'
    steel, pale, teal, wood, red = '#68787f', '#dedfd7', '#188b8b', '#ad673d', '#a02552'
    for name,base,edge,high in (
        ('brillo','#d8dfdd','#b0bcbb','#f5f9ef'),('beige','#c8b89c','#a2947e','#dfd4ba'),
        ('clinica','#e5e9e5','#bfcdca','#fbfff7'),('madera','#b88653','#956b48','#d2a574'),
        ('estudio','#b9bfc1','#9da7ad','#ccd2d3'),('vereda','#9ca7a8','#798689','#b9c1bd')):
        a=Art((32,32));a.rect((0,0,31,31),base);rng=Random(name)
        if name=='madera':
            for y in (0,8,16,24):
                a.line((0,y,31,y),edge);a.line((0,y+1,31,y+1),high)
                x=(y*5+6)%32;a.line((x,y,x,y+7),edge)
        else:
            a.line((0,0,31,0),edge);a.line((0,0,0,31),edge)
            a.line((1,1,31,1),high);a.line((1,2,1,31),high)
        if name in ('brillo','clinica'):
            a.line((16,11,28,11),high);a.line((23,12,30,12),high)
        else:
            for _ in range(9):
                x,y=rng.randrange(3,29),rng.randrange(2,30);a.line((x,y,x+2,y),rng.choice((high,edge)))
        save('tour-suelo-'+name,a)

    for name in ('blanco','vidrio','clinica','estudio','exterior'):
        a=Art((32,48));a.rect((0,6,31,42),(32,50,64,35),0)
        a.rect((0,4,31,39),white if name!='exterior' else '#bfccc7')
        a.rect((0,5,31,9), '#d2dbdc');a.line((0,4,31,4),steel);a.rect((0,33,31,39),'#a0adac');a.line((0,40,31,40),steel)
        if name=='vidrio':
            a.rect((0,10,31,32),glass);a.rect((0,15,31,21),'#cfe0dc');a.rect((0,25,31,29),'#c2d4d2')
            a.line((0,9,0,33),steel);a.line((4,14,15,10),white);a.line((22,29,30,26),white)
        elif name=='clinica':
            for y in (15,23,31):a.line((0,y,31,y),'#c5d0ce')
            a.line((15,10,15,32),'#d2dcd8')
        elif name=='estudio':
            for x in range(1,31,4):
                a.line((x,11,x,32),silver);a.line((x+1,11,x+1,32),pale)
        else:
            a.line((0,11,31,11),'#fbfbf4');a.line((0,32,31,32),'#d5dbd7')
        save('tour-muro-'+name,a)

    def rect(a,box,color,hi=white):
        x,y,r,b=box;a.rect(box,ink);a.rect((x+2,y+2,r-2,b-2),color);a.line((x+2,y+2,r-2,y+2),hi)
    def panel(a,box):
        rect(a,box,glass);x,y,r,b=box;a.rect((x+3,y+8,r-3,y+13),'#dce9e4');a.line((x+5,b-4,r-5,y+4),white)
    for name in ('ascensor-rojo','puerta-madera','puerta-vidrio','entrada','gradas','escalera-metal','ventanal','viga-verde','listones','banca-metal','mostrador-blanco','jardinera','estatua','torniquete','sillon-odontologia','campana','monitor-mac','mesa-blanca','asiento-azul','cabina-sonido','mesa-streaming','escritorio-ventana','linea-led','marca-piso','atril-auditorio','sofa-juego'):
        a=Art((64,56));a.shadow((3,42,61,54))
        if name in ('ascensor-rojo','puerta-madera','puerta-vidrio','entrada'):
            rect(a,(7,3,57,47),red if name=='ascensor-rojo' else wood if name=='puerta-madera' else steel)
            if name=='ascensor-rojo':
                rect(a,(16,10,49,44),silver);a.line((32,12,32,41),steel,width=2);a.rect((27,5,37,9),ink,2);a.rect((31,6,33,7),'#f9be67',2);a.rect((51,23,54,27),white,2)
            elif name=='puerta-madera':
                rect(a,(14,8,49,44),'#bc784b');rect(a,(21,12,43,30),'#76969f');a.rect((40,32,45,34),silver,2);a.line((18,33,18,42),'#dd9a65')
            else:
                panel(a,(13,7,33,44));panel(a,(33,7,52,44));a.rect((27,21,29,33),steel,2);a.rect((36,21,38,33),steel,2)
                if name=='entrada':a.rect((18,5,47,9),ink,2);a.line((24,7,41,7),white)
        elif name in ('gradas','escalera-metal'):
            rect(a,(7,4,57,48),pale if name=='gradas' else steel)
            for i,y in enumerate(range(8,46,5)):
                a.rect((12,y,52,y+3),(['#c4b59e','#dfd0b6'] if name=='gradas' else ['#778d92','#a4bab9'])[i%2]);a.line((12,y+4,52,y+4),steel)
            for x in (8,54):a.line((x,5,x,48),ink,width=2);a.line((x+1,6,x+1,46),silver)
        elif name in ('ventanal','viga-verde'):
            if name=='ventanal':
                rect(a,(1,6,62,42),'#a2cad4');a.rect((4,18,59,39),'#c7e0dd');a.rect((8,23,19,34),'#d8bc82');a.rect((26,20,37,34),'#a4a99b');a.rect((45,24,55,36),'#d6a178')
                for x in (3,22,41,60):a.line((x,8,x,41),ink,width=2)
                a.line((4,39,59,39),white)
            else:
                a.line((3,8,60,8),ink,width=8);a.line((3,8,60,8),teal,width=4);a.line((5,8,17,29,32,8,46,29,58,8),steel,width=3);a.line((4,29,59,29),teal,width=3)
        elif name=='listones':
            for i,x in enumerate(range(3,61,5)):
                color=('#67489a','#3a70af','#268fa4','#389d7a','#88a64b','#dfa735','#d96640','#ae4665')[i*8//12];a.rect((x,3,x+3,39),color,outline=ink);a.line((x+1,5,x+1,37),white)
            rect(a,(3,38,59,45),teal)
        elif name=='banca-metal':
            for x in (7,50):a.rect((x,30,x+5,45),ink)
            for x in (2,22,42):
                rect(a,(x,12,x+18,34),steel);a.line((x+3,17,x+15,17),silver);a.line((x+3,28,x+15,28),white);a.line((x+2,35,x+16,35),silver)
        elif name in ('mostrador-blanco','mesa-streaming','mesa-blanca','escritorio-ventana','atril-auditorio'):
            rect(a,(4,20,59,43),white if name!='escritorio-ventana' else wood);rect(a,(2,15,61,24),pale if name!='mesa-streaming' else teal)
            if name in ('mostrador-blanco','mesa-streaming'):
                rect(a,(36,6,51,18),steel);a.rect((12,18,21,20),white,2)
            else:
                a.rect((9,28,54,40), '#546972');a.line((8,26,56,26),silver)
        elif name=='jardinera':
            rect(a,(1,31,62,45),'#79594a');a.rect((4,28,60,34),'#405b40')
            for i,(x,y) in enumerate(((9,22),(18,14),(28,21),(36,11),(44,18),(56,15))):
                a.ellipse((x-7,y-7,x+6,y+8), ['#456d4f','#6f9960','#3e785b'][i%3],outline=ink);a.line((x-3,y,x+2,y-3),'#aec581')
        elif name=='estatua':
            rect(a,(12,36,51,48),pale);a.ellipse((26,4,37,15),steel,outline=ink);a.poly([(25,15),(38,15),(43,26),(38,35),(24,35),(20,26)],steel);a.line((27,17,24,29,29,34),silver,width=2)
        elif name=='torniquete':
            for x in (10,46):rect(a,(x,17,x+7,43),silver)
            a.rect((17,19,45,34),'#83b6bd');a.line((18,20,44,20),white);a.rect((7,15,21,19),steel,2);a.rect((43,15,57,19),steel,2)
        elif name=='sillon-odontologia':
            rect(a,(23,32,43,46),white);a.ellipse((15,12,35,33),'#279db6',outline=ink);a.ellipse((26,23,49,40),'#2aa5be',outline=ink);a.ellipse((15,10,25,20),'#79c8d4',outline=ink)
            a.line((47,34,47,11,38,8),steel,width=3);a.ellipse((33,5,43,12),white,outline=steel)
        elif name=='campana':
            a.poly([(18,5),(45,5),(46,13),(59,25),(5,25),(17,13)],ink);a.poly([(20,7),(43,7),(44,14),(55,23),(9,23),(19,14)],silver);a.line((13,22,50,22),white);rect(a,(9,34,55,43),steel)
        elif name=='monitor-mac':
            rect(a,(3,24,61,42),white);rect(a,(16,4,48,28),silver);a.rect((19,6,45,21),ink);a.line((22,10,39,10),teal);a.rect((29,28,35,34),steel);a.rect((24,34,40,36),silver,2)
        elif name=='asiento-azul':
            rect(a,(14,8,48,29),'#376f9b');rect(a,(12,24,51,36),'#558bad');a.rect((16,36,19,45),ink);a.rect((44,36,47,45),ink)
        elif name=='cabina-sonido':
            rect(a,(2,4,61,43),steel)
            for y in range(9,39,5):
                for x in range(6,59,5):a.rect((x,y,x+1,y+1),ink,2)
            panel(a,(16,11,48,35))
        elif name=='linea-led':
            a.layers[0].paste((0,0,0,0),(0,0,64,56));a.rect((0,24,63,31),(213,239,255,55),0);a.rect((0,27,63,28),(240,254,254,174),2)
        elif name=='marca-piso':
            a.layers[0].paste((0,0,0,0),(0,0,64,56));a.ellipse((16,15,47,39),'#48739e',outline=white);a.line((23,26,39,26),white,width=2);a.line((35,22,40,26,35,31),white,width=2)
        else:
            rect(a,(4,14,59,38),'#256e92');rect(a,(5,28,59,43),'#4586a5');a.rect((12,23,26,33),'#e8b647',2);a.rect((34,23,48,33),'#74a9ad',2)
        save('tour-'+name,a)

    for key,text in (('recepcion','RECEPCION'),('cajas','CAJAS'),('arca','ARCA'),('salud','SIMULACION'),('cocina','COCINA'),
                     ('mac','SALA MAC'),('sistemas','SISTEMAS'),('aulas','AULAS'),('dinamica','AULA DINAMICA'),
                     ('biblioteca','BIBLIOTECA'),('auditorio','AUDITORIO'),('juegos','GAME ROOM'),('entrada','BLOQUE A')):
        a=Art((104,20));a.rect((0,2,103,18),ink);a.rect((2,4,101,16),'#243138');a.rect((2,4,6,16),'#bc753f')
        a.draws[2].text((55,10),text,font=ImageFont.load_default(),fill=white,anchor='mm',stroke_width=0)
        save('tour-rotulo-'+key,a)
