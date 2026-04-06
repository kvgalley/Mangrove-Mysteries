using UnityEngine;

public class WaterAudioController : MonoBehaviour
{
    public AudioSource shallowAudio;
    public AudioSource deepAudio;

    private int shallowCount = 0;
    private int deepCount = 0;

    public enum WaterType
    {
        None,
        Shallow,
        Deep
    }

    public WaterType currentWater = WaterType.None;

    private void UpdateWaterState()
    {
        WaterType newState;

        if (deepCount > 0)
            newState = WaterType.Deep;
        else if (shallowCount > 0)
            newState = WaterType.Shallow;
        else
            newState = WaterType.None;

        // Only update if changed
        if (newState != currentWater)
        {
            currentWater = newState;
            Debug.Log("Water State: " + currentWater);
            UpdateAudio();
        }
    }

    private void UpdateAudio()
    {
        // Stop everything first
        if (shallowAudio.isPlaying) shallowAudio.Stop();
        if (deepAudio.isPlaying) deepAudio.Stop();

        switch (currentWater)
        {
            case WaterType.Shallow:
                if (!shallowAudio.isPlaying)
                    shallowAudio.Play();
                break;

            case WaterType.Deep:
                if (!deepAudio.isPlaying)
                    deepAudio.Play();
                break;

            case WaterType.None:
                // nothing plays
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterShallow"))
        {
            shallowCount++;
        }
        else if (other.CompareTag("WaterDeep"))
        {
            deepCount++;
        }

        UpdateWaterState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterShallow"))
        {
            shallowCount--;
        }
        else if (other.CompareTag("WaterDeep"))
        {
            deepCount--;
        }

        UpdateWaterState();
    }
}