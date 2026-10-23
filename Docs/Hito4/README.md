# Versión jugable y exportación

Unity 6000.3.16f1. Abrir MenuPrincipal y pulsar Jugar. SessionFlow construye tres sectores de 32 × 20 casillas dentro de Nivel01. La cámara sigue al personaje y se limita al borde del mapa. El campus anterior se conserva desactivado durante la partida.

## Recorrido
- Recepción: explorar oficinas y recoger la tarjeta de acceso y el primer apunte. Ambos desbloquean el ascensor.
- Pasillos: activar el cuadro eléctrico para abrir la reja, encontrar el segundo apunte y alcanzar el ascensor. La sombra patrulla y persigue al jugador al verlo; las paredes bloquean su visión.
- Biblioteca: atravesar aulas y estanterías, recoger el tercer apunte y llegar a la salida evitando a la sombra.

WASD/flechas mueve, E/Espacio interactúa a corta distancia, Shift corre y Escape pausa. En Android aparecen cuatro flechas gráficas y un botón de interacción. El HUD muestra únicamente zona, apuntes, tarjeta y energía. Los avisos contextuales son breves; no hay rótulos flotantes ni instrucciones permanentes sobre el mapa. Derrota y victoria permiten reintentar o volver al menú.

## Arte y tipografía
Los gráficos nuevos incluyen paredes, suelos, muebles, casilleros, plantas, libros, ascensores, luces, objetos e iconos. Yamil tiene 16 fotogramas distribuidos en cuatro direcciones. Los paneles usan bordes pixelados y los textos la tipografía Backfranz Pixel.
Las fuentes gráficas editables están en SourceAssets/Backfranz, con un manifiesto de correspondencia ASE/PNG. Las imágenes anteriores importadas tienen una sola capa; el arte nuevo tiene tres capas. Los PNG de Assets son exportaciones necesarias para ejecutar el juego. Fuentes TTF y audio conservan sus formatos propios. Ver SourceAssets/Backfranz/README.md para edición y validación.

## Exportación reproducible
Tools > Backfranz > Exportar > Windows x64 genera Builds/Backfranz-Jugable junto al proyecto. Distribuir la carpeta completa.
Tools > Backfranz > Exportar > Android APK genera Builds/Android/Backfranz.apk. Configura orientación horizontal, com.grupo64.backfranz, API mínima 26 y las escenas MenuPrincipal y Nivel01. Android Build Support, SDK/NDK y OpenJDK ya están instalados y verificados en la computadora del coordinador. Cada integrante debe instalar esos componentes para Unity 6000.3.16f1 en su propia computadora antes de exportar. Verificar o aceptar las licencias del SDK que solicite Unity durante la primera exportación. APK y pruebas en dispositivo/emulador siguen pendientes.

## Colaboración y validación
Controles, escenarios, arte y flujo de partida son módulos complementarios que deben probarse juntos. Integrar las ramas conservando sus autores. El resultado y los límites de las pruebas se registran en VERIFICACION.txt.
Las capturas se obtienen ejecutando Unity. La captura 06 es una previsualización de controles táctiles en el editor; no acredita una prueba en Android. Las pruebas automáticas verifican geometría y flujo, pero no sustituyen un recorrido manual completo ni la evaluación de la dificultad y la ergonomía en dispositivo.
