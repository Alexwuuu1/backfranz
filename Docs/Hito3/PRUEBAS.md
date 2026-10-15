# Verificación de Hito 3

Abrir con Unity 6000.3.16f1. La escena inicial es Assets/Scenes/MenuPrincipal.unity.
Si las escenas no están generadas: Tools > Backfranz > Preparar menú y pausa. Este comando reconstruye el menú y reemplaza solo la UI de Backfranz en Nivel01; guardar trabajo antes de ejecutarlo.

## Prueba manual del equipo
- [ ] Menú diurno legible; se transforma después de 5 segundos.
- [ ] Ratón y flechas + Enter activan botones.
- [ ] Opciones controla volumen y conserva preferencias al reiniciar.
- [ ] Extras abre la galería y Créditos muestra los integrantes.
- [ ] Jugar abre Nivel01; A/D mueve al estudiante del prototipo.
- [ ] Escape o Pausa detiene movimiento y animación.
- [ ] Opciones durante pausa funciona sin reanudar el juego.
- [ ] Continuar restaura movimiento; Volver al menú permite jugar otra vez.
- [ ] Cancelar salida mantiene la pantalla; Salir detiene Play.
- [ ] En una compilación Windows, pantalla completa y Salir funcionan.
- [ ] Revisar 1280x720, 1600x900 y 1920x1080 sin textos cortados.

BackfranzSmoke.Run ejecuta una comprobación automática de escenas y estados; su resultado se registra en VERIFICACION.txt si termina correctamente. No sustituye las casillas manuales.
Las capturas de Docs/Hito3/Capturas se obtienen de Unity; no confundir los fondos de arte con evidencia de ejecución.
