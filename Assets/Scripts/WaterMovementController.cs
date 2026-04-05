using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

// Sits on Feet Controller. Slows ContinuousMoveProvider when entering water zones.
// Note: normalMoveSpeed (3) overrides whatever moveSpeed is set in the inspector on Start().
public class WaterMovementController : MonoBehaviour
{
    public ContinuousMoveProvider moveProvider;

    public float normalMoveSpeed = 3.0f;
    public float shallowWaterSpeed = 1.8f;
    public float deepWaterSpeed = 0.9f;
    public float speedChangeRate = 4f;

    // Counts handle overlapping volumes; Mathf.Max prevents going negative on Exit
    private int shallowCount = 0;
    private int deepCount = 0;
    private float targetSpeed;

    private void Start()
    {
        targetSpeed = normalMoveSpeed;
        if (moveProvider != null)
            moveProvider.moveSpeed = normalMoveSpeed;
    }

    private void Update()
    {
        if (moveProvider == null) return;
        // Lerp toward target so speed transitions feel fluid rather than instant
        moveProvider.moveSpeed = Mathf.Lerp(moveProvider.moveSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount++;
        if (other.CompareTag("WaterDeep"))   deepCount++;
        UpdateTargetSpeed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount = Mathf.Max(0, shallowCount - 1);
        if (other.CompareTag("WaterDeep"))   deepCount   = Mathf.Max(0, deepCount - 1);
        UpdateTargetSpeed();
    }

    private void UpdateTargetSpeed()
    {
        // Deep takes priority over shallow, matching WaterAudioController priority
        if (deepCount > 0)         targetSpeed = deepWaterSpeed;
        else if (shallowCount > 0) targetSpeed = shallowWaterSpeed;
        else                       targetSpeed = normalMoveSpeed;
    }
}