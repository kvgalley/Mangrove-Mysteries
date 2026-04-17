using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProximityPopUp : MonoBehaviour
{
    [Header("Drag your AnimalFact file here")]
    public AnimalFact fact;

    [Header("How close the player needs to be")]
    public float triggerDistance = 3f;

    [Header("Only show the popup once?")]
    public bool showOnlyOnce = true;

    [Header("UI References - set these up in the prefab")]
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

    private void Start()
    {
        // Find the player's VR camera
        player = Camera.main.transform;

        // Make sure both panels are hidden at the start
        promptPanel.SetActive(false);
        infoPanel.SetActive(false);

        // Hook up the buttons so they call our methods when clicked
        yesButton.onClick.AddListener(OnYesPressed);
        closeButton.onClick.AddListener(OnClosePressed);
    }

    private void Update()
    {
        if (player == null || fact == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Player gets close enough — show the "Learn More?" prompt
        if (distance <= triggerDistance && !isPromptVisible && !isInfoVisible && !hasShown)
        {
            ShowPrompt();
        }

        // Player walks away — hide everything
        if (distance > triggerDistance + 1f)
        {
            HideAll();
        }

        // Keep panels facing the player at all times
        if (isPromptVisible)
            FacePlayer(promptPanel.transform);
        if (isInfoVisible)
            FacePlayer(infoPanel.transform);
    }

    private void ShowPrompt()
    {
        // Pull the animal name from the fact file
        promptTitleText.text = fact.animalName + "\nLearn More?";
        promptPanel.SetActive(true);
        isPromptVisible = true;
    }

    private void OnYesPressed()
    {
        // Hide the prompt, show the full info
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
    }

    private void FacePlayer(Transform panel)
    {
        panel.LookAt(player);
        panel.Rotate(0, 180, 0); // flip so text isn't mirrored
    }
}