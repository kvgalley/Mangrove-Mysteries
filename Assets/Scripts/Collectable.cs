using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attach to any GameObject you want to be grabbable and collectable through XR controllers.
/// Check isTrash in the Inspector for trash items.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class Collectable : MonoBehaviour
{
    [Header("Settings")]
    public string collectableName  = "Item";
    public bool   isTrash          = false;   // check this for trash items
    public bool   destroyOnCollect = true;

    [Header("Feedback optional")]
    public AudioClip  collectSound;
    public GameObject collectEffect;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grabInteractable;
    AudioSource        _audio;

    void Awake()
    {
        _grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Fire OnGrabbed when the controller grabs this object
        _grabInteractable.selectEntered.AddListener(OnGrabbed);

        // Configure Rigidbody for XR grabbing
        var rb = GetComponent<Rigidbody>();
        rb.useGravity  = true;
        rb.isKinematic = false;
    }

    void Start()
    {
        if (collectSound)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.clip        = collectSound;
        }
    }

    /// <summary>
    /// Fires the moment an XR controller grabs this object.
    /// </summary>
    void OnGrabbed(SelectEnterEventArgs args)
    {
        // Notify HUD if this is a trash item
        if (isTrash)
            HUDBuilder.Instance?.OnTrashCollected();

        // Spawn visual effect at grab position
        if (collectEffect)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Play sound detached so it survives object destruction
        if (_audio && collectSound)
        {
            _audio.transform.SetParent(null);
            _audio.Play();
            Destroy(_audio.gameObject, collectSound.length);
        }

        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_grabInteractable != null)
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }
}