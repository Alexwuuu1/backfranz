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
        readonly Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
        public Vector2 TouchInput {
            get { Vector2 axis = Vector2.zero; foreach (var direction in touches.Values) axis += direction; return Vector2.ClampMagnitude(axis, 1); }
        }
        float animationTime;
        SpriteRenderer view;
        public Vector2 Facing { get; private set; } = Vector2.down;
        public Vector2 Position => transform.position;
        void Start() { view = GetComponent<SpriteRenderer>(); }
        public void PressDirection(int pointerId, Vector2 value) { touches[pointerId] = value; }
        public void ReleaseDirection(int pointerId) { touches.Remove(pointerId); }
        public void Interact() { if (Time.timeScale > 0 && !session.Finished) session.Interact(); }
        void Update()
        {
            if (Time.timeScale == 0 || session.Finished) { touches.Clear(); return; }
            var axis = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + TouchInput, 1);
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)) Interact();
            Move(axis, Time.deltaTime);
            if (axis.sqrMagnitude > .01f) Facing = axis;
            animationTime += axis.magnitude * Time.deltaTime;
            view.sprite = CampusLook.Student(Facing, axis.sqrMagnitude > .01f ? (int)(animationTime * 8) % 4 : 0);
            view.sortingOrder = CampusLook.Depth(transform.position.y);
        }
        public void Move(Vector2 axis, float delta)
        {
            // Substeps also prevent tunnelling at low frame rates on mobile.
            Vector2 movement = Vector2.ClampMagnitude(axis, 1) * (Input.GetKey(KeyCode.LeftShift) ? 4.5f : 3.2f) * Mathf.Min(delta, .1f);
            int steps = Mathf.Max(1, Mathf.CeilToInt(movement.magnitude / .08f));
            for (int i = 0; i < steps; i++) {
                Vector2 p = transform.position;
                if (world.CanStand(p + new Vector2(movement.x / steps, 0))) p.x += movement.x / steps;
                if (world.CanStand(p + new Vector2(0, movement.y / steps))) p.y += movement.y / steps;
                transform.position = p;
            }
        }
        void OnApplicationFocus(bool focused) { if (!focused) touches.Clear(); }
        void OnDisable() { touches.Clear(); }
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
