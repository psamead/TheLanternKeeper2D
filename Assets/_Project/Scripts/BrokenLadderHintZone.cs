using System.Collections.Generic;
using UnityEngine;

/// <summary>One fresh physical entry advances one ordered step after pickup and Story A.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class BrokenLadderHintZone : MonoBehaviour
{
    [SerializeField] private bool discovery;
    private readonly HashSet<Collider2D> contacts = new HashSet<Collider2D>();
    private bool retired;

    public void ClearOccupancy() => contacts.Clear();
    public void Retire()
    {
        retired = true;
        contacts.Clear();
        var world = GetComponent<WorldLayerObject>();
        if (world) world.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
    }

    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (retired || !enabled || !player) return;
        // Track occupancy even while locked or out of order. Another child entering,
        // or an unlock while still inside, must never count as a fresh visit.
        contacts.RemoveWhere(c => !c || !c.isActiveAndEnabled);
        bool freshEntry = contacts.Count == 0;
        if (!contacts.Add(other) || !freshEntry) return;
        if (GameState.Instance) GameState.Instance.RecordExploration(discovery, player);
    }

    private void OnTriggerExit2D(Collider2D other) => contacts.Remove(other);
    private void OnDisable() => contacts.Clear();
}
