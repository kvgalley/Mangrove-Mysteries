using System.Collections;
using UnityEngine;

// Attach to any trash object with a Collider. Ensure isTrigger is set to True on BoxCollider
// PlayerGrabber detects this via OverlapSphere and calls OnCollected() on trigger press.
public class Collectable : MonoBehaviour
{
    [Header("Settings")]
    public string collectableName = "Trash";
    public int pointValue = 10;
    [Tooltip("If true, pickup increments the HUD's trash counter.")]
    public bool isTrash = true;

    [Header("Idle Animation")]
    public float bobSpeed = 1.5f;
    public float bobHeight = 0.2f;
    public float rotateSpeed = 90f;

    [Header("Collection")]
    [Tooltip("Seconds to shrink from full size to zero before destroying.")]
    public float collectDuration = 0.5f;

    [Header("Feedback")]
    public AudioClip collectSound;

    private Vector3 _startPos;
    private Vector3 _originalScale;
    private AudioSource _audioSource;
    private bool _collected = false;

    void Start()
    {
        _startPos = transform.position;
        _originalScale = transform.localScale;

        if (collectSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.clip = collectSound;
        }
    }

    void Update()
    {
        if (_collected) return;

        // No Rigidbody on this object, so direct transform writes are safe
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // Called by PlayerGrabber — not wired via inspector
    public void OnCollected(PlayerGrabber collector)
    {
        if (_collected) return;
        _collected = true;

        // Notify the HUD. Null-check the singleton so the game still runs
        // if the HUD isn't in the scene (e.g. when testing in isolation).
        // Collectable is the source of truth for its own point value —
        // HUD just adds whatever we tell it.
        if (isTrash && HUDBuilder.Instance != null)
        {
            HUDBuilder.Instance.OnTrashCollected(pointValue);
        }

        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        // Play immediately so sound overlaps with shrink animation
        if (_audioSource != null && collectSound != null)
        {
            _audioSource.transform.SetParent(null);
            _audioSource.Play();
            Destroy(_audioSource.gameObject, collectSound.length);
        }

        // Shrink to zero, then destroy
        float elapsed = 0f;
        while (elapsed < collectDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, Mathf.Clamp01(elapsed / collectDuration));
            yield return null;
        }

        Destroy(gameObject);
    }
}
