using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class MaintButton : MonoBehaviour
{
    [Header("Références")]
    public Transform player;
    public GameObject promptObject;
    public GameObject qtePanel;
    public TextMeshProUGUI qteText;

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public float qteTimeLimit = 3f;
    public int pressesRequired = 4;
    public float qteStartDelay = 0.3f; // Délai avant de commencer à compter les pressions

    private bool playerNearby = false;
    private bool qteActive = false;
    private bool qteStarted = false; // Pour vérifier si le QTE a vraiment commencé
    private int pressCount = 0;
    private float timer = 0f;

    private InputAction interactAction;

    void Awake()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.Enable();
    }

    void OnDestroy()
    {
        interactAction.Disable();
        interactAction.Dispose();
    }

    void Start()
    {
        promptObject.SetActive(false);
        qtePanel.SetActive(false);
    }

    void Update()
    {
        // Vérifie la proximité du joueur
        if (!qteActive)
        {
            CheckProximity();
        }
        // Gère le QTE une fois qu'il est actif et que le délai est passé
        else if (qteStarted)
        {
            if (interactAction.WasPressedThisFrame())
            {
                pressCount++;
                UpdateQTEText();

                if (pressCount >= pressesRequired)
                {
                    QTESuccess();
                }
            }
        }
    }

    void CheckProximity()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (!playerNearby)
            {
                playerNearby = true;
                promptObject.SetActive(true);
            }

            if (interactAction.WasPressedThisFrame())
            {
                StartQTE();
            }
        }
        else
        {
            if (playerNearby)
            {
                playerNearby = false;
                promptObject.SetActive(false);
            }
        }
    }

    void StartQTE()
    {
        qteActive = true;
        pressCount = 0;
        timer = qteTimeLimit;
        qteStarted = false; // Le QTE n'a pas encore vraiment commencé

        promptObject.SetActive(false);
        qtePanel.SetActive(true);
        UpdateQTEText();

        Debug.Log("QTE lancé !");

        StartCoroutine(StartQTEDelayed()); // Lance le QTE après un délai
    }

    IEnumerator StartQTEDelayed()
    {
        yield return new WaitForSeconds(qteStartDelay); // Attend 0.3 secondes
        qteStarted = true; // Le QTE commence vraiment maintenant
        StartCoroutine(UpdateQTETimer()); // Lance le timer
    }

    IEnumerator UpdateQTETimer()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            UpdateQTEText();
            yield return null;
        }

        QTEFail();
    }

    void UpdateQTEText()
    {
        if (qteText != null)
            qteText.text = "E : " + pressCount + " / " + pressesRequired + Mathf.Ceil(timer).ToString();
    }

    void QTESuccess()
    {
        qteActive = false;
        qteStarted = false;
        qtePanel.SetActive(false);
        StopAllCoroutines();
        Debug.Log("QTE réussi !");
        // Ajoute ici ton code pour la réussite
    }

    void QTEFail()
    {
        qteActive = false;
        qteStarted = false;
        qtePanel.SetActive(false);
        playerNearby = false;
        StopAllCoroutines();
        Debug.Log("QTE échoué !");
        // Ajoute ici ton code pour l'échec
    }
}