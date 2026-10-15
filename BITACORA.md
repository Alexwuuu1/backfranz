# Bitácora del proyecto

## 03/09/2026

### Actividades realizadas

- Se definió la idea inicial del videojuego: `Backfranz`.
- Se establecieron el género, la plataforma y el estilo visual.
- Se preparó el repositorio Git local.
- Se creó la estructura inicial del proyecto Unity 2D.
- Se organizaron las carpetas de arte, escenas, prefabs y materiales.
- Se añadió un `.gitignore` adecuado para Unity.
- Se creó una escena inicial llamada `Nivel01`.
- Se prepararon assets gráficos originales para el fondo, tiles, personaje y decoración.
- Se construyó una zona del campus usando Grid y Tilemap.
- Se redactó el README del proyecto.
- Se documentó el flujo de ramas `main`, `develop` y `feature/*`.
- Se agregaron y aceptaron los tres colaboradores restantes en GitHub.
- Se configuró el proyecto para abrirse en modo 2D por defecto.
- Se incorporó el personaje original del equipo con cuatro cuadros de animación.
- Se añadió movimiento horizontal con teclado, física 2D y cambio de orientación.
- Alex Joel incorporó archivos fuente de Aseprite mediante el commit `9c2c60e`.
- Cristopher creó e incorporó un fondo, un personaje alternativo, ocho tiles y cuatro decoraciones para el campus desde `feature/fondo-campus`.
- Los aportes visuales de Cristopher se integraron en `develop` y `main` conservando su autoría.
- Se fusionaron Pull Requests de `develop` hacia `main` y se comprobó una clonación limpia con Unity 6000.3.16f1.

### Problemas encontrados

- La primera creación automática del proyecto tardó más de lo esperado mientras Unity intentaba conectarse con el servicio local de licencias.
- La consigna menciona assets libres en una sección y assets propios en las condiciones generales.
- GitHub reconoce commits de Alejandro y Cristopher.
- El commit de Alex Joel existe, pero el correo `alexjoelquispeticona48@gmail.com` no está vinculado por GitHub a la cuenta `lpzealexjoelquispeti-dot`.
- Galilea todavía no aparece como autora de ningún commit.

### Soluciones aplicadas

- Se conservó la estructura válida generada por Unity y se preparó una herramienta de Editor para crear la escena al abrir el proyecto normalmente.
- Se optó por generar gráficos originales dentro del proyecto para cumplir la interpretación más estricta de la consigna.
- Se registraron los nombres completos y los usuarios de GitHub confirmados en el README.
- Se verificó en GitHub que los cuatro integrantes tienen permiso de colaboración.
- Se comprobó que el repositorio no tiene protección de ramas ni reglas que impidan a los colaboradores realizar commits.
- Se añadieron los `.meta` faltantes de los archivos Aseprite para mantener una clonación limpia y portable.
- Se actualizó el inventario y la autoría de los assets principales en el README.

### Tareas pendientes

- Vincular y verificar en la cuenta de Alex Joel el correo usado en su commit, o crear un commit nuevo desde su cuenta de GitHub.
- Hacer que Galilea realice un commit real desde `Galileya`.
- Integrar esos aportes finales en `develop` y luego en `main`.
- Practicar la defensa oral y entregar la URL pública del repositorio.

### Distribución de responsabilidades

| Integrante | Tarea | Rama sugerida | Evidencia esperada |
| --- | --- | --- | --- |
| Alejandro Villalpando Rojas (`Alexwuuu1`) | Estructura Unity y repositorio | `feature/escena-inicial` | Commit de configuración inicial |
| Cristopher Iori Lazcano Gutierrez (`Crisshubb`) | Fondo y decoración | `feature/fondo-campus` | Commit de un cambio visual propio |
| Galilea Alison Llusco Asistiri (`Galileya`) | Tiles y Tilemap | `feature/tilemap-campus` | Commit que modifique el escenario |
| Alex Joel Quispe Ticona (`lpzealexjoelquispeti-dot`) | Documentación | `feature/documentacion` | Commit que complete README y bitácora |

## 15/10/2026 - Hito 3
- Nombre definitivo: Backfranz. Repositorio renombrado y configuración de Unity actualizada.
- Se incorporaron menú principal, opciones, galería, créditos, confirmación de salida y pausa.
- Transición normal/backrooms después de cinco segundos con sonido procedural.
- Fuente propia Backfranz Pixel y nuevos fondos basados en las referencias de Galileya.
- Se preparó la implementación con asistencia de IA para revisión e integración del equipo.
- Validación automática documentada en Docs/Hito3/VERIFICACION.txt. Pruebas manuales pendientes en PRUEBAS.md.
