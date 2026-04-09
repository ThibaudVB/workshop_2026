using UnityEngine;

public class PickupSeringue : Pickup
{
    public static bool seringueCollected = false;
    public CarlController carl;

    [System.Serializable]
    public class VoiceLine : StorylineManager.VoiceLine { }

    [SerializeField] private StorylineManager.VoiceLine[] voiceLines;

    protected override void OnCollected()
    {
        seringueCollected = true;
        carl.OnSoinRecupere();
        ObjectifManager.Instance.DesactiverTout();
        StorylineManager.Instance.PlayVoiceLines(voiceLines, () =>
        {
            ObjectifManager.Instance.ActiverObjectif(3);
        });
    }
}