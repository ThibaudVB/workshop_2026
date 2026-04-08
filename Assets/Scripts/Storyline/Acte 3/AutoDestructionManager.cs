using System.Collections;
using UnityEngine;
using TMPro;

public class AutoDestructionManager : MonoBehaviour
{
    public static AutoDestructionManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Son")]
    [SerializeField] private AudioSource alarmAudioSource;
    [SerializeField] private AudioClip alarmSound;

    [Header("Lumières")]
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Storyline")]
    [SerializeField] private StorylineManager.VoiceLine[] startVoiceLines;

    private float timeRemaining = 60f;
    private bool isRunning = false;
    private Light[] sceneLights;
    private bool pulsing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (timerPanel != null) timerPanel.SetActive(false);
    }

    public void StartCountdown()
    {
        if (startVoiceLines != null && startVoiceLines.Length > 0)
            StorylineManager.Instance.PlayVoiceLines(startVoiceLines, () => LaunchCountdown());
        else
            LaunchCountdown();
    }

    private void LaunchCountdown()
{
    Debug.Log("LaunchCountdown appelé");
    isRunning = true;
    timerPanel.SetActive(true);
    Debug.Log("TimerPanel activé");

    alarmAudioSource.clip = alarmSound;
    alarmAudioSource.loop = true;
    alarmAudioSource.Play();
    Debug.Log("Alarme lancée");

    sceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
    Debug.Log("Lumières trouvées : " + sceneLights.Length);
    pulsing = true;
    StartCoroutine(PulseLights());
    StartCoroutine(Countdown());
}

    private IEnumerator PulseLights()
    {
        while (pulsing)
        {
            float intensity = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            foreach (Light l in sceneLights)
            {
                if (!l.CompareTag("Flashlight"))
                {
                    l.color = Color.red;
                    l.intensity = Mathf.Lerp(0.2f, 20f, intensity);
                }
            }
            yield return null;
        }
    }

    private IEnumerator Countdown()
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateDisplay();
            yield return null;
        }

        timeRemaining = 0;
        UpdateDisplay();
        pulsing = false;
        // TODO: Game Over si le joueur n'est pas dans la capsule
    }

    private void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeRemaining <= 15f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }
}