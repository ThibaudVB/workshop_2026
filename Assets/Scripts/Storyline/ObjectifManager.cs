using UnityEngine;

public class ObjectifManager : MonoBehaviour
{
    public static ObjectifManager Instance;

    [SerializeField] private GameObject[] objectifs;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ActiverObjectif(int index)
    {
        // Désactive tous les objectifs
        foreach (GameObject obj in objectifs)
            obj.SetActive(false);

        // Active uniquement celui voulu
        if (index >= 0 && index < objectifs.Length)
            objectifs[index].SetActive(true);
    }

    public void DesactiverTout()
    {
        foreach (GameObject obj in objectifs)
            obj.SetActive(false);
    }
}