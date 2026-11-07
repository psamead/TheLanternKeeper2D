using UnityEngine;
using UnityEngine.InputSystem;

// Maela movement for the Home demo: A/D or arrows to move, Space/W/Up to jump, E = raise lantern.
// Drives Maela.controller parameters: Speed, Grounded, VelY, RaiseLantern.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")] public float moveSpeed = 3.2f;
    [Header("Jump")] public float jumpVelocity = 6.5f;
    public float coyoteTime = 0.08f, jumpBuffer = 0.1f;
    [Header("Ground check")] public LayerMask groundMask = ~0;
    public Vector2 groundBoxSize = new Vector2(0.3f, 0.06f);
    [Header("Refs")] public Animator animator;   // on the Maela visual child
    public Transform visual;                     // child flipped by scale.x

    Rigidbody2D rb; float lastGrounded = -1, lastJumpPress = -1; bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visual && animator) visual = animator.transform;
    }

    void Update()
    {
        var k = Keyboard.current; if (k == null) return;
        float x = 0;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1;
        if (k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) lastJumpPress = Time.time;
        if (k.eKey.wasPressedThisFrame && animator) animator.SetTrigger("RaiseLantern");

        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
        if (x != 0 && visual) { var s = visual.localScale; s.x = Mathf.Abs(s.x) * Mathf.Sign(x); visual.localScale = s; }

        if (Time.time - lastJumpPress < jumpBuffer && Time.time - lastGrounded < coyoteTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            lastJumpPress = -1; lastGrounded = -1;
        }
        // shorter hop when jump released early
        if (rb.linearVelocity.y > 0 && !(k.spaceKey.isPressed || k.wKey.isPressed || k.upArrowKey.isPressed))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * rb.gravityScale * Time.deltaTime;

        if (animator)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetFloat("VelY", rb.linearVelocity.y);
            animator.SetBool("Grounded", grounded);
        }
    }

    void FixedUpdate()
    {
        grounded = false;
        if (rb.linearVelocity.y <= 0.01f)
            foreach (var c in Physics2D.OverlapBoxAll((Vector2)transform.position, groundBoxSize, 0, groundMask))
                if (c.attachedRigidbody != rb && !c.isTrigger) { grounded = true; break; }
        if (grounded) lastGrounded = Time.time;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, groundBoxSize);
    }
}
