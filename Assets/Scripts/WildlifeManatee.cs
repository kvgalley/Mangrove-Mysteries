using UnityEngine;

// Attach to the manatee GameObject.
// Patrols through waypoints with a gentle sine-wave vertical bob.
// Starts animation at a random offset so multiple instances don't sync up.
//
// Setup:
//   1. Create empty GameObjects as waypoints and assign them to the Waypoints array.
//   2. If the manatee moves sideways, adjust Model Rotation Offset (try 90 or -90).
public class WildlifeManatee : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Swimming")]
    public float swimSpeed = 0.6f;
    public float arrivalRadius = 0.6f;

    [Header("Vertical Bob")]
    public float bobAmplitude = 0.15f;
    public float bobFrequency = 0.4f;

    [Header("Model Orientation")]
    [Tooltip("Adjust if the manatee faces the wrong direction. Try 90 or -90 for sideways.")]
    public float modelRotationOffset = 0f;

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
        Gizmos.color = Color.blue;
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
