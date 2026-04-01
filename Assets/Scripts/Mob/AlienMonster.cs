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

    [Header("Screamer")]
    public GameObject screamerImage;
    public AudioClip screamerSound;
    public float screamerDuration = 1f;

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

        // Envoie la vitesse à l'Animator
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        
        // Envoie l'état de chasse
        animator.SetBool("IsChasing", isChasing);
    }

    void ChasePlayer()
    {
        agent.speed = 3.5f;
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
        agent.isStopped = true;
        isChasing = false;

        // Jouer l'animation Attack
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Affiche screamer
        if (screamerImage != null)
            screamerImage.SetActive(true);

        // Joue le son
        if (screamerSound != null && audioSource != null)
            audioSource.PlayOneShot(screamerSound);

        // Attendre
        yield return new WaitForSeconds(screamerDuration);

        // Cache screamer
        if (screamerImage != null)
            screamerImage.SetActive(false);

        // Respawn joueur
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

        // Respawn monstre
        agent.enabled = false;
        transform.position = monsterSpawnPoint;
        agent.enabled = true;
        agent.isStopped = false;

        // Reset
        hasScreamed = false;
        isChasing = false;

        // Reset animations
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", false);
        }
    }
}
