using System;
using System.Collections.Generic;
using Backfranz.Playable;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Backfranz.Playable.CampusWorld;

public static class BackfranzMapAudit
{
    static readonly Vector2Int[] Directions={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};

    public static void Run()
    {
        GameObject root=null;
        int code=0;
        try
        {
            root=new GameObject("Mapa de verificación");
            var world=root.AddComponent<CampusWorld>();
            CheckRosterArt();
            int samples=0;
            var campusProps=new HashSet<string>();
            var alteredProps=new HashSet<string>();
            int checkedRooms=0;
            for(int room=0;room<TotalRooms;room++){
                if(IsFormerReception(room))continue;
                samples+=CheckRoom(world,room);
                foreach(var renderer in world.GetComponentsInChildren<SpriteRenderer>()){
                    if(!renderer.sprite)continue;
                    string asset=renderer.sprite.name;
                    if(asset.StartsWith("utileria-"))campusProps.Add(asset);
                    if(asset.StartsWith("backfranz-utileria-"))alteredProps.Add(asset.Substring("backfranz-".Length));
                }
                checkedRooms++;
            }
            CheckWalkingStudent(world);
            if(campusProps.Count<40||!campusProps.SetEquals(alteredProps))
                throw new InvalidOperationException($"MAP FAIL: utilería incompleta ({campusProps.Count} normal, {alteredProps.Count} alterada).");
            CheckEscortCorner(world);
            Debug.Log($"MAP PASS: {checkedRooms} mapas, {samples} puntos libres, accesos y NPC móvil.");
        }
        catch(Exception error)
        {
            code=1;
            Debug.LogException(error);
        }
        finally
        {
            if(root)UnityEngine.Object.DestroyImmediate(root);
        }
        EditorApplication.Exit(code);
    }

    static void Require(bool passes,int room,string message)
    {
        if(!passes)throw new InvalidOperationException($"MAP FAIL: mapa {room}, {Title(room)} · {message}");
    }

    static void CheckRosterArt()
    {
        var portraits=new HashSet<string>();
        foreach(var person in EscapeArchive.People)
        {
            string art=CampusLook.Person(person.Name,person.Art);
            if((person.Name!="Yamil"&&!art.StartsWith("npc-"))||!portraits.Add(art))
                throw new InvalidOperationException($"Falta un sprite propio para {person.Name}");
            if(CampusLook.Sprite(art).texture.filterMode!=FilterMode.Point)
                throw new InvalidOperationException($"El sprite de {person.Name} pierde su filtro de pixel art");
        }
        foreach(string name in new[]{"Damián","Estudiante de Derecho","Inés","Marcos","Elena"})
        {
            var frames=new HashSet<string>();
            foreach(var direction in Directions)
                for(int i=0;i<4;i++)frames.Add(CampusLook.PersonPose(name,"pb-estudiante",direction,i).name);
            if(frames.Count!=16)throw new InvalidOperationException($"Ciclo de caminar incompleto para {name}");
        }
        Debug.Log($"ART PASS: {portraits.Count} identidades del archivo y cinco ciclos de caminar.");
    }

    static int CheckRoom(CampusWorld world,int room)
    {
        world.Show(room);
        if(room==Ground)CheckSecurity(world);
        if(room==DarkGround){
            var route=world.FindRoute(world.Entry(room),world.GalleryEntrance);
            Require(!world.CanStand(world.Cell(room,38,15)),room,"Sigue abierto el trayecto diurno de los molinetes");
            Require(route.Exists(point=>point.y<=world.Cell(room,0,11).y&&point.x>=world.Cell(room,32,0).x&&
                point.x<=world.Cell(room,41,0).x),room,"La ruta de noche no pasa por el corredor inferior");
        }
        if(room==Basement)CheckBasement(world);
        var origin=world.Offset(room);
        var surface=world.GetComponentInChildren<Tilemap>();
        Require(surface,room,"Falta el mapa de suelo");
        Require(world.GetComponentInChildren<TextMesh>()==null,room,"Quedó un texto flotante en el mapa");
        if(IsInterior(room))CheckInteriorPeople(world,room);
        if(room>Ground&&room<=Top)CheckHallPopulation(world,room);
        if(IsUpperInterior(room)&&UpperInteriorName(room)=="Sala de lectura")
        {
            string prefix=IsDark(room)?"backfranz-mob-":"mob-";
            var sprites=new HashSet<string>();
            foreach(var renderer in world.GetComponentsInChildren<SpriteRenderer>())
                if(renderer.sprite)sprites.Add(renderer.sprite.name);
            foreach(string shelf in new[]{"estante-roble","estante-bajo","estante-acero","estante-exhibidor"})
                Require(sprites.Contains(prefix+shelf),room,$"Falta el mueble {shelf}");
        }
        var open=new HashSet<Vector2Int>();
        for(int y=-20;y<=26;y++)for(int x=-33;x<=94;x++)
        {
            var point=origin+new Vector2(x*.5f,y*.5f);
            if(!world.CanStand(point))continue;
            Require(surface.HasTile(surface.WorldToCell(point)),room,$"Se puede pisar fuera del suelo en {point-origin}");
            open.Add(new Vector2Int(x,y));
        }

        var entry=IsStairwell(room)?world.StairLanding(room,false):IsLiftCabin(room)?
            world.CabinExit(room)+Vector2.up:world.Entry(room);
        var start=Vector2Int.RoundToInt((entry-origin)*2);
        Require(open.Contains(start),room,"El punto de entrada está bloqueado");
        var reached=new HashSet<Vector2Int>{start};
        var queue=new Queue<Vector2Int>();
        queue.Enqueue(start);
        while(queue.Count>0)
        {
            var previous=queue.Dequeue();
            foreach(var direction in Directions)
            {
                var next=previous+direction;
                if(!open.Contains(next)||reached.Contains(next))continue;
                if(!world.HasLineOfSight(origin+(Vector2)previous*.5f,origin+(Vector2)next*.5f))continue;
                reached.Add(next);
                queue.Enqueue(next);
            }
        }

        foreach(var cell in open)
            Require(reached.Contains(cell),room,$"Suelo aislado en {(Vector2)cell*.5f}");

        void Anchor(string label,Vector2 point)
        {
            Require(reached.Contains(Vector2Int.RoundToInt((point-origin)*2)),room,$"Acceso bloqueado a {label}");
        }

        foreach(var stop in world.Stops(room))Anchor(stop.Label,stop.Position);
        if(room==Entrance)
        {
            Anchor("Pie de las gradas",world.StreetFoot);
            Anchor("Rellano",world.StreetLanding);
            Anchor("Entrada del Bloque A",world.StreetDoor);
            Anchor("Regreso desde planta baja",world.StreetReturn);
            Anchor("Estudiante de Landaeta",world.StudentContact);
            Require(world.HasLineOfSight(world.StreetFoot,world.StreetLanding),room,"Los peldaños bloquean la subida central");
            var halfway=world.StreetFoot+Vector2.up;
            Require(!world.HasLineOfSight(world.Cell(room,7,11),halfway),room,"Se puede atravesar el muro del lado de las tiendas");
            Require(!world.HasLineOfSight(world.Cell(room,15,11),halfway),room,"Se puede atravesar el muro del lado del Bloque A");
            Require(!world.HasLineOfSight(world.Cell(room,13,10),world.StreetReturn),room,"Se puede saltar al rellano desde la vereda");
            Require(!world.CanStand(world.StreetDoor+Vector2.right),room,"Se puede pasar por dentro de la fachada");
            Require(!world.HasLineOfSight(world.Cell(room,20,4),world.Cell(room,20,8)),room,"Se puede cruzar por debajo de un auto");
            Require(!world.HasLineOfSight(world.Cell(room,31,4),world.Cell(room,31,8)),room,"Se puede cruzar por debajo de la furgoneta");
        }
        else if(room==Basement)
        {
            Anchor("Ascensor",world.Lift(room));
            Anchor("Fusible",world.Fuse);
            Anchor("Cuadro eléctrico",world.PowerPanel);
        }
        else if(room==ServiceGallery)
        {
            Anchor("Salida",world.GalleryExit);
            for(int i=0;i<world.GalleryLightCount;i++)Anchor("Luz de refugio",world.GalleryLight(i));
        }
        else if(IsStairwell(room))Anchor("Rellano superior",world.StairLanding(room,true));
        else if(IsLiftCabin(room))
        {
            Anchor("Salida",world.CabinExit(room));
            Anchor("Panel del ascensor",world.CabinPanel(room));
        }
        else if(IsUpperInterior(room))Anchor("Salida",world.UpperInteriorExit(room));
        else if(IsInterior(room))Anchor("Salida",world.InteriorExit(room));
        else
        {
            Anchor("Ascensor",world.Lift(room));
            Anchor("Llegada del ascensor",world.Lift(room)+Vector2.down*2);
            if(!IsDark(room)||IsGroundHall(room))
                for(int i=0;i<StairwellCount;i++)Anchor("Gradas",world.Stairs(room,i));
            if(IsGroundHall(room))
            {
                for(int i=FirstGroundDoor;i<GroundDoors.Length;i++)
                {
                    var door=world.DoorPoint(room,i);
                    Anchor(GroundDoors[i].Name,door);
                    Anchor("Regreso de "+GroundDoors[i].Name,door+Vector2.up*(GroundDoors[i].South?1:-1));
                }
                Anchor("Salida",world.Exit(room));
                if(room==Ground)
                {
                    Anchor("Ventanilla",world.ReceptionContact);
                    Anchor("Guardia",world.GuardContact);
                }
                else Anchor("Galería de servicio",world.GalleryEntrance);
            }
            foreach(int interior in world.UpperInteriorsOn(room))
                Anchor(Title(interior),world.UpperInteriorDoor(interior));
        }
        return open.Count;
    }

    static void CheckSecurity(CampusWorld world)
    {
        Require(world.FindRoute(world.Entry(Ground),world.Lift(Ground)).Count==0,Ground,
            "Hay una ruta al ascensor sin cruzar el control");
        for(int lane=0;lane<AccessLaneCount;lane++)
        {
            var exterior=world.SecurityPoint(lane,true);
            var interior=world.SecurityPoint(lane,false);
            Require(!world.HasLineOfSight(exterior,interior),Ground,$"El control cerrado deja pasar por el carril {lane}");
            world.ShowSecurityOpening(lane,1);
            Require(world.HasLineOfSight(exterior,interior),Ground,$"El carril {lane} sigue bloqueado al abrir");
        }
        Require(!world.CanStand(world.ReceptionContact+Vector2.up),Ground,"Se puede atravesar la ventanilla");
    }

    static void CheckInteriorPeople(CampusWorld world,int room)
    {
        var students=new List<SpriteRenderer>();
        foreach(var renderer in world.GetComponentsInChildren<SpriteRenderer>())
            if(renderer.sprite&&renderer.sprite.name.StartsWith("estudiante-campus-"))students.Add(renderer);
        bool privateSpace=IsUpperInterior(room)&&UpperInteriorName(room)=="Baños";
        if(IsDark(room)||privateSpace){
            Require(students.Count==0,room,"Apareció población regular en un espacio reservado");
            return;
        }
        Require(students.Count>=(IsUpperInterior(room)?2:4),room,"El interior quedó vacío");
        var before=students.ConvertAll(student=>student.transform.position);
        for(int i=0;i<5;i++)world.UpdatePeople(room,world.Entry(room),.2f,0);
        Require(students.Exists(student=>!before.Contains(student.transform.position)),room,"Los estudiantes no se mueven");
    }

    static void CheckHallPopulation(CampusWorld world,int room)
    {
        string[] names={"Jimena","Nadia","Bruno","Abril","Sergio","Rebeca"};
        string expected=names[Floor(room)-1];
        int students=0;
        bool namedStudent=false;
        foreach(var renderer in world.GetComponentsInChildren<SpriteRenderer>()){
            if(!renderer.sprite||!renderer.sprite.name.StartsWith("estudiante-campus-"))continue;
            students++;
            if(renderer.name==expected)namedStudent=true;
        }
        Require(students>=4,room,"Faltan estudiantes en el pasillo");
        Require(namedStudent,room,$"No apareció {expected} en su ruta");
    }

    static void CheckBasement(CampusWorld world)
    {
        Require(world.FindRoute(world.Entry(Basement),world.Lift(Basement)).Count==0,Basement,
            "La reja deja cruzar antes de restablecer la energía");
        world.RestorePower();
    }

    static void CheckWalkingStudent(CampusWorld world)
    {
        world.Show(Ground);
        var from=world.Cell(Ground,19,17);
        var to=from+Vector2.right;
        Require(world.CanPlayerStep(from,from+Vector2.right*.08f),Ground,"Yamil queda atrapado si coincide con un estudiante");
        Require(!world.CanPlayerStep(from+Vector2.right*.46f,from+Vector2.right*.38f),Ground,"Yamil atraviesa al estudiante al acercarse");
        var midpoint=Vector2.Lerp(from,to,.5f);
        world.UpdatePeople(Ground,midpoint,.5f,0);
        Require(!world.CanStand(from),Ground,"El estudiante se acerca al jugador cuando el pasillo está ocupado");
        world.UpdatePeople(Ground,world.Entry(Ground),.5f,.5f);
        world.UpdatePeople(Ground,world.Entry(Ground),.5f,1);
        Require(world.CanStand(from)&&world.CanStand(to)&&!world.CanStand(midpoint),Ground,
            "El estudiante no ocupa el tramo intermedio de la ruta de prueba");
        var detour=world.FindRoute(from,to);
        Require(detour.Count>1,Ground,"La ruta atraviesa al estudiante o se queda sin desvío");
        foreach(var point in detour)
        {
            Require(world.HasLineOfSight(from,point),Ground,"Un tramo de la ruta atraviesa una colisión");
            from=point;
        }
    }

    static void CheckEscortCorner(CampusWorld world)
    {
        int room=ForFloor(4,true);
        world.Show(room);
        var from=world.Offset(room)+new Vector2(6,-2.4f);
        var refuge=world.Cell(room,4,5)+Vector2.right*.04f;
        Require(!world.HasLineOfSight(from,refuge),room,"El recorrido directo corta la esquina de un banco");
        var path=world.FindRoute(from,refuge);
        Require(path.Count>1&&Vector2.Distance(path[path.Count-1],refuge)<.05f,room,
            "El acompañante no encuentra el camino al refugio");
    }
}
