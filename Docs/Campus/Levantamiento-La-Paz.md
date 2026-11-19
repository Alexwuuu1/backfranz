# Unifranz La Paz — levantamiento para Backfranz

Consulta: 12 de noviembre de 2026. Sede de Héroes del Acre 1855, esquina Landaeta. Este archivo distingue las referencias públicas, lo observado por el usuario y las decisiones de diseño. No sustituye un plano de la sede.

| Referencia | Hallazgo | Cómo usarlo |
|---|---|---|
| [Ficha de la sede y enlace al tour](https://unifranz.edu.bo/sedes/la-paz/) | Confirma la dirección e identifica el recorrido virtual oficial de La Paz. | Referencia para el acceso exterior y la ambientación. |
| [Tour virtual](https://ux.unifranz.edu.bo/tour/lpz/index.htm) · [datos del visor](https://ux.unifranz.edu.bo/tour/lpz/script_general.js?v=1754924297692) | El visor incluye rótulos de `PISO 1` a `PISO 6`, además de referencias a recepción, biblioteca, cafetería, computación, sala Mac, auditorio y laboratorio de sonido. | Confirma que computación y sala Mac están representadas en el recorrido oficial. No usar los rótulos sueltos para deducir el bloque, piso o inventario de cada ambiente. |
| [Biblioteca Sede La Paz](https://bibliovirtual.unifranz.edu.bo/cgi-bin/koha/opac-library.pl?branchcode=BLP) | Su ficha la ubica en el 6.º piso y describe préstamo en sala y a domicilio, además de uso de computadoras. El menú del tour la coloca en el 5.º. | El mapa dirá «Biblioteca» sin número definitivo hasta verificar el ascensor o el letrero de la ruta elegida. La inducción puede enseñar a consultar el mostrador. |
| [Laboratorios iMac](https://unifranz.edu.bo/blog/unifranz-apuesta-por-la-tecnologia-como-eje-transversal-del-nuevo-modelo-por-competencias/) | La universidad documenta laboratorios iMac. La publicación no verifica el piso, el bloque ni el inventario actual de La Paz. | Diseñar un sprite de equipo todo en uno y diferenciarlo visualmente de las estaciones PC. |
| [Centro de Desarrollo en La Paz](https://unifranz.edu.bo/blog/unifranz-la-paz-inaugura-el-centro-de-desarrollo-el-nuevo-corazon-tecnologico-de-ingenieria-de-sistemas/) | Documenta un espacio de Ingeniería de Sistemas con mesas amplias, superficies para ensamblar electrónica y mobiliario móvil. No da su ubicación por piso. | Referencia para un taller especializado opcional; no contarlo como uno de los cuatro laboratorios descritos por el usuario. |
| [Aulas dinámicas](https://unifranz.edu.bo/blog/aulas-dinamicas-un-gran-avance-para-la-educacion-en-bolivia/) | La universidad documenta su implementación en La Paz para actividades prácticas y colaborativas. No identifica las puertas ni pisos específicos. | Crear una variante de aula con mesas agrupadas, pizarra y pantalla; tratar el mobiliario exacto como diseño hasta contar con referencias. |
| [Gastro Lab de Unifranz La Paz](https://unifranz.edu.bo/blog/gastro-lab-el-nuevo-espacio-de-innovacion-gastronomica-en-unifranz-la-paz/) | Unifranz documenta una cocina industrial inaugurada en 2025, pensada para prácticas de hasta 40 estudiantes simultáneos. | Referencia para el arte de cocina. El artículo no indica su piso ni prueba que sea el mismo ambiente llamado «Cocina» en el tour. |

## Bloque A: distribución aportada desde la sede

El usuario confirmó que esta distribución corresponde al **Bloque A**. La descripción separa la **planta baja** y enumera seis niveles por encima. La numeración de la tabla sigue el orden del relato, como hipótesis para montar el mapa. Coincide con los rótulos 1–6 del tour y con la ficha oficial que ubica la biblioteca en el 6.º piso, pero eso no verifica la asignación de todos los ambientes.

| Nivel de trabajo | Ambientes descritos | Tarea de inducción propuesta |
|---|---|---|
| Planta baja | Recepción, cafetería y Game Room. | Conseguir el horario y marcar en el mapa dónde pedir ayuda y dónde reunirse. |
| Piso 1 | Pagos, Posgrado y jefaturas de carrera. | Ubicar Pagos y saber a qué jefatura dirigirse según la carrera. |
| Piso 2 | Laboratorios de Medicina y Gastronomía, entre otros. | Reconocer los laboratorios especializados por su señalización; acceso interior solo donde corresponda. |
| Piso 3 | Laboratorios de computación y aulas dinámicas. Inventario observado: tres salas PC y una Mac. | Distinguir las salas PC y Mac y localizar un aula dinámica. |
| Piso 4 | Aulas normales y dinámicas. | Encontrar un aula usando el horario y la señalización. |
| Piso 5 | Aulas; posiblemente laboratorios de otras carreras. | Explorar las aulas; reservar módulos para los laboratorios aún sin identificar. |
| Piso 6 | Biblioteca, en el último nivel de la ruta según el relato; piso respaldado por el catálogo oficial. | Consultar el mostrador y completar el recorrido de inducción. |

- La descripción anterior ubicaba computación en el piso 2. El menú público del tour respalda el **piso 3** para Sala Mac y Lab. de Sistemas. El inventario de **tres salas PC y una Mac** viene de la observación del usuario: el tour no documenta tres puertas PC ni su orden.
- El dato previo de **aproximadamente nueve aulas por piso** sirve para dimensionar los niveles de aulas. No se toma como un conteo exacto de puertas ni se copia automáticamente a los pisos de laboratorios.
- Falta confirmar el paso entre bloques y registrar qué puertas del A corresponden a aulas dinámicas. También queda por confirmar el inventario de laboratorios del piso 5 del A.

### Contraste con el directorio del tour

Se cruzaron los botones visibles de cada nivel en el [visor oficial](https://ux.unifranz.edu.bo/tour/lpz/script_general.js?v=1754924297692) con sus [rótulos en español](https://ux.unifranz.edu.bo/tour/lpz/locale/es.txt), descartando controles ocultos y listas genéricas repetidas. Los códigos internos de panoramas no corresponden necesariamente al número que muestra el menú. El tour es una selección de lugares y no un plano exhaustivo. El usuario identifica este recorrido principal como Bloque A; el visor no identifica individualmente el bloque de todos sus ambientes.

| Nivel visible | Ambientes que lista el tour |
|---|---|
| Subsuelo | Osteoteca, anfiteatro, sala de Anatomía y pasillo principal. Fuera de los siete mapas iniciales; verificar el acceso si se amplía el alcance. |
| Planta baja | Entrada principal, recepción, admisiones, clínica odontológica, hall principal, cafetería y Game Room. Hay además una entrada del menú llamada Auditorio; la asignación a un bloque y la relación con el auditorio documentado del B siguen pendientes. |
| Piso 1 | Dirección de Carrera, Cajas, Arca y Postgrado. En los panoramas este grupo aparece como `Mezanine`. |
| Piso 2 | Cocina, sala de simulación, laboratorios de Histología y Fisiología, y pasillos. |
| Piso 3 | Sala Mac, Lab. de Sistemas, simulación empresarial, streaming, fotografía, sonido, pasillo principal y pasillo de aulas. |
| Piso 4 | Cámara Gesell, Sala Jurídica y pasillo principal. No incluye inventario de aulas normales o dinámicas. |
| Piso 5 | Aula Dinámica, Biblioteca y pasillo principal. |
| Piso 6 | Oficinas Académicas y pasillo principal. **La ficha oficial de biblioteca dice 6.º piso; el menú del tour dice 5.º.** La ubicación actual se debe contrastar en la sede. |

Un [anuncio de FUCREEIN](https://ec.linkedin.com/in/jeniffer-faytong-5b3357182) menciona el **Aula 201 del Bloque A** como sede de una actividad, pero no indica el piso en texto. No usar el prefijo `2` para deducir un directorio de las demás aulas. Existe un [plano histórico de red del Edificio Central](https://es.scribd.com/document/1035238178/3) con planta baja, mezanine y primera planta; sus usos antiguos difieren de las fuentes actuales y solo sirve como referencia histórica. No se encontró un directorio público actualizado con todas las puertas numeradas.

## Bloque B: referencias encontradas y datos pendientes

| Dato | Evidencia pública | Límite |
|---|---|---|
| Auditorio del Bloque B | [El anuncio de ROC Roaster Coffee, coorganizador del taller Café de Origen](https://www.restaurants10.com/BO/La-Paz/108015419032253/ROC-Roaster-Coffee), lo identifica como sede del taller del 16 de octubre de 2026. [El anuncio de la Alcaldía, reproducido en un visor](https://www.sotwe.com/hashtag/Gesti%C3%B3nDockweiler?lang=tr), coincide en el bloque. | No indica piso, entrada ni distribución interior. |
| Referencia exterior sobre Landaeta | El [programa de la Larga Noche de Museos](https://es.scribd.com/document/863620322/PROGRAMACION) ubica al espacio cultural El CoRa en Landaeta 432, frente al Bloque B. | El número 432 corresponde a El CoRa; no se debe presentar como dirección del bloque. |
| Ambientes del tour | Los [datos del visor oficial](https://ux.unifranz.edu.bo/tour/lpz/script_general.js?v=1754924297692) incluyen un panorama `PLB - AUDITORIO` y rótulos de niveles. | No identifican ese panorama como Bloque B ni publican un inventario de aulas por bloque. No usarlo para dar por hecho que el auditorio del B está en planta baja. |

No se encontró un plano público ni un directorio verificable de aulas del Bloque B. Quedan pendientes el total de niveles, los números de las aulas por piso, los laboratorios, la planta del auditorio y el acceso desde el A. Para Backfranz, se puede reservar el auditorio como destino del B y dejar configurables las conexiones; no copiarle la distribución del A. La referencia más útil para cerrar el mapa sería el directorio del ascensor o la señalética de cada piso.

## Orden de producción para el campus normal

1. **Acceso y recepción.** Conservar la maqueta actual, con la esquina y la señalética como referencias a mejorar. La recepción da la credencial y el horario del primer día. Validar planta de acceso y ubicación del vestíbulo.
2. **Circulación vertical.** Montar un mapa por nivel y un ascensor normal con destinos configurables. Separar el ID del mapa de su número visible. Empezar por recepción, servicios del piso 1 y biblioteca para probar que se puede subir, bajar y volver al punto correcto.
3. **Laboratorios.** Separar Medicina/Gastronomía del nivel de informática. En informática, montar un pasillo con cuatro puertas provisionales y reutilizar estaciones y mesas para PC; Mac tiene una silueta propia. Las puertas se configuran por datos para reordenarlas sin rehacer el mapa.
4. **Niveles de aulas.** Usar dos plantillas de interior: tradicional y dinámica. El horario dirige a un destino que se lee como bloque + nivel + aula; usar nombres provisionales hasta confirmar placas.
5. **Bloques y Backfranz.** Completar el enlace entre bloques cuando esté documentado. Terminar la inducción en biblioteca; solo después el ascensor podrá habilitar el lado Backfranz, que reutilizará las rutas ya conocidas.

Primer lote de sprites específicos: directorio de ascensor y señalética de servicios; caja de Pagos y ventanillas; máquinas y mandos para Game Room; monitores todo en uno y torres PC; camilla y maniquí de prácticas; módulos de cocina; mesas agrupables, pizarra móvil y panel de aula dinámica. Aprovechar los 94 sprites existentes como base de suelos, muros, señalética y mobiliario.

Las cinco zonas existentes en Unity siguen siendo un prototipo. Este levantamiento sustituye la distribución genérica de la propuesta de 12 zonas como referencia de diseño; no implementa ni renumera escenas por sí solo.
