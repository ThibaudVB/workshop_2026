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
    public Transform[] waypoints;
    public float waypointStopTime = 1f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("Screamer RE7")]
    public MonoBehaviour playerController;
    public Camera playerCamera;
    public GameObject deathScreen;
    public float cameraTurnSpeed = 3f;
    public AudioClip screamerSound;

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

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (deathScreen != null)
            deathScreen.SetActive(false);

        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        UpdateAnimations();

        if (LockerDoor.IsPlayerHiding)
        {
            if (isChasing)
                isChasing = false;
            Patrol();
            return;
        }

        if (distanceToPlayer <= screamRange && !hasScreamed)
        {
            hasScreamed = true;
            StartCoroutine(ScreamerSequence());
            return;
        }

        if (distanceToPlayer < detectionRange)
            ChasePlayer();
        else
            Patrol();
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

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            StartCoroutine(WaitAtWaypoint());
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waypointStopTime);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        isWaiting = false;
    }

    IEnumerator ScreamerSequence()
    {
        // Stop le monstre
        agent.isStopped = true;
        isChasing = false;

        // Désactive les contrôles joueur
        if (playerController != null)
            playerController.enabled = false;

        // Son
        if (screamerSound != null && audioSource != null)
            audioSource.PlayOneShot(screamerSound);

        // 1. Tourne la caméra vers le monstre
        yield return StartCoroutine(TurnCameraToMonster());

        // 2. Lance l'anim du monstre
        if (animator != null)
            animator.SetTrigger("Grab");

        // 3. Shake
        yield return StartCoroutine(CameraShake(0.4f, 0.15f));

        // 4. Chute caméra
        yield return StartCoroutine(FallCamera());

        // 5. Écran de mort
        if (deathScreen != null)
            deathScreen.SetActive(true);
    }

    IEnumerator TurnCameraToMonster()
    {
        Quaternion startRot = playerCamera.transform.rotation;
        Vector3 dir = transform.position - playerCamera.transform.position;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * cameraTurnSpeed;
            playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            yield return null;
        }
    }

    IEnumerator FallCamera()
    {
        Vector3 startPos = playerCamera.transform.localPosition;
        Quaternion startRot = playerCamera.transform.localRotation;
        Vector3 endPos = new Vector3(startPos.x, -0.5f, startPos.z);
        Quaternion endRot = Quaternion.Euler(60f, startRot.eulerAngles.y, 15f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * 0.8f;
            playerCamera.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed);
            playerCamera.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed);
            yield return null;
        }
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            playerCamera.transform.localPosition = originalPos +
                new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
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

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}