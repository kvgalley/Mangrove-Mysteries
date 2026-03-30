using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this script to your Player GameObject.
/// Handles both trigger-based auto-collection and manual grab (press key to collect).
/// </summary>
public class PlayerGrabber : MonoBehaviour
{
    [Header("Collection Settings")]
    public bool autoCollect = false;             // true = collect on touch; false = require input
    public KeyCode grabKey = KeyCode.E;          // Key to press for manual grab
    public float grabRadius = 2.0f;              // Radius for nearby-item detection (manual mode)
    public LayerMask collectableLayer;           // Set this to your "Collectable" layer in Inspector

    [Header("Score Tracking")]
    public int totalScore = 0;
    public int totalItemsCollected = 0;

    [Header("UI Feedback (optional)")]
    public TMPro.TextMeshProUGUI scoreText;      // Drag a TMP UI text element here
    public TMPro.TextMeshProUGUI promptText;     // "Press E to collect" prompt

    // Tracks the nearest collectable for manual mode
    private Collectable _nearestCollectable;
    private List<Collectable> _itemsInRange = new List<Collectable>();

    void Update()
    {
        if (!autoCollect)
        {
            FindNearestCollectable();

            // Show prompt if item is nearby
            if (promptText != null)
                promptText.text = _nearestCollectable != null
                    ? $"Press [{grabKey}] to collect {_nearestCollectable.collectableName}"
                    : "";

            // Manual grab input
            if (Input.GetKeyDown(grabKey) && _nearestCollectable != null)
                Collect(_nearestCollectable);
        }
    }

    /// <summary>
    /// Auto-collection: triggers when player walks into a collectable's trigger collider.
    /// Requires autoCollect = true AND the collectable to have a Trigger Collider.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!autoCollect) return;

        Collectable item = other.GetComponent<Collectable>();
        if (item != null)
            Collect(item);
    }

    /// <summary>
    /// Track items entering grab radius for manual mode.
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        if (autoCollect) return;

        Collectable item = other.GetComponent<Collectable>();
        if (item != null && !_itemsInRange.Contains(item))
            _itemsInRange.Add(item);
    }

    void OnTriggerExit(Collider other)
    {
        Collectable item = other.GetComponent<Collectable>();
        if (item != null && _itemsInRange.Contains(item))
        {
            _itemsInRange.Remove(item);
            if (_nearestCollectable == item)
                _nearestCollectable = null;
        }
    }

    /// <summary>
    /// Finds the closest collectable within grabRadius using OverlapSphere.
    /// </summary>
    void FindNearestCollectable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRadius, collectableLayer);

        float closestDist = float.MaxValue;
        _nearestCollectable = null;

        foreach (Collider hit in hits)
        {
            Collectable item = hit.GetComponent<Collectable>();
            if (item == null) continue;

            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _nearestCollectable = item;
            }
        }
    }

    /// <summary>
    /// Core collection logic. Adds score and notifies the Collectable.
    /// </summary>
    void Collect(Collectable item)
    {
        totalScore += item.pointValue;
        totalItemsCollected++;

        UpdateScoreUI();

        item.OnCollected(this);

        // Clean up list
        if (_itemsInRange.Contains(item))
            _itemsInRange.Remove(item);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}  |  Collected: {totalItemsCollected}";
    }

    /// <summary>
    /// Draw grab radius gizmo in Scene view for easy tuning.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
