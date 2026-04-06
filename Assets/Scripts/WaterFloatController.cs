using UnityEngine;

// Sits on Camera Offset. Reads WaterAudioController.currentWater
// Camera Offset Y upward in deep water, adding a sine-wave bob on top.
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
            baseLocalPosition = cameraOffset.localPosition; // snapshot resting Y
    }

    private void Update()
    {
        if (cameraOffset == null || waterAudioController == null) return;

        bool inDeepWater = waterAudioController.currentWater == WaterAudioController.WaterType.Deep;

        // Smoothly approach target lift (0 when not in deep water)
        float targetLift = inDeepWater ? deepWaterLift : 0f;
        currentLift = Mathf.Lerp(currentLift, targetLift, Time.deltaTime * transitionSpeed);

        // Bob only active in deep water
        float bob = inDeepWater
            ? Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude
            : 0f;

        cameraOffset.localPosition = baseLocalPosition + new Vector3(0f, currentLift + bob, 0f);
    }
}