using UnityEngine;
using UnityEngine.InputSystem;

// Sits on Right Controller. Each frame sweeps a sphere around the hand
// and caches the nearest Collectable. When the trigger fires, that item is collected.
public class PlayerGrabber : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Cyan sphere in Scene view shows this radius around the controller.")]
    public float grabRadius = 0.3f;
    public LayerMask collectableLayer;

    [Header("VR Input")]
    [Tooltip("XRI Default Input Actions > XRI Right Interaction > Activate")]
    public InputActionReference grabAction;

    [Header("Debug")]
    // HUDBuilder is the single source of truth for score.
    // We only track item count here for logging/diagnostics.
    public int totalItemsCollected = 0;

    private Collectable _nearestCollectable;

    void OnEnable()
    {
        if (grabAction != null)
            grabAction.action.performed += OnTriggerPressed;
    }

    void OnDisable()
    {
        if (grabAction != null)
            grabAction.action.performed -= OnTriggerPressed;
    }

    void Update()
    {
        FindNearest();
    }

    void FindNearest()
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

    void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        if (_nearestCollectable != null)
            Collect(_nearestCollectable);
    }

    void Collect(Collectable item)
    {
        totalItemsCollected++;
        Debug.Log($"[PlayerGrabber] '{item.collectableName}' +{item.pointValue}pts | {totalItemsCollected} items picked up");
        item.OnCollected(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
