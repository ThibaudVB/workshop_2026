using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MaintButton1 : MonoBehaviour
{
    [Header("Références")]
    public Transform player;
    public GameObject promptObject;
    public GameObject QTEPanel;
    public Slider QTESlider;
    public Image SuccessZone;
    public Image Needle;
    public TextMeshProUGUI QTEText;

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public float qteDuration = 3f;
    public float needleSpeed = 1f;
    public float successZoneMin = 0.4f;
    public float successZoneMax = 0.6f;

    [Header("QTE Multiple")]
    public int qteTotalRequired = 2;
    private int qteSuccessCount = 0;

    private bool playerNearby = false;
    private bool qteActive = false;
    private float timer = 0f;
    private InputAction interactAction;
    private Coroutine qteCoroutine;

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
        if (promptObject != null)
            promptObject.SetActive(false);
        if (QTEPanel != null)
            QTEPanel.SetActive(false);
    }

    void Update()
    {
        if (!qteActive)
        {
            CheckProximity();
        }
    }

    void CheckProximity()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (!playerNearby && promptObject != null)
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
            if (playerNearby && promptObject != null)
            {
                playerNearby = false;
                promptObject.SetActive(false);
            }
        }
    }

    void StartQTE()
    {
        qteActive = true;
        timer = qteDuration;

        if (promptObject != null)
            promptObject.SetActive(false);

        if (QTEPanel != null)
            QTEPanel.SetActive(true);
        else
            Debug.LogError("QTEPanel non assigné !");

        if (SuccessZone != null)
            SuccessZone.color = Color.green;
        else
            Debug.LogError("SuccessZone non assigné !");

        if (Needle != null)
        {
            Needle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            Needle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            Needle.color = Color.yellow;
        }
        else
            Debug.LogError("Needle non assigné !");

        qteCoroutine = StartCoroutine(MoveNeedle());
        Debug.Log("QTE lancé !");
    }

    IEnumerator MoveNeedle()
    {
        yield return new WaitForSeconds(0.2f);

        float needlePosition = 0f;
        bool movingRight = true;
        float sliderWidth = QTESlider.GetComponent<RectTransform>().rect.width;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (movingRight)
            {
                needlePosition += needleSpeed * Time.deltaTime;
                if (needlePosition >= 1f)
                {
                    needlePosition = 1f;
                    movingRight = false;
                }
            }
            else
            {
                needlePosition -= needleSpeed * Time.deltaTime;
                if (needlePosition <= 0f)
                {
                    needlePosition = 0f;
                    movingRight = true;
                }
            }

            if (Needle != null)
            {
                Needle.rectTransform.anchoredPosition = new Vector2(
                    (needlePosition - 0.5f) * sliderWidth, 0
                );
            }

            if (interactAction.WasPressedThisFrame())
            {
                CheckNeedlePosition(needlePosition);
                yield break;
            }

            yield return null;
        }

        QTEFail();
    }

    void CheckNeedlePosition(float needlePosition)
    {
        if (needlePosition >= successZoneMin && needlePosition <= successZoneMax)
        {
            QTESuccess();
        }
        else
        {
            QTEFail();
        }
    }

    void QTESuccess()
    {
        qteSuccessCount++;
        StopAllCoroutines();

        if (qteSuccessCount >= qteTotalRequired)
        {
            qteActive = false;
            qteSuccessCount = 0;
            if (QTEPanel != null)
                QTEPanel.SetActive(false);
            Debug.Log("Tous les QTE réussis !");
        }
        else
        {
            Debug.Log("QTE " + qteSuccessCount + " / " + qteTotalRequired + " réussi !");
            timer = qteDuration;
            qteCoroutine = StartCoroutine(MoveNeedle());
        }
    }

    void QTEFail()
    {
        qteActive = false;
        qteSuccessCount = 0;
        if (QTEPanel != null)
            QTEPanel.SetActive(false);
        playerNearby = false;
        StopAllCoroutines();
        Debug.Log("QTE échoué ! Recommence depuis le début.");
    }
}