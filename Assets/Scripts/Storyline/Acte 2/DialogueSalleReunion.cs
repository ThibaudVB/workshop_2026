using System.Collections;
using UnityEngine;

public class DialogueSalleReunion : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
        public float delayAfter;
    }

    [SerializeField] private VoiceLine[] voiceLines;
    [SerializeField] private AudioSource audioSource;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            ObjectifManager.Instance.DesactiverTout();
            StartCoroutine(PlayDialogue());
        }
    }

    private IEnumerator PlayDialogue()
    {
        foreach (VoiceLine line in voiceLines)
        {
            audioSource.clip = line.clip;
            audioSource.Play();
            SubtitleManager.Instance.ShowSubtitle(line.subtitle);
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        SubtitleManager.Instance.HideSubtitle();
    }
}