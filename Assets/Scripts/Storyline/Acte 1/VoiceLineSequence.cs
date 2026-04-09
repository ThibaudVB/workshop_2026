using System.Collections;
using UnityEngine;

public class VoiceLineSequence : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        public StorylineManager.SubtitleLine[] subtitles;
        public float delayAfter;
    }

    [SerializeField] private VoiceLine[] introVoiceLines;
    [SerializeField] private VoiceLine[] afterBlackoutVoiceLines;
    [SerializeField] private AudioClip blackoutSound;
    [SerializeField] private AudioSource ambianceAudioSource;
    [SerializeField] private AudioClip pressionSound;
    private AudioSource audioSource;
    private Animator playerAnimator;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerAnimator = GameObject.FindWithTag("Player").GetComponentInChildren<Animator>();

        if (StorylineManager.DebugMode) return;

        PlayerController.blockMovement = true;
        playerAnimator.SetBool("SitDown", true);

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (VoiceLine line in introVoiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            StartCoroutine(PlaySubtitles(line.subtitles, line.clip.length));
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
            if (!l.CompareTag("Flashlight"))
                l.enabled = false;

        VentiloManager.Instance.TurnAllOff();

        playerAnimator.SetBool("SitDown", false);
        PlayerController.blockMovement = false;

        // Lance le son de pression ambiant
        if (ambianceAudioSource != null && pressionSound != null)
        {
            ambianceAudioSource.clip = pressionSound;
            ambianceAudioSource.loop = true;
            ambianceAudioSource.Play();
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
            StartCoroutine(PlaySubtitles(line.subtitles, line.clip.length));
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
    }

    private IEnumerator PlaySubtitles(StorylineManager.SubtitleLine[] subtitles, float clipDuration)
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