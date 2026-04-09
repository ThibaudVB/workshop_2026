using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AlienMonster : MonoBehaviour
{
    public static bool IsDead = false;
    public static bool cinematicMode = false;

    public Transform player;
    public float screamRange = 1.5f;

    [Header("Détection FOV")]
    public float fovAngle = 120f;
    public float fovRange = 15f;
    public LayerMask obstacleMask;

    [Header("Détection Son")]
    public float soundRangeWalk = 5f;
    public float soundRangeRun = 12f;
    public float soundRangeCrouch = 0f;

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

    [Header("Screamer Camera")]
    [SerializeField] private float cameraHeightOffset = 1.5f;
    [SerializeField] private bool overrideCameraPosition = true;

    [Header("Screamer Player Position")]
    [SerializeField] private Vector3 playerPositionOffset = new Vector3(0f, 0f, -1f);

    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Sons")]
    [SerializeField] private AudioClip patrolSound;
    [SerializeField] private AudioClip[] chaseSounds;
    [SerializeField] private AudioSource voiceAudioSource;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private bool hasScreamed = false;
    private bool isChasing = false;
    private bool wasChasing = false;

    private bool isCameraFrozen = false;
    private Vector3 frozenCameraPos;
    private Quaternion frozenCameraRot;
    private Vector3 frozenPlayerPos;

    IEnumerator Start()
    {
        IsDead = false;
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (deathScreen != null)
            deathScreen.SetActive(false);

        yield return null;

        if (agent.isOnNavMesh && waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);

        // Lance le son de patrouille au démarrage
        if (voiceAudioSource != null && patrolSound != null)
        {
            voiceAudioSource.clip = patrolSound;
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }
    }

    void Update()
    {
        if (IsDead || cinematicMode) return;
        if (!agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        UpdateAnimations();

        if (LockerDoor.IsPlayerHiding)
        {
            if (isChasing)
            {
                isChasing = false;
                SwitchToPatrolSound();
            }
            Patrol();
            return;
        }

        if (distanceToPlayer <= screamRange && !hasScreamed)
        {
            hasScreamed = true;
            StartCoroutine(ScreamerSequence());
            return;
        }

        if (CanDetectPlayer())
            ChasePlayer();
        else if (!isChasing)
            Patrol();
        else
        {
            if (agent.remainingDistance < 0.5f)
            {
                isChasing = false;
                SwitchToPatrolSound();
                Patrol();
            }
        }

        // Détecte le changement d'état pour les sons
        if (isChasing && !wasChasing)
        {
            StartCoroutine(PlayChaseSounds());
            wasChasing = true;
        }
        else if (!isChasing && wasChasing)
        {
            wasChasing = false;
        }
    }

    void SwitchToPatrolSound()
    {
        if (voiceAudioSource == null || patrolSound == null) return;
        voiceAudioSource.loop = true;
        voiceAudioSource.clip = patrolSound;
        voiceAudioSource.Play();
    }

    IEnumerator PlayChaseSounds()
    {
        if (voiceAudioSource == null || chaseSounds == null || chaseSounds.Length == 0) yield break;

        voiceAudioSource.loop = false;
        voiceAudioSource.Stop();

        while (isChasing)
        {
            AudioClip clip = chaseSounds[Random.Range(0, chaseSounds.Length)];
            voiceAudioSource.clip = clip;
            voiceAudioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }

        SwitchToPatrolSound();
    }

    bool CanDetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < fovAngle / 2f && distance < fovRange)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstacleMask))
                return true;
        }

        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isCrouching = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;

        float soundRange = isCrouching ? soundRangeCrouch :
                           isRunning ? soundRangeRun : soundRangeWalk;

        if (distance < soundRange)
            return true;

        return false;
    }

    void LateUpdate()
    {
        if (isCameraFrozen)
        {
            playerCamera.transform.localPosition = frozenCameraPos;
            playerCamera.transform.localRotation = frozenCameraRot;
            player.position = frozenPlayerPos;
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
        // Arrête tous les sons du monstre
        if (voiceAudioSource != null)
            voiceAudioSource.Stop();

        CharacterLoader characterLoader = player.GetComponent<CharacterLoader>();
        if (characterLoader != null && characterLoader.CurrentModel != null)
            characterLoader.CurrentModel.SetActive(false);

        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<Bloom>(out var bloom))
                bloom.active = false;
        }

        Vector3 targetPlayerPos = transform.position + transform.rotation * playerPositionOffset;
        targetPlayerPos.y = player.position.y;
        player.position = targetPlayerPos;
        player.rotation = Quaternion.LookRotation(transform.position - player.position);

        if (overrideCameraPosition)
        {
            playerCamera.transform.localPosition = new Vector3(0f, cameraHeightOffset, 0f);
            playerCamera.transform.localRotation = Quaternion.identity;
        }

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

        yield return StartCoroutine(TurnCameraToMonster());

        if (animator != null)
            animator.SetTrigger("Grab");

        yield return StartCoroutine(LookUpAtMonster());
        yield return StartCoroutine(CameraShake(0.4f, 0.15f));
        yield return StartCoroutine(FallCamera());

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

        Gizmos.color = Color.red;
        Vector3 fovLeft = Quaternion.Euler(0, -fovAngle / 2f, 0) * transform.forward * fovRange;
        Vector3 fovRight = Quaternion.Euler(0, fovAngle / 2f, 0) * transform.forward * fovRange;
        Gizmos.DrawRay(transform.position, fovLeft);
        Gizmos.DrawRay(transform.position, fovRight);
        Gizmos.DrawRay(transform.position, transform.forward * fovRange);
    }
}