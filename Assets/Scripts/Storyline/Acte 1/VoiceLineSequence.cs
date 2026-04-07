using System.Collections;
using UnityEngine;

public class VoiceLineSequence : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
        public float delayAfter;
    }

    [SerializeField] private VoiceLine[] introVoiceLines;
    [SerializeField] private VoiceLine[] afterBlackoutVoiceLines;
    [SerializeField] private AudioClip blackoutSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (StorylineManager.DebugMode) return;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (VoiceLine line in introVoiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            SubtitleManager.Instance.ShowSubtitle(line.subtitle);
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
        TriggerBlackout();
    }

    private void TriggerBlackout()
    {
        audioSource.PlayOneShot(blackoutSound);
        StartCoroutine(BlackoutDelay());
    }

    private IEnumerator BlackoutDelay()
    {
        yield return new WaitForSeconds(blackoutSound.length - 0.65f);

        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (!l.CompareTag("Flashlight"))
                l.enabled = false;
        }

        ObjectifManager.Instance.ActiverObjectif(0);
        StartCoroutine(PlayAfterBlackout());
    }

    private IEnumerator PlayAfterBlackout()
    {
        foreach (VoiceLine line in afterBlackoutVoiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            SubtitleManager.Instance.ShowSubtitle(line.subtitle);
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
    }
}