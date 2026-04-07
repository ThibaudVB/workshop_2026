using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AlienMonster : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float screamRange = 1.5f;
    public float randomMoveRadius = 5f;
    public float randomMoveInterval = 3f;

    [Header("Screamer Setup")]
    public GameObject screamerScene;        // La scène entière du screamer
    public GameObject screamerPanel;        // Le panel noir UI
    public Animator screamerMonsterAnimator; // L'Animator du monstre dans la scène screamer
    public string screamerAnimationTrigger = "Scream"; // Le trigger de l'animation screamer
    public AudioClip screamerSound;
    public float screamerDuration = 3f;

    [Header("Cameras")]
    public Camera mainCamera;               // La caméra principale du joueur
    public Camera screamerCamera;           // La caméra de la scène screamer

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private float timer;
    private bool hasScreamed = false;
    private bool isChasing = false;
    private Vector3 playerSpawnPoint;
    private Vector3 monsterSpawnPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        playerSpawnPoint = player.position;
        monsterSpawnPoint = transform.position;

        // Trouver la main camera si pas assignée
        if (mainCamera == null)
            mainCamera = Camera.main;

        // S'assurer que le screamer est désactivé au départ
        if (screamerScene != null)
            screamerScene.SetActive(false);
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Mettre à jour les animations
        UpdateAnimations();

        if (distanceToPlayer <= screamRange && !hasScreamed)
        {
            hasScreamed = true;
            StartCoroutine(ScreamerSequence());
            return;
        }

        if (distanceToPlayer < detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            WanderRandomly();
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsChasing", isChasing);
    }

    void ChasePlayer()
    {
        agent.speed = 30f;
        agent.SetDestination(player.position);
        isChasing = true;
    }

    void WanderRandomly()
    {
        agent.speed = 1.5f;
        isChasing = false;
        timer += Time.deltaTime;

        if (timer >= randomMoveInterval)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * randomMoveRadius;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPos, out hit, randomMoveRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            timer = 0f;
        }
    }

    IEnumerator ScreamerSequence()
    {
        // 1. Arrêter le monstre
        agent.isStopped = true;
        isChasing = false;

        // 2. Désactiver la caméra principale
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        // 3. Activer le panel noir
        if (screamerPanel != null)
            screamerPanel.SetActive(true);

        // 4. Activer la scène screamer + caméra
        if (screamerScene != null)
            screamerScene.SetActive(true);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(true);

        // 5. Jouer l'animation du monstre screamer
        if (screamerMonsterAnimator != null)
        {
            screamerMonsterAnimator.SetTrigger(screamerAnimationTrigger);
        }

        // 6. Jouer le son
        if (screamerSound != null && audioSource != null)
            audioSource.PlayOneShot(screamerSound);

        // 7. Attendre la durée du screamer
        yield return new WaitForSeconds(screamerDuration);

        // 8. Désactiver le screamer
        if (screamerScene != null)
            screamerScene.SetActive(false);
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(false);

        // 9. Réactiver la caméra principale
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        // 10. Respawn joueur
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.position = playerSpawnPoint;
        }
        else
        {
            player.position = playerSpawnPoint;
        }

        // 11. Respawn monstre
        agent.enabled = false;
        transform.position = monsterSpawnPoint;
        agent.enabled = true;
        agent.isStopped = false;

        // 12. Reset
        hasScreamed = false;
        isChasing = false;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", false);
        }
    }
}