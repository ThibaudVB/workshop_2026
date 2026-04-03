using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AlienMonster : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float screamRange = 1.5f;

    [Header("Speeds")]
    public float walkSpeed = 3f;
    public float runSpeed = 10f;

    [Header("Patrol Waypoints")]
    public Transform[] waypoints;           // Les points de patrouille
    public float waypointStopTime = 1f;     // Temps d'arrêt à chaque point
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("Screamer Setup")]
    public GameObject screamerScene;
    public GameObject screamerPanel;
    public Animator screamerMonsterAnimator;
    public string screamerAnimationTrigger = "Scream";
    public AudioClip screamerSound;
    public float screamerDuration = 3f;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera screamerCamera;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
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

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (screamerScene != null)
            screamerScene.SetActive(false);
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(false);

        // Aller au premier waypoint
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

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
            Patrol();
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
        agent.speed = runSpeed;
        agent.SetDestination(player.position);
        isChasing = true;
        isWaiting = false;
        StopCoroutine("WaitAtWaypoint");
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        if (isWaiting) return;

        isChasing = false;
        agent.speed = walkSpeed;

        // Vérifie si on est arrivé au waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        
        // Attendre au waypoint
        yield return new WaitForSeconds(waypointStopTime);

        // Passer au waypoint suivant
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        isWaiting = false;
    }

    IEnumerator ScreamerSequence()
    {
        agent.isStopped = true;
        isChasing = false;

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        if (screamerPanel != null)
            screamerPanel.SetActive(true);

        if (screamerScene != null)
            screamerScene.SetActive(true);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(true);

        if (screamerMonsterAnimator != null)
        {
            screamerMonsterAnimator.SetTrigger(screamerAnimationTrigger);
        }

        if (screamerSound != null && audioSource != null)
            audioSource.PlayOneShot(screamerSound);

        yield return new WaitForSeconds(screamerDuration);

        if (screamerScene != null)
            screamerScene.SetActive(false);
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
        if (screamerCamera != null)
            screamerCamera.gameObject.SetActive(false);

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

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

        agent.enabled = false;
        transform.position = monsterSpawnPoint;
        agent.enabled = true;
        agent.isStopped = false;

        // Reset waypoint au plus proche
        currentWaypointIndex = GetClosestWaypointIndex();
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);

        hasScreamed = false;
        isChasing = false;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", false);
        }
    }

    int GetClosestWaypointIndex()
    {
        int closest = 0;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }

        return closest;
    }

    // Dessine les waypoints dans l'éditeur
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Dessine une sphère à chaque waypoint
            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

            // Dessine une ligne vers le prochain waypoint
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
    }
}
