using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;
    [SerializeField] private TextMeshProUGUI subtitleText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowSubtitle(string text)
    {
        subtitleText.text = text;
    }

    public void HideSubtitle()
    {
        subtitleText.text = "";
    }
}