"""Modular 32 px campus kit. Each sprite is authored on the three ASE layers."""
from random import Random


def draw_campus(Art, save):
    ink, cream, glass = '#263448', '#fff0bd', '#8ccacb'
    wood, metal, shadow = '#ac815d', '#70858e', '#4c5968'
    teal, blue, rust, leaf = '#508d82', '#517a9d', '#ba7462', '#648b60'
    warm, white = '#ceb497', '#dcd6bd'

    # Surfaces tile on the same 32 px grid as CampusWorld.
    surfaces = {
        'terrazo': ('#b9b9a4','#e3d7b6','#89998e',32), 'baldosa': ('#b3c7be','#dee6cc','#879d95',16),
        'hormigon': ('#8b989d','#afb4ae','#73848a',32), 'vereda': ('#9e9d8e','#bdbaa4','#747e76',16),
        'ladrillo': ('#a87564','#bf8a72','#775b55',8), 'laboratorio': ('#8ab3bc','#afd2ce','#688f99',16),
        'bano': ('#c2d1cb','#f1e7ce','#92aca9',8), 'goma': ('#586d80','#829499','#485a69',32),
        'madera': ('#a98260','#cfa779','#795d4b',8), 'alfombra': ('#736683','#97869d','#5b506e',32),
        'cesped': ('#68835f','#91a879','#536f54',0), 'tierra': ('#9a795b','#b59972','#755f4e',0),
        'adoquin': ('#888d91','#aeaea6','#67757c',8), 'asfalto': ('#4f5969','#687685','#414c5a',0),
    }
    for index,(name,(base,light,dark,step)) in enumerate(surfaces.items()):
        a=Art((32,32));a.rect((0,0,31,31),base);rng=Random(360+index)
        if step:
            for y in range(0,32,step):
                a.line((0,y,31,y),dark);a.line((0,y+1,31,y+1),light)
                if name!='madera':
                    for x in range((y//step%2)*4 if name in ('adoquin','ladrillo') else 0,32,step):
                        a.line((x,y,x,min(31,y+step-1)),dark)
        for _ in range(24 if name in ('cesped','terrazo','asfalto') else 9):
            x,y=rng.randrange(2,30),rng.randrange(2,31);a.line((x,y,x+1,y),rng.choice((light,dark)))
        save('campus-suelo-'+name,a)

    for name,color in [('crema',white),('ladrillo',rust),('azulejo',glass),('panel',warm),('vidrio',blue),('exterior',metal)]:
        a=Art((32,48));a.shadow((0,36,31,47));a.rect((0,2,31,39),ink);a.rect((0,4,31,34),color)
        a.rect((0,3,31,7),cream);a.rect((0,32,31,38),shadow)
        if name in ('ladrillo','azulejo'):
            for y in (14,22,30):
                a.line((0,y,31,y),shadow)
                for x in range(4 if y%16==6 else 0,32,8):a.line((x,y-6,x,y),shadow)
        elif name=='vidrio':
            for x in (0,16,31):a.line((x,8,x,31),ink)
            a.line((4,27,13,13),glass,width=2);a.line((21,28,28,17),glass,width=2)
        else:
            for x in (1,16,31):a.line((x,8,x,31),shadow)
            a.line((2,9,14,9),cream);a.line((18,9,29,9),cream)
        save('campus-muro-'+name,a)

    def body(a, box, color, top=cream):
        x,y,r,b=box;a.rect(box,ink);a.rect((x+2,y+2,r-2,b-2),color);a.line((x+2,y+2,r-2,y+2),top)

    def glass_panel(a, box):
        x,y,r,b=box;body(a,box,blue,glass);a.line((x+4,b-4,r-4,y+5),glass,width=2)

    for name in ('aula','biblioteca','doble-vidrio','exterior','bano','emergencia'):
        a=Art((48,64));a.shadow((3,49,45,61));body(a,(4,4,43,55),metal)
        double=name in ('doble-vidrio','exterior');color={'aula':wood,'biblioteca':teal,'bano':blue,'emergencia':rust}.get(name,shadow)
        body(a,(8,8,39,52),color)
        if double:
            glass_panel(a,(10,11,23,44));glass_panel(a,(24,11,37,44))
            a.rect((20,28,21,36),cream);a.rect((26,28,27,36),cream)
        else:
            if name in ('aula','biblioteca'):glass_panel(a,(12,12,26,28))
            a.rect((29,31,35,34),cream,2)
            if name=='emergencia':a.rect((12,33,35,38),white)
        a.rect((7,53,40,56),white);save('campus-puerta-'+name,a)

    for name in ('ventana','ventanal','escalera-subida','escalera-bajada','rampa','baranda','torniquete','columna','techo','macetero-exterior'):
        a=Art((48,48));a.shadow((1,35,46,46))
        if name.startswith('ventan'):
            glass_panel(a,(1,8,46,34));a.line((23,10,23,33),ink,width=2)
            if name=='ventanal':a.line((3,21,44,21),ink,width=2)
            a.rect((0,34,47,37),white)
        elif name.startswith('escalera') or name=='rampa':
            body(a,(6,4,41,43),metal)
            if name=='rampa':a.poly([(9,39),(38,39),(35,10),(12,10)],teal);a.line((10,40,13,8),cream);a.line((37,40,34,8),cream)
            else:
                for i,y in enumerate(range(8,40,5)):
                    a.rect((10,y,37,y+3),white if (i%2==0)==(name.endswith('subida')) else metal);a.line((10,y+4,37,y+4),shadow)
            for x in (7,40):a.line((x,5,x,43),ink,width=2)
        elif name=='baranda':
            for x in (3,14,24,34,44):a.rect((x,14,x+2,37),metal,outline=ink)
            body(a,(1,10,46,16),metal)
        elif name=='torniquete':
            body(a,(17,12,29,40),metal);a.line((23,15,10,24),white,width=3);a.line((23,15,36,24),white,width=3);a.line((23,15,23,7),white,width=3);a.rect((20,33,26,35),teal)
        elif name=='columna':body(a,(14,3,33,40),white);a.rect((12,35,35,41),shadow)
        elif name=='techo':
            a.poly([(2,21),(24,5),(46,21),(46,30),(2,30)],ink);a.poly([(4,21),(24,8),(44,21),(44,26),(4,26)],rust);a.line((7,22,41,22),cream)
        else:
            body(a,(4,28,43,42),wood);a.rect((6,26,41,30),shadow)
            for x,y in [(10,21),(18,15),(26,20),(34,14),(39,22)]:a.ellipse((x-6,y-7,x+5,y+8),leaf,outline=ink);a.line((x-3,y,x+2,y-4),cream)
        save('campus-arq-'+name,a)

    # Area-specific furniture silhouettes, with consistent floor shadows.
    def screen(a,x,y,w,h):
        body(a,(x,y,x+w,y+h),blue,glass);a.line((x+4,y+5,x+w-4,y+5),glass)
    for name,color in [
        ('mostrador-orientacion',teal),('mostrador-biblioteca',wood),('puesto-seguridad',blue),
        ('pizarra',teal),('pantalla-proyector',white),('proyector',metal),('pupitre',wood),('banco',wood),('sofa',teal),('mesa-redonda',wood),
        ('mesa-cafeteria',rust),('barra-cafeteria',wood),('vitrina-cafeteria',blue),('estante-revistas',rust),('carro-libros',metal),
        ('mesa-laboratorio',glass),('microscopio',white),('impresora',white),('estacion-pc',blue),('dispensador-agua',glass),
        ('basurero',metal),('reciclaje',teal),('extintor',rust),('botiquin',white),('lavamanos',white),('inodoro',white),
        ('caballete',wood),('tablero-avisos',warm),('bicicletero',metal),('luminaria-patio',blue),('arbusto',leaf),('arbol',leaf),
    ]:
        a=Art((48,48));a.shadow((3,34,44,46))
        if name.startswith(('mostrador','puesto','barra','vitrina')):
            body(a,(3,17,44,40),color);body(a,(1,13,46,23),wood)
            if name.startswith('vitrina'):
                glass_panel(a,(4,4,43,19))
                for x in (10,19,29):a.ellipse((x,13,x+6,16),warm,2)
            elif name.startswith('barra'):
                for x in (6,17,29):a.rect((x,10,x+5,15),cream,2)
            else:screen(a,27,4,13,10);a.rect((9,15,18,18),cream,2)
            for x in (7,22,36):a.line((x,27,x,37),shadow)
        elif name in ('pizarra','pantalla-proyector','tablero-avisos'):
            body(a,(1,5,46,31),color);a.rect((4,30,43,34),metal)
            for i,(x,y) in enumerate([(8,12),(21,10),(12,22),(30,18)]):
                if name=='tablero-avisos':a.rect((x,y,x+6,y+5),[cream,glass,white,rust][i],2)
                elif name=='pizarra':a.line((x,y,x+6,y),cream)
            if name=='pantalla-proyector':a.rect((6,9,41,27),'#f5edd6',2)
        elif name in ('pupitre','mesa-laboratorio','estacion-pc','mesa-cafeteria'):
            for x in (8,36):a.rect((x,25,x+3,40),ink)
            body(a,(4,14,43,29),color)
            if name=='estacion-pc':screen(a,18,4,21,13);a.rect((13,22,33,25),ink)
            elif name=='mesa-laboratorio':a.rect((9,18,18,25),blue);a.rect((25,16,28,23),cream)
            elif name=='pupitre':a.rect((16,17,28,23),cream);a.line((17,19,25,19),shadow)
            else:a.ellipse((16,17,26,25),white,2);a.rect((31,17,34,23),cream,2)
        elif name in ('banco','sofa'):
            for x in (7,36):a.rect((x,29,x+3,40),ink)
            body(a,(4,11,43,29),color);body(a,(3,23,44,34),color)
            if name=='sofa':body(a,(2,21,9,34),color);body(a,(38,21,45,34),color);a.line((23,25,23,32),shadow)
            else:a.line((6,17,41,17),shadow)
        elif name=='mesa-redonda':
            a.rect((20,22,27,40),shadow,outline=ink);a.ellipse((3,9,44,30),color,outline=ink);a.line((11,14,34,14),cream)
        elif name in ('estante-revistas','carro-libros'):
            body(a,(7,7,40,37),color)
            for y in (11,23):
                for i,x in enumerate(range(10,36,6)):a.rect((x,y,x+3,y+7),[cream,blue,teal,rust,white][i],2)
                a.line((9,y+8,38,y+8),ink)
            if name=='carro-libros':
                for x in (9,32):a.ellipse((x,34,x+6,42),ink)
        elif name in ('arbol','arbusto'):
            if name=='arbol':a.rect((21,16,26,42),wood,outline=ink)
            for x,y in ((13,22),(25,15),(35,21),(24,26)):
                ry=10 if name=='arbol' else 6;y+=0 if name=='arbol' else 8;a.ellipse((x-9,y-ry,x+9,y+ry),leaf,outline=ink);a.line((x-4,y-3,x,y-5),cream)
        elif name=='microscopio':
            body(a,(10,32,37,40),metal);a.line((16,32,28,21,26,11),metal,width=5);a.line((15,8,24,16),white,width=6);a.rect((10,25,26,28),ink)
        elif name=='impresora':
            body(a,(6,16,41,35),color);a.rect((13,7,35,19),cream,outline=ink);a.rect((12,25,35,28),ink);a.rect((15,28,32,34),white);a.rect((35,20,37,22),teal)
        elif name=='proyector':
            body(a,(8,17,40,30),color);a.ellipse((28,20,35,27),glass,outline=ink);a.line((12,22,22,22),ink)
        elif name=='dispensador-agua':
            body(a,(15,19,34,39),white);a.ellipse((15,5,34,22),glass,outline=ink);a.rect((22,25,28,30),blue);a.rect((21,31,29,33),shadow)
        elif name in ('basurero','reciclaje'):
            a.poly([(11,16),(36,16),(33,39),(14,39)],ink);a.poly([(13,19),(34,19),(31,37),(16,37)],color);body(a,(10,13,37,20),metal)
            if name=='reciclaje':a.poly([(23,23),(29,31),(24,31),(24,34),(19,28)],cream,2)
        elif name=='extintor':
            a.ellipse((16,14,32,39),rust,outline=ink);a.rect((19,8,27,16),metal,outline=ink);a.line((26,11,34,13,36,25),ink,width=2);a.rect((20,21,27,27),cream)
        elif name=='botiquin':
            body(a,(9,12,38,35),white);a.rect((21,17,26,30),rust);a.rect((17,21,30,26),rust)
        elif name in ('lavamanos','inodoro'):
            if name=='inodoro':body(a,(12,8,36,20),white)
            else:a.line((24,11,24,18,28,18),metal,width=3)
            a.ellipse((10,18,38,39),white,outline=ink);a.ellipse((15,22,33,33),glass,outline=metal)
        elif name=='caballete':
            a.line((12,39,21,9,31,39),wood,width=4);body(a,(9,12,38,33),white);a.line((14,20,32,20),rust);a.line((15,25,28,25),shadow)
        elif name=='bicicletero':
            for x in (6,18,30):a.line((x,36,x,18,x+8,18,x+8,36),color,width=3)
        elif name=='luminaria-patio':
            a.rect((22,11,26,40),metal,outline=ink);body(a,(13,7,35,17),blue);a.rect((17,15,31,17),cream,2)
        save('campus-prop-'+name,a)

    # Pictograms are drawn in pixels, so the same source works in UI or on the map.
    def glyph(a,name,x=0,y=0,c=cream):
        def line(points,width=2):a.line(tuple(v+(x if i%2==0 else y) for i,v in enumerate(points)),c,width=width)
        def rect(box,color=c):a.rect((box[0]+x,box[1]+y,box[2]+x,box[3]+y),color,2)
        if name in ('a','b'):
            if name=='a':line((2,12,6,2,8,2,12,12));line((4,8,10,8))
            else:line((3,12,3,2,9,2,11,4,9,7,4,7,10,7,12,9,10,12,3,12))
        elif name=='informacion':rect((6,2,8,4));rect((6,6,8,12))
        elif name=='aula':line((2,12,2,3,12,3,12,12));line((8,8,10,8),1)
        elif name=='biblioteca':line((1,3,6,3,7,5,8,3,13,3,13,11,8,11,7,12,6,11,1,11,1,3),1);line((7,5,7,11),1)
        elif name=='ascensor':line((2,6,5,2,8,6));line((6,9,9,13,12,9));line((5,2,5,9),1);line((9,13,9,6),1)
        elif name=='escalera':line((1,12,5,12,5,9,8,9,8,6,11,6,11,3,14,3))
        elif name=='laboratorio':line((5,2,9,2));line((6,2,6,6,2,12,12,12,8,6,8,2),1);line((4,9,10,9),1)
        elif name=='cafeteria':line((3,4,3,10,10,10,10,4,3,4));line((10,5,13,5,13,8,10,8),1);line((2,13,12,13),1)
        elif name=='bano':rect((6,1,8,3));line((3,7,7,5,11,7));line((7,5,7,10,4,13));line((7,10,10,13))
        elif name=='salida':line((2,12,2,2,8,2,8,5),1);line((5,8,13,8,10,5));line((13,8,10,11))
        elif name=='salud':rect((5,2,9,12));rect((2,5,12,9))
        elif name=='accesible':rect((6,1,8,3));line((7,5,7,8,11,8,13,12));line((5,5,2,7,2,11,5,13,9,12),1)
        elif name=='izquierda':line((13,7,2,7,6,3));line((2,7,6,11))
        elif name=='derecha':line((1,7,12,7,8,3));line((12,7,8,11))
        elif name=='recto':line((7,13,7,2,3,6));line((7,2,11,6))

    for name in ('a','b','informacion','aula','biblioteca','ascensor','escalera','laboratorio','cafeteria','bano','salida','salud','accesible','izquierda','derecha','recto'):
        a=Art((32,24));body(a,(1,2,30,21),blue if name not in ('a','b','salida','salud') else {'a':teal,'b':rust,'salida':teal,'salud':rust}[name]);glyph(a,name,9,5)
        save('campus-senal-'+name,a)

    for name in ('horario','mapa','cuaderno','mochila','llave','pase-biblioteca','botella','lapiz','auriculares','laptop'):
        a=Art((32,32));a.shadow((4,22,27,30))
        if name in ('horario','mapa','cuaderno','pase-biblioteca'):
            body(a,(5,5,26,26),white)
            if name=='mapa':a.line((8,21,13,21,13,12,21,12,21,8),teal,width=2);a.rect((19,8,23,11),rust,2)
            else:
                a.rect((8,8,23,11),blue if name=='horario' else rust,2)
                for y in (15,18,21):a.line((9,y,22,y),shadow)
                if name=='horario':a.line((15,13,15,23),teal)
        elif name=='mochila':
            body(a,(7,8,25,27),rust);body(a,(10,17,22,24),wood);a.line((12,7,12,5,20,5,20,7),ink)
        elif name=='llave':
            a.ellipse((5,8,16,19),cream,outline=ink);a.ellipse((8,11,13,16),ink);a.line((14,15,25,23,23,26),warm,width=3)
        elif name=='botella':
            body(a,(10,10,22,27),glass);body(a,(13,5,19,11),metal);a.rect((13,15,19,19),white,2)
        elif name=='lapiz':
            a.line((7,25,24,8),cream,width=4);a.line((21,10,25,6),rust,width=3);a.poly([(5,27),(9,25),(7,22)],ink)
        elif name=='auriculares':
            a.line((7,21,7,13,12,8,20,8,25,13,25,21),ink,width=3)
            body(a,(5,17,11,26),blue);body(a,(21,17,27,26),blue)
        else:screen(a,7,6,18,13);body(a,(4,20,28,26),metal)
        save('campus-item-'+name,a)
