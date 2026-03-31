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
    public float crouchHeight = 0.5f;
    public float normalHeight = 2f;

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

    private Rigidbody rb;
    private Camera cam;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool isCrouching;
    private CapsuleCollider capsule;
    private AudioSource audioSource;
    private float stepTimer = 0f;

    // Stamina
    private float currentStamina;
    private float regenTimer;

    // Properties pour l'UI
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

        // Initialiser la stamina
        currentStamina = maxStamina;
    }

    void Update()
    {
        LookAround();
        HandleCrouch();
        HandleJump();
        HandleFootsteps();
        HandleStamina();
    }

    void FixedUpdate()
    {
        Move();
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
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;

        // Déterminer la vitesse
        float speed = walkSpeed;
        bool wantsToRun = Keyboard.current.leftShiftKey.isPressed && !isCrouching;
        bool canRun = wantsToRun && currentStamina > 0 && moveInput != Vector2.zero;
        
        if (canRun) speed = runSpeed;
        if (isCrouching) speed = crouchSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = move * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void HandleStamina()
    {
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;

        bool isRunning = Keyboard.current.leftShiftKey.isPressed && !isCrouching && moveInput != Vector2.zero && currentStamina > 0;

        if (isRunning)
        {
            // Drainer la stamina
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            regenTimer = staminaRegenDelay;
        }
        else
        {
            // Régénérer après le délai
            if (regenTimer > 0)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    void HandleFootsteps()
    {
        if (!isGrounded) return;

        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;

        if (moveInput == Vector2.zero)
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleCrouch()
    {
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            isCrouching = !isCrouching;
            capsule.height = isCrouching ? crouchHeight : normalHeight;
            capsule.center = new Vector3(0, (isCrouching ? crouchHeight : normalHeight) / 2f, 0);
            cam.transform.localPosition = new Vector3(0, isCrouching ? 0.2f : 0.7f, 0);
        }
    }
}
