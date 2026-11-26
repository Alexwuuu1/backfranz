# Backfranz: el Bloque A

La revisión de [planta baja](Planta-Baja.md) incorpora la ventanilla de Recepción, el control de seguridad, seis salas y los baños accesibles desde el pasillo en L. Las capturas versionadas en `Capturas-PB/` corresponden al prototipo anterior.

La [propuesta de rediseño](Rediseno/README.md) contiene los planos de día y de noche de planta baja y las rutas del rescate del Auditorio.

Ver [la adaptación del tour 360](Adaptacion-Tour.md) para conocer las referencias, la distribución y los sprites de esta revisión.

La partida empieza con Yamil en la subida de Landaeta el día de la inducción. Avanza el diálogo con E, Enter, Espacio o el botón del cuadro de texto. Puedes hablar con un estudiante en la calle. En la ventanilla de Recepción obtienes la credencial, el horario y el acceso a los molinetes. En cada piso superior hay un guía que te encarga una actividad y señala dónde completarla. El directorio del ascensor muestra las constancias pendientes. Las plantas son adaptaciones jugables. [Levantamiento de la sede](../Campus/Levantamiento-La-Paz.md)

En el exterior, el Instituto Americano queda al otro lado de Landaeta. Desde la vereda de las tiendas se sube por los peldaños entre dos barandas. En el rellano se gira a la derecha hacia la puerta lateral del Bloque A. Al salir desde planta baja, Yamil reaparece en ese rellano.

Las gradas centrales y las del auditorio conectan las plantas normales. Cada núcleo tiene un mapa transitable con dos tramos y un descansillo. Al salir, Yamil aparece junto al mismo núcleo en la planta de destino. Llama al ascensor con E, camina hasta el panel de la cabina y elige el piso. Puedes visitar la biblioteca y volver a los pisos inferiores durante la inducción. El cambio de estado ocurre al bajar en ascensor desde la biblioteca después de obtener las seis constancias. La [campaña de escape](Escape.md) habilita el sótano después de validar el permiso de evacuación en la biblioteca.

El guardado local conserva la posición, las visitas, los accesos y la fase del mundo. Puedes guardar en un descansillo y continuar desde ese tramo. Las ventanas de ascensor y los desplazamientos en curso deben terminar antes de guardar.

La pausa indica la zona y el tiempo de partida. Al elegir Volver al menú puedes guardar y salir, salir sin guardar o cancelar. Cada final muestra su ilustración, el epílogo, el número de personas reunidas o a salvo y los tiempos de partida y escape.

- **Windows:** WASD o flechas para moverte, E o Espacio para interactuar, Shift para correr, Escape para pausar o cerrar el ascensor. Puedes usar el ratón o las flechas y Enter en el directorio.
- **Controles táctiles:** flechas para moverte y botón de mano para interactuar; los destinos del ascensor se eligen tocándolos.
- **Unity**. Usar `6000.3.16f1`, abrir `Assets/Scenes/MenuPrincipal.unity` y pulsar Play. Para ejecutar la prueba de recorrido y persistencia en una copia de verificación, usar `-batchmode -backfranzSmokeSave -executeMethod BackfranzPlayableSmoke.Run` sin `-quit`. El argumento de prueba separa el guardado automatizado de la partida personal.

## Implementado

- Movimiento en cuatro direcciones, carrera, colisiones y cámara con escala entera para los tiles de 16 × 16. Las texturas usan filtro Point.
- Exterior de Landaeta, pasillo en L, Recepción, cinco molinetes, paso ancho, guardia, bancas y accesos de vidrio.
- Dos escaleras de doble tramo, ascensor con cabina transitable y panel, y puertas con hojas que se deslizan dentro del marco.
- Placas de sala fijadas a los marcos y a las paredes, y señalización de salida integrada. Los diálogos y menús usan el alfabeto Backfranz Pixel, con minúsculas y tildes.
- Siete plantas normales y sus variantes alteradas. Los pisos superiores tienen salas de distintos tamaños, entre ellas Posgrado, Caja, Cocina, laboratorios, aulas y sala de lectura. El sótano tiene una zona de mantenimiento para la fase alterada.
- Materiales de 16 × 16 para distinguir las zonas. La biblioteca despeja el eje hacia la sala de lectura y lleva las estanterías al perímetro. Los estudios tienen paneles acústicos y el subsuelo separa el servicio eléctrico de la zona de bombas mediante un paso metálico y una rejilla de drenaje.
- Estudiantes de pie, sentados y caminando, guardia, personal administrativo, profesora, bibliotecaria y personal de cafetería. En el piso 4 un estudiante sigue una rutina de entrada y salida de clase. Los diálogos tienen texto progresivo. El guardia ofrece dos respuestas para el paso ancho.
- Guardado y carga desde el menú, tránsito persistente en escaleras y activación única de la fase alterada.
- Banco de 84 clips con 19 ambientes estéreo y cinco pisadas por superficie. Los fondos se funden al cruzar puertas y bajan de volumen durante los diálogos y la pausa. El ascensor, los acertijos, el diario y los rescates tienen sonidos propios. Los pasillos alterados tienen detalles ocasionales en estéreo y cada final tiene su propia cadencia.

El mezclador organiza las fuentes por ambiente, música, efectos y pasos. Los pasos tienen un canal propio dentro de Efectos. [MezclaBackfranz](../../Assets/Resources/Backfranz/MezclaBackfranz.mixer) · [Fuentes y licencias de audio](../../SourceAssets/Backfranz/AUDIO.md)

Las fuentes `.ase`, los PNG y su manifiesto están documentados en [Arte](../../SourceAssets/Backfranz/README.md).

## Verificación

En Unity 6000.3.16f1 pasaron 1.295 comprobaciones del recorrido, incluidos el cierre del molinete después del paso, los retornos a las puertas de planta baja y los tres finales. La prueba específica del acceso pasó 164 comprobaciones en la revisión anterior.

El verificador de arte comprobó 791 fuentes ASE y 1.103 fotogramas PNG. La prueba de escenografía en Unity recorrió los interiores con los materiales nuevos y verificó que sus suelos conservan el tamaño de 16 × 16 y el filtro Point.

La auditoría de colisiones revisó 69 mapas y 55.372 puntos libres. Comprueba el suelo transitable, las conexiones y que Yamil pueda apartarse si coincide con un estudiante. También verifica las gradas exteriores, los vehículos estacionados, los seis carriles de planta baja y la reja del subsuelo. Se puede repetir en la copia de verificación con `-batchmode -nographics -executeMethod BackfranzMapAudit.Run`.

## Pendientes de contraste y desarrollo

- Confirmar la geometría de las dos escaleras, puertas y medidas de los pisos superiores, baños, distribución interna del Game Room y del auditorio, y accesos efectivos al sótano. Las ubicaciones, dimensiones y objetos actuales de esos espacios son provisionales.
- Ampliar los eventos posteriores al descenso y calibrar la duración de la [campaña de escape](Escape.md). `Assets/Resources/Backfranz/DescentPreview.json` define la duración y el mensaje de la transición.
- Ampliar los horarios a más personajes, asignar pistas originales de música y realizar una prueba manual de controles en móvil.

## Incidencias conocidas

- Unity puede registrar una excepción interna de indexación al arrancar en batch. Las herramientas de configuración históricas emiten avisos de API obsoleta.
- El guardado usa una sola ranura local. Se recomienda guardar después de cerrar los diálogos y salir de la cabina.
