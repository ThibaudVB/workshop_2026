using UnityEngine;
using UnityEngine.Events;

public class ObjectifElectricite : MonoBehaviour
{
    private Light objectifLight;
    private AudioSource objectifSound;

    [SerializeField] private UnityEvent onObjectifReached;

    void Awake()
    {
        objectifLight = GetComponent<Light>();
        objectifSound = GetComponent<AudioSource>();
    }

    public void ActivateObjectif()
    {
        objectifLight.enabled = true;
        objectifSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectifLight.enabled = false;
            objectifSound.Stop();
            onObjectifReached.Invoke();
        }
    }
}