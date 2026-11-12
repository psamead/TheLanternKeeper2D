using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// Maela movement for the Home demo: A/D or arrows to move, Shift to walk, Space/W/Up to jump, E = raise lantern.
// Drives Maela.controller parameters: Speed, Grounded, VelY, RaiseLantern.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [FormerlySerializedAs("moveSpeed")] public float runSpeed = 3.2f;
    public float walkSpeed = 0.8f;
    [Header("Jump")] public float jumpVelocity = 6.5f;
    [Header("Ladder")] public float ladderSpeed = 2.1f;
    public float coyoteTime = 0.08f, jumpBuffer = 0.1f;
    [Header("Ground check")] public LayerMask groundMask = ~0;
    public Vector2 groundBoxSize = new Vector2(0.3f, 0.06f);
    [Header("Refs")] public Animator animator;   // on the Maela visual child
    public Transform visual;                     // child flipped by scale.x

    Rigidbody2D rb;
    MaelaAudio maelaAudio;
    float lastGrounded = -1, lastJumpPress = -1, lanternActionEndsAt;
    bool grounded, lanternActionActive, lockAllDuringLantern;
    LadderClimbZone nearbyLadder;
    bool climbing;
    float normalGravityScale = 1f;

    public event System.Action OnJump;
    public event System.Action OnLanternRaised;
    public bool IsGrounded => grounded;
    public bool IsWalking { get; private set; }
    public bool LanternLocked => lanternActionActive && lockAllDuringLantern;
    public bool IsClimbing => climbing;
    public bool ControlsLocked { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        normalGravityScale = rb.gravityScale;
        maelaAudio = GetComponent<MaelaAudio>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visual && animator) visual = animator.transform;
    }

    void Update()
    {
        if (lanternActionActive && Time.time >= lanternActionEndsAt)
            CompleteLanternAction();

        var k = Keyboard.current;
        if (!lanternActionActive && LanternSwitch.Instance == null && k != null && k.eKey.wasPressedThisFrame)
            RaiseLantern(true);

        if (ControlsLocked)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("VelY", 0f);
            }
            return;
        }

        if (climbing && nearbyLadder != null && nearbyLadder.IsClimbable)
        {
            if (k != null && k.spaceKey.wasPressedThisFrame)
            {
                ExitLadder(true);
            }
            else
            {
                float climb = 0f;
                if (k != null && (k.wKey.isPressed || k.upArrowKey.isPressed)) climb += 1f;
                if (k != null && (k.sKey.isPressed || k.downArrowKey.isPressed)) climb -= 1f;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(0f, climb * ladderSpeed);
                grounded = false;
                if (animator)
                {
                    animator.SetFloat("Speed", 0f);
                    animator.SetFloat("VelY", climb * ladderSpeed);
                    animator.SetBool("Grounded", false);
                }
                return;
            }
        }

        float x = 0;
        if (!LanternLocked && k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1;
            if (k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame)
                lastJumpPress = Time.time;
        }

        bool slowWalk = !LanternLocked && k != null && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed);
        IsWalking = !LanternLocked && slowWalk && x != 0;
        float horizontalSpeed = slowWalk ? walkSpeed : runSpeed;
        rb.linearVelocity = new Vector2(x * horizontalSpeed, rb.linearVelocity.y);
        if (x != 0 && visual) { var s = visual.localScale; s.x = Mathf.Abs(s.x) * Mathf.Sign(x); visual.localScale = s; }

        if (!LanternLocked && Time.time - lastJumpPress < jumpBuffer && Time.time - lastGrounded < coyoteTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            lastJumpPress = -1; lastGrounded = -1;
            OnJump?.Invoke();
        }
        // shorter hop when jump released early
        if (!LanternLocked && k != null && rb.linearVelocity.y > 0 && !(k.spaceKey.isPressed || k.wKey.isPressed || k.upArrowKey.isPressed))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * rb.gravityScale * Time.deltaTime;

        if (animator)
        {
            animator.SetFloat("Speed", LanternLocked ? 0f : Mathf.Abs(rb.linearVelocity.x));
            animator.SetFloat("VelY", rb.linearVelocity.y);
            animator.SetBool("Grounded", grounded);
        }
    }

    public void EnterLadder(LadderClimbZone ladder)
    {
        if (ladder == null || !ladder.IsClimbable) return;
        nearbyLadder = ladder;
        climbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public void LeaveLadder(LadderClimbZone ladder)
    {
        if (nearbyLadder != ladder) return;
        if (climbing) ExitLadder(false);
        nearbyLadder = null;
    }

    public void DropToGroundSafely()
    {
        ExitLadder(false);
        var hits = Physics2D.RaycastAll(transform.position, Vector2.down, 20f, groundMask);
        RaycastHit2D best = default;
        foreach (var hit in hits)
            if (hit.collider != null && hit.collider.attachedRigidbody != rb && !hit.collider.isTrigger &&
                (best.collider == null || hit.distance < best.distance)) best = hit;
        if (best.collider != null)
        {
            float halfHeight = GetComponent<Collider2D>().bounds.extents.y;
            Vector3 p = transform.position;
            p.y = best.point.y + halfHeight + 0.03f;
            transform.position = p;
            rb.position = p;
        }
        rb.linearVelocity = Vector2.zero;
    }

    private void ExitLadder(bool jumpOff)
    {
        climbing = false;
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = jumpOff ? new Vector2(rb.linearVelocity.x, jumpVelocity) : Vector2.zero;
        if (nearbyLadder == null || !nearbyLadder.IsClimbable) nearbyLadder = null;
    }

    public bool RaiseLantern(bool lockAll)
    {
        if (GameState.Instance == null || !GameState.Instance.HasLantern || !grounded || lanternActionActive || !animator)
            return false;

        lanternActionActive = true;
        lockAllDuringLantern = lockAll;
        lanternActionEndsAt = Time.time + (maelaAudio != null ? maelaAudio.LanternRaiseDuration : 0f);
        animator.ResetTrigger("RaiseLantern");
        animator.SetBool("LanternActive", true);
        animator.SetTrigger("RaiseLantern");
        OnLanternRaised?.Invoke();
        return true;
    }

    private void CompleteLanternAction()
    {
        lanternActionActive = false;
        lockAllDuringLantern = false;
        if (animator)
        {
            animator.ResetTrigger("RaiseLantern");
            animator.SetBool("LanternActive", false);
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
