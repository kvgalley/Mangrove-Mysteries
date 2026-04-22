using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProximityPopUp : MonoBehaviour
{
    // Global registry + active-popup tracker so only one popup is ever visible.
    // When multiple animals are in range simultaneously, the closest one wins.
    private static readonly List<ProximityPopUp> _allInstances = new List<ProximityPopUp>();
    private static ProximityPopUp _activePopup;

    [Header("Drag your AnimalFact file here")]
    public AnimalFact fact;

    [Header("How close the player needs to be")]
    public float triggerDistance = 5f;

    [Header("Only show the popup once?")]
    public bool showOnlyOnce = true;

    [Header("UI References")]
    public GameObject promptPanel;
    public GameObject infoPanel;
    public TextMeshProUGUI promptTitleText;
    public TextMeshProUGUI infoTitleText;
    public TextMeshProUGUI infoBodyText;
    public Button yesButton;
    public Button closeButton;

    private Transform player;
    private bool hasShown = false;
    private bool isPromptVisible = false;
    private bool isInfoVisible = false;

    private void OnEnable()
    {
        _allInstances.Add(this);
    }

    private void OnDisable()
    {
        _allInstances.Remove(this);
        if (_activePopup == this) _activePopup = null;
    }

    private void Start()
    {
        player = Camera.main.transform;

        promptPanel.SetActive(false);
        infoPanel.SetActive(false);

        yesButton.onClick.AddListener(OnYesPressed);
        closeButton.onClick.AddListener(OnClosePressed);
    }

    private void Update()
    {
        if (player == null || fact == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // If I'm the active popup, manage my own exit and face-player behavior.
        if (_activePopup == this)
        {
            if (distance > triggerDistance + 1f)
                HideAll();

            if (isPromptVisible)
                FacePlayer(promptPanel.transform);
            if (isInfoVisible)
                FacePlayer(infoPanel.transform);
            return;
        }

        // Someone else is active — stay quiet until they finish.
        if (_activePopup != null) return;

        // No popup active. Decide whether to open mine.
        if (distance <= triggerDistance && !isPromptVisible && !isInfoVisible && !hasShown)
        {
            // Only open if I'm the closest eligible instance to the player.
            if (FindClosestEligible() == this)
            {
                ShowPrompt();
                _activePopup = this;
            }
        }
    }

    // Returns the eligible (in-range, not-yet-shown) ProximityPopUp closest to the player.
    private static ProximityPopUp FindClosestEligible()
    {
        if (Camera.main == null) return null;
        Vector3 playerPos = Camera.main.transform.position;

        ProximityPopUp best = null;
        float bestDist = float.MaxValue;

        foreach (ProximityPopUp p in _allInstances)
        {
            if (p == null || p.fact == null) continue;
            if (p.hasShown) continue;

            float d = Vector3.Distance(playerPos, p.transform.position);
            if (d <= p.triggerDistance && d < bestDist)
            {
                best = p;
                bestDist = d;
            }
        }
        return best;
    }

    private void ShowPrompt()
    {
        promptTitleText.text = $"{fact.animalName} — Learn More?";
        promptPanel.SetActive(true);
        isPromptVisible = true;
    }

    private void OnYesPressed()
    {
        promptPanel.SetActive(false);
        isPromptVisible = false;

        infoTitleText.text = fact.animalName;
        infoBodyText.text = fact.educationalText;
        infoPanel.SetActive(true);
        isInfoVisible = true;

        if (showOnlyOnce)
            hasShown = true;
    }

    private void OnClosePressed()
    {
        HideAll();
        if (showOnlyOnce)
            hasShown = true;
    }

    private void HideAll()
    {
        promptPanel.SetActive(false);
        infoPanel.SetActive(false);
        isPromptVisible = false;
        isInfoVisible = false;

        // Release the global lock so another animal can open its popup.
        if (_activePopup == this) _activePopup = null;
    }

    private void FacePlayer(Transform panel)
    {
        panel.LookAt(player);
        panel.Rotate(0, 180, 0);
    }
}