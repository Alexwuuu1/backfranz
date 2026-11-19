"""Layered sprites for the seven public floors and the fictional basement."""
from PIL import Image


def draw_block(Art, save):
    ink, paper, red = '#243144', '#f3dfb6', '#b85257'
    steel, blue, aqua, wood = '#718894', '#45668c', '#6cb8a8', '#a87857'
    amber, soft, black = '#e3aa62', '#ccd5c0', '#172536'

    def slab(a, b, color, hi=paper):
        x,y,r,d=b;a.rect(b,ink);a.rect((x+2,y+2,r-2,d-2),color);a.line((x+2,y+2,r-2,y+2),hi)
    def screen(a, b):
        slab(a,b,black,aqua);x,y,r,d=b;a.line((x+4,y+5,r-4,y+5),aqua);a.line((x+4,y+8,r-8,y+8),blue)
    def legs(a, xs, y, bottom):
        for x in xs:a.rect((x,y,x+3,bottom),ink);a.line((x+1,y,x+1,bottom-2),steel)
    def equipment(a, name):
        if name in ('arcade','terminal','rack','nevera','horno','caja-pagos','maquina-ticket'):
            if name=='arcade':
                a.poly([(6,40),(6,25),(10,20),(10,5),(36,5),(39,39)],ink);a.poly([(8,38),(8,26),(12,21),(12,7),(34,7),(37,38)],red)
                screen(a,(13,9,32,24));slab(a,(8,24,36,30),steel);a.rect((13,23,15,27),amber,2);a.ellipse((27,25,30,28),aqua,2)
                a.line((11,37,34,37),amber)
            elif name=='caja-pagos':
                slab(a,(1,13,46,40),wood);slab(a,(5,3,42,23),steel,aqua);screen(a,(24,9,38,18));a.rect((9,20,23,24),paper,2);a.line((4,30,43,30),ink,width=2)
            else:
                slab(a,(8,4,39,41),{'terminal':blue,'rack':black,'nevera':soft,'horno':steel,'maquina-ticket':blue}[name]);
                if name in ('terminal','horno','maquina-ticket'):
                    screen(a,(12,8,35,23));a.rect((14,26,33,29),black);a.rect((14,34,18,36),amber);a.rect((28,34,33,36),red)
                elif name=='rack':
                    for y in range(10,38,6):a.rect((12,y,35,y+3),steel);a.rect((15,y+1,17,y+2),aqua);a.rect((31,y+1,33,y+2),red)
                else:a.line((10,19,37,19),ink,width=2);a.rect((30,10,32,17),steel);a.rect((30,23,32,32),steel)
        elif name in ('pc','mac','streaming','mesa-dinamica','mesa-cocina','mesa-diseccion','mesa-fotografia','mesa-lectura','mesa-registro'):
            legs(a,(7,37),25,40);slab(a,(3,15,44,30),steel if name in ('mesa-cocina','mesa-diseccion') else wood)
            if name in ('pc','mac','streaming'):
                screen(a,(9,3,32 if name=='pc' else 38,18));a.rect((18,19,24,22),steel);a.rect((11,24,29,26),soft,2)
                if name=='pc':slab(a,(34,15,43,35),black);a.rect((38,20,40,22),aqua,2)
                if name=='mac':a.line((11,17,36,17),soft,2,width=2)
                if name=='streaming':a.ellipse((36,4,45,13),paper,outline=ink);a.line((40,13,40,23),ink,width=2)
            elif name=='mesa-dinamica':
                a.rect((8,17,20,25),paper,2);a.rect((28,19,39,25),aqua,2);a.line((10,21,17,21),steel);a.line((31,22,36,22),blue)
            elif name=='mesa-cocina':
                for x in (11,26):a.ellipse((x,17,x+8,25),black,outline=paper)
                a.rect((36,18,41,23),red,2)
            elif name=='mesa-diseccion':a.rect((9,17,37,26),soft,2);a.line((11,20,35,20),aqua)
            elif name=='mesa-fotografia':screen(a,(11,10,29,24));a.ellipse((16,14,23,20),ink,outline=aqua)
            else:
                a.rect((9,17,23,24),paper,2);a.line((16,17,16,24),steel);a.rect((28,18,37,22),blue,2)
        elif name in ('camilla','maniqui','vitrina-anatomia','armario-medico','vitrina-codigo'):
            if name in ('camilla','maniqui'):
                legs(a,(10,34),27,41);slab(a,(6,9,41,31),aqua);slab(a,(9,11,21,20),paper)
                if name=='maniqui':
                    a.ellipse((14,12,21,19),amber,2,ink);a.line((21,18,31,23,38,23),paper,width=4);a.line((24,21,30,16),paper,width=2)
            else:
                slab(a,(5,4,42,40),steel);screen(a,(9,8,38,30));a.line((10,23,37,23),soft)
                for i,x in enumerate((13,21,29)):a.rect((x,16,x+4,22),[amber,aqua,red][i],2);a.rect((x+1,25,x+4,28),paper,2)
                if name=='vitrina-anatomia':a.ellipse((16,12,31,25),paper,2);a.rect((19,16,21,18),ink,2);a.rect((26,16,28,18),ink,2)
                if name=='vitrina-codigo':a.rect((21,33,27,37),amber,2)
        elif name in ('pizarra-movil','panel-horario','placa-aula','pantalla-gesell','cartel-aviso'):
            slab(a,(2,5,45,34),{'pizarra-movil':aqua,'panel-horario':blue,'placa-aula':wood,'pantalla-gesell':black,'cartel-aviso':red}[name]);
            for i,y in enumerate((12,18,24)):
                a.rect((7,y,11,y+2),[amber,aqua,paper][i],2);a.line((15,y+1,38-i*3,y+1),paper,width=2)
            if name=='pizarra-movil':legs(a,(6,37),31,42)
        elif name in ('tripode','microfono','tuberia','cable-caido','alarma','reja-cerrada','charco'):
            if name=='tripode':
                a.line((24,16,10,41,24,18,38,41),steel,width=3);screen(a,(14,7,33,17));a.ellipse((29,9,36,16),aqua,2,ink)
            elif name=='microfono':
                a.line((24,17,24,39,15,39),steel,width=3);a.ellipse((18,6,30,22),steel,outline=ink);a.line((20,11,28,11),paper)
            elif name=='tuberia':
                a.line((3,8,34,8,34,31,44,31),ink,width=8);a.line((3,7,34,7,34,30,44,30),steel,width=5);a.rect((13,3,16,12),red,2)
            elif name=='cable-caido':
                a.line((2,10,15,10,27,32,43,36),ink,width=5);a.line((2,9,15,9,27,31,43,35),amber,width=2)
            elif name=='alarma':
                slab(a,(11,21,36,34),ink);a.ellipse((14,9,33,26),red,outline=ink);a.line((19,13,26,13),paper,width=2)
            elif name=='reja-cerrada':
                for x in range(3,46,7):a.rect((x,5,x+3,40),steel,outline=ink)
                slab(a,(1,10,46,16),steel);slab(a,(1,30,46,36),steel);a.rect((21,20,28,28),amber,2)
            else:
                a.ellipse((2,27,44,43),(43,101,114,165),1);a.ellipse((12,25,31,39),(43,101,114,165),1);a.line((7,32,28,32),aqua)

    names=('arcade','terminal','rack','nevera','horno','caja-pagos','maquina-ticket','pc','mac','streaming',
           'mesa-dinamica','mesa-cocina','mesa-diseccion','mesa-fotografia','mesa-lectura','mesa-registro',
           'camilla','maniqui','vitrina-anatomia','armario-medico','vitrina-codigo','pizarra-movil','panel-horario',
           'placa-aula','pantalla-gesell','cartel-aviso','tripode','microfono','tuberia','cable-caido','alarma','reja-cerrada','charco')
    for name in names:
        a=Art((48,48));a.shadow((2,35,45,46));equipment(a,name);save('bloque-a-'+name,a)

    for name,color in [('fusible',amber),('llave-tecnica',steel),('credencial-salida',aqua),('guia-biblioteca',paper),
                       ('luz-azul',blue),('luz-roja',red),('luz-verde',aqua),('tablero-activo',aqua),('tablero-apagado',steel)]:
        a=Art((32,32));a.shadow((4,24,28,30))
        if name=='fusible':
            a.rect((4,12,27,21),ink);a.rect((8,13,23,20),color);a.rect((4,14,8,19),soft);a.rect((23,14,27,19),soft);a.line((10,15,21,15),paper)
        elif name=='llave-tecnica':
            a.ellipse((4,4,16,16),color,outline=ink);a.ellipse((8,8,12,12),ink);a.line((14,14,25,24,27,22),color,width=4);a.line((21,22,20,25),paper,width=2)
        elif name.startswith('luz-'):
            slab(a,(5,4,26,26),steel);a.ellipse((9,7,22,20),color,outline=ink);a.line((13,10,17,10),paper);a.rect((13,23,18,25),ink,2)
        else:
            slab(a,(5,6,26,26),color);a.rect((9,10,22,14),ink,2);a.line((10,19,21,19),paper);a.line((10,22,18,22),ink)
        save('bloque-a-item-'+name,a)


def dark_layers(layers):
    """Grade the three original layers without flattening their transparency."""
    result=[]
    for layer in layers:
        rgba=layer.convert('RGBA');mapped=Image.new('RGBA',rgba.size)
        mapped.putdata([((r*5+g)//9,(g*5+b*2)//10,min(255,(b*7+r)//9+13),a) if a else (0,0,0,0)
                        for r,g,b,a in rgba.getdata()]);result.append(mapped)
    return result
