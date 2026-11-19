"""Pixel-art illustrations for the three ending cards in Extras."""


def draw_endings(Art, save):
    ink = '#171e2c'
    for name, sky, wall, line in (
        ('amanecer', '#ba7c76', '#4c4058', '#f6c285'),
        ('eco', '#26283e', '#374354', '#63c0af'),
        ('umbral', '#32243d', '#46304d', '#ba6e91'),
        ('bloqueado', '#222b3c', '#313c48', '#778294'),
    ):
        art = Art((128, 80))
        art.rect((0, 0, 127, 79), sky)
        art.poly(((0, 0), (35, 12), (48, 52), (20, 79), (0, 79)), wall)
        art.poly(((127, 0), (91, 12), (80, 52), (109, 79), (127, 79)), wall)
        art.poly(((0, 79), (48, 52), (80, 52), (127, 79)), ink)
        for x in (0, 28, 57, 70, 99, 127):
            art.line((64, 51, x, 79), wall)
        art.line((0, 2, 127, 2), line, width=2)
        art.line((0, 77, 127, 77), line, width=2)
        if name == 'amanecer':
            art.ellipse((49, 10, 78, 39), line)
            for x in (44, 55, 65, 76):
                art.ellipse((x, 47, x+7, 55), ink)
                art.rect((x+1, 54, x+6, 66), ink)
        elif name == 'eco':
            art.rect((42, 27, 85, 62), ink)
            art.rect((46, 31, 81, 45), '#314e4f', outline=line)
            art.line((48, 39, 54, 39, 58, 34, 62, 43, 67, 35, 71, 40, 79, 40), line)
            art.ellipse((49, 51, 56, 58), line)
            art.line((74, 29, 89, 8), line, width=2)
        elif name == 'umbral':
            art.rect((51, 15, 77, 66), ink)
            art.rect((55, 19, 73, 65), '#623454', outline=line)
            art.ellipse((60, 32, 68, 42), ink)
            art.poly(((59, 42), (69, 42), (72, 60), (55, 60)), ink)
            art.line((60, 36, 63, 36), '#f9a9a2')
            art.line((66, 36, 69, 36), '#f9a9a2')
        else:
            art.ellipse((54, 21, 74, 44), ink, outline=line)
            art.ellipse((59, 26, 69, 41), sky)
            art.rect((49, 35, 79, 59), ink, outline=line)
            art.rect((63, 42, 65, 51), line)
        save('final-'+name, art)
