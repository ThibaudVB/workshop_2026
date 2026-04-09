using UnityEngine;

public class VentiloManager : MonoBehaviour
{
    public static VentiloManager Instance;
    [SerializeField] private VentiloFan[] ventilos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TurnAllOn()
    {
        foreach (VentiloFan v in ventilos)
            v.TurnOn();
    }

    public void TurnAllOff()
    {
        foreach (VentiloFan v in ventilos)
            v.TurnOff();
    }
}