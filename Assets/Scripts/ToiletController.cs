using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ToiletController : MonoBehaviour
{
    [Header("Références")]
    public Transform sitPosition;
    public Transform player;
    public Camera playerCam;
    public ParticleSystem fartSmoke;

    [Header("Sons")]
    public AudioClip[] fartSounds;
    public float minFartInterval = 1f;
    public float maxFartInterval = 3f;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.05f;

    [Header("Distance")]
    public float sitDistance = 2f;

    private bool isSitting = false;
    private Vector3 playerOriginalPosition;
    private Quaternion playerOriginalRotation;
    private Quaternion camOriginalRotation;
    private AudioSource audioSource;
    private PlayerController playerController;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        playerController = player.GetComponent<PlayerController>();

        if (fartSmoke != null)
            fartSmoke.Stop();
    }

    void Update()
    {
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (!isSitting && distance < sitDistance)
        {
            SitDown();
        }
        else if (isSitting)
        {
            StandUp();
        }
    }

    void SitDown()
    {
        if (sitPosition == null || playerCam == null || playerController == null) 
        {
            Debug.LogError("ToiletController: références manquantes !");
            return;
        }

        isSitting = true;
        playerOriginalPosition = player.position;
        playerOriginalRotation = player.rotation;
        camOriginalRotation = playerCam.transform.localRotation;

        player.position = sitPosition.position;
        player.rotation = sitPosition.rotation;
        playerCam.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        playerController.enabled = false;

        if (fartSmoke != null) fartSmoke.Play();

        StartCoroutine(FartLoop());
        StartCoroutine(CameraShakeLoop());
    }

    void StandUp()
    {
        isSitting = false;
        StopAllCoroutines();

        player.position = playerOriginalPosition;
        player.rotation = playerOriginalRotation;
        playerCam.transform.localRotation = camOriginalRotation;
        playerController.enabled = true;

        if (fartSmoke != null) fartSmoke.Stop();
    }

    IEnumerator FartLoop()
    {
        while (isSitting)
        {
            float wait = Random.Range(minFartInterval, maxFartInterval);
            yield return new WaitForSeconds(wait);
            if (fartSounds != null && fartSounds.Length > 0)
            {
                AudioClip fart = fartSounds[Random.Range(0, fartSounds.Length)];
                audioSource.PlayOneShot(fart);
            }
        }
    }

    IEnumerator CameraShakeLoop()
    {
        Vector3 originalCamPos = playerCam.transform.localPosition;
        while (isSitting)
        {
            playerCam.transform.localPosition = originalCamPos + Random.insideUnitSphere * shakeIntensity;
            yield return new WaitForSeconds(0.05f);
        }
        playerCam.transform.localPosition = originalCamPos;
    }
}