# Backfranz

Videojuego 2D de exploración y terror universitario del Grupo 64. Yamil se enfrenta a una universidad que se transforma en un espacio inspirado en backrooms.

## Abrir y jugar
1. Abrir esta carpeta con Unity **6000.3.16f1**.
2. Abrir `Assets/Scenes/MenuPrincipal.unity` y presionar Play.
3. A los cinco segundos el campus cambia con un trueno y destellos.
4. Jugar carga Nivel01. A/D o flechas horizontales mueven al estudiante del prototipo. Escape pausa.

El menú incluye Jugar, Opciones, Extras (galería), Créditos y Salir. En el editor, Salir detiene Play; en la aplicación la cierra. Volumen, pantalla completa y reducción de destellos guardan preferencias locales.

## Estado real
Menú y pausa implementados. El campus jugable conserva el personaje del Hito 2; Yamil está representado en el nuevo fondo. Sus ciclos de caminar/correr aún requieren integración. No hay guardado de partida, enemigos ni niveles backrooms completos.

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
- Assets/Art/UI/Backfranz: fondos, logotipo y fuente propia.
- Assets/Editor/Hito3Setup.cs: generador reproducible de escenas/UI.
- Docs/Hito3: pruebas, evidencias y coordinación de entrega.
- SourceAssets: referencias originales del equipo y generador de tipografía.

Las referencias de Galileya se conservaron. Fondos y logotipo se adaptaron con generación de imágenes asistida por IA; fuente y audio se generan por código. Detalles en Docs/Hito3/ARTE.md.

Repositorio: https://github.com/Alexwuuu1/backfranz
Flujo: ramas individuales -> develop -> main, mediante merge sin squash para conservar autores. Ver Docs/Hito3/ENTREGA.md.

## Aporte de Alex Joel

Responsable: **Alex Joel Quispe Ticona**, cuenta [lpzealexjoelquispeti-dot](https://github.com/lpzealexjoelquispeti-dot).

Este aporte reúne documentación, verificación automática, capturas e informe PDF en la rama `feature/hito3-alexjoel`. Se integra mediante PR hacia `develop`. La implementación completa del menú depende de los aportes de los demás integrantes.
