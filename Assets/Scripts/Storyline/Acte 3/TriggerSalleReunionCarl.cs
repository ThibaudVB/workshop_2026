using UnityEngine;

public class TriggerSalleReunionCarl : MonoBehaviour
{
    public static bool carlDecouvert = false;

    [SerializeField] private StorylineManager.VoiceLine[] decouverteVoiceLines;
    [SerializeField] private StorylineManager.VoiceLine[] finalVoiceLines;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered && PickupSeringue.seringueCollected)
        {
            triggered = true;
            carlDecouvert = true;
            AlienMonster.cinematicMode = false;
            StorylineManager.Instance.PlayVoiceLines(decouverteVoiceLines, () =>
            {
                StorylineManager.Instance.PlayVoiceLines(finalVoiceLines, () =>
                {
                    ObjectifManager.Instance.ActiverObjectif(4);
                });
            });
        }
    }
}