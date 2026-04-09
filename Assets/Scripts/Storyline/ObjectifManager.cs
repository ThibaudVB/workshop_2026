using UnityEngine;
using TMPro;

public class ObjectifManager : MonoBehaviour
{
    public static ObjectifManager Instance;

    [System.Serializable]
    public class Objectif
    {
        public GameObject ping;
        [TextArea] public string texte;
    }

    [SerializeField] private Objectif[] objectifs;
    [SerializeField] private TextMeshProUGUI objectifText;
    [SerializeField] private GameObject objectifUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (objectifText != null)
            objectifText.text = "";

        if (objectifUI != null)
            objectifUI.SetActive(false);

        foreach (Objectif obj in objectifs)
            if (obj.ping != null)
                obj.ping.SetActive(false);
    }

    public void ActiverObjectif(int index)
    {
        foreach (Objectif obj in objectifs)
            if (obj.ping != null)
                obj.ping.SetActive(false);

        if (index >= 0 && index < objectifs.Length)
        {
            if (objectifs[index].ping != null)
                objectifs[index].ping.SetActive(true);

            if (objectifText != null)
                objectifText.text = objectifs[index].texte;

            if (objectifUI != null)
                objectifUI.SetActive(true);
        }
    }

    public void DesactiverTout()
    {
        foreach (Objectif obj in objectifs)
            if (obj.ping != null)
                obj.ping.SetActive(false);

        if (objectifText != null)
            objectifText.text = "";

        if (objectifUI != null)
            objectifUI.SetActive(false);
    }
}