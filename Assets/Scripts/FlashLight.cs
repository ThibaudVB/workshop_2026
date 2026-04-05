using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;
    public bool isOn = true;

    [Header("Flicker")]
    public bool enableFlicker = true;
    public float flickerSpeed = 0.05f;

    private float flickerTimer = 0f;
    private float targetIntensity;
    public float baseIntensity = 3f;

    void Start()
{
    if (flashlight == null)
        flashlight = GetComponentInChildren<Light>();

    // Lit l'intensité depuis l'Inspector au lieu du hardcode
    baseIntensity = flashlight.intensity;
    targetIntensity = baseIntensity;
    flashlight.enabled = isOn;
}

    void Update()
    {
        if (AlienMonster.IsDead) return;

        // On/Off avec F
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }

        // Flicker réaliste
        if (isOn && enableFlicker)
            HandleFlicker();
    }

    void HandleFlicker()
    {
        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0f)
        {
            // Légère variation d'intensité aléatoire
            targetIntensity = baseIntensity + Random.Range(-0.3f, 0.3f);
            flickerTimer = flickerSpeed + Random.Range(0f, 0.05f);
        }

        flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, Time.deltaTime * 20f);
    }
}