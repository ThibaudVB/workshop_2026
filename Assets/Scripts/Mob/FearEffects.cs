using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FearEffects : MonoBehaviour
{
    [Header("=== RÉFÉRENCES ===")]
    public FearSystem fearSystem;
    public Camera playerCamera;
    public Volume postProcessVolume;

    [Header("=== SONS ===")]
    public AudioClip heartbeatSound;
    public AudioClip breathingSound;
    [Range(0f, 1f)] public float maxHeartbeatVolume = 0.8f;
    [Range(0f, 1f)] public float maxBreathingVolume = 0.5f;

    [Header("=== BATTEMENT DE COEUR ===")]
    public float minHeartbeatInterval = 1.2f;
    public float maxHeartbeatInterval = 0.3f;

    [Header("=== VIGNETTE ===")]
    public float maxVignetteIntensity = 0.6f;

    [Header("=== ABERRATION CHROMATIQUE ===")]
    public float maxChromaticAberration = 1f;

    [Header("=== SCREEN SHAKE ===")]
    public float maxShakeIntensity = 0.08f;
    public float shakeSpeed = 15f;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private AudioSource heartbeatSource;
    private AudioSource breathingSource;
    private float heartbeatTimer = 0f;
    private Vector3 originalCameraPos;
    private float shakeOffset = 0f;
    private float lastFear = -1f;
    private float updateTimer = 0f;
    private float updateInterval = 0.05f;

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
        heartbeatSource = gameObject.AddComponent<AudioSource>();
        heartbeatSource.spatialBlend = 0f;
        heartbeatSource.loop = false;
        heartbeatSource.playOnAwake = false;

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
            postProcessVolume = FindAnyObjectByType<Volume>();

        if (postProcessVolume == null || postProcessVolume.profile == null)
        {
            Debug.LogWarning("FearEffects: Pas de Volume trouvé !");
            return;
        }

        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out chromaticAberration);
    }

    void Update()
    {
        if (fearSystem == null) return;

        float fear = fearSystem.CurrentFear;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval || Mathf.Abs(fear - lastFear) > 0.05f)
        {
            UpdatePostProcessing(fear);
            lastFear = fear;
            updateTimer = 0f;
        }

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
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                originalCameraPos,
                10f * Time.deltaTime
            );
        }
    }

    void OnDisable()
    {
        if (vignette != null) vignette.intensity.Override(0f);
        if (chromaticAberration != null) chromaticAberration.intensity.Override(0f);

        if (playerCamera != null)
            playerCamera.transform.localPosition = originalCameraPos;
    }
}