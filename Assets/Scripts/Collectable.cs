using UnityEngine;

/// <summary>
/// Attach this script to any GameObject you want to be collectable.
/// The object needs a Collider (set to Trigger) to be detected by the player.
/// </summary>
public class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    public string collectableName = "Item";       // Name shown in UI or log
    public int pointValue = 10;                   // Score/points awarded on collect
    public bool destroyOnCollect = true;          // Remove object after collection

    [Header("Feedback")]
    public AudioClip collectSound;               // Optional sound effect on collect
    public GameObject collectEffect;            // Optional particle effect on collect
    public float bobSpeed = 1.5f;               // Speed of up/down bob animation
    public float bobHeight = 0.2f;              // Height of bob in world units
    public float rotateSpeed = 90f;             // Degrees per second rotation

    private Vector3 _startPos;
    private AudioSource _audioSource;

    void Start()
    {
        _startPos = transform.position;

        // Add AudioSource if a sound clip is assigned
        if (collectSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.clip = collectSound;
        }
    }

    void Update()
    {
        // Bob up and down
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Slowly rotate
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Called by the PlayerGrabber when this item is collected.
    /// </summary>
    public void OnCollected(PlayerGrabber collector)
    {
        Debug.Log($"[Collectable] '{collectableName}' collected by {collector.gameObject.name}. +{pointValue} pts");

        // Spawn visual effect
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Play sound (detach so it survives object destruction)
        if (_audioSource != null && collectSound != null)
        {
            _audioSource.transform.SetParent(null);
            _audioSource.Play();
            Destroy(_audioSource.gameObject, collectSound.length);
        }

        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
