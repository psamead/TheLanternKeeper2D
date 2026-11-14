using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// Maela movement for the Home demo: A/D or arrows to move, Shift to walk, Space/W/Up to jump.
// Drives Maela.controller parameters: Speed, Grounded, VelY, RaiseLantern.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [FormerlySerializedAs("moveSpeed")] public float runSpeed = 3.2f;
    public float walkSpeed = 0.8f;
    [Header("Jump")] public float jumpVelocity = 6.5f;
    public float coyoteTime = 0.08f, jumpBuffer = 0.1f;
    [Header("Ground check")] public LayerMask groundMask = ~0;
    public Vector2 groundBoxSize = new Vector2(0.3f, 0.06f);
    [Header("Refs")] public Animator animator;   // on the Maela visual child
    public Transform visual;                     // child flipped by scale.x

    Rigidbody2D rb;
    MaelaAudio maelaAudio;
    float lastGrounded = -1, lastJumpPress = -1, lanternActionEndsAt;
    bool grounded, lanternActionActive, lockAllDuringLantern;
    [System.Flags]
    public enum ControlLock { None = 0, LanternBell = 1, AtticTransition = 2, EndCard = 4, StoryReturn = 8 }
    private ControlLock controlLocks;

    public event System.Action OnJump;
    public event System.Action OnLanternRaised;
    public bool IsGrounded => grounded;
    public bool IsWalking { get; private set; }
    public bool LanternLocked => lanternActionActive && lockAllDuringLantern;
    public bool ControlsLocked => controlLocks != ControlLock.None;

    public void SetControlLock(ControlLock owner, bool locked)
    {
        if (locked) controlLocks |= owner;
        else controlLocks &= ~owner;
        if (locked && rb) rb.linearVelocity = Vector2.zero;
        lastJumpPress = lastGrounded = -100f;
    }

    /// <summary>Place collider feet on real support near a scene anchor, under an opaque fade.</summary>
    public bool PlaceOnGround(Transform anchor)
    {
        if (!anchor || !rb) return false;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        lastJumpPress = lastGrounded = -100f;
        Physics2D.SyncTransforms();
        float footOffset = float.PositiveInfinity;
        foreach (var c in GetComponentsInChildren<Collider2D>())
            if (c.enabled && !c.isTrigger && c.attachedRigidbody == rb)
                footOffset = Mathf.Min(footOffset, c.bounds.min.y - transform.position.y);
        if (float.IsPositiveInfinity(footOffset)) return false;
        RaycastHit2D support = default;
        foreach (var hit in Physics2D.RaycastAll(anchor.position + Vector3.up, Vector2.down, 2f, groundMask))
            if (!hit.collider.isTrigger && hit.rigidbody != rb && hit.normal.y > 0.9f)
            { support = hit; break; }
        if (!support.collider) return false;
        Vector3 position = new Vector3(anchor.position.x, support.point.y - footOffset + 0.002f, transform.position.z);
        transform.position = position;
        rb.position = position;
        Physics2D.SyncTransforms();
        FixedUpdate();
        if (animator)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("VelY", 0f);
            animator.SetBool("Grounded", grounded);
            animator.Update(0f);
        }
        return grounded;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        maelaAudio = GetComponent<MaelaAudio>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visual && animator) visual = animator.transform;
    }

    void Update()
    {
        if (lanternActionActive && Time.time >= lanternActionEndsAt)
            CompleteLanternAction();

        var k = Keyboard.current;
        if (!ControlsLocked && !StoryboardPlayer.BlocksGameplayInput && !lanternActionActive && LanternSwitch.Instance == null && k != null && k.eKey.wasPressedThisFrame)
            RaiseLantern(true);

        if (ControlsLocked || StoryboardPlayer.BlocksGameplayInput)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("VelY", 0f);
            }
            return;
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

    public bool RaiseLantern(bool lockAll)
    {
        if (GameState.Instance == null || !GameState.Instance.CanUseLantern || ControlsLocked ||
            StoryboardPlayer.BlocksGameplayInput || !grounded || lanternActionActive || !animator)
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
