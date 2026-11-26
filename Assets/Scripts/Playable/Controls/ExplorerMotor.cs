using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Backfranz.Playable
{
    // Keyboard and multitouch share the same normalized movement path.
    public sealed class ExplorerMotor : MonoBehaviour
    {
        public CampusWorld world;
        public SessionFlow session;
        Vector2 touchAxis;
        readonly Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
        public Vector2 TouchInput
        {
            get
            {
                Vector2 axis = touchAxis;
                foreach (var direction in touches.Values) axis += direction;
                return Vector2.ClampMagnitude(axis, 1);
            }
        }
        const float ReachDuration=.30f;
        float strideDistance;
        float stepDistance;
        float reachStarted=-1;
        SpriteRenderer view;
        public Vector2 Facing { get; private set; } = Vector2.down;
        public Vector2 Position => transform.position;
        void Start() { view = GetComponent<SpriteRenderer>(); }
        public void TouchAxis(Vector2 value)
        {
            touchAxis = value;
            if (value == Vector2.zero) touches.Clear();
        }
        public void PressDirection(int pointerId, Vector2 value) { touches[pointerId] = value; }
        public void ReleaseDirection(int pointerId) { touches.Remove(pointerId); }
        internal void Face(Vector2 direction)
        {
            if(direction.sqrMagnitude<.001f)return;
            Facing=direction.normalized;
            if(view)view.sprite=CampusLook.Student(Facing,0);
        }

        internal void Reach(Vector2 direction)
        {
            if(direction.sqrMagnitude>.001f)Face(direction);
            reachStarted=Time.unscaledTime;
            if(view)view.sprite=CampusLook.YamilReach(Facing,1);
        }

        bool Reaching => reachStarted>=0&&Time.unscaledTime-reachStarted<ReachDuration;

        void Pose(bool moving)
        {
            if(!view)return;
            if(Reaching){
                float age=Time.unscaledTime-reachStarted;
                int frame=age<.08f?1:age<.22f?2:3;
                view.sprite=CampusLook.YamilReach(Facing,frame);
            }else{
                reachStarted=-1;
                view.sprite=moving?CampusLook.YamilWalk(Facing,(int)(strideDistance*4.5f)%6):CampusLook.Student(Facing,0);
            }
            view.sortingOrder=CampusLook.Depth(transform.position.y);
        }
        public void Interact() { if (Time.timeScale > 0 && !session.Finished) session.Interact(); }
        void Update()
        {
            if (Time.timeScale == 0 || session.Finished || session.Crossing) { TouchAxis(Vector2.zero); Pose(false); return; }
            bool wasTalking = session.DialogueOpen;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) ||
                wasTalking && Input.GetKeyDown(KeyCode.Return)) Interact();
            if (wasTalking || session.DialogueOpen || session.Crossing || Time.timeScale==0) { Pose(false); return; }
            var axis = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + TouchInput, 1);
            Vector2 previous = Position;
            Move(axis, Time.deltaTime);
            float moved = Vector2.Distance(previous,Position);
            if(moved>0 && moved<=.46f){
                stepDistance+=moved;
                if(stepDistance>=1.05f)
                {
                    session.Sound.Footstep(world.Footing(session.Room,Position),Input.GetKey(KeyCode.LeftShift));
                    stepDistance-=1.05f;
                }
            }
        }
        public void Move(Vector2 axis, float delta)
        {
            if (session.DialogueOpen || session.Crossing) return;
            // Substeps also prevent tunnelling at low frame rates on mobile.
            bool sprinting = Input.GetKey(KeyCode.LeftShift);
            var start=Position;
            Vector2 movement = Vector2.ClampMagnitude(axis, 1) * (sprinting ? 4.5f : 3.2f) * Mathf.Min(delta, .1f);
            int steps = Mathf.Max(1, Mathf.CeilToInt(movement.magnitude / .08f));
            for (int i = 0; i < steps; i++) {
                Vector2 from = transform.position;
                Vector2 p = from;
                var horizontal=p+new Vector2(movement.x/steps,0);
                if(world.CanPlayerStep(p,horizontal))p=horizontal;
                var vertical=p+new Vector2(0,movement.y/steps);
                if(world.CanPlayerStep(p,vertical))p=vertical;
                transform.position = p;
                if(p!=from)session.ObserveFirstNightMovement(p);
                if(session.ObserveClassroomLoop(from,p)){Pose(false);return;}
                session.ObserveAcousticMovement(p);
                if(session.ObserveGalleryMovement(from,p,sprinting)){Pose(false);return;}
            }
            Vector2 travelled=Position-start;
            if(travelled.sqrMagnitude>.000001f){
                reachStarted=-1;
                // Keep the facing stable when two cardinal inputs are held together.
                if(Mathf.Abs(Mathf.Abs(travelled.x)-Mathf.Abs(travelled.y))>.004f||Mathf.Abs(axis.x)<.01f||Mathf.Abs(axis.y)<.01f)
                    Facing=travelled.normalized;
                strideDistance+=travelled.magnitude;
                Pose(true);
            }else{
                if(!Reaching&&axis.sqrMagnitude>.01f)Facing=axis.normalized;
                Pose(false);
            }
        }
        void OnApplicationFocus(bool focused) { if (!focused) TouchAxis(Vector2.zero); }
        void OnDisable() { TouchAxis(Vector2.zero); }
    }

    public sealed class TouchDirection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ExplorerMotor motor;
        public Vector2 direction;
        readonly HashSet<int> pointers = new HashSet<int>();
        public void OnPointerDown(PointerEventData data) { pointers.Add(data.pointerId); motor.PressDirection(data.pointerId, direction); }
        public void OnPointerUp(PointerEventData data) { if (pointers.Remove(data.pointerId)) motor.ReleaseDirection(data.pointerId); }
        void OnDisable() { if (motor) foreach (int pointer in pointers) motor.ReleaseDirection(pointer); pointers.Clear(); }
    }
}
