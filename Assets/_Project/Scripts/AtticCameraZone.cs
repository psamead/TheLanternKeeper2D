using Unity.Cinemachine;
using UnityEngine;

/// <summary>Frames the attic as its own room while Maela is inside it.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class AtticCameraZone : MonoBehaviour
{
    public CinemachineCamera cameraTarget;
    public PolygonCollider2D roomBounds;
    public PolygonCollider2D atticBounds;
    public SpriteRenderer atticArtwork;
    public float atticOrthographicSize = 4f;
    public Vector3 atticTargetOffset = new Vector3(0f, 2.7f, 0f);

    private CinemachinePositionComposer composer;
    private CinemachineConfiner2D confiner;
    private float roomOrthographicSize;
    private Vector3 roomTargetOffset;
    private PlayerController playerInside;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (atticArtwork) atticArtwork.enabled = false;
        ResolveCamera();
    }

    private void ResolveCamera()
    {
        if (!cameraTarget) cameraTarget = FindFirstObjectByType<CinemachineCamera>();
        if (!cameraTarget) return;
        composer = cameraTarget.GetComponent<CinemachinePositionComposer>();
        confiner = cameraTarget.GetComponent<CinemachineConfiner2D>();
        roomOrthographicSize = cameraTarget.Lens.OrthographicSize;
        if (composer) roomTargetOffset = composer.TargetOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        playerInside = player;
        SetAtticFraming(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerInside || other.GetComponentInParent<PlayerController>() != playerInside) return;
        playerInside = null;
        SetAtticFraming(false);
    }

    public void SetAtticFraming(bool attic)
    {
        if (atticArtwork) atticArtwork.enabled = attic;
        if (!cameraTarget) ResolveCamera();
        if (!cameraTarget) return;
        var lens = cameraTarget.Lens;
        lens.OrthographicSize = attic ? atticOrthographicSize : roomOrthographicSize;
        cameraTarget.Lens = lens;
        if (composer) composer.TargetOffset = attic ? atticTargetOffset : roomTargetOffset;
        if (confiner)
        {
            confiner.BoundingShape2D = attic ? atticBounds : roomBounds;
            confiner.InvalidateBoundingShapeCache();
        }
        cameraTarget.PreviousStateIsValid = false;
    }

    private void OnDisable()
    {
        if (playerInside) playerInside = null;
        SetAtticFraming(false);
    }
}
