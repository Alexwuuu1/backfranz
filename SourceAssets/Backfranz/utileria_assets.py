"""Original pixel silhouettes for the room-specific props in Bloque A."""

from bloque_a_assets import dark_layers


# Each row is a pixel-art swatch. Two output pixels per cell leave room for highlights.
PROPS = {
    "sello-caja":         ("#a36a4d", "....xx.... ....xa.... ...xaaa... ....xx.... ...xppx... ..xaaaax.. ..xxxxxx.."),
    "revistero":          ("#708e7a", ".xx..xx... .xp..xp... .xpaaxp... .xxxxxxxx. .xaaaaaax. .xapapaax. .xaaaaaax. ..x....x.."),
    "consola-auditorio": ("#547981", "..xxxxxx.. .xaxaxaxx. xapapapaxx xaaaaaaaax xaxxxxxaax .xa....xa. .xx....xx."),
    "control-juegos":    ("#6a638b", ".....x.... ....x..... ..xxxxxx.. .xaaaaaax. xpxaaapaxx xaxaaapapx xaxxxxaaax .x....xx.."),
    "servilletero":      ("#96794e", "..pppppp.. ..pappap.. ..pppppp.. .xxxxxxxx. .xaaaaaax. .xappppax. .xxxxxxxx."),
    "folletos-entrada":  ("#4e8b8e", "......xx.. ..xx..xp.. ..xppaxp.. ..xapaxp.. .xxxxxxx.. .xaaaaax.. .xppapax.. .xxxxxxx.."),
    "agenda":            ("#aa725c", "....xxxx.. ..xxapax.. .xappapax. xaxpappaxx xaxppapaxx .xaxxxxxx. ..xx......"),
    "archivador":        ("#607887", ".xxxxxxxx. .xaaaaaax. .xppppaax. .xaaaaaax. .xxxxxxxx. .xaaaaaax. .xppaapax. .xaaaaaax. .xxxxxxxx. .xx....xx."),
    "bandeja-monedas":   ("#577b85", "..xxxxxx.. .xaaaaaax. xappapapax xapapppaax xaaaaaaaax .xxxxxxxx."),
    "registro-turno":    ("#447d77", ".xxxxxxxx. .xaaaaaax. .xppappax. .xapppaax. .xxxxxxxx. ...xaax... ..xxxxxx.."),
    "bandeja-documentos":("#997353", "...ppppp.. ..ppappp.. .xpppppx.. xaxxxxaxx. xaaaaaaaxx xaxxxxxaax .xxxxxxxx."),
    "organizador":       ("#6c7996", ".x.x.x.x.. .xpxaxpx.. xxxxxxxx.. xaxaxaxx.. xpppppax.. xaaaaaax.. xxxxxxxx.."),
    "batidora":          ("#ae7659", "...xxxxx.. ..xaaaax.. ..xpaaaxx. ....x.xax. ..x.x.xax. ..xaaxxax. ...xxxaxx. ..xaaaaax. ..xxxxxxx."),
    "tabla-ingredientes":("#b38857","...xx..... .xxaaxxx.. xaaaapppx. xaapapapax xaapaapapx .xaaaaaax. ..xxxxxx.."),
    "monitor-paciente":  ("#447c84", ".xxxxxxxx. .xaaaaaax. .xapapaax. .xpapapax. .xaaaapax. .xxxxxxxx. ....xx.... ...xxxx..."),
    "maletin-clinico":   ("#467c83", "...xxxx... ...x..x... .xxxxxxxx. xaaaaaaaax xaapppaax. xaappaaax. xaapppaax. .xxxxxxxx."),
    "gradilla":          ("#8972a1", ".px.px.px. .pa.pa.pa. .pa.pa.pa. xxxxxxxxxx xaaaaaaaax .xaxaxaxax .xxxxxxxx."),
    "portaobjetos":      ("#5f959a", "....xxxxx. ..xxaaapx. .xapaapx.. xaaapaxx.. xaapxx.... .xxx......"),
    "torre-pc":          ("#505c75", "..xxxxx... .xaaaaax.. .xapxxax.. .xaaaaax.. .xpppaax.. .xaaaaax.. .xaaaaax.. .xpxaaax.. .xxxxxxx."),
    "teclado-pc":        ("#697488", "..xxxxxxx. .xaaaaaaax xpppppppax xpapapapax xaaaaaaaax .xxxxxxxx."),
    "tableta-grafica":   ("#656c88", ".......xp. ......xp.. ..xxxxp... .xaaapaxx. xaaaapapax xaaaaapaax .xxxxxxxx."),
    "disco-externo":     ("#5b7d8d", "..xxxxxx.. .xaaaaaax. xaapaaaaxx xaaaapapxx xaxxxxxaxx .xxxxxxxx. .......xx."),
    "auriculares":       ("#635b84", "...xxxx... ..xaaaax.. .xa....ax. .xa....ax. .xax..xax. .xax..xax. ..xx..xx.."),
    "cartel-aire":       ("#ba6967", ".xxxxxxxx. xaaaaaaaax xpapppapax xpapapapax xaaaaaaaax .xxxxxxxx."),
    "rollo-fondo":       ("#887da4", "xxxxxxxxxx ppaaaaaapp xxaaaaaaxx ..apppaa.. ..aappaa.. ..aaaaaa.. ..xxxxxx.."),
    "camara-reflex":     ("#59617a", "...xxx.... ..xapx.... .xxxxxxxx. xaaaxaaaax xaaxpxaaax xaaxpxapax xaaaxaaaax .xxxxxxxx."),
    "altavoz":           ("#646185", "..xxxxxx.. .xaaaaaax. .xaappax.. .xaaaaax.. .xaxxxax.. .xaxpxax.. .xaxxxax.. .xaaaaax.. .xxxxxxx."),
    "bobina-cable":      ("#636c92", "...xxxx... .xxaaaaxx. xaaxxxxaxx xaxaaaaxax xaxaxxxxax xaxaapaaax .xxaaaaax. ...xxxxxx."),
    "mazo":              ("#ac7550", "........x. ...xx..xa. ..xaaxxa.. .xaaaax... ..xax..... ...xxxx... ..xaaaax.. ..xxxxxx.."),
    "expediente":        ("#9a7757", "....xxxx.. ..xxapax.. .xappppax. xaappppaxx xaapapaxxx xaaaaxxxx. .xxxxx...."),
    "portapapeles":      ("#997f61", "...xxxx... ..xppppx.. ..xaaaax.. ..xpppax.. ..xappax.. ..xpppax.. ..xaaaax.. ..xxxxxx.."),
    "grabadora":         ("#596c89", "...xxxx... ..xaaaax.. ..xpppax.. ..xapxax.. ..xaaaax.. ..xpppax.. ..xaaaax.. ..xxxxxx.."),
    "radio-mano":        ("#526e69", "..xx...... ..xx...... ..xx...... .xxxxxxx.. .xapppax.. .xaaaaax.. .xpxaaxx.. .xaaaaax.. .xpxpaxx.. .xxxxxxx.."),
    "cuna-goma":         ("#a56d43", ".......xx. .....xxpx. ...xxapax. ..xpaaxax. .xaaxaaxx. xaaaaaaxx. xxxxxxxxx."),
    "lapicero":          ("#6b9683", "..p.p.p... ..p.x.p... ..x.x.x... .xxxxxxxx. .xaaaaaax. ..xaaaax.. ..xxxxxx.."),
    "borrador":          ("#84967d", "...xxxxx.. ..xpppaax. .xpppaaaxx xaaaaaaaax xxxxxxxxxx"),
    "tiza-blanca":      ("#9ea9a5", ".......xx. .....xxppx ...xxppaxx ..xppaxx.. .xpaxx.... .xxx......"),
    "tubo-planos":       ("#638491", ".......xx. .....xxpa. ...xxaapx. .xxaaaax.. xpaaxx.... xapx...... .xx......."),
    "panel-notas":       ("#79817b", ".xxxxxxxx. .xaaaaaax. .xppapax.. .xppapaax. .xaappax.. .xapppax.. .xaaaaaax. .xxxxxxxx."),
    "multimetro":        ("#ab7850", "..xxxxxx.. .xaaaaaax. .xppppaax. .xaaaaaax. .xapxxxax. .xapxpaxx. .xaaaaaax. .xxaxxaxx. ...p..p..."),
    "prensa-banco":     ("#667d87", ".xxxxxxxxxxxx. .xppppppppaax. .xaaxxaaaaaax. .x..ppxx..xax. .x..xaax..xax. .x..xxxx..xax. .xaappppppaax. .xaxxxxxxxxax. .xaxaaaaaaxax. .xxxxxxxxxxxx."),
    "bomba-presion":    ("#65898a", "......xxxxxxxx...... ......xpaaaaax...... ......xpaxxxxx...... ...xxxxxxxaxxxxxx... ..xpaaaaaaaaaaaaxx.. .xpaapppaaaxaaaaaxx. xpaapxaxpaaxxxaaaaxx xpaapaxapaaxxxaaaaxx xpaapxaxpaaxxxaaaaxx .xaaapppaaaaaaaaaxx. ..xxxxxxxxxxxxxxxx.. ..xaaaax....xaaaax.. .xxxxxxxxxxxxxxxxxx."),
    "medidor-industrial": ("#80918c", "....xxxx.... ..xxppxx.... .xppppppx... .xppxpppx... .xpxppppx... .xppppppx... ..xxxxxx.... ....xaax.... .xxxxaaxxxx. .xpaaaaaaax. .xxxxxxxxxx."),
    "ventilador-muro": ("#637882", ".xxxxxxxxxxxx. .xppppppppaax. .xaaxxxaaaaax. .xaxxaaxxxaax. .xaxaaaxaaxax. .xaxxpxxxaxax. .xaxaaaxxxxax. .xaaxxxaaxaax. .xaaaaaaaapax. .xxxxxxppxxxx."),
    "base-lavamanos":   ("#79908f", "..xxxxxxxxx... .xppppppppxx.. xppppppppppax. xxxxxxxxxxxxx. .xpaaaaaaaax.. .xpaxxaxaaax.. .xpaaaaaaaaax. .xpaxxaxaaaax. .xpaaaaaaaaax. .xxxxxxxxxxxx. ..xx......xx.."),
    "caja-componentes": ("#5c8983", "...xxxxxx. .xxpppapxx xppppaaxax xaxaxaxaxx xpapapapax xaaaaaaaax .xxxxxxxx."),
    "separalibros":      ("#a7785a", "..x...x... ..xx.xx... .xpx.xpx.. .xpx.xpx.. .xpx.xpx.. .xpx.xpx.. xxxxxxxxxx"),
    "lupa-lectura":      ("#638488", "...xxxx... ..xppppx.. .xppapppx. .xppapppx. ..xppppx.. ...xxxxx.. ......xax. .......xx."),
}


def draw_campus_utileria(Art, save):
    for name, (body, silhouette) in PROPS.items():
        rows = silhouette.split()
        width, height = max(map(len, rows))*2+4, len(rows)*2+4
        art = Art((width,height))
        art.shadow((1,height-5,width-2,height-1))
        palette = {"x":"#263141", "a":body, "p":"#e8d7b0"}
        for y, row in enumerate(rows):
            for x, shade in enumerate(row):
                if shade == ".":
                    continue
                left, top = 2+2*x, 2*y
                art.rect((left,top,left+1,top+1),palette[shade],layer=2 if shade=="p" else 1)
        asset = "utileria-" + name
        save(asset,art)
        altered = Art((width,height))
        altered.layers = dark_layers(art.layers)
        save("backfranz-"+asset,altered)
