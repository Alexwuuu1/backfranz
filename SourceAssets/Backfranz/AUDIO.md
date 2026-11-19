# Audio de Backfranz

Unity carga los 84 clips de `Assets/Resources/Backfranz/Audio` a 48 kHz. Incluyen 19 bucles de ambiente, un tema de menú, 25 pisadas y 39 efectos y señales. El juego mezcla en los grupos existentes de Ambiente, Música, Efectos y Pasos.

Las grabaciones de calle y cafetería sirven para ambientar la adaptación ficticia de Unifranz. Proceden de las bibliotecas indicadas abajo. El diseño de avisos del lector, el motivo del menú y las tres cadencias de final son originales de Backfranz.

## Grabaciones CC0

| Fuente y autor | Entrada de la receta | Uso |
| --- | --- | --- |
| [Impact Sounds, Kenney](https://kenney.nl/assets/impact-sounds) | `impact/Audio/` | Pisadas de baldosa, adoquín, madera y alfombra |
| [Different steps on wood, stone, leaves, gravel and mud, TinyWorlds](https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud) | `surface/gravel.ogg` | Textura de gravilla bajo el adoquín |
| [Metal footsteps on concrete, Thimras](https://opengameart.org/content/metal-footsteps-on-concrete) | `metal/metal_steps_*.wav` | Pisadas metálicas |
| [100 CC0 SFX, rubberduck](https://opengameart.org/content/100-cc0-sfx) | `foley/` | Puertas, papeles, interruptores, campana, tazas y golpes |
| [Elevator_Door, PagDev](https://opengameart.org/content/elevatordoor) | `elevator.wav` | Apertura y cierre de la cabina |
| [Old elevator door, sinny](https://opengameart.org/content/old-elevator-door) | `old-lift.mp3` | Puertas metálicas, motor y descenso |
| [Mechanical Sounds, BMacZero](https://opengameart.org/content/mechanical-sounds) | `clank.wav` desde `clank1.wav`, `squeak.wav` desde `squeakyclick2.wav` | Pestillos mecánicos, golpe y raspado |
| [Heartbeat, Independent.nu](https://opengameart.org/content/heartbeat-single-sound) | `heartbeat.flac` | Aviso y retorno de la sombra |
| [Street and Roads, Joseph Sardin](https://bigsoundbank.com/street-and-roads-s0608.html) | `street.ogg` | Tráfico de Landaeta y filtrado de escaleras |
| [Coffee Shop at the Capucins, Joseph Sardin](https://bigsoundbank.com/coffee-shop-at-the-capucins-s2561.html) | `coffee.ogg` | Café y conversaciones filtradas en el campus |
| [The Shop, LEGIT Audio](https://opengameart.org/content/the-shop) | `rooms/TheShopCollection_convenience_store_drinks_fridge_drone_2.wav` | Cabina, computadoras y maquinaria |
| [AMB Outside 1, Kresiek The Furry](https://opengameart.org/content/amb-outside-1) | `outside.ogg` | Aire filtrado de espacios silenciosos y alterados |

## Rehacer los clips

Instala NumPy, SciPy y SoundFile con `python -m pip install numpy scipy soundfile`. Descarga y extrae las fuentes en una carpeta local con los nombres de la tabla. Después, ejecuta `python SourceAssets/Backfranz/crear_audio.py RUTA_DE_LAS_GRABACIONES` desde la raíz del proyecto. Los clips ya editados se incluyen en el proyecto para abrirlo directamente en Unity.
