using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Gère tous les effets visuels et sonores de la peur.
/// VERSION OPTIMISÉE - À placer sur le Player.
/// </summary>
public class FearEffects : MonoBehaviour
{
    [Header("=== RÉFÉRENCES ===")]
    public FearSystem fearSystem;
    public Camera playerCamera;
    public PostProcessVolume postProcessVolume;

    [Header("=== SONS ===")]
    public AudioClip heartbeatSound;
    public AudioClip breathingSound;
    [Range(0f, 1f)] public float maxHeartbeatVolume = 0.8f;
    [Range(0f, 1f)] public float maxBreathingVolume = 0.5f;

    [Header("=== BATTEMENT DE COEUR ===")]
    public float minHeartbeatInterval = 1.2f;
    public float maxHeartbeatInterval = 0.3f;

    [Header("=== VIGNETTE POST-PROCESS ===")]
    public Color vignetteColor = new Color(0.5f, 0f, 0f, 1f);
    public float maxVignetteIntensity = 0.6f;

    [Header("=== ABERRATION CHROMATIQUE ===")]
    public float maxChromaticAberration = 1f;

    [Header("=== GRAIN ===")]
    public float maxGrainIntensity = 0.5f;

    [Header("=== SCREEN SHAKE ===")]
    public float maxShakeIntensity = 0.08f;
    public float shakeSpeed = 15f;

    // Post-process effects (cached)
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Grain grain;
    private ColorGrading colorGrading;

    // Audio (cached)
    private AudioSource heartbeatSource;
    private AudioSource breathingSource;
    private float heartbeatTimer = 0f;

    // Shake
    private Vector3 originalCameraPos;
    private float shakeOffset = 0f;

    // Optimisation : cache les valeurs précédentes
    private float lastFear = -1f;
    private float updateInterval = 0.05f; // Update post-process tous les 50ms
    private float updateTimer = 0f;

    void Start()
    {
        if (fearSystem == null)
            fearSystem = GetComponent<FearSystem>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
            originalCameraPos = playerCamera.transform.localPosition;

        SetupAudioSources();
        SetupPostProcessing();
    }

    void SetupAudioSources()
    {
        // Heartbeat - réutilise un AudioSource existant ou en crée un
        heartbeatSource = gameObject.AddComponent<AudioSource>();
        heartbeatSource.spatialBlend = 0f;
        heartbeatSource.loop = false;
        heartbeatSource.playOnAwake = false;

        // Breathing
        breathingSource = gameObject.AddComponent<AudioSource>();
        breathingSource.spatialBlend = 0f;
        breathingSource.loop = true;
        breathingSource.playOnAwake = false;
        if (breathingSound != null)
            breathingSource.clip = breathingSound;
    }

    void SetupPostProcessing()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindAnyObjectByType<PostProcessVolume>();

        if (postProcessVolume == null || postProcessVolume.profile == null)
        {
            Debug.LogWarning("FearEffects: Pas de PostProcessVolume trouvé !");
            return;
        }

        PostProcessProfile profile = postProcessVolume.profile;

        // Cache les références une seule fois
        profile.TryGetSettings(out vignette);
        profile.TryGetSettings(out chromaticAberration);
        profile.TryGetSettings(out grain);
        profile.TryGetSettings(out colorGrading);

        // Active si trouvé
        if (vignette != null)
        {
            vignette.enabled.Override(true);
            vignette.color.Override(vignetteColor);
        }
        if (chromaticAberration != null)
            chromaticAberration.enabled.Override(true);
        if (grain != null)
        {
            grain.enabled.Override(true);
            grain.colored.Override(false);
        }
        if (colorGrading != null)
            colorGrading.enabled.Override(true);
    }

    void Update()
    {
        if (fearSystem == null) return;

        float fear = fearSystem.CurrentFear;

        // Optimisation : update le post-processing moins souvent
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval || Mathf.Abs(fear - lastFear) > 0.05f)
        {
            UpdatePostProcessing(fear);
            lastFear = fear;
            updateTimer = 0f;
        }

        // Ces effets doivent rester fluides
        UpdateHeartbeat(fear);
        UpdateBreathing(fear);
        UpdateScreenShake(fear);
    }

    void UpdatePostProcessing(float fear)
    {
        if (vignette != null)
            vignette.intensity.Override(fear * maxVignetteIntensity);

        if (chromaticAberration != null)
            chromaticAberration.intensity.Override(fear * maxChromaticAberration);

        if (grain != null)
            grain.intensity.Override(fear * maxGrainIntensity);

        if (colorGrading != null)
            colorGrading.saturation.Override(fear * -30f);
    }

    void UpdateHeartbeat(float fear)
    {
        if (heartbeatSound == null || fear < 0.05f) return;

        float interval = Mathf.Lerp(minHeartbeatInterval, maxHeartbeatInterval, fear);
        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer >= interval)
        {
            heartbeatTimer = 0f;
            float volume = Mathf.Lerp(0.2f, 1f, fear) * maxHeartbeatVolume;
            heartbeatSource.PlayOneShot(heartbeatSound, volume);
        }
    }

    void UpdateBreathing(float fear)
    {
        if (breathingSound == null) return;

        if (fear > 0.15f)
        {
            if (!breathingSource.isPlaying)
                breathingSource.Play();

            breathingSource.volume = (fear - 0.15f) / 0.85f * maxBreathingVolume;
            breathingSource.pitch = Mathf.Lerp(0.8f, 1.3f, fear);
        }
        else if (breathingSource.isPlaying)
        {
            breathingSource.Stop();
        }
    }

    void UpdateScreenShake(float fear)
    {
        if (playerCamera == null) return;

        if (fear > 0.3f)
        {
            // Shake simple et optimisé avec sin/cos au lieu de PerlinNoise
            shakeOffset += Time.deltaTime * shakeSpeed;
            float intensity = (fear - 0.3f) / 0.7f * maxShakeIntensity;

            float offsetX = Mathf.Sin(shakeOffset * 1.1f) * intensity;
            float offsetY = Mathf.Cos(shakeOffset * 1.3f) * intensity;

            playerCamera.transform.localPosition = new Vector3(
                originalCameraPos.x + offsetX,
                originalCameraPos.y + offsetY,
                originalCameraPos.z
            );
        }
        else
        {
            // Retour smooth à la position normale
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                originalCameraPos,
                10f * Time.deltaTime
            );
        }
    }

    void OnDisable()
    {
        // Reset propre
        if (vignette != null) vignette.intensity.Override(0f);
        if (chromaticAberration != null) chromaticAberration.intensity.Override(0f);
        if (grain != null) grain.intensity.Override(0f);
        if (colorGrading != null) colorGrading.saturation.Override(0f);

        if (playerCamera != null)
            playerCamera.transform.localPosition = originalCameraPos;
    }
}
