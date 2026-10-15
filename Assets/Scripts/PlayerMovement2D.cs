using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(BoxCollider2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    [SerializeField, Min(0f)] private float speed = 4.5f;
    [SerializeField] private float minimumX = -13.8f;
    [SerializeField] private float maximumX = 13.8f;

    [Header("Animación")]
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField] private Sprite[] walkingFrames;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private float animationTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) { horizontalInput = 0f; return; }
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
            animationTime += Time.deltaTime;
            ShowAnimationFrame();
        }
        else
        {
            animationTime = 0f;
            if (walkingFrames != null && walkingFrames.Length > 0)
            {
                spriteRenderer.sprite = walkingFrames[0];
            }
        }
    }

    private void FixedUpdate()
    {
        float nextX = Mathf.Clamp(
            body.position.x + horizontalInput * speed * Time.fixedDeltaTime,
            minimumX,
            maximumX);

        body.MovePosition(new Vector2(nextX, body.position.y));
    }

    private void ShowAnimationFrame()
    {
        if (walkingFrames == null || walkingFrames.Length == 0)
        {
            return;
        }

        int frame = Mathf.FloorToInt(animationTime * framesPerSecond) % walkingFrames.Length;
        spriteRenderer.sprite = walkingFrames[frame];
    }

#if UNITY_EDITOR
    public void Configure(SpriteRenderer renderer, Sprite[] frames)
    {
        spriteRenderer = renderer;
        walkingFrames = frames;
        if (walkingFrames != null && walkingFrames.Length > 0)
        {
            spriteRenderer.sprite = walkingFrames[0];
        }
    }
#endif
}
