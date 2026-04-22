using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProximityPopUp : MonoBehaviour
{
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

        if (distance <= triggerDistance && !isPromptVisible && !isInfoVisible && !hasShown)
        {
            ShowPrompt();
        }

        if (distance > triggerDistance + 1f)
        {
            HideAll();
        }

        if (isPromptVisible)
            FacePlayer(promptPanel.transform);
        if (isInfoVisible)
            FacePlayer(infoPanel.transform);
    }

    private void ShowPrompt()
    {
        promptTitleText.text = fact.animalName + "\nLearn More?";
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
    }

    private void FacePlayer(Transform panel)
    {
        panel.LookAt(player);
        panel.Rotate(0, 180, 0);
    }
}