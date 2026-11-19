# Backfranz: adaptación del tour del Bloque A

Referencia visual: grabación del tour 360 facilitada por el usuario, de 7 min 7 s. La grabación se consultó localmente; no se integra al proyecto. Los mapas son una interpretación en vista cenital: el tour muestra ambientes y conexiones, pero no un plano acotado. Los sprites son dibujos originales editables por capas.

| Tramo de la grabación | Referencia que pasa al juego |
|---|---|
| 0:09–0:36 | Acceso acristalado, jardineras interiores, pedestal, torniquete y mostrador blanco. |
| 0:45–1:30 | Sillones odontológicos turquesa, clínica blanca, cafetería acristalada y sofás azules. |
| 1:48–2:42 | Hall longitudinal con luz lineal, bancos metálicos, listones de color, gradas y vidrio esmerilado. |
| 2:51–3:27 | Auditorio de tonos claros y pasillo de subsuelo blanco. Son referencias para el mobiliario y el −1 ficticio, sin atribuir un bloque al auditorio. |
| 3:36–4:21 | Arca, Cajas, mostradores claros, marcas de fila, pasillos y asientos azules. |
| 4:30–5:24 | Cocina de acero, puertas de madera, estudio de noticias y cabina de sonido. |
| 5:33–6:36 | Corredores beige, sala Mac clara, aulas de mesas agrupadas y núcleo de ascensor con marco rojo junto a las gradas. |
| 6:45–7:03 | Acceso a Biblioteca y ambiente de ventanales, vigas verdes, puestos de lectura y escalera metálica. |

Los pisos ya no comparten la misma división interior. El lado normal y Backfranz sí comparten la plantilla de cada piso, con materiales, luces y decoración de emergencia distintos. Se conservan la inducción, el descenso al −1 y la ruta de escape. El número de Biblioteca se sigue dejando fuera de la interfaz por la discrepancia entre las referencias; el video no permite fijar todas las puertas ni las medidas reales.

El lote `tour-*` contiene 50 sprites; `backfranz-tour-*` contiene las 50 variantes oscuras correspondientes. Las fuentes `.ase` conservan sus capas y los PNG son los recursos de Unity. El proyecto mantiene Unity 6000.3.16f1.

La revisión pasó 146 comprobaciones en Unity, entre ellas la accesibilidad de los puntos de visita, los destinos del ascensor, el descenso al −1, el circuito y la salida. Las capturas nuevas están en `Capturas-Tour/`. El verificador de arte comparó las 407 fuentes ASE del proyecto con los 425 fotogramas PNG.
