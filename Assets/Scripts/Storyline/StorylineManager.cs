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
                AutoDestructionManager.Instance.StartCountdown();
                break;
        }
    }

    [System.Serializable]
    public class SubtitleLine
    {
        [TextArea] public string text;
        public float showAtTime;
    }

    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        public SubtitleLine[] subtitles;
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
            StartCoroutine(PlaySubtitles(line.subtitles, line.clip.length));
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
        onComplete?.Invoke();
    }

    private IEnumerator PlaySubtitles(SubtitleLine[] subtitles, float clipDuration)
    {
        if (subtitles == null || subtitles.Length == 0) yield break;

        float elapsed = 0f;
        int index = 0;

        while (elapsed < clipDuration)
        {
            if (index < subtitles.Length && elapsed >= subtitles[index].showAtTime)
            {
                SubtitleManager.Instance.ShowSubtitle(subtitles[index].text);
                index++;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}