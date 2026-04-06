using UnityEngine;

public class WaterFloatController : MonoBehaviour
{
    [Header("References")]
    public WaterAudioController waterAudioController;
    public Transform cameraOffset;

    [Header("Deep Water Float Settings")]
    public float deepWaterLift = 0.18f;
    public float bobAmplitude = 0.03f;
    public float bobFrequency = 1.2f;
    public float transitionSpeed = 3f;

    private Vector3 baseLocalPosition;
    private float currentLift = 0f;

    private void Start()
    {
        if (cameraOffset != null)
        {
            baseLocalPosition = cameraOffset.localPosition;
        }
    }

    private void Update()
    {
        if (cameraOffset == null || waterAudioController == null)
            return;

        bool inDeepWater = waterAudioController.currentWater == WaterAudioController.WaterType.Deep;

        float targetLift = inDeepWater ? deepWaterLift : 0f;

        currentLift = Mathf.Lerp(
            currentLift,
            targetLift,
            Time.deltaTime * transitionSpeed
        );

        float bob = 0f;

        if (inDeepWater)
        {
            bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        }

        Vector3 targetPosition = baseLocalPosition + new Vector3(0f, currentLift + bob, 0f);
        cameraOffset.localPosition = targetPosition;
    }
}