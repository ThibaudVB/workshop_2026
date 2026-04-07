using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Vitesses")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float jumpForce = 4f;

    [Header("Souris")]
    public float mouseSensitivity = 2f;

    [Header("Accroupissement")]
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    public float crouchTransitionSpeed = 10f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float staminaRegenDelay = 1.5f;

    [Header("Bruits de pas")]
    public AudioClip[] footstepSounds;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    public float crouchStepInterval = 0.7f;
    public float footstepVolume = 0.5f;

    public static bool cinematicMode = false;

    private Rigidbody rb;
    private Camera cam;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool isCrouching;
    private bool wantsToCrouch;
    private CapsuleCollider capsule;
    private AudioSource audioSource;
    private float stepTimer = 0f;

    private float currentStamina;
    private float regenTimer;

    private Vector2 moveInput;

    public float CurrentStamina => currentStamina;
    public float StaminaPercent => currentStamina / maxStamina;
    public float MaxStamina => maxStamina;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        capsule = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        currentStamina = maxStamina;
    }

    void Update()
    {
        if (AlienMonster.IsDead || cinematicMode) return;

        ReadInput();
        LookAround();
        HandleCrouch();
        HandleJump();
        HandleFootsteps();
        HandleStamina();
    }

    void FixedUpdate()
    {
        if (AlienMonster.IsDead || cinematicMode) return;
        Move();
    }

    void ReadInput()
    {
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
    }

    void LookAround()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float speed = walkSpeed;
        bool wantsToRun = Keyboard.current.leftShiftKey.isPressed && !isCrouching;
        bool canRun = wantsToRun && currentStamina > 0 && moveInput != Vector2.zero;

        if (canRun) speed = runSpeed;
        if (isCrouching) speed = crouchSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = move.normalized * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void HandleStamina()
    {
        bool isRunning = Keyboard.current.leftShiftKey.isPressed && !isCrouching && moveInput != Vector2.zero && currentStamina > 0;

        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            regenTimer = staminaRegenDelay;
        }
        else
        {
            if (regenTimer > 0)
                regenTimer -= Time.deltaTime;
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    void HandleFootsteps()
    {
        if (!isGrounded || moveInput == Vector2.zero)
        {
            stepTimer = 0f;
            return;
        }

        bool isRunning = Keyboard.current.leftShiftKey.isPressed && !isCrouching && currentStamina > 0;
        float interval = isRunning ? runStepInterval : isCrouching ? crouchStepInterval : walkStepInterval;

        stepTimer += Time.deltaTime;
        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(clip, footstepVolume);
    }

    void HandleJump()
    {
        float rayLength = (capsule.height / 2f) + 0.2f;
        isGrounded = Physics.SphereCast(transform.position + Vector3.up * 0.3f, 0.25f, Vector3.down, out _, rayLength);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            if (isCrouching && CanStandUp())
            {
                isCrouching = false;
                capsule.height = normalHeight;
                capsule.center = new Vector3(0, normalHeight / 2f, 0);
                cam.transform.localPosition = new Vector3(0, normalHeight - 0.3f, 0);
            }
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleCrouch()
    {
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                capsule.height = crouchHeight;
                capsule.center = new Vector3(0, crouchHeight / 2f, 0);
                cam.transform.localPosition = new Vector3(0, crouchHeight - 0.2f, 0);
            }
            else if (CanStandUp())
            {
                isCrouching = false;
                capsule.height = normalHeight;
                capsule.center = new Vector3(0, normalHeight / 2f, 0);
                cam.transform.localPosition = new Vector3(0, normalHeight - 0.3f, 0);
            }
        }

        isCrouching = wantsToCrouch;

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        capsule.center = new Vector3(0, capsule.height / 2f, 0);

        float targetCamY = isCrouching ? crouchHeight - 0.2f : normalHeight - 0.3f;
        Vector3 camPos = cam.transform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
        cam.transform.localPosition = camPos;
    }

    bool CanStandUp()
    {
        float checkDistance = normalHeight - capsule.height;
        Vector3 origin = transform.position + Vector3.up * capsule.height;
        return !Physics.Raycast(origin, Vector3.up, checkDistance + 0.1f);
    }
}