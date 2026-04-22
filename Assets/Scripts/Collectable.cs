using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Attach to any trash object.
// Requires: Collider (Is Trigger OFF), Rigidbody, XRGrabInteractable
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class Collectable : MonoBehaviour
{
    [Header("Settings")]
    public string collectableName = "Trash";
    public bool   isTrash         = true;

    [Header("Idle Animation")]
    public float bobSpeed    = 1.5f;
    public float bobHeight   = 0.2f;
    public float rotateSpeed = 90f;

    [Header("Collection")]
    public float collectDuration = 0.5f;

    [Header("Feedback")]
    public AudioClip collectSound;

    Vector3     _startPos;
    Vector3     _originalScale;
    AudioSource _audioSource;
    bool        _collected = false;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;

    void Start()
    {
        _startPos      = transform.position;
        _originalScale = transform.localScale;

        // Wire up XR grab event
        _grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _grab.selectEntered.AddListener(OnGrabbed);

        if (collectSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.clip        = collectSound;
        }
    }

    void Update()
    {
        if (_collected) return;
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // Called by XRGrabInteractable when controller grabs this object
    void OnGrabbed(SelectEnterEventArgs args)
    {
        Collect();
    }

    // Still works if you want to call it from anywhere else
    public void OnCollected(PlayerGrabber collector)
    {
        Collect();
    }

    void Collect()
    {
        if (_collected) return;
        _collected = true;

        // Notify HUD
        if (isTrash)
            HUDBuilder.Instance?.OnTrashCollected();

        StartCoroutine(CollectRoutine());
    }

    IEnumerator CollectRoutine()
    {
        if (_audioSource != null && collectSound != null)
        {
            _audioSource.transform.SetParent(null);
            _audioSource.Play();
            Destroy(_audioSource.gameObject, collectSound.length);
        }

        float elapsed = 0f;
        while (elapsed < collectDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, Mathf.Clamp01(elapsed / collectDuration));
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (_grab != null)
            _grab.selectEntered.RemoveListener(OnGrabbed);
    }
}