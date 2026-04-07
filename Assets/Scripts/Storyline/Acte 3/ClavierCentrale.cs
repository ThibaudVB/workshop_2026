using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ClavierCentrale : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject promptObject;

    [Header("UI")]
    [SerializeField] private GameObject clavierPanel;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button[] numButtons; // 0-9
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Storyline")]
    [SerializeField] private StorylineManager.VoiceLine[] successVoiceLines;

    private readonly string correctCode = "6767";
    private string currentInput = "";
    private bool panelOpen = false;
    private bool completed = false;

    private InputAction interactAction;
    private InputAction closeAction;

    void Awake()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.Enable();
        closeAction = new InputAction("Close", binding: "<Keyboard>/escape");
        closeAction.Enable();
    }

    void OnDestroy()
    {
        interactAction.Disable();
        interactAction.Dispose();
        closeAction.Disable();
        closeAction.Dispose();
    }

    void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
        if (clavierPanel != null) clavierPanel.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";

        // Bind les boutons numériques
        for (int i = 0; i < numButtons.Length; i++)
        {
            int num = i;
            numButtons[i].onClick.AddListener(() => PressNumber(num.ToString()));
        }

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteLast);
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    void Update()
    {
        if (completed) return;

        if (panelOpen)
        {
            if (closeAction.WasPressedThisFrame())
                ClosePanel();
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactionDistance)
        {
            if (promptObject != null) promptObject.SetActive(true);
            if (interactAction.WasPressedThisFrame())
                OpenPanel();
        }
        else
        {
            if (promptObject != null) promptObject.SetActive(false);
        }
    }

    void OpenPanel()
    {
        panelOpen = true;
        currentInput = "";
        UpdateDisplay();
        if (feedbackText != null) feedbackText.text = "";
        clavierPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerController.cinematicMode = true;
    }

    void ClosePanel()
    {
        panelOpen = false;
        clavierPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerController.cinematicMode = false;
    }

    void PressNumber(string num)
    {
        if (currentInput.Length >= 4) return;
        currentInput += num;
        UpdateDisplay();
    }

    void DeleteLast()
    {
        if (currentInput.Length == 0) return;
        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = currentInput.PadRight(4, '_');
    }

    void Confirm()
    {
        if (currentInput == correctCode)
        {
            completed = true;
            ClosePanel();
            StorylineManager.Instance.PlayVoiceLines(successVoiceLines, () =>
            {
              AutoDestructionManager.Instance.StartCountdown();
            });
        }
        else
        {
            StartCoroutine(ShowError());
        }
    }

    IEnumerator ShowError()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "CODE INCORRECT";
            feedbackText.color = Color.red;
        }
        currentInput = "";
        UpdateDisplay();
        yield return new WaitForSeconds(1.5f);
        if (feedbackText != null)
            feedbackText.text = "";
    }
}