"""Layered street tiles and architecture for the Landaeta entrance."""
from random import Random

INK = "#252c37"
STONE = "#8b8a7e"
CEMENT = "#b0ac99"
WHITE = "#dddccf"
GLASS = "#365963"
GLOW = "#66939a"


def stone_tiles(Art, save):
    for variant in range(3):
        rng=Random(108+variant)
        a=Art((16,16))
        a.rect((0,0,15,15),"#5d6567")
        for row in range(4):
            for column in range(-1,4):
                x=column*5+(2 if row%2 else 0)
                y=row*4
                color=rng.choice(("#747b7a","#798180","#818685","#6c7576"))
                a.rect((x+1,y,x+4,y+2),color)
                a.line((x+2,y,x+4,y),"#888d88")
        save(f"tile16-acceso-calle-{variant}-adoquin",a)
    for name,base,seam,light in (
        ("vereda","#a49e90","#848074","#c0b9a6"),
        ("rellano","#909b98","#626c67","#b6bfac"),
    ):
        a=Art((16,16))
        a.rect((0,0,15,15),base)
        a.line((0,0,15,0),light)
        a.line((0,15,15,15),seam)
        a.line((5,0,5,14),seam)
        a.line((6,1,6,14),light)
        for x,y in ((2,3),(10,6),(13,11),(8,13)):
            a.rect((x,y,x+1,y),seam)
        save("tile16-acceso-"+name,a)
    for name,stone_y in (("bordillo-norte",9),("bordillo-sur",1)):
        a=Art((16,16))
        a.rect((0,0,15,15),"#676f71")
        if stone_y==9:a.rect((0,0,15,8),"#a49e90")
        else:a.rect((0,7,15,15),"#a49e90")
        a.rect((0,stone_y,15,stone_y+5),"#a9a89a")
        a.line((0,stone_y,15,stone_y),"#d3cbb6")
        a.line((0,stone_y+5,15,stone_y+5),"#4d5458")
        a.line((9,stone_y+1,9,stone_y+4),"#83837b")
        save("tile16-acceso-"+name,a)
    a=Art((16,16))
    a.rect((0,0,15,15),"#7f8d8e")
    for y in (1,6,11):
        a.rect((0,y,15,y+1),"#c4c9b9")
        a.line((0,y+2,15,y+2),"#586a70")
        a.line((0,y+3,15,y+3),"#687a7c")
    save("tile16-acceso-escalon",a)


def campus(Art, save):
    for corner in (True,False):
        a=Art((80,112))
        a.rect((0,6,79,111),INK)
        a.rect((0,8,79,94),WHITE)
        a.rect((0,94,79,111),"#737f87")
        a.rect((0,92,79,95),"#9da7a9")
        a.rect((0,7,79,16),"#a4aeac")
        a.line((0,8,79,8),"#eff1db")
        for y in (25,59):
            a.rect((0,y-7,79,y+24),"#b4b9ae")
            for x in range(7,80,17):
                x1=min(79,x+13)
                a.rect((x,y,x1,y+23),INK)
                a.rect((x+1,y+1,x1-1,y+21),GLASS)
                a.line((x+2,y+1,x1-1,y+1),GLOW)
                a.line((x+2,y+8,min(x1-1,x+7),y+3),GLOW)
                a.line((x+7,y+2,x+7,y+21),INK)
            a.line((0,y+25,79,y+25),"#eef0de")
        a.line((0,103,79,103),"#8b999e")
        for x in range(0,80,16):a.line((x,96,x,109),"#59676e")
        if corner:
            a.poly([(0,8),(12,8),(10,107),(3,111),(0,111)],"#aeb8b3")
            a.line((11,9,9,106),"#dee2d5")
            a.rect((16,18,70,49),WHITE)
            a.rect((18,24,70,40),"#303d46")
            a.line((18,41,70,41),"#d28a51")
            for x,color in ((18,"#a1474d"),(36,"#c79c54"),(54,"#507e69")):
                a.line((x,0,x,19),"#606e73")
                a.poly([(x,1),(x+13,4),(x+10,10),(x,8)],color)
                a.line((x+1,5,x+10,7),"#eee1ad")
        save("acceso-campus-esquina" if corner else "acceso-campus-modulo",a)

    a=Art((32,80))
    a.poly([(0,18),(26,2),(31,2),(31,79),(0,79)],INK)
    a.poly([(2,19),(27,4),(31,4),(31,76),(2,78)],"#a1aca9")
    a.poly([(2,23),(27,9),(31,9),(31,56),(2,65)],"#c4ccc3")
    a.poly([(5,29),(26,18),(26,49),(5,58)],INK)
    a.poly([(7,30),(24,21),(24,47),(7,55)],GLASS)
    a.line((10,42,19,32),GLOW,width=2)
    a.line((14,26,14,52),"#75878a")
    a.poly([(2,66),(31,58),(31,76),(2,78)],"#64747c")
    a.line((4,69,29,62),"#9ea9a9")
    save("acceso-lateral",a)

    a=Art((37,50))
    a.poly([(4,16),(33,5),(36,9),(36,39),(7,49),(3,45)],INK)
    a.poly([(5,16),(33,7),(34,38),(6,47)],"#a5b1a9")
    a.poly([(8,18),(31,10),(31,37),(8,45)],"#334954")
    a.poly([(10,19),(29,12),(29,35),(10,42)],"#58868e")
    a.line((19,16,19,39),"#c3cac0",width=2)
    a.line((11,25,17,19),"#a7c6bb",width=2)
    a.line((21,25,28,18),"#a7c6bb",width=2)
    a.line((15,33,17,32),"#dee0bd",width=2)
    a.line((22,31,24,30),"#dee0bd",width=2)
    a.line((5,17,32,8),"#f0e5c6")
    a.line((7,47,34,38),"#b5bbaa",width=2)
    save("acceso-puerta-lateral",a)


def shops(Art, save):
    a=Art((128,87))
    a.rect((0,4,127,86),INK)
    a.rect((0,7,127,84),"#9a8873")
    a.rect((0,7,127,18),"#bca682")
    a.rect((0,18,127,24),"#6f625a")
    for x,width,awning in ((5,39,"#83584d"),(46,37,"#486d6b"),(85,38,"#916d49")):
        a.rect((x,26,x+width-1,84),"#d0b994")
        a.rect((x+2,44,x+width-3,82),INK)
        if x==46:
            a.rect((x+4,46,x+width-5,81),"#4d4841")
            a.rect((x+6,48,x+width-7,63),"#8b7859")
            for bx,color in ((x+8,"#ca8961"),(x+14,"#cdb878"),(x+20,"#76958a"),(x+25,"#b7946e")):
                a.rect((bx,52,bx+4,60),color)
                a.line((bx+1,53,bx+3,53),"#ede2b6")
            a.rect((x+5,64,x+width-6,68),"#be9968")
            a.rect((x+5,77,x+width-6,80),"#b6a27d")
        else:
            a.rect((x+4,46,x+width-5,81),"#5b6463")
            for y in range(48,82,4):a.line((x+5,y,x+width-6,y),"#869190")
        a.rect((x+3,28,x+width-4,38),"#303b42")
        a.poly([(x,39),(x+width-1,39),(x+width-2,48),(x+1,48)],awning)
        for px in range(x+2,x+width-2,8):
            a.line((px,40,px,46),"#d6bc9a")
    a.line((1,84,127,84),"#615d54")
    save("acceso-tiendas",a)

    a=Art((80,46))
    a.rect((0,3,79,45),INK)
    a.rect((0,7,79,44),"#8a8f7f")
    a.rect((0,6,79,9),"#dfd8b6")
    a.rect((0,13,79,16),"#b8bba0")
    for x in (9,31,54):
        a.ellipse((x,17,x+14,36),"#d6d0b1")
        a.rect((x,24,x+14,42),"#d6d0b1")
        a.ellipse((x+2,20,x+12,35),"#4a6157")
        a.rect((x+2,25,x+12,39),"#4a6157")
        a.line((x+6,23,x+6,40),"#819484")
        a.line((x+2,28,x+12,28),"#819484")
    save("acceso-instituto",a)

    a=Art((16,80))
    a.rect((0,0,15,79),INK)
    a.rect((1,1,14,78),"#5f6c72")
    a.rect((1,1,11,78),"#a4ada7")
    a.line((1,3,1,77),"#d4d6c3")
    for y in (5,31,58):
        a.line((7,y,14,y),"#465862")
    save("acceso-murete",a)

    a=Art((48,32))
    a.rect((0,0,47,31),INK)
    a.rect((0,0,47,5),"#a5afaa")
    a.line((0,0,47,0),"#c5c9b7")
    a.rect((1,7,46,29),"#747f82")
    for y in (13,22,29):a.line((1,y,46,y),"#59676e")
    for x,y in ((14,7),(34,7),(8,14),(25,14),(42,14),(15,23),(34,23)):
        a.line((x,y,x,y+6),"#59676e")
    a.line((2,30,46,30),"#505d65")
    save("acceso-plinto",a)

    a=Art((18,80))
    a.rect((7,5,13,75),INK)
    a.rect((9,6,12,74),"#77868c")
    a.line((9,7,9,74),"#bdc6bc")
    for y in (6,37,72):
        a.rect((4,y-3,11,y+4),"#6c7b80",outline=INK)
        a.rect((5,y-3,8,y-1),"#d0cdb5")
    save("acceso-baranda",a)


def cars(Art, save):
    for van in (False,True):
        a=Art((80,36))
        a.shadow((2,4,77,35))
        a.rect((12,3,20,10),INK)
        a.rect((12,26,20,33),INK)
        a.rect((60,3,68,10),INK)
        a.rect((60,26,68,33),INK)
        shell,light,dark=("#c1c6be","#e0dfcb","#8a9996") if van else ("#3b6379","#69899b","#294454")
        roof=(22,6,61,27) if van else (23,8,53,25)
        a.poly([(4,11),(11,7),(16,5),(62,5),(72,9),(76,13),(76,25),(69,30),(9,30),(3,26)],INK)
        a.poly([(5,12),(13,8),(62,7),(72,11),(74,25),(69,28),(10,28),(5,25)],shell)
        a.line((10,26,68,26),dark,width=2)
        a.rect(roof,dark)
        a.rect((roof[0]+2,roof[1]+3,roof[2]-2,roof[3]-2),light)
        a.poly([(roof[2]-1,9),(roof[2]+8,12),(roof[2]+8,21),(roof[2]-1,25)],"#2e4552")
        a.line((roof[2]+1,12,roof[2]+6,14),"#799ba3")
        a.poly([(roof[0]-6,12),(roof[0]+3,9),(roof[0]+3,25),(roof[0]-6,22)],"#3a535e")
        a.line((roof[0]-4,13,roof[0]+1,11),"#8ba7a8")
        a.line((26,9,48,9),"#f3e7ca" if van else "#92b1b4")
        for y in (12,23):
            a.rect((5,y,8,y+2),"#bd665a")
            a.rect((71,y,74,y+2),"#e5d797")
        a.line((31,27,34,27),WHITE)
        a.line((44,27,47,27),WHITE)
        if van:a.rect((29,13,48,18),"#dbb76b")
        save("acceso-furgoneta" if van else "acceso-auto",a)


def draw_street(Art, save):
    stone_tiles(Art, save)
    campus(Art, save)
    shops(Art, save)
    cars(Art, save)
