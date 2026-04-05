using UnityEngine;

// Sits on Feet Controller. Tracks water zone entry/exit and plays matching audio loop.
// currentWater is read by WaterFloatController and WaterMovementController.
// Trigger colliders must be tagged "WaterShallow" or "WaterDeep".
public class WaterAudioController : MonoBehaviour
{
    public AudioSource shallowAudio;
    public AudioSource deepAudio;

    // Counts (not bools) correctly handle simultaneous/overlapping volumes
    private int shallowCount = 0;
    private int deepCount = 0;

    public enum WaterType { None, Shallow, Deep }

    public WaterType currentWater = WaterType.None;

    private void UpdateWaterState()
    {
        // Deep takes priority over shallow
        WaterType newState;
        if (deepCount > 0)         newState = WaterType.Deep;
        else if (shallowCount > 0) newState = WaterType.Shallow;
        else                       newState = WaterType.None;

        if (newState != currentWater)
        {
            currentWater = newState;
            Debug.Log("Water State: " + currentWater);
            UpdateAudio();
        }
    }

    private void UpdateAudio()
    {
        if (shallowAudio.isPlaying) shallowAudio.Stop();
        if (deepAudio.isPlaying)   deepAudio.Stop();

        switch (currentWater)
        {
            case WaterType.Shallow: shallowAudio.Play(); break;
            case WaterType.Deep:    deepAudio.Play();    break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount++;
        else if (other.CompareTag("WaterDeep")) deepCount++;
        UpdateWaterState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterShallow")) shallowCount--;
        else if (other.CompareTag("WaterDeep")) deepCount--;
        UpdateWaterState();
    }
}