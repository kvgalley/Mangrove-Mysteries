using UnityEngine;

// Attach to sea turtle GameObjects.
// Patrols through waypoints with a gentle sine-wave vertical bob.
// Starts animation at a random offset so multiple turtles don't sync up.
//
// Setup:
//   1. Create empty GameObjects as waypoints and assign them to the Waypoints array.
//   2. Model Rotation Offset defaults to 180 to fix the backwards-swimming import issue.
//      Adjust if needed.
public class WildlifeTurtle : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Swimming")]
    public float swimSpeed = 1.0f;
    public float arrivalRadius = 0.6f;

    [Header("Vertical Bob")]
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 0.5f;

    [Header("Model Orientation")]
    [Tooltip("180 corrects the backwards-swimming import issue on the hawksbill turtle GLB.")]
    public float modelRotationOffset = 180f;

    private int _waypointIndex = 0;
    private float _bobPhase;

    void Start()
    {
        _bobPhase = Random.Range(0f, Mathf.PI * 2f);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.Play(0, 0, Random.value);

        if (waypoints != null && waypoints.Length > 0)
            _waypointIndex = Random.Range(0, waypoints.Length);
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 target = waypoints[_waypointIndex].position;

        float bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f + _bobPhase) * bobAmplitude;
        Vector3 swimTarget = new Vector3(target.x, target.y + bob, target.z);

        Vector3 dir = (swimTarget - transform.position).normalized;
        transform.position += dir * swimSpeed * Time.deltaTime;

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir) * Quaternion.Euler(0f, modelRotationOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        }

        float flatDist = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(target.x, target.z));

        if (flatDist < arrivalRadius)
            _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
