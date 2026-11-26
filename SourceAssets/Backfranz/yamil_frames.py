"""Layered directional sprites for Yamil and the original campus walker."""
from PIL import ImageOps


def draw_yamil(Art, save, student=False):
    if not student:
        draw_protagonist(Art, save)
        return
    outline, hair, sheen = '#151d2c', '#232735', '#444453'
    skin, light, shade = '#dda071', '#f3bb8b', '#ab694f'
    jacket, warm, red_dark = '#ab4249', '#da6460', '#762e40'
    denim, denim_light = '#344762', '#587492'
    bag, bag_light = '#445c62', '#75958f'
    if student:
        hair, sheen = '#3c2932', '#694348'
        skin, light, shade = '#b47858', '#d99a71', '#875247'
        jacket, warm, red_dark = '#3f777b', '#6ba49c', '#2e535e'
        denim, denim_light = '#3f4959', '#677687'
        bag, bag_light = '#6a5545', '#a88a65'
    frames=[]
    for direction in ('abajo','izquierda','derecha','arriba'):
        for frame in range(4):
            a=Art((32,48));a.shadow((6,39,25,46))
            profile=direction in ('izquierda','derecha');back=direction=='arriba'
            stride=(0,1,0,-1)[frame];bob=-abs(stride)

            def box(x,y,r,b,c,layer=1):a.rect((x,y+bob,r,b+bob),c,layer)
            def shape(points,c,layer=1):a.poly([(x,y+bob) for x,y in points],c,layer)
            def line(coords,c,width=1):a.line(tuple(v+bob if i%2 else v for i,v in enumerate(coords)),c,width=width)

            # Shoes and legs have separate silhouettes. Side strides spread along X.
            def leg(x,dy,far=False):
                box(x,29,x+5,39+dy,outline);box(x+1,31,x+4,37+dy,denim if far else denim_light)
                box(x-1,38+dy,x+5,42+dy,outline);box(x,41+dy,x+4,41+dy,'#d0d3bc',2)
            if profile:
                leg(17+2*stride,-stride,True);leg(12-2*stride,stride)
            else:
                leg(9,2*stride,back);leg(17,-2*stride)

            if profile:
                # The backpack always sits behind the shoulder; the nose leads the silhouette.
                shape([(20,20),(26,20),(28,24),(28,32),(25,34),(20,32)],outline)
                box(22,22,26,31,bag);box(23,26,26,29,bag_light,2)
                shape([(12,19),(20,18),(23,22),(22,32),(11,32),(10,25)],outline)
                box(12,21,21,30,jacket);box(13,22,15,29,warm,2)
                arm=14+stride
                box(arm,22,arm+5,31,outline,2);box(arm+1,23,arm+4,28,warm,2);box(arm+1,29,arm+4,31,skin,2)
                shape([(10,5),(15,2),(21,3),(25,7),(25,14),(22,18),(18,20),(12,20),(8,17),(6,15),(8,12),(8,8)],outline)
                shape([(10,10),(17,8),(22,10),(22,16),(19,18),(12,18),(9,16),(7,15),(9,13)],skin)
                box(10,15,12,16,light,2);box(13,11,14,13,outline,2);box(13,11,13,11,'#fff1d0',2)
                box(10,17,13,17,shade,2);box(20,12,21,15,shade,2);box(21,12,22,13,light,2)
                shape([(9,10),(9,7),(13,4),(17,3),(21,4),(24,7),(24,12),(22,13),(20,10),(18,10),(17,8),(13,10)],hair,2)
                line((12,7,15,5,20,5,22,7),sheen)
            else:
                shape([(10,19),(21,19),(25,22),(26,28),(23,31),(22,33),(9,33),(8,30),(5,29),(6,23)],outline)
                shape([(10,21),(21,21),(24,23),(24,27),(22,28),(21,31),(10,31),(9,28),(7,27),(7,24)],jacket)
                box(8,23,10,26,warm,2);box(20,23,22,30,red_dark,2)
                box(6,27+stride,8,30+stride,outline,2);box(7,27+stride,8,29+stride,skin,2)
                box(23,27-stride,25,30-stride,outline,2);box(23,27-stride,24,29-stride,skin,2)
                shape([(8,7),(11,3),(17,2),(21,3),(25,7),(26,13),(24,17),(22,20),(18,21),(13,21),(9,19),(6,15),(6,11)],outline)
                if back:
                    shape([(8,8),(12,4),(18,3),(23,6),(24,9),(24,14),(22,18),(18,19),(12,18),(8,15)],hair)
                    line((10,9,13,6,18,5,21,7),sheen)
                    box(12,18,19,19,shade,2)
                    box(9,22,10,28,bag_light,2);box(21,22,22,28,bag_light,2)
                    shape([(11,22),(13,20),(18,20),(21,22),(22,30),(20,33),(11,33),(9,30)],outline,2)
                    box(11,23,20,30,bag,2);box(12,26,19,29,bag_light,2);box(15,26,17,26,'#ebbb7b',2)
                else:
                    shape([(8,11),(12,8),(19,8),(23,10),(24,14),(22,17),(20,19),(12,19),(9,17),(7,14)],skin)
                    box(10,15,12,16,light,2);box(19,15,21,16,light,2)
                    for eye in (10,19):
                        box(eye,12,eye+2,14,outline,2);box(eye,12,eye,12,'#fff1d0',2)
                    box(14,15,16,15,light,2);box(14,17,17,17,shade,2)
                    shape([(7,12),(8,8),(12,4),(17,3),(21,4),(24,8),(25,12),(22,11),(21,9),(19,10),(16,8),(14,9),(11,9),(10,11)],hair,2)
                    line((10,8,13,5,19,5,22,7),sheen)
                    box(12,21,13,27,bag_light,2);box(19,21,20,27,bag_light,2)
                    box(15,22,16,29,'#efaa79',2)
            if direction=='derecha':a.layers=[ImageOps.mirror(layer) for layer in a.layers]
            frames.append(a.layers)
    tags=[(i*4,i*4+3,'Caminar '+direction) for i,direction in enumerate(('abajo','izquierda','derecha','arriba'))]
    save('pb-estudiante-camina' if student else 'yamil',a,frames,tags)


DIRECTIONS = ('abajo', 'izquierda', 'derecha', 'arriba')


def protagonist_frame(Art, direction, step=0, gesture=0):
    art=Art((40,56))
    ink, hair, hair_light = '#101724', '#242732', '#40404c'
    skin, skin_light, skin_shadow = '#d69b72', '#efb78b', '#ad715b'
    red, red_light, red_shadow = '#a94a49', '#d57664', '#693540'
    blue, blue_light, sole = '#304156', '#536982', '#bcb9ac'
    back, profile = direction=='arriba', direction in ('izquierda','derecha')
    stride = (0, 2, 4, 0, -2, -4)[step]
    lift = (0, -1, -2, 0)[gesture]
    bob = lift + (1 if step in (0,3) else 0)
    art.shadow((7,46,32,53))

    def shape(points,color,layer=1): art.poly([(x,y+bob) for x,y in points],color,layer)
    def box(x,y,right,bottom,color,layer=1): art.rect((x,y+bob,right,bottom+bob),color,layer)
    def line(points,color,width=1,layer=2):
        art.line(tuple(value+bob if i%2 else value for i,value in enumerate(points)),color,width=width,layer=layer)

    def leg(hip,foot,y,far):
        colors = blue if far else blue_light
        shape([(hip-3,37),(hip+3,37),(foot+3,y-3),(foot+3,y),(foot-3,y),(foot-4,y-3)],ink)
        line((hip,38,foot,y-4),colors,width=4,layer=1)
        shape([(foot-3,y-4),(foot+2,y-4),(foot+3,y),(foot-4-(2 if profile else 0),y),(foot-4-(2 if profile else 0),y-2)],ink)
        line((foot-3-(2 if profile else 0),y-1,foot+2,y-1),sole)
        if not far:line((hip-1,40,foot-1,y-5),'#7a8692')

    if profile:
        leg(21,22-stride,49,True)
        leg(17,17+stride,50,False)
        shape([(22,24),(29,24),(33,28),(34,36),(30,41),(22,39)],ink)
        shape([(25,26),(29,26),(31,29),(32,35),(29,39),(24,37)],red_shadow)
        line((28,27,30,29,30,35),red_light)
        box(29,32,31,33,'#dfa86d',2)
    else:
        leg(15,14,49+stride//2,back)
        leg(24,25,49-stride//2,not back)

    shape([(13,24),(25,24),(29,27),(28,33),(27,39),(13,40),(10,36),(10,28)],ink)
    shape([(13,26),(24,26),(27,28),(26,37),(24,38),(14,38),(12,35),(12,28)],red)
    line((14,28,14,35,16,37),red_light)
    line((25,30,25,36,22,37),red_shadow,2)
    if not profile and not back:
        line((19,27,20,37),'#e3b987')
        line((14,34,17,33),red_shadow)
        line((22,33,25,34),red_shadow)
        line((14,27,15,31),'#484553',2)
        line((25,27,24,31),'#484553',2)

    # The near arm swings on each footfall and reaches out in the interaction poses.
    reach=(0,3,7,3)[gesture]
    if profile:
        hand=(15-stride//2-reach,37-abs(stride)//2-reach//2)
        shoulder=(17,28)
    elif back:
        hand=(28+reach//2,37-stride//2-reach*2)
        shoulder=(26,28)
    else:
        hand=(29+reach//2,37-stride//2-reach)
        shoulder=(26,28)
    if not profile:
        line((12,28,10,36+stride//2),ink,5)
        line((12,29,10,34+stride//2),red_shadow,3)
        box(9,35+stride//2,11,37+stride//2,skin_shadow,2)
    line((*shoulder,*hand),ink,7)
    elbow=(round((shoulder[0]+hand[0])/2),round((shoulder[1]+hand[1])/2))
    line((*shoulder,*elbow,*hand),red_light,4)
    box(hand[0]-1,hand[1]-1,hand[0]+1,hand[1]+1,skin_light,2)

    if back:
        shape([(11,29),(15,25),(24,25),(29,29),(29,38),(25,41),(14,41),(11,38)],ink,2)
        shape([(13,29),(16,27),(24,27),(27,30),(27,37),(24,39),(15,39),(13,37)],red_shadow,2)
        box(15,30,25,35,red,2)
        line((15,35,25,35),red_light)
        box(20,35,22,36,'#dfa86d',2)
        line((14,31,14,36),'#b46b61')

    # Hair and highlights are drawn strand by strand at the larger pixel grid.
    if profile:
        shape([(8,11),(13,5),(19,4),(27,7),(30,13),(29,20),(25,25),(15,26),(10,23),(7,21),(5,19),(8,17)],ink,2)
        shape([(9,15),(15,11),(23,13),(26,17),(26,22),(20,25),(14,24),(9,21),(7,19),(10,18)],skin,2)
        shape([(9,13),(11,8),(16,5),(23,5),(28,9),(29,15),(26,19),(23,16),(20,13),(16,12),(12,15)],hair,2)
        line((12,11,16,8,21,8,25,10),hair_light)
        line((15,10,19,7,23,7),hair_light)
        box(11,16,13,18,ink,2);box(11,16,11,16,sole,2)
        box(23,17,25,20,skin_shadow,2);box(24,17,25,18,skin_light,2)
        line((9,20,13,20),skin_light)
        line((11,23,16,23),skin_shadow)
    else:
        shape([(8,12),(10,8),(14,5),(20,3),(25,5),(29,8),(32,12),(32,19),(29,23),(25,26),(14,26),(10,23),(7,18)],ink,2)
        if back:
            shape([(9,13),(12,8),(17,5),(22,5),(28,8),(30,13),(30,19),(26,23),(14,23),(10,20)],hair,2)
            box(15,23,24,25,skin_shadow,2)
            line((11,13,15,9,21,7,26,10),hair_light)
            line((14,14,18,11,23,10),hair_light)
            line((25,12,28,14,28,17),hair_light)
        else:
            shape([(9,15),(14,10),(23,10),(29,15),(30,19),(26,23),(23,25),(15,25),(11,23),(8,19)],skin,2)
            shape([(9,15),(10,10),(14,7),(20,4),(26,6),(30,10),(31,16),(27,13),(24,15),(21,12),(17,13),(15,11),(12,15)],hair,2)
            line((11,12,15,9,20,7,24,8,27,10),hair_light)
            line((15,10,19,8,22,8),hair_light)
            for eye in (12,24):
                box(eye,16,eye+3,19,ink,2)
                box(eye,16,eye,17,sole,2)
            line((17,20,20,19,21,20),skin_light)
            line((16,23,22,23),skin_shadow)
            box(11,20,14,21,skin_light,2);box(25,20,28,21,skin_light,2)
    if direction=='derecha':art.layers=[ImageOps.mirror(layer) for layer in art.layers]
    return art


def draw_protagonist(Art, save):
    legacy = [protagonist_frame(Art,d,step).layers for d in DIRECTIONS for step in (0,2,3,5)]
    walking = [protagonist_frame(Art,d,step).layers for d in DIRECTIONS for step in range(6)]
    interacting = [protagonist_frame(Art,d,gesture=step).layers for d in DIRECTIONS for step in range(4)]
    tags = [(16+6*i,21+6*i,'Caminar '+d) for i,d in enumerate(DIRECTIONS)]
    tags += [(40+4*i,43+4*i,'Interactuar '+d) for i,d in enumerate(DIRECTIONS)]
    save('yamil',protagonist_frame(Art,'abajo'),legacy+walking+interacting,tags)
