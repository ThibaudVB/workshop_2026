using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MiniJeuMaintenance : MonoBehaviour
{
    [Header("Références")]
    public Transform player;
    public GameObject promptObject;
    public GameObject QTEPanel;
    public Image SuccessZone;
    public GameObject Needle;
    public TextMeshProUGUI QTEText;

    [Header("Lumières à allumer")]
    public Light[] spotLights;

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public float qteDuration = 5f;
    public float needleSpeed = 90f;

    [Header("QTE Multiple")]
    public int qteTotalRequired = 2;
    private int qteSuccessCount = 0;

    [Header("Rallumage")]
    [SerializeField] private AudioClip rallumageSound;
    [SerializeField] private AudioSource audioSource;

    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
        public float delayAfter;
    }
    [SerializeField] private VoiceLine[] voiceLines;

    [Header("Storyline")]
    [SerializeField] private UnityEvent onMinijeuCompleted;

    private bool playerNearby = false;
    private bool qteActive = false;
    private bool waitingForInput = false;
    private float timer = 0f;
    private float needleAngle = 0f;
    private float successAngle = 0f;

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
        if (promptObject != null)
            promptObject.SetActive(false);
        if (QTEPanel != null)
            QTEPanel.SetActive(false);

        foreach (Light light in spotLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(false);
                light.enabled = false;
            }
        }
    }

    void Update()
    {
        if (qteActive)
        {
            if (waitingForInput)
                UpdateQTE();
        }
        else
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

            if (!qteActive && interactAction.WasPressedThisFrame())
                StartCoroutine(LaunchQTENextFrame());
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

    IEnumerator LaunchQTENextFrame()
    {
        StartQTE();
        yield return null;
        yield return null;
        waitingForInput = true;
    }

    void StartQTE()
    {
        qteActive = true;
        waitingForInput = false;
        timer = qteDuration;
        needleAngle = 0f;

        successAngle = Random.Range(0f, 360f);

        if (promptObject != null)
            promptObject.SetActive(false);
        if (QTEPanel != null)
            QTEPanel.SetActive(true);
        if (SuccessZone != null)
        {
            SuccessZone.rectTransform.localEulerAngles = new Vector3(0, 0, -successAngle);
            SuccessZone.color = Color.green;
        }
        if (Needle != null)
            Needle.transform.localEulerAngles = Vector3.zero;
        if (QTEText != null)
            QTEText.text = "";
    }

    void UpdateQTE()
    {
        timer -= Time.deltaTime;

        needleAngle += needleSpeed * Time.deltaTime;
        if (needleAngle >= 360f)
            needleAngle -= 360f;

        if (Needle != null)
            Needle.transform.localEulerAngles = new Vector3(0, 0, -needleAngle);

        if (interactAction.WasPressedThisFrame())
        {
            CheckNeedlePosition();
            return;
        }

        if (timer <= 0f)
            QTEFail();
    }

    void CheckNeedlePosition()
    {
        float successZoneWidth = SuccessZone.GetComponent<RectTransform>().rect.width;
        float successZoneAngleWidth = (successZoneWidth / Needle.GetComponent<RectTransform>().rect.width) * 360f / 2f;

        float diff = Mathf.Abs(needleAngle - successAngle);
        if (diff > 180f)
            diff = 360f - diff;

        if (diff <= successZoneAngleWidth)
            QTESuccess();
        else
            QTEFail();
    }

    void QTESuccess()
    {
        qteSuccessCount++;

        if (qteSuccessCount >= qteTotalRequired)
        {
            qteActive = false;
            waitingForInput = false;
            qteSuccessCount = 0;

            if (QTEPanel != null)
                QTEPanel.SetActive(false);

            AllumerLumieres();
        }
        else
        {
            StartCoroutine(LaunchQTENextFrame());
        }
    }

    void AllumerLumieres()
    {
        foreach (Light light in spotLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
                light.enabled = true;
            }
        }

        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (!l.CompareTag("Flashlight"))
                l.enabled = true;
        }

        StartCoroutine(RallumageSequence());
    }

    IEnumerator RallumageSequence()
    {
        audioSource.PlayOneShot(rallumageSound);
        yield return new WaitForSeconds(rallumageSound.length);

        foreach (VoiceLine line in voiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            SubtitleManager.Instance.ShowSubtitle(line.subtitle);
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
        ObjectifManager.Instance.ActiverObjectif(1);
        onMinijeuCompleted.Invoke();
    }

    void QTEFail()
    {
        qteActive = false;
        waitingForInput = false;
        qteSuccessCount = 0;

        if (QTEPanel != null)
            QTEPanel.SetActive(false);

        playerNearby = false;
    }
}