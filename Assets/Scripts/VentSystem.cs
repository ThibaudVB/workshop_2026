using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class VentSystem : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rampingSound;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float rampingDuration = 3f;

    private bool playerInRange = false;
    private bool isUsing = false;
    private PlayerController playerController;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && !isUsing && Keyboard.current.eKey.wasPressedThisFrame)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                StartCoroutine(VentSequence());
        }
    }

    IEnumerator VentSequence()
    {
        isUsing = true;

        if (promptUI != null)
            promptUI.SetActive(false);

        playerController.enabled = false;
        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;

        // Coupe tous les sons sauf le nôtre
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        bool[] wasPlaying = new bool[allSources.Length];
        for (int i = 0; i < allSources.Length; i++)
        {
            wasPlaying[i] = allSources[i].isPlaying;
            if (allSources[i] != audioSource)
                allSources[i].Pause();
        }

        yield return StartCoroutine(Fade(0f, 1f));

        if (rampingSound != null && audioSource != null)
            audioSource.PlayOneShot(rampingSound);

        yield return new WaitForSeconds(rampingDuration);

        playerController.transform.position = exitPoint.position;
        playerController.transform.rotation = exitPoint.rotation;

        yield return StartCoroutine(Fade(1f, 0f));

        // Restaure tous les sons
        for (int i = 0; i < allSources.Length; i++)
        {
            if (wasPlaying[i] && allSources[i] != audioSource)
                allSources[i].UnPause();
        }

        playerController.enabled = true;
        isUsing = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = to;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null && !isUsing)
                promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}