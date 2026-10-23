# Fuentes gráficas de Backfranz

`Editables/` contiene el arte nuevo en formato Aseprite `.ase`: suelos, paredes, muebles, objetos interactivos, botones, iconos, Yamil y la sombra. Los nuevos gráficos tienen tres capas editables (sombras, volumen/color y detalles/luces). Yamil incluye 16 fotogramas y cuatro etiquetas de dirección; la sombra tiene cuatro fotogramas.

`Importados/` conserva en `.ase` los PNG anteriores del proyecto, incluidos los fondos del menú. Son imágenes importadas en **una capa**; no se recuperaron ni se inventaron capas originales. Los cuatro archivos históricos del equipo permanecen en `SourceAssets/Aseprite/`.

Unity consume las exportaciones de `Assets/Resources/Backfranz/Art/` y los gráficos anteriores de `Assets/Art/`. Cada exportación tiene su fuente identificada en `assets.json`. Mantener los PNG junto con sus `.meta`: no son duplicados descartables, son los recursos que utiliza el ejecutable. Tipografías TTF y sonidos no son documentos de Aseprite.

## Editar

Abrir el `.ase` con Aseprite o un editor compatible, modificar las capas y exportar a las rutas PNG de `assets.json`. Para animaciones, conservar los nombres y el orden de fotogramas indicados en ese manifiesto. No ejecutar el generador después de editar manualmente: reconstruye sus propias fuentes y podría reemplazar esas mejoras.

El arte nuevo se dibujó mediante un generador de pixel art con asistencia de Codex, no mediante dibujo manual atribuido a integrantes del equipo. `crear_assets.py` guarda archivos binarios ASE reales según la [especificación oficial](https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md), no cambia la extensión de imágenes PNG.

## Verificación

`python SourceAssets/Backfranz/verificar_ase.py` comprueba cabeceras, capas, duración, celdas comprimidas y correspondencia píxel a píxel entre los ASE generados y sus PNG. Requiere Pillow. Esta comprobación no equivale a abrirlos en Aseprite; se debe revisar esa apertura en el editor que use el equipo.

`catalogo.png` es una hoja de consulta para localizar los recursos; no es un sprite utilizado en el juego.
