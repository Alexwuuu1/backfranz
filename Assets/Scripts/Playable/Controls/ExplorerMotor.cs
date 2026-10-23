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
        float animationTime;
        SpriteRenderer view;
        public Vector2 Facing { get; private set; } = Vector2.down;
        public Vector2 Position => transform.position;
        void Start() { view = GetComponent<SpriteRenderer>(); }
        public void TouchAxis(Vector2 value) { touchAxis = value; }
        public void Interact() { if (Time.timeScale > 0 && !session.Finished) session.Interact(); }
        void Update()
        {
            if (Time.timeScale == 0 || session.Finished) { touchAxis = Vector2.zero; return; }
            var axis = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + touchAxis, 1);
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
        void OnApplicationFocus(bool focused) { if (!focused) touchAxis = Vector2.zero; }
        void OnDisable() { touchAxis = Vector2.zero; }
    }

    public sealed class TouchDirection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ExplorerMotor motor;
        public Vector2 direction;
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData data) { pointer = data.pointerId; motor.TouchAxis(direction); }
        public void OnPointerUp(PointerEventData data) { if (pointer == data.pointerId) { motor.TouchAxis(Vector2.zero); pointer = int.MinValue; } }
        void OnDisable() { if (motor) motor.TouchAxis(Vector2.zero); }
    }
}
