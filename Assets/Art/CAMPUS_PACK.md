# Pack visual del campus - aporte de Cristopher

Este archivo registra el aporte visual preparado en la rama `feature/fondo-campus` para que el equipo pueda identificar, integrar y defender los assets sin modificar la escena de otro integrante.

## Escala de trabajo

- Unidad base del escenario: `16 x 16 px` por tile.
- Pixels Per Unit configurado: `16`.
- Fondo horizontal: `256 x 128 px`, equivalente a `16 x 8` tiles.
- Personaje: `24 x 32 px`, equivalente a `1.5 x 2` tiles. Esta proporción permite que sea visible sin ocultar el escenario.
- Filtro de Unity: Point, sin mipmaps y sin compresión.

## Assets añadidos

### Fondo

- `Backgrounds/CampusMapHorizontal_256x128.png`: mapa horizontal del ingreso universitario.

### Personaje

- `Characters/StudentCampus_24x32.png`: estudiante provisional con fondo transparente.

### Tiles 16 x 16

- `Tiles/CampusPack/CampusGrass16.png`: césped.
- `Tiles/CampusPack/CampusPath16.png`: adoquín peatonal.
- `Tiles/CampusPack/CampusCurb16.png`: transición entre acera y césped.
- `Tiles/CampusPack/CampusFlowerbed16.png`: jardín con flores.
- `Tiles/CampusPack/CampusDirt16.png`: tierra.
- `Tiles/CampusPack/CampusWall16.png`: muro claro del edificio.
- `Tiles/CampusPack/CampusWindow16.png`: ventana azul.
- `Tiles/CampusPack/CampusRoof16.png`: techo gris.

### Decoraciones transparentes

- `Decorations/CampusPack/TreeCampus_16x24.png`.
- `Decorations/CampusPack/BenchCampus_24x12.png`.
- `Decorations/CampusPack/LampCampus_8x24.png`.
- `Decorations/CampusPack/KioskCampus_16x24.png`.

## Autoría y uso

- Responsable del aporte: Cristopher Iori Lazcano Gutierrez (`Crisshubb`).
- El fondo y el personaje fueron creados específicamente para el proyecto con apoyo de OpenAI Image Generation y posteriormente adaptados a las dimensiones exactas del juego.
- Los tiles y las decoraciones fueron reconstruidos píxel a píxel con una paleta limitada para asegurar tamaños exactos y compatibilidad con Tilemap.
- No se utilizó ningún paquete gráfico externo ni se copiaron assets de otros videojuegos.
- Uso autorizado para el proyecto académico `Backfranz`, Grupo 64.

## Integración sugerida

El integrante responsable del Tilemap puede crear objetos `Tile` usando las imágenes de `Tiles/CampusPack`. El fondo puede añadirse como `SpriteRenderer` con orden negativo. El personaje y las decoraciones deben colocarse por encima del suelo mediante el Sorting Layer correspondiente.
