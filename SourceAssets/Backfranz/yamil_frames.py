"""Yamil's 32×48, four-direction walk cycle, drawn on three pixel layers."""
from PIL import ImageOps


def draw_yamil(Art, save, student=False):
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
