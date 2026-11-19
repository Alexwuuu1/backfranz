# Backfranz

## Continuar el desarrollo en otra PC

Clonar `https://github.com/Alexwuuu1/backfranz.git` y trabajar con la versión integrada de `main`.
Añadir la carpeta clonada a Unity Hub y abrirla con Unity **6000.3.16f1**. La primera apertura
regenera la caché y descarga los paquetes. Abrir `Assets/Scenes/MenuPrincipal.unity` y pulsar Play.

Para generar el APK, instalar Android Build Support, Android SDK & NDK Tools y OpenJDK desde
Unity Hub. Los exportadores están en `Tools → Backfranz → Exportar`. Windows y Android deben
probarse fuera del editor después de generar los ejecutables de esta revisión.

Videojuego 2D de exploración y terror universitario del Grupo 64. Yamil se enfrenta a una universidad que se transforma en un espacio inspirado en backrooms.

## Abrir y jugar
1. Abrir esta carpeta con Unity **6000.3.16f1**.
2. Abrir `Assets/Scenes/MenuPrincipal.unity` y presionar Play.
3. Elige **Nueva partida**. Muévete con WASD o flechas, interactúa con E o Espacio y avanza los diálogos también con Enter. Escape abre la pausa. En móvil aparecen la cruceta y el botón de interacción.
4. En la ventanilla de Recepción obtienes credencial y horario. Escanea en un molinete o habla con el guardia para cruzar por el paso ancho. Después puedes explorar y subir por las gradas o el ascensor.
5. El campus cambia cuando eliges un destino inferior en el ascensor desde el piso 6. Consulta el diario, busca a los testigos y resuelve los mecanismos para abrir el portón. [Campaña de escape](Docs/BloqueA/Escape.md)
6. Elige **Guardar partida** en la pausa. **Continuar partida** recupera ese punto y el progreso en este equipo.

Cada nueva partida presenta a Yamil y el motivo de la inducción. Un tutorial de cuatro pasos acompaña el desplazamiento, la interacción, la pausa y la llegada a Recepción. Muestra controles de teclado o táctiles según la plataforma y permite omitirlo. Continuar partida recupera el guardado directamente.

El menú incluye Nueva partida, Continuar partida, Opciones, Extras, Créditos y Salir. En el editor, Salir detiene Play. Las opciones se conservan en este equipo.

## Estado real
Prototipo jugable con exterior, registro y seguridad, dos escaleras transitables, ascensor, siete plantas navegables e interiores independientes. Las zonas se construyen durante la partida en `Nivel01`. El arte se conserva en fuentes ASE con exportaciones PNG. Consulta la [guía del Bloque A](Docs/BloqueA/README.md) para ver el alcance, las pruebas y los datos por confirmar.

## Equipo
| Integrante | GitHub |
|---|---|
| Alejandro Villalpando Rojas | Alexwuuu1 |
| Cristopher Iori Lazcano Gutierrez | Crisshubb |
| Galilea Alison Llusco Asistiri | Galileya |
| Alex Joel Quispe Ticona | [lpzealexjoelquispeti-dot](https://github.com/lpzealexjoelquispeti-dot) |

## Organización
- Assets/Scenes: MenuPrincipal y Nivel01.
- Assets/Scripts/UI: navegación, sonido y transición.
- Assets/Scripts/Playable: controles, escenarios, arte y flujo de partida.
- Assets/Art/UI/Backfranz: fondos, logotipo y fuente propia.
- Assets/Editor/Hito3Setup.cs: generador reproducible de escenas/UI.
- Docs/Hito3: pruebas, evidencias e informe técnico.
- Docs/Hito4: recorrido, exportadores, alcance de pruebas y capturas del prototipo jugable.
- SourceAssets: referencias originales, generador de tipografía y fuentes gráficas ASE con sus verificadores.

Las referencias de Galileya se conservaron. Fondos y logotipo se adaptaron con generación de imágenes asistida por IA; fuente y audio se generan por código. Detalles en Docs/Hito3/ARTE.md.

Repositorio: https://github.com/Alexwuuu1/backfranz
Flujo: ramas individuales -> develop -> main, mediante merge sin squash para conservar autores. Ver Docs/Hito3/ENTREGA.md.

## Evidencias del Hito 3

- [Informe PDF](Docs/Hito3/Backfranz-Hito3.pdf)
- [Menú principal](Docs/Hito3/Capturas/02-menu-backrooms.png)
- [Menú de pausa](Docs/Hito3/Capturas/03-pausa.png)
- [Pruebas y alcance](Docs/Hito3/PRUEBAS.md)

Los aportes de los cuatro integrantes están integrados conservando sus commits y autores.

La nueva versión jugable está en preparación para revisión del equipo. Ver [recorrido y exportación](Docs/Hito4/README.md).
