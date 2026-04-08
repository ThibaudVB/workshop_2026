using UnityEngine;

/// <summary>
/// Fait le pont entre le PlayerController existant et l'Animator.
/// Envoie deux paramètres : Speed (0 à 1) et Direction (-1 gauche, 0 devant, 1 droite)
/// À placer sur le modèle 3D (enfant du Player).
/// </summary>
public class PlayerAnimatorBridge : MonoBehaviour
{
    [Header("Référence au Player")]
    public Rigidbody playerRb;

    [Header("Lissage")]
    public float dampTime = 0.1f;

    private Animator animator;
    private Transform playerTransform;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirectionHash = Animator.StringToHash("Direction");

    void Start()
    {
        animator = GetComponent<Animator>();

        if (playerRb == null)
            playerRb = GetComponentInParent<Rigidbody>();

        if (playerRb != null)
            playerTransform = playerRb.transform;
    }

    void Update()
    {
        if (animator == null || playerRb == null) return;

        // --- SPEED (comme avant) ---
        Vector3 horizontalVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(speed / 7f);

        // --- DIRECTION (nouveau) ---
        // Convertit la vélocité en espace local du joueur
        // localVelocity.x > 0 = droite, < 0 = gauche
        Vector3 localVelocity = playerTransform.InverseTransformDirection(horizontalVelocity);

        float direction = 0f;
        if (speed > 0.1f)
        {
            if (Mathf.Abs(localVelocity.x) > Mathf.Abs(localVelocity.z) * 0.8f)
            {
                direction = localVelocity.x > 0f ? 1f : -1f;
            }
        }

        animator.SetFloat(SpeedHash, normalizedSpeed, dampTime, Time.deltaTime);
        animator.SetFloat(DirectionHash, direction, dampTime, Time.deltaTime);
    }
}