# Pasos para completar la entrega del Hito 2

## 1. Completar los datos del equipo

Los usuarios confirmados son `Crisshubb`, `Galileya` y `lpzealexjoelquispeti-dot`. Comprobar que coincidan con las cuentas que aceptaron las invitaciones.

## 2. Comprobar el proyecto en Unity

1. Abrir Unity Hub.
2. Agregar la carpeta del repositorio como proyecto.
3. Abrirlo con Unity 6000.3.16f1.
4. Esperar a que termine la importación.
5. Confirmar que se genere `Assets/Scenes/Nivel01.unity`.
6. Abrir esa escena y revisar cámara, fondo, Grid, Tilemaps y decoraciones.
7. Presionar Play y verificar que no aparezcan errores rojos en Console.

Si la escena no se genera automáticamente, utilizar el menú **Tools > Hito 2 > Crear o reconstruir escena**.

## 3. Crear el repositorio público

Nombre recomendado: `backfranz`.

Descripción recomendada: `Videojuego 2D de orientación y vida universitaria - Grupo 64`.

El repositorio debe ser público y contener `Assets/`, `Packages/`, `ProjectSettings/`, `README.md`, `BITACORA.md`, `.gitignore` y este material de apoyo.

## 4. Agregar colaboradores

Los cuatro integrantes ya aparecen como colaboradores con acceso al repositorio. Antes de entregar, volver a comprobar esta sección y confirmar que cada cuenta tenga al menos un commit propio.

## 5. Preparar las ramas

La estructura obligatoria es:

```text
main
└── develop
    ├── feature/fondo-campus
    ├── feature/tilemap-campus
    └── feature/documentacion
```

Cada rama feature debe salir de `develop`, no directamente de `main`.

Las tres ramas de los compañeros ya fueron creadas en GitHub. Cada integrante debe cambiar a su rama asignada antes de modificar archivos.

## 6. Garantizar participación individual

Cada integrante debe clonar el repositorio o trabajar con su propia cuenta y realizar una modificación real:

- Alejandro: configuración y estructura del proyecto.
- Cristopher: agregar o ajustar una decoración o parte del fondo.
- Galilea: agregar varios tiles al escenario y guardar la escena.
- Alex: completar los datos del equipo y actualizar la bitácora.

Ramas asignadas:

- Cristopher (`Crisshubb`): `feature/fondo-campus`.
- Galilea (`Galileya`): `feature/tilemap-campus`.
- Alex Joel (`lpzealexjoelquispeti-dot`): `feature/documentacion`.

No se debe cambiar el autor de los commits ni realizar los cuatro commits desde una sola cuenta. El docente revisará las identidades en el historial.

## 7. Usar mensajes claros

Ejemplos válidos:

```text
Crear estructura inicial del proyecto Unity
Agregar fondo de la entrada del campus
Construir sendero principal con Tilemap
Completar README y bitácora del equipo
```

Evitar mensajes como `cambios`, `cosas`, `aaa`, `prueba` o `final`.

## 8. Integrar el trabajo

1. Subir cada rama feature a GitHub.
2. Crear un Pull Request hacia `develop`.
3. Revisar que no existan conflictos.
4. Hacer merge sin eliminar la evidencia de los commits individuales.
5. Abrir Unity desde `develop` y comprobar la escena.
6. Hacer un Pull Request final de `develop` hacia `main`.

## 9. Revisar el repositorio

Antes de entregar, confirmar:

- El repositorio es público.
- Los cuatro integrantes aparecen como colaboradores.
- Existen `main`, `develop` y al menos una `feature/*`.
- Existe al menos un merge.
- Cada integrante tiene un commit propio.
- No están versionadas `Library/`, `Temp/`, `Logs/` ni `UserSettings/`.
- Sí están versionadas las carpetas `Assets/`, `Packages/` y `ProjectSettings/`.
- Los archivos `.meta` están incluidos.
- `Assets/Scenes/Nivel01.unity` existe.
- El fondo y los tiles se ven en la escena.
- README y BITACORA están actualizados.
- La URL pública abre sin iniciar sesión.

## 10. Entregar

Entregar la URL pública del repositorio. Todos los integrantes deben poder explicar su aporte, su rama y su commit.
