using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CapsuleFinale : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject promptObject;

    [Header("Cinématique")]
    [SerializeField] private Animator capsuleAnimator;
    [SerializeField] private StorylineManager.VoiceLine[] voiceLines;

    private InputAction interactAction;
    private bool triggered = false;
    private bool playerNearby = false;

    void Awake()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.Enable();
    }

    void OnDestroy()
    {
        interactAction.Disable();
        interactAction.Dispose();
    }

    void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
    }

    void Update()
    {
        if (triggered) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (!playerNearby && promptObject != null)
            {
                playerNearby = true;
                promptObject.SetActive(true);
            }

            if (interactAction.WasPressedThisFrame())
                StartCoroutine(LaunchEnding());
        }
        else
        {
            if (playerNearby && promptObject != null)
            {
                playerNearby = false;
                promptObject.SetActive(false);
            }
        }
    }

    private IEnumerator LaunchEnding()
    {
        triggered = true;
        if (promptObject != null) promptObject.SetActive(false);

        // Bloque le joueur
        PlayerController.cinematicMode = true;
        AlienMonster.cinematicMode = true;

        // Stoppe le timer
        // AutoDestructionManager.Instance.StopCountdown();

        // Voicelines finales
        StorylineManager.Instance.PlayVoiceLines(voiceLines, () =>
        {
            // TODO : lancer l'animation de la capsule
            // capsuleAnimator.SetTrigger("Launch");

            // TODO : cinématique de fin (explosion station)
        });

        yield return null;
    }
}