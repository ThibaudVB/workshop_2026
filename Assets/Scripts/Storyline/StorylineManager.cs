using System.Collections;
using UnityEngine;

public class StorylineManager : MonoBehaviour
{
    public static StorylineManager Instance;
    public static bool DebugMode = false;

    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private int debugStartStep = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DebugMode = debugMode;
    }

    void Start()
    {
        if (debugMode)
            SkipToStep(debugStartStep);
    }

    private void SkipToStep(int step)
    {
        switch (step)
        {
            case 1:
                Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (Light l in allLights)
                    if (!l.CompareTag("Flashlight"))
                        l.enabled = false;
                ObjectifManager.Instance.ActiverObjectif(0);
                break;
            case 2:
                ObjectifManager.Instance.ActiverObjectif(1);
                break;
            case 3:
                ObjectifManager.Instance.ActiverObjectif(2);
                break;
            case 4:
                PickupSeringue.seringueCollected = true;
                TriggerSalleReunionCarl.carlDecouvert = true;
                ObjectifManager.Instance.ActiverObjectif(3);
                break;
            case 5:
                PickupSeringue.seringueCollected = true;
                TriggerSalleReunionCarl.carlDecouvert = true;
                AlienMonster.cinematicMode = false;
                break;
            case 6:
                PickupSeringue.seringueCollected = true;
                TriggerSalleReunionCarl.carlDecouvert = true;
                AlienMonster.cinematicMode = false;
                ObjectifManager.Instance.ActiverObjectif(4);
                break;
            case 7:
                PickupSeringue.seringueCollected = true;
                TriggerSalleReunionCarl.carlDecouvert = true;
                AlienMonster.cinematicMode = false;
                ObjectifManager.Instance.ActiverObjectif(5);
                break;
        }
    }

    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
        public float delayAfter;
    }

    public void PlayVoiceLines(VoiceLine[] voiceLines, System.Action onComplete = null)
    {
        StartCoroutine(PlaySequence(voiceLines, onComplete));
    }

    private IEnumerator PlaySequence(VoiceLine[] voiceLines, System.Action onComplete)
    {
        foreach (VoiceLine line in voiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            SubtitleManager.Instance.ShowSubtitle(line.subtitle);
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
        onComplete?.Invoke();
    }
}