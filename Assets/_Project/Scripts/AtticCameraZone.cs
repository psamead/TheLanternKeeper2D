using Unity.Cinemachine;
using UnityEngine;

/// <summary>Frames the attic as its own room while Maela is inside it.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class AtticCameraZone : MonoBehaviour
{
    public CinemachineCamera cameraTarget;
    public float atticOrthographicSize = 4f;
    public Vector3 atticTargetOffset = new Vector3(0f, 2.7f, 0f);

    private CinemachinePositionComposer composer;
    private float roomOrthographicSize;
    private Vector3 roomTargetOffset;
    private PlayerController playerInside;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (!cameraTarget) cameraTarget = FindFirstObjectByType<CinemachineCamera>();
        if (!cameraTarget) return;
        composer = cameraTarget.GetComponent<CinemachinePositionComposer>();
        roomOrthographicSize = cameraTarget.Lens.OrthographicSize;
        if (composer) roomTargetOffset = composer.TargetOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        playerInside = player;
        ApplyAtticFraming(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerInside || other.GetComponentInParent<PlayerController>() != playerInside) return;
        playerInside = null;
        ApplyAtticFraming(false);
    }

    private void ApplyAtticFraming(bool attic)
    {
        if (!cameraTarget) return;
        var lens = cameraTarget.Lens;
        lens.OrthographicSize = attic ? atticOrthographicSize : roomOrthographicSize;
        cameraTarget.Lens = lens;
        if (composer) composer.TargetOffset = attic ? atticTargetOffset : roomTargetOffset;
    }

    private void OnDisable()
    {
        if (playerInside) playerInside = null;
        ApplyAtticFraming(false);
    }
}
