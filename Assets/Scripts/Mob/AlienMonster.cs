using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AlienMonster : MonoBehaviour
{
    public static bool IsDead = false;

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

    [Header("Post Processing")]
    public Volume postProcessVolume;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private bool hasScreamed = false;
    private bool isChasing = false;
    private Vector3 playerSpawnPoint;
    private Vector3 monsterSpawnPoint;

    private bool isCameraFrozen = false;
    private Vector3 frozenCameraPos;
    private Quaternion frozenCameraRot;
    private Vector3 frozenPlayerPos;

    void Start()
    {
        IsDead = false;
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
        if (IsDead) return;

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

    void LateUpdate()
    {
        if (isCameraFrozen)
        {
            player.position = frozenPlayerPos;
            playerCamera.transform.localPosition = frozenCameraPos;
            playerCamera.transform.localRotation = frozenCameraRot;
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
        // Désactive le Depth of Field
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<DepthOfField>(out var dof))
                dof.active = false;
        }

        // Éteint la flashlight
        foreach (Light l in playerCamera.GetComponentsInChildren<Light>())
            l.enabled = false;

        agent.isStopped = true;
        isChasing = false;
        IsDead = true;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (screamerSound != null && audioSource != null)
            audioSource.PlayOneShot(screamerSound);

        // 1. Tourne vers le monstre
        yield return StartCoroutine(TurnCameraToMonster());

        // 2. Lance l'anim
        if (animator != null)
            animator.SetTrigger("Grab");

        // 3. Lève la caméra pour voir le monstre
        yield return StartCoroutine(LookUpAtMonster());

        // 4. Shake
        yield return StartCoroutine(CameraShake(0.4f, 0.15f));

        // 5. Tombe
        yield return StartCoroutine(FallCamera());

        // 6. Écran de mort
        if (deathScreen != null)
            deathScreen.SetActive(true);
    }

    IEnumerator TurnCameraToMonster()
    {
        Vector3 dir = transform.position - playerCamera.transform.position;
        Quaternion worldTarget = Quaternion.LookRotation(dir);

        Quaternion localStart = playerCamera.transform.localRotation;
        Quaternion localTarget = Quaternion.Inverse(playerCamera.transform.parent.rotation) * worldTarget;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * cameraTurnSpeed;
            playerCamera.transform.localRotation = Quaternion.Slerp(localStart, localTarget, elapsed);
            yield return null;
        }
    }

    IEnumerator LookUpAtMonster()
    {
        Quaternion startRot = playerCamera.transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(-20f, startRot.eulerAngles.y, 0f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * 2f;
            playerCamera.transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            yield return null;
        }
    }

    IEnumerator FallCamera()
    {
        Vector3 startPos = playerCamera.transform.localPosition;
        Quaternion startRot = playerCamera.transform.localRotation;
        Vector3 endPos = new Vector3(startPos.x, 0.2f, startPos.z);
        Quaternion endRot = Quaternion.Euler(-80f, startRot.eulerAngles.y, 0f);

        yield return new WaitForSeconds(1f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * 0.8f;
            float smoothT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed));
            playerCamera.transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
            playerCamera.transform.localRotation = Quaternion.Slerp(startRot, endRot, smoothT);
            yield return null;
        }

        // Force la position finale
        playerCamera.transform.localPosition = endPos;
        playerCamera.transform.localRotation = endRot;

        frozenCameraPos = endPos;
        frozenCameraRot = endRot;
        frozenPlayerPos = player.position;
        isCameraFrozen = true;
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