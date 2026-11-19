"""Build the original Backfranz Pixel font from the game's bitmap alphabet."""
from pathlib import Path
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
from Backfranz.letras import glyph, patterns

characters = sorted(set(patterns) | set('áéíóúüñÁÉÍÓÚÜÑ¿¡'))
glyphs = {'.notdef': TTGlyphPen(None).glyph()}
metrics = {'.notdef': (500, 0)}
cmap = {}


def square(pen, x, y):
    pen.moveTo((x, y))
    pen.lineTo((x, y+100))
    pen.lineTo((x+100, y+100))
    pen.lineTo((x+100, y))
    pen.closePath()


for character in characters:
    name = 'uni%04X' % ord(character)
    cmap[ord(character)] = name
    pen = TTGlyphPen(None)
    rows = glyph(character)
    descender = 2 if character in 'gjpqy' else 0
    for y, row in enumerate(rows):
        for x, pixel in enumerate(row):
            if pixel == '1':
                square(pen, x*100+40, (9-y-descender)*100)
    glyphs[name] = pen.glyph()
    metrics[name] = (len(rows[0])*100 + 130, 40 if character != ' ' else 0)

font = FontBuilder(1000,isTTF=True)
font.setupGlyphOrder(list(glyphs))
font.setupCharacterMap(cmap)
font.setupGlyf(glyphs)
font.setupHorizontalMetrics(metrics)
font.setupHorizontalHeader(ascent=1020,descent=-240)
font.setupNameTable({'familyName':'Backfranz Pixel','styleName':'Regular','uniqueFontIdentifier':'Grupo64-BackfranzPixel-2',
                     'fullName':'Backfranz Pixel','psName':'BackfranzPixel-Regular'})
font.setupOS2(sTypoAscender=1020,sTypoDescender=-240,usWinAscent=1020,usWinDescent=240)
font.setupPost()
font.setupMaxp()
path=Path(__file__).resolve().parents[1]/'Assets/Art/UI/Backfranz/BackfranzPixel.ttf'
font.save(str(path))
print(path)
