"""Framed sliding doors and building lettering for Backfranz."""
from math import atan2, hypot
from letras import glyph
from personajes_assets import slug

FRAME_COUNT = 7
LABELS = (
    "Recepción", "Ventanilla", "Financiero", "Servicios Estudiantiles", "Auditorio",
    "Game Room", "Café", "Admisiones", "Ascensor", "Gradas centrales",
    "Gradas del auditorio", "Salida", "Entrada", "Galería de servicio",
    "Servicio eléctrico", "Bombas y presión", "Posgrado", "Caja", "Dirección de Carrera",
    "Cocina de prácticas", "Sala de Simulación", "Histología y Fisiología",
    "Laboratorio PC", "Sala Mac", "Streaming", "Fotografía", "Sonido", "Sala Jurídica",
    "Cámara Gesell", "Aula 4A", "Aula de grupos", "Taller", "Sala de lectura", "Baños",
    "Cajas", "Simulación", "Sistemas", "Aulas", "Aula dinámica", "Biblioteca", "Arca", "Cocina",
)
INK = "#202b36"
WARM = "#f3dfae"


def lettering(Art, label, plate=None):
    letters = [glyph(c) for c in label.upper()]
    width = sum(len(g[0])+1 for g in letters)-1
    if plate is None:
        art = Art((width,10))
        x,y,color=0,0,WARM
    else:
        background,light,edge,color=plate
        art=Art((width+14,16))
        right=width+13
        art.rect((0,1,right,15),INK)
        art.rect((1,1,right-1,13),edge)
        art.rect((2,2,right-2,12),background)
        art.line((2,2,right-2,2),light)
        for screw in (3,right-3):
            art.rect((screw,7,screw+1,7),edge,layer=2)
            art.rect((screw,6,screw,6),light,layer=2)
        x,y=7,4
    for rows in letters:
        for j,row in enumerate(rows):
            for i,value in enumerate(row):
                if value=='1':art.rect((x+i,y+j,x+i,y+j),color,layer=2)
        x+=len(rows[0])+1
    return art


FLOOR_DOORS = {
    "servicios": (("#729caa", "#c5ddda", "#4a6c79"), ("#3f5b6b", "#72888c", "#304453")),
    "clinica": (("#d5dbcb", "#f4f3db", "#769b98"), ("#6f8984", "#9eb19c", "#425960")),
    "sistemas": (("#445b75", "#8dbdc8", "#2d3d54"), ("#35455d", "#5d858e", "#232e43")),
    "aulas": (("#81638e", "#bf9ac7", "#523e63"), ("#594660", "#8a6f8b", "#372b46")),
    "grupos": (("#a48353", "#dfbd7f", "#685538"), ("#6d6348", "#a18c62", "#453e31")),
    "taller": (("#667b72", "#acb790", "#3b544d"), ("#50625a", "#8b8e69", "#333f3a")),
    "biblioteca": (("#a47c53", "#ead5a0", "#6e523b"), ("#705d42", "#a9936e", "#45392f")),
    "acustica": (("#403c56", "#9482b0", "#302d44"), ("#353144", "#706684", "#211f30")),
}


def front_door(Art, material, frame, dark):
    art=Art((48,52))
    # The jamb, transom and tracks are fixed. Each full-size leaf moves behind them.
    if dark:
        frame_light,rail,shadow = "#666c6b","#414e54","#212932"
        colors={"vidrio":("#3e626b","#5e8081","#334951"),
                "madera":("#625044","#827058","#493944"),
                "metal":("#515d5e","#727b73","#3c414a")}
    else:
        frame_light,rail,shadow = "#bec3b6","#7d8990","#3f5360"
        colors={"vidrio":("#67949b","#a8c4b9","#487682"),
                "madera":("#986f50","#bb9163","#785442"),
                "metal":("#637e84","#9aaead","#4b666d")}
    fill,bright,edge=FLOOR_DOORS[material][int(dark)] if material in FLOOR_DOORS else colors[material]
    art.rect((3,3,44,48),INK)
    art.rect((5,5,42,47),shadow)
    art.rect((8,10,39,42),"#151e29" if dark else "#253540")
    art.rect((8,37,39,43),"#34424a")
    distance=round(16*frame/(FRAME_COUNT-1))
    for side, origin in ((-1,8),(1,24)):
        leading=origin+side*distance
        for offset in range(16):
            x=leading+offset
            if not 8<=x<=39:
                continue
            art.rect((x,10,x,42),edge if offset in (0,15) else fill)
            art.rect((x,10,x,12),bright)
            art.rect((x,40,x,42),edge)
            if material=="madera" and offset in (3,10,13):
                art.line((x,14,x,39),bright if offset==3 else edge)
            elif material=="metal" and offset in (1,13):
                art.line((x,14,x,39),bright)
            elif material=="vidrio" and 2<=offset<=11:
                y=23-offset
                art.rect((x,y,x,y+1),bright,layer=2)
            elif material in FLOOR_DOORS:
                # Details are attached to the leaves, so they disappear into the jamb with them.
                pane = 2<=offset<=13 and material in ("servicios","biblioteca","grupos") or 5<=offset<=10 and material in ("clinica","sistemas","aulas","taller")
                if pane:
                    bottom = 35 if material in ("servicios","biblioteca") else 25
                    art.line((x,15,x,bottom),"#678e9a" if dark else "#7ba7b1")
                    if offset%5==0:art.line((x,17,x,bottom-2),bright)
                if material=="clinica":art.line((x,32,x,34),edge,width=1)
                if material=="taller" and 2<=offset<=13:
                    art.line((x,32,x,33),"#cfa85e" if offset%6<3 else edge)
                if material=="acustica" and offset in (2,5,8,11,14):art.line((x,14,x,39),edge)
                if material=="biblioteca" and offset in (2,7,13):art.line((x,36,x,40),edge)
            if offset==(12 if side==-1 else 3):art.rect((x,26,x,31),WARM,layer=2)
    art.rect((4,5,43,9),rail,layer=2)
    art.line((5,6,42,6),frame_light)
    for x in (5,40):
        art.rect((x,9,x+2,44),rail,layer=2)
        art.line((x,9,x,42),frame_light)
    art.rect((5,43,42,47),rail,layer=2)
    art.line((7,43,41,43),frame_light)
    art.line((7,46,41,46),shadow)
    return art


def side_door(Art, frame):
    art=Art((37,50))
    art.poly([(4,16),(33,5),(36,9),(36,39),(7,49),(3,45)],INK)
    art.poly([(5,16),(33,7),(34,38),(6,47)],"#a5b1a9")
    travel=round(11*frame/(FRAME_COUNT-1))
    for x in range(8,31):
        top=18-round((x-8)*.36)
        bottom=44-round((x-8)*.36)
        leaf=x+travel if x<19 else x-travel
        drawn=(x<19 and leaf<19) or (x>=19 and leaf>=19)
        art.rect((x,top,x,bottom),"#58868e" if drawn else "#243640")
        if drawn and (8<=leaf<=15 or 21<=leaf<=29):
            y=top+12-(leaf%8)
            art.rect((x,y,x,y+1),"#9ac0b8",layer=2)
        if drawn and leaf in (16,21):
            art.rect((x,bottom-12,x,bottom-9),WARM,layer=2)
        art.rect((x,bottom,x,bottom+1),"#424e57",layer=2)
    art.line((5,17,32,8),"#f0e5c6")
    art.line((7,47,34,38),"#b5bbaa",width=2)
    return art


def stair_opening(Art, dark):
    art = Art((48,46))
    plaster, crown, side, tread, edge = (
        ("#565753", "#767569", "#303641", "#535a58", "#77796d") if dark else
        ("#c8bda8", "#f0dfbf", "#626265", "#9b9587", "#e6d8b9")
    )
    art.rect((0,0,47,8),plaster)
    art.rect((0,0,47,3),crown)
    art.rect((0,8,47,11),side)
    art.rect((3,12,44,24),INK)
    art.poly([(8,15),(39,15),(46,45),(1,45)],tread)
    for y in (18,22,27,33,40,45):
        spread = (y-15)//5
        art.line((8-spread,y,39+spread,y),edge)
        if y<45:art.line((8-spread,y+1,39+spread,y+1),side)
    for x,foot in ((7,0),(40,47)):
        art.line((x,13,foot,45),side,width=2)
        art.line((x,13,foot,42),crown)
    return art


def security_gate(Art, frame, wide):
    art = Art((44,36))
    art.shadow((1,4,42,34))
    slide = frame/(FRAME_COUNT-1)
    # Two panels pivot on their own bases and park parallel to the walking lane.
    for y, direction in ((7,1),(28,-1)):
        length = 10 if not wide else 11
        tip_x = 18+round(slide*length)
        tip_y = y+direction*round((1-slide)*length)
        glass = "#6caaa8" if wide else "#549193"
        art.poly([(16,y),(19,y),(tip_x+2,tip_y),(tip_x,tip_y+3),(tip_x-2,tip_y+3)],INK)
        art.poly([(17,y),(19,y),(tip_x+1,tip_y),(tip_x,tip_y+2)],glass)
        art.line((18,y,tip_x,tip_y+1),"#a7d0bd")
    for y in (0,29):
        art.rect((4,y,39,y+6),INK,layer=2)
        art.rect((6,y,37,y+4),"#637680",layer=2)
        art.line((8,y+1,35,y+1),"#a8b6b5")
        art.rect((31,y+1,36,y+4),"#263c43",layer=2)
    return art


def draw_access_fixtures(Art, save):
    save("escalera-empotrada",stair_opening(Art,False))
    save("backfranz-escalera-empotrada",stair_opening(Art,True))
    for key,wide in (("control-molinete",False),("control-accesible",True)):
        frames = [security_gate(Art,frame,wide).layers for frame in range(FRAME_COUNT)]
        save(key,security_gate(Art,0,wide),frames,[(0,6,"Giro de hojas")])


def ground_bend(Art, exterior, dark):
    width,height = (80,96) if exterior else (64,64)
    art = Art((width,height))
    void = (6,9,11) if dark else (26,41,51)
    if exterior:
        crown,glass,frame,shadow = ((133,157,209),(60,90,134),(38,58,93),(27,36,64)) if dark else (
            (196,225,225),(84,124,144),(54,79,96),(39,48,60))
    else:
        crown,glass,frame,shadow = ((168,163,209),(140,136,177),(83,83,119),(27,36,64)) if dark else (
            (255,240,216),(213,200,181),(127,120,119),(39,48,60))
    for row in range(height):
        y = (height-row-.5)/16
        for col in range(width):
            x = (col+.5)/16
            if exterior:
                d = 4-hypot(x,y)
                if d < -.93: color=void
                elif d < -.77: color=crown
                elif d < -.16: color=frame if int(atan2(y,x)*73)%16<2 else glass
                elif d < .015: color=shadow
                else: continue
            else:
                dx,dy = x-1,y-1
                if dx<0: d=dy-2
                elif dy<0: d=dx-2
                else: d=hypot(dx,dy)-2
                if d < -.47: color=void
                elif d < -.36: color=crown
                elif d < -.12: color=glass
                elif d < 0: color=frame
                else: continue
            art.rect((col,row,col,row),color)
    return art


def finance_counter(Art, dark):
    art=Art((112,39))
    wood,light,glass,rail = ("#6c4d48","#a2806d","#375762","#58636c") if dark else (
        "#916346","#dcad78","#62929c","#b5cccb")
    art.shadow((0,28,111,38))
    art.rect((2,19,109,35),INK)
    art.rect((4,22,107,32),wood)
    art.rect((2,18,109,22),light)
    for x in (9,56):
        art.rect((x,5,x+44,17),rail)
        art.rect((x+2,6,x+42,15),glass)
        art.line((x+7,14,x+19,7),rail)
        art.line((x+28,14,x+39,7),rail)
        art.rect((x+16,16,x+29,18),INK)
    return art


def draw_ground_layout_details(Art, save):
    for dark in (False,True):
        prefix="backfranz-" if dark else ""
        save(prefix+"giro-pb-vidrio",ground_bend(Art,True,dark))
        save(prefix+"giro-pb-zocalo",ground_bend(Art,False,dark))
        save(prefix+"pb-caja-corrida",finance_counter(Art,dark))


def draw_floor_doors(Art,save):
    tags=[(0,FRAME_COUNT-1,"Abrir dentro del marco")]
    for material in FLOOR_DOORS:
        for dark in (False,True):
            name=("backfranz-" if dark else "")+"puerta-marco-"+material
            frames=[front_door(Art,material,i,dark).layers for i in range(FRAME_COUNT)]
            save(name,front_door(Art,material,0,dark),frames,tags)


def draw_building_details(Art,save):
    tags=[(0,FRAME_COUNT-1,"Abrir dentro del marco")]
    for material in ("vidrio","madera","metal"):
        for dark in (False,True):
            name=("backfranz-" if dark else "")+"puerta-marco-"+material
            frames=[front_door(Art,material,i,dark).layers for i in range(FRAME_COUNT)]
            save(name,front_door(Art,material,0,dark),frames,tags)
    frames=[side_door(Art,i).layers for i in range(FRAME_COUNT)]
    save("puerta-marco-lateral",side_door(Art,0),frames,tags)
    draw_access_fixtures(Art,save)
    draw_ground_layout_details(Art,save)
    draw_floor_doors(Art,save)

    for label in LABELS:
        exit_sign=label=="Salida"
        normal=("#375b50","#8aa598","#25433e",WARM) if exit_sign else ("#d7cfb3","#f4e6bc","#9d9b88","#263644")
        altered=("#344c47","#58746c","#252e36","#c2d7ba") if exit_sign else ("#424549","#646563","#303641","#c5c4ae")
        key=slug(label)
        save("placa-"+key,lettering(Art,label,normal))
        save("backfranz-placa-"+key,lettering(Art,label,altered))
    for label in ("Tiendas","Bloque A","Instituto Americano","1 · 2 · 1 · 1","1 · 1 · 1 · 1","1 · 3 · 1 · 1",
                  "2 · 2 · 1 · 1","1 · 3 · 6","1 · 2 · 4","2 · 2 · 4","2 · 3 · 6","1 · 2 · 2",
                  "2 · 2 · 1","1 · 4 · 2","1 · 3 · 2"):
        save("letras-"+slug(label.replace(" · "," ")),lettering(Art,label))
