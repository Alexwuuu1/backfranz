# Avance de proyecto grupal — Backfranz

**Grupo 64** · Unity 6000.3.16f1 · Revisión documental del 19 de noviembre de 2026.

## Integrantes

| Integrante | GitHub |
|---|---|
| Alejandro Villalpando Rojas | Alexwuuu1 |
| Cristopher Iori Lazcano Gutierrez | Crisshubb |
| Galilea Alison Llusco Asistiri | Galileya |
| Alex Joel Quispe Ticona | lpzealexjoelquispeti-dot |

## Repositorio de entrega

La versión integrada se encuentra en `main`. `develop` conserva la integración de las ramas individuales y los autores de sus commits. El repositorio incluye las escenas, scripts, recursos, fuentes editables y configuración de paquetes necesarios para abrir el proyecto. [Repositorio GitHub](https://github.com/Alexwuuu1/backfranz) · [Historial de main](https://github.com/Alexwuuu1/backfranz/commits/main/) · [Créditos](../CREDITOS.md)

## Cumplimiento del avance

| Criterio | Implementación y evidencia |
|---|---|
| Introducción | Diálogos de Yamil sobre su primer día, la inducción y el contexto del Bloque A |
| Tutorial | Cuatro pasos de movimiento, interacción, pausa y llegada a Recepción, con opción de omitir y controles según plataforma |
| Mecánicas principales | Exploración, puertas, registro, control de acceso, ascensor, escaleras, inventario, pistas, puzzles, diario y guardado |
| Narrativa del género | Inducción, cambio a Backfranz, investigación, rescates, coordinación de evacuación y tres finales |
| Recorrido jugable en Unity | 1.310 comprobaciones del flujo y auditoría de 69 mapas y 54.290 puntos transitables |
| Recursos editables | 935 fuentes ASE verificadas y 1.463 fotogramas coincidentes con sus PNG |
| Trabajo en Git | Aportes individuales integrados mediante merges, con commits y autores conservados |
| Créditos | Equipo, procedencia del proyecto y recursos externos documentados en README y CREDITOS.md |
| Ejecutable Windows | Debe generarse y probarse la exportación de esta revisión |
| APK Android | Debe generarse e instalarse en un dispositivo o emulador para probar esta revisión |

Los resultados automatizados corresponden al proyecto en Unity. El registro identifica su alcance y las comprobaciones pendientes de los ejecutables. [Verificación de integración](BloqueA/VERIFICACION-INTEGRACION.txt)

## Descargas y evidencias de ejecución

| Entregable | Enlace de descarga | Evidencia de funcionamiento |
|---|---|---|
| Windows x64, ZIP con la carpeta completa | Pendiente de proporcionar por el equipo | Pendiente de registrar tras ejecutar el build |
| Android, archivo APK | Pendiente de proporcionar por el equipo | Pendiente de registrar tras instalar y abrir el APK |

Una vez subidos los archivos a Google Drive, incorporar sus enlaces aquí y en el PDF. Comprobar que el docente pueda descargar ambos archivos con el acceso previsto para la entrega.

## Documento PDF solicitado

La entrega final debe incluir una carátula formal con el nombre **Backfranz**, **Grupo 64**, los cuatro integrantes y los datos académicos que use el equipo. Incluir el enlace a GitHub y los dos enlaces de descarga de la tabla anterior. Los nombres de la asignatura y del docente se completan con los datos de la materia.

## Generar los ejecutables en otra PC

Abrir `main` con Unity 6000.3.16f1 y esperar la importación. Para Windows usar `Tools → Backfranz → Exportar → Windows x64` y comprimir la carpeta completa de salida. Para Android instalar Android Build Support, SDK, NDK y OpenJDK y usar `Tools → Backfranz → Exportar → Android APK`. El exportador incluye `MenuPrincipal` y `Nivel01` y configura orientación horizontal en Android.

Las pruebas posteriores a exportar deben comprobar que se puede abrir el juego, comenzar una partida, completar la introducción y el tutorial, moverse e interactuar, cambiar de zona, pausar, guardar y continuar. Registrar el dispositivo o equipo utilizado y el resultado de Windows y Android.
