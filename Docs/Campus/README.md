# Campus normal: recorrido y kit de arte

La continuación jugable del proyecto está en [Bloque A y Backfranz](../BloqueA/README.md). Esta página documenta el prototipo anterior del campus.

Para ampliar el campus, usar primero el [levantamiento por pisos de Unifranz La Paz](Levantamiento-La-Paz.md). La tabla de zonas de esta página describe la maqueta y la propuesta anterior al levantamiento.

Esta etapa es una introducción a la universidad. Se desarrolla primero la ruta normal, sin persecución ni puzzles de terror. El lado Backfranz usará después **las mismas ubicaciones y conexiones reconocibles**, con obstáculos y objetos para escapar. Los nombres de aulas, pisos y conexiones de la maqueta son provisionales; confirmarlos con un recorrido de la sede antes de usarla como guía real.

## Recorrido propuesto

| Orden | Zona | Qué aprende o hace el estudiante | Estado |
|---|---|---|---|
| 01 | Exterior y acceso | Reconoce la entrada y sigue la señal hacia recepción. | Jugable |
| 02 | Recepción | Recoge credencial y horario, ubica el punto de información. | Jugable |
| 03 | Circulación del bloque A | Lee el bloque, identifica el piso y encuentra una escalera. | Planeada |
| 04 | Aula del bloque A | Usa el horario para confirmar su aula (A-01 provisional). | Jugable |
| 05 | Orientación o servicios | Identifica dónde resolver una duda académica. | Planeada |
| 06 | Conexión entre bloques | Encuentra el paso al otro bloque, si la sede lo confirma. | Planeada |
| 07 | Circulación del bloque B | Distingue bloque, piso y dirección; regresa sin perderse. | Planeada |
| 08 | Aulas del bloque B | Ubica una segunda aula a partir de su señalización. | Planeada |
| 09 | Biblioteca | Consulta dónde pedir ayuda o conseguir material. | Jugable, desvío desde el aula |
| 10 | Sala práctica | Reconoce un espacio de laboratorio o informática, si existe en la ruta. | Planeada |
| 11 | Área común | Ubica un punto de encuentro, descanso y servicios cercanos. | Planeada |
| 12 | Vestíbulo | Termina la inducción al consultar el ascensor después de encontrar su aula. Puede seguir explorando. | Jugable |

En la maqueta, la ruta es **entrada → recepción → aula → vestíbulo**, con biblioteca opcional desde el aula. Hay puertas de regreso. En la expansión, los lugares intermedios se conectarán según la planta real. Para el levantamiento registrar por zona: bloque, piso, función, letrero, salida, acceso alternativo, escalera/ascensor y referencias de color o mobiliario. Los espacios como baños o patios se pueden incorporar dentro de una zona en lugar de convertirlos en mapas independientes.

## Kit modular

94 sprites nuevos: **14 suelos, 6 muros, 6 puertas, 10 piezas arquitectónicas, 32 muebles y elementos de entorno, 16 señales y 10 objetos**. Las fuentes editables están en `SourceAssets/Backfranz/Editables` con prefijo `campus-`. Cada una tiene tres capas (sombra, volumen/color y detalles). Los PNG de Unity conservan píxeles nítidos sin filtrado suave; el catálogo permite localizar piezas por nombre. Los suelos usan 32 × 32 px por casilla y los objetos pueden ocupar más de una.

Para montar cada zona: hacer el contorno y las conexiones, comprobar que sea recorrible con el personaje, colocar señalética y finalmente amueblar. Conservar las puertas, pasillos y puntos de referencia como datos del mismo mapa para que el lado oscuro reutilice la ruta. El prefijo `campus-` distingue este kit del arte preexistente.

## Próximo corte

Ampliar el recorrido normal con las siete zonas planeadas en el orden de las conexiones verificadas. Añadir mapa de descubrimientos y señalética real. No habilitar el cruce del ascensor hasta que el trayecto normal completo esté listo. Al diseñar Backfranz, usar los mismos IDs de zona y guardar el estado de cada puzzle en la partida; por ejemplo, restablecer luz en circulación, localizar una pista en el aula y regresar por un acceso alternativo que el jugador ya conoce.

## Verificación

En Unity 6000.3.16f1, `BackfranzPlayableSmoke.Run` pasó 42 comprobaciones de menú, colisiones, navegación de ida y vuelta, requisitos de credencial y horario, visita opcional, cierre de la inducción y retorno al menú. La prueba recorre la geometría usando el motor del jugador. Las imágenes de `Capturas/` se tomaron en esa ejecución dentro del editor. Todavía falta validar el recorrido con personas y con una build de cada plataforma.
