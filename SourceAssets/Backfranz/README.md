# Fuentes gráficas de Backfranz

`Editables/` contiene el arte en formato Aseprite `.ase` con tres capas editables para sombras, color y detalles. El sprite de Yamil mide 40 × 56 px e incluye 24 fotogramas de caminata, 16 de interacción y 16 posiciones compatibles con las referencias anteriores. La sombra tiene cuatro fotogramas.

`Importados/` conserva en `.ase` los PNG anteriores del proyecto, incluidos los fondos del menú. Son imágenes importadas en **una capa**; no se recuperaron ni se inventaron capas originales. Los cuatro archivos históricos del equipo permanecen en `SourceAssets/Aseprite/`.

Unity consume las exportaciones de `Assets/Resources/Backfranz/Art/` y los gráficos anteriores de `Assets/Art/`. Cada exportación tiene su fuente identificada en `assets.json`. Mantener los PNG junto con sus `.meta`: no son duplicados descartables, son los recursos que utiliza el ejecutable. Tipografías TTF y sonidos no son documentos de Aseprite.

## Editar

Abrir el `.ase` con Aseprite o un editor compatible, modificar las capas y exportar a las rutas PNG de `assets.json`. Para animaciones, conservar los nombres y el orden de fotogramas indicados en ese manifiesto. No ejecutar el generador después de editar manualmente: reconstruye sus propias fuentes y podría reemplazar esas mejoras.

El arte nuevo se produce mediante un generador de pixel art; no corresponde a dibujo manual atribuido a integrantes del equipo. `crear_assets.py` guarda archivos binarios ASE reales según la [especificación oficial](https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md), no cambia la extensión de imágenes PNG.

## Verificación

`python SourceAssets/Backfranz/verificar_ase.py` comprueba cabeceras, capas, duración, celdas comprimidas y correspondencia píxel a píxel entre los ASE generados y sus PNG. Requiere Pillow. Esta comprobación no equivale a abrirlos en Aseprite; se debe revisar esa apertura en el editor que use el equipo.

`catalogo.png` es una hoja de consulta para localizar los recursos; no es un sprite utilizado en el juego.

El kit modular del campus agrupa 94 fuentes `campus-*.ase`. `campus_assets.py` define los suelos, la arquitectura, el mobiliario, las señales y los objetos; el generador principal los añade al manifiesto. Ver [la ruta del campus](../../Docs/Campus/README.md) para saber en qué zonas se usan.

`planta_baja_assets.py` define los materiales, la fachada, los personajes de la introducción y el mobiliario propio de planta baja. Las fuentes tienen prefijo `pb-`. Las piezas de los interiores y del pasillo tienen exportaciones `backfranz-pb-` para la fase alterada.

Los archivos con prefijo `tile16-` son los suelos de 16 × 16 píxeles utilizados por los mapas actuales, con las mismas capas editables que sus fuentes. El ciclo `pb-estudiante-camina` tiene 16 fotogramas en cuatro direcciones.

`escenografia_assets.py` dibuja 17 materiales directamente a 16 × 16, además de un muro de servicio, divisiones de baño y un panel de herramientas. El manifiesto incluye las versiones normales y alteradas de las 20 fuentes y sus exportaciones.

`personajes_assets.py` define 60 NPC con peinados, ropa, siluetas y accesorios propios. Damián, el Estudiante de Derecho, Inés, Marcos y Elena tienen cuatro fotogramas por dirección. El guardia y Aylin también tienen una pose sentada. Cada personaje conserva su diseño en el mapa, el diálogo y la enciclopedia.

Los diez diseños `estudiante-campus-` incluyen caminata en cuatro direcciones y poses sentadas de frente y de espaldas. Se usan en los pasillos, las oficinas, el café y las salas durante la inducción.

`muebles_assets.py` define 23 modelos de estantes, mesas, asientos y un carro de libros. Cada mueble tiene una versión para el campus normal y otra para Backfranz. El contraste entre madera, acero, tapicería y accesorios distingue los espacios sin estrechar los pasillos.

`acceso_assets.py` dibuja los adoquines, bordillos y peldaños de Landaeta. También aporta los módulos del Bloque A, la puerta lateral, las tiendas, el Instituto Americano y dos vehículos estacionados. `CampusWorld.Street.cs` monta las gradas transitables y fija el límite de los muretes, las fachadas y los vehículos.

`rotulos_assets.py` genera las placas de las salas, las letras de las fachadas y los siete fotogramas de cada puerta corrediza. Los marcos conservan su tamaño y posición mientras las hojas se deslizan. Las puertas y placas interiores incluyen variantes para la fase alterada.

El mismo generador dibuja las gradas encajadas en el muro y los seis fotogramas intermedios y finales de los controles de acceso a partir de su pose cerrada. En los molinetes solo giran las hojas. El lector tiene una luz propia.

`letras.py` contiene el alfabeto original de Backfranz Pixel con mayúsculas, minúsculas, tildes y signos. Las placas y fachadas usan ese dibujo directamente. Para regenerar la TTF que usan los menús y los diálogos, ejecutar `python SourceAssets/crear_fuente.py` desde la raíz del proyecto.

La paleta [Suelos Backfranz](../../Assets/Art/Tiles/Backfranz/Suelos%20Backfranz.prefab) se puede seleccionar en **Window → 2D → Tile Palette**. La fila superior muestra los suelos normales y la inferior sus variantes alteradas. Cada baldosa ocupa una celda en una cuadrícula rectangular de una unidad. La paleta permite pintar prototipos en el editor. Los mapas del recorrido continúan construyéndose mediante `CampusWorld`.

## Audio

El banco combina grabaciones CC0 con diseño sonoro original de Backfranz. La receta de edición, las fuentes y sus licencias figuran en [Audio](AUDIO.md).

`bloque_a_assets.py` añade 42 sprites específicos para el Bloque A, los puzzles y el subsuelo. El generador produce también 108 variantes oscuras con prefijo `backfranz-`, conservando las tres capas para que puedan editarse en Aseprite. Los PNG se importan con filtro Point. Ver [Bloque A y Backfranz](../../Docs/BloqueA/README.md) para el recorrido completo.
