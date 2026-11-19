using System;
using UnityEngine;

namespace Backfranz.Playable
{
    public static class EscapeArchive
    {
        public enum Ending { None, Amanecer, Eco, Umbral }
        static string key=>Application.isBatchMode&&Array.Exists(Environment.GetCommandLineArgs(),
            arg=>arg=="-backfranzSmokeSave")?"Backfranz.Smoke.Endings":"Backfranz.Endings";

        public static string Title(Ending ending)=>ending switch
        {
            Ending.Amanecer=>"Hasta el amanecer",
            Ending.Eco=>"El último eco",
            Ending.Umbral=>"Del otro lado",
            _=>"Desenlace"
        };

        public static string Story(Ending ending)=>ending switch
        {
            Ending.Amanecer=>"El grupo llega a Landaeta. Paula pasa lista mientras Camila apaga la radio por primera vez en toda la noche.",
            Ending.Eco=>"Yamil sale con la grabación. Antes de apagarla, oye una voz pronunciar su nombre desde el piso que acaba de dejar.",
            Ending.Umbral=>"Yamil toma la salida estrecha. En la calle amanece, pero detrás de la puerta siguen contestando por radio.",
            _=>""
        };

        public static string Cover(Ending ending)=>"final-"+ending.ToString().ToLowerInvariant();
        public static bool Unlocked(Ending ending)=>ending!=Ending.None&&(PlayerPrefs.GetInt(key)&(1<<(int)ending))!=0;
        public static void Unlock(Ending ending)
        {
            if(ending==Ending.None)return;
            PlayerPrefs.SetInt(key,PlayerPrefs.GetInt(key)|(1<<(int)ending));
            PlayerPrefs.Save();
        }

        public static readonly (string Name,string Art,string Bio)[] People=
        {
            ("Yamil","yamil-00","Llegó al Bloque A para su primer día de inducción. Empezó a anotar el recorrido cuando los números del ascensor dejaron de corresponder con los pasillos."),
            ("Camila","pb-recepcionista","Conoció el edificio llevando expedientes de Dirección de Carrera. Aprendió a describir una ruta sin ver a la persona que la sigue."),
            ("Iván","pb-estudiante","Ayudaba a guardar el instrumental después de las prácticas. En Histología conserva los guantes y las herramientas que usará para sostener el paso."),
            ("Paula","pb-recepcionista","En el aula de grupos ensayaba el protocolo de primeros auxilios. En el apagón encontró una utilidad inesperada para la lista de asistencia."),
            ("Dalia","pb-recepcionista","Solía esperar a sus compañeros en el café. Se queda allí al cortarse la luz y reconoce a los que vuelven por el ruido de la puerta."),
            ("Lázaro","pb-estudiante","Estudiante de Fotografía. Aprendió que el visor a veces muestra un pasillo anterior al que tiene delante. Busca una linterna para poder salir sin mirar por él."),
            ("Nuria","pb-recepcionista","Ensayaba en el auditorio cuando fallaron las luces. Se lastimó el pie en las gradas y desde el escenario ha estado indicando la salida a otros."),
            ("Lina","pb-estudiante","En Streaming guardaba muestras antes de borrar los directos. Una de esas grabaciones conservó el patrón azul del estudio."),
            ("Diego","pb-estudiante","En Fotografía medía las exposiciones con un cronómetro. Esa costumbre le permitió reconocer cómo se duplicaban las ráfagas de la pista ámbar."),
            ("Hugo","pb-estudiante","Se refugió en Sala Mac y anotó los ritmos que se filtraban por los auriculares. La pista violeta es la última que pudo transcribir."),
            ("Iris","pb-recepcionista","Guardó una copia del registro de llaves en Caja. Recuerda los horarios, aunque la columna con los nombres quedó borrada."),
            ("Renata","pb-recepcionista","En Sala Jurídica anotó el orden de los retiros. Sus apuntes completan la parte del registro que Iris perdió."),
            ("Andrés","pb-estudiante","Conoce las conducciones de la cocina de prácticas. Siguió el vapor hasta que las tuberías se enfriaron."),
            ("Mateo","pb-estudiante","En Sala Mac etiquetó los retornos de la instalación cuando fallaron los medidores. Su indicación distingue una tubería activa de una fría."),
            ("Eva","pb-recepcionista","Verificaba los accesos del taller. Anotó qué ruta quedó bloqueada antes de que el panel perdiera la alimentación."),
            ("Alma","pb-recepcionista","En la sala de lectura conservó el esquema que usaban en el simulacro. Pudo comprobar qué estación dejó de responder."),
            ("Vera","pb-recepcionista","Se ocupa del cierre de Admisiones. Antes del apagón, anotó la hora de la última ficha y la comparó con el aviso que llegó desde Simulación."),
            ("Mónica","pb-recepcionista","Recibe los partes de los laboratorios en Dirección de Carrera. El del taller fue el primero que llevó la firma de alguien que ya había salido."),
            ("Sara","pb-estudiante","Repasa frente al ventanal de Aula 4A. El reflejo de Fotografía le permitió seguir contando el tiempo cuando el reloj de pared se detuvo."),
            ("Raúl","pb-estudiante","Estudia en la sala de lectura hasta que cierran. Antes hacía turnos de apoyo en recepción y reconoce el pitido del último molinete."),
            ("Mara","pb-recepcionista","Revisa las luces de emergencia antes del cierre. Cuando la planta perdió energía comprobó que unas pocas lámparas seguían conectadas a la batería de la galería."),
            ("Felipe","pb-estudiante","Se ofreció a llevar material al depósito y conoce el recorrido de los tubos. Espera junto a la luz del retorno desde que oyó su propio paso adelantarse por el corredor."),
            ("Celia","pb-estudiante","Ayuda a inventariar herramientas en el turno de la tarde. Tiene la costumbre de revisar dos veces cada cerrojo, incluso el que se abre hacia un pasillo que no estaba allí al llegar."),
            ("Inés","pb-estudiante","Lleva los formularios de Posgrado a dirección al final de cada turno. Aprendió a orientarse por el ruido de las puertas del ascensor hasta el día en que sonaron a la vez."),
            ("Marcos","pb-estudiante","Practica observación en Cámara Gesell. Desde el apagón prefiere caminar acompañado y se detiene cuando el vidrio refleja algo que todavía no ha pasado."),
            ("Elena","pb-estudiante","Se quedó en el aula de grupos para ordenar los materiales. Conoce las luces de emergencia porque antes ayudaba en los simulacros de evacuación."),
            ("Bibliotecaria","pb-recepcionista","Custodia el índice de evacuación. Cuando se altera el campus sigue clasificando los planos para que alguien pueda encontrarlos."),
            ("Guardia","pb-guardia","Vigila el acceso antes de los molinetes. Sabe a quién ha visto entrar y cuándo tiene que abrir el paso ancho."),
            ("Recepcionista","pb-recepcionista","Entrega los horarios y credenciales en la ventanilla del Bloque A. Recibe a Yamil en su primer día."),
            ("Mauro","pb-estudiante","Ensayaba en el auditorio la noche del apagón. Al quedarse solo oyó cómo el telón se abría sin que nadie tirara de él."),
            ("Rita","pb-recepcionista","Contó siete tazas intactas en el café y esperó a que alguien volviera a reclamarlas."),
            ("Nico","pb-estudiante","Probó los mandos del Game Room contra el sistema del portón antes de darse cuenta de que faltaba un mando mecánico."),
            ("Elías","pb-guardia","Conoce el cuadro de Financiero y recuerda por qué no conviene conectar todas las líneas juntas."),
            ("Mariela","pb-recepcionista","En Servicios empezó a registrar quién había bajado. Conserva testimonios cuando ya no puede fiarse de la apariencia del pasillo."),
            ("Lucía","pb-estudiante","Consultó su nombre en la lista de Admisiones después de que el lector se apagara. Solo quedó el de Yamil."),
            ("Samuel","pb-estudiante","En Posgrado anotó la hora local del cierre. Dejó esa referencia para quien tuviera que revisar el archivo."),
            ("Irene","pb-recepcionista","Supervisaba material en Simulación. Se aseguró de que al menos una muestra siguiera sellada."),
            ("David","pb-estudiante","Alcanzó a oír la radio del Laboratorio PC durante el breve regreso de la energía."),
            ("Belén","pb-estudiante","Escuchó al técnico contando la carga de cada línea hasta que saltaron las luces."),
            ("Pablo","pb-estudiante","En Fotografía descubrió que el visor podía mostrar un número de piso distinto del que marcaba el ascensor."),
            ("Clara","pb-recepcionista","En Aula 4A agrupó las tres horas válidas del registro de la observación."),
            ("Valeria","pb-estudiante","En el Taller guardó la plantilla de la prensa y señaló de qué borde había que empezar a leer."),
            ("Joel","pb-estudiante","Se quedó cerca de los planos en la sala de lectura. Conoce a quien puede interpretar los símbolos del portón."),
            ("Noelia","pb-recepcionista","Vio que los indicadores de Dirección dejaron de responder cuando cayó la presión del subsuelo."),
            ("Omar","pb-estudiante","Ensayó en Streaming cómo revertir los cambios de un selector hasta recuperar el patrón anterior."),
            ("Sofía","pb-estudiante","En el aula de grupos comparó los selectores de los extremos con los del centro."),
            ("Mica","pb-estudiante","Oyó la alarma por las tuberías de los baños. Espera que la sirena vuelva a orientar a quienes siguen ocultos."),
            ("Estudiante","pb-estudiante","Conoce la subida desde Landaeta y señala la ventanilla a quienes llegan por primera vez. Otros hacen fila o esperan junto al Game Room."),
            ("Personal administrativo","pb-recepcionista","Orienta a quienes llegan al piso de servicios y suele derivar a los estudiantes a Caja o Posgrado al inicio de semestre."),
            ("Profesora","pb-recepcionista","Durante la inducción recuerda dónde consultar el horario. En el campus alterado distingue la muestra que se mantuvo intacta."),
            ("Estudiante de Derecho","pb-estudiante","Va y viene entre el pasillo y la Sala Jurídica para practicar su intervención. Se quedó repasándola hasta tarde."),
            ("Personal de cafetería","pb-recepcionista","Atiende el café y ofrece un asiento a quienes recién llegan. Conoce a los estudiantes por sus pedidos habituales."),
            ("Estudiante de audiovisual","pb-estudiante","Escuchó el aviso de emergencia desde una consola. Retuvo el orden de la grabación a pesar de las interferencias."),
            ("Técnico","pb-guardia","Trabajaba con el tablero de respaldo. Conservó las cargas de las tres líneas cuando las etiquetas dejaron de iluminarse."),
            ("Delegada","pb-estudiante","Llevó el registro de ingreso y observación. Puede reconstruir el cierre a partir de los intervalos que anotó."),
            ("Encargada de cocina","pb-recepcionista","Está acostumbrada a reconocer útiles por sus marcas. Explica cómo orientar la plantilla que llegó desde el taller."),
            ("Aylin","pb-estudiante","Espera a sus compañeros junto al Game Room y recomienda la biblioteca si buscas un lugar tranquilo."),
            ("Damián","pb-estudiante","Revisa el horario mientras recorre la planta baja. También es su primer día."),
            ("Docente de Computación","pb-recepcionista","Orienta al grupo de primer semestre entre las salas de computación y los estudios."),
            ("Docente de Derecho","pb-recepcionista","Supervisa las prácticas y ayuda a ubicar las aulas del cuarto piso."),
            ("Docente del Taller","pb-recepcionista","Prepara las mesas de trabajo y recuerda al grupo dónde consultar el horario.")
        };
    }
}
