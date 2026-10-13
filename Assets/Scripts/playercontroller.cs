using UnityEngine;

/// <summary>
/// Player character: run with A/D or arrows, jump with Space/W/Up.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpVelocity = 10.8f;
    public float gravityScale = 2.5f;
    public float maxFallSpeed = 20f;

    /// <summary>Включен ли ввод игрока (ЛР №4, вариант 8): false отключает управление.</summary>
    public bool controlEnabled = true;

    Rigidbody2D rb;
    bool grounded;

    /// <summary>
    /// Применяет горизонтальный ввод: axis -1 (влево), +1 (вправо), 0 (стоим).
    /// При controlEnabled = false ввод игнорируется. Выделено в метод,
    /// чтобы PlayMode-тесты могли проверять реакцию на ввод без симуляции клавиш.
    /// </summary>
    public void ApplyInput(float axis)
    {
        if (!controlEnabled) axis = 0f;
        rb.velocity = new Vector2(axis * moveSpeed, rb.velocity.y);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (GameManager.Won)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float axis = 0f;
        if (controlEnabled)
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) axis -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) axis += 1f;
        }
        ApplyInput(axis);

        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);

        grounded = GroundCheck();
        bool jumpHeld = controlEnabled &&
            (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        if (jumpHeld && grounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            grounded = false;
        }

        // Fell out of the world -> respawn
        if (transform.position.y < -8f)
            GameManager.Respawn(this);
    }

    bool GroundCheck()
    {
        // Cast a small box just under the feet (capsule spans [-0.7, 0.8] around the pivot)
        Vector2 origin = (Vector2)transform.position + new Vector2(0f, -0.62f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(0.6f, 0.1f), 0f, Vector2.down, 0.15f);
        return hit.collider != null && rb.velocity.y <= 0.01f;
    }
}
