using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class WaterMovementController : MonoBehaviour
{
    public ContinuousMoveProvider moveProvider;

    public float normalMoveSpeed = 3.0f;
    public float shallowWaterSpeed = 1.8f;
    public float deepWaterSpeed = 0.9f;
    public float speedChangeRate = 4f;

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

        moveProvider.moveSpeed = Mathf.Lerp(
            moveProvider.moveSpeed,
            targetSpeed,
            Time.deltaTime * speedChangeRate
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount++;
        if (other.CompareTag("WaterDeep")) deepCount++;
        UpdateTargetSpeed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount = Mathf.Max(0, shallowCount - 1);
        if (other.CompareTag("WaterDeep")) deepCount = Mathf.Max(0, deepCount - 1);
        UpdateTargetSpeed();
    }

    private void UpdateTargetSpeed()
    {
        if (deepCount > 0) targetSpeed = deepWaterSpeed;
        else if (shallowCount > 0) targetSpeed = shallowWaterSpeed;
        else targetSpeed = normalMoveSpeed;
    }
}