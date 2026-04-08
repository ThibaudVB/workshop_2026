using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bob Marche")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmountY = 0.05f;
    [SerializeField] private float walkBobAmountX = 0.025f;

    [Header("Bob Course")]
    [SerializeField] private float runBobSpeed = 16f;
    [SerializeField] private float runBobAmountY = 0.1f;
    [SerializeField] private float runBobAmountX = 0.05f;

    [Header("Bob Accroupi")]
    [SerializeField] private float crouchBobSpeed = 6f;
    [SerializeField] private float crouchBobAmountY = 0.025f;
    [SerializeField] private float crouchBobAmountX = 0.01f;

    [Header("Respiration")]
    [SerializeField] private float breathSpeed = 1.2f;
    [SerializeField] private float breathAmountY = 0.006f;
    [SerializeField] private float breathAmountX = 0.003f;

    [Header("Balancement latéral")]
    [SerializeField] private float tiltAmount = 1.5f;
    [SerializeField] private float tiltSpeed = 5f;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 12f;

    private Camera cam;
    private Vector3 defaultPos;
    private Quaternion defaultRot;
    private float bobTimer = 0f;
    private float breathTimer = 0f;
    private float currentTilt = 0f;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        defaultPos = cam.transform.localPosition;
        defaultRot = cam.transform.localRotation;
    }

    void Update()
    {
        if (PlayerController.blockMovement) return;

        bool isMoving = UnityEngine.InputSystem.Keyboard.current.wKey.isPressed ||
                        UnityEngine.InputSystem.Keyboard.current.sKey.isPressed ||
                        UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ||
                        UnityEngine.InputSystem.Keyboard.current.dKey.isPressed;

        bool isRunning = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
        bool isCrouching = UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed;

        Vector3 posOffset = Vector3.zero;
        float tiltTarget = 0f;

        // Bob
        if (isMoving)
        {
            float speed = isCrouching ? crouchBobSpeed : isRunning ? runBobSpeed : walkBobSpeed;
            float amountY = isCrouching ? crouchBobAmountY : isRunning ? runBobAmountY : walkBobAmountY;
            float amountX = isCrouching ? crouchBobAmountX : isRunning ? runBobAmountX : walkBobAmountX;

            bobTimer += Time.deltaTime * speed;
            posOffset.y += Mathf.Abs(Mathf.Sin(bobTimer)) * amountY;
            posOffset.x += Mathf.Cos(bobTimer / 2f) * amountX;
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * smoothSpeed);
        }

        // Respiration (toujours active)
        breathTimer += Time.deltaTime * breathSpeed;
        posOffset.y += Mathf.Sin(breathTimer) * breathAmountY;
        posOffset.x += Mathf.Cos(breathTimer * 0.7f) * breathAmountX;

        // Tilt latéral
        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ||
            UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed)
            tiltTarget = tiltAmount;
        else if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ||
                 UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed)
            tiltTarget = -tiltAmount;

        currentTilt = Mathf.Lerp(currentTilt, tiltTarget, tiltSpeed * Time.deltaTime);

        // Application
        cam.transform.localPosition = Vector3.Lerp(
            cam.transform.localPosition,
            defaultPos + posOffset,
            smoothSpeed * Time.deltaTime
        );

        // Tilt appliqué sur Z uniquement sans toucher X (géré par PlayerController)
        float currentX = cam.transform.localEulerAngles.x;
        cam.transform.localRotation = Quaternion.Euler(currentX, 0f, currentTilt);
    }
}