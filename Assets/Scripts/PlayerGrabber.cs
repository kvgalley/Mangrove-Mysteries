using UnityEngine;
using UnityEngine.InputSystem;

// Sits on Right Controller. Sweeps a sphere around the hand each frame
// and collects the nearest Collectable when the trigger fires.
// Works alongside XRGrabInteractable — whichever fires first wins.
public class PlayerGrabber : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Cyan sphere in Scene view shows this radius around the controller.")]
    public float grabRadius = 0.3f;
    public LayerMask collectableLayer;

    [Header("VR Input")]
    [Tooltip("XRI Default Input Actions > XRI Right Interaction > Activate")]
    public InputActionReference grabAction;

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
        FindAndHintNearest();
    }

    void FindAndHintNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRadius, collectableLayer);

        float      closestDist = float.MaxValue;
        Collectable nearest    = null;

        foreach (Collider hit in hits)
        {
            Collectable item = hit.GetComponent<Collectable>();
            if (item == null) continue;
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < closestDist) { closestDist = dist; nearest = item; }
        }

        // Update interact hint on HUD
        if (nearest != null)
            HUDBuilder.Instance?.SetInteractHint($"Squeeze trigger to collect {nearest.collectableName}", true);
        else
            HUDBuilder.Instance?.SetInteractHint("", false);
    }

    void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRadius, collectableLayer);

        float      closestDist = float.MaxValue;
        Collectable nearest    = null;

        foreach (Collider hit in hits)
        {
            Collectable item = hit.GetComponent<Collectable>();
            if (item == null) continue;
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < closestDist) { closestDist = dist; nearest = item; }
        }

        if (nearest != null)
            nearest.OnCollected(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}