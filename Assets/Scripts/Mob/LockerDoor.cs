using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LockerDoor : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float interactionDistance = 3f;
    
    [Header("UI Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("Hide Position")]
    [SerializeField] private Transform hidePosition;  // Position à l'intérieur du casier
    [SerializeField] private float enterDuration = 1f;  // Durée de l'animation d'entrée
    [SerializeField] private float exitDuration = 0.5f;  // Durée de l'animation de sortie
    [SerializeField] private float lookRotationOffset = 0f;  // Ajuste la rotation finale (0, 90, 180, -90...)
    [SerializeField] private float exitDirectionAngle = 0f;  // Direction de sortie (0=forward, 90=right, 180=back, -90=left)
    [SerializeField] private float exitDistance = 1.5f;  // Distance de sortie
    
    private bool isOpen = false;
    private bool isPlayerInside = false;
    private bool isPlayerNear = false;
    private bool isAnimating = false;
    private Transform player;
    private Camera playerCamera;
    
    // Sauvegarde position/rotation avant d'entrer
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Quaternion savedCameraRotation;
    
    // Propriété statique pour que le monstre sache si le joueur est caché
    public static bool IsPlayerHiding { get; private set; } = false;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerCamera = Camera.main;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    void Update()
    {
        if (player == null || isAnimating) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= interactionDistance || isPlayerInside;
        
        // Afficher/cacher le prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isPlayerNear && !isAnimating);
        }
        
        // Interaction
        if (isPlayerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPlayerInside)
            {
                StartCoroutine(EnterLockerSequence());
            }
            else
            {
                StartCoroutine(ExitLockerSequence());
            }
        }
    }
    
    IEnumerator EnterLockerSequence()
    {
        isAnimating = true;
        
        // Cacher le prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // Sauvegarder la position/rotation actuelle
        savedPlayerPosition = player.position;
        savedPlayerRotation = player.rotation;
        savedCameraRotation = playerCamera.transform.localRotation;
        
        // Désactiver le contrôle du joueur
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;
        
        // Désactiver le Rigidbody pour éviter les collisions pendant l'animation
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
        
        // 1. Ouvrir les portes
        isOpen = true;
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.3f);  // Attendre que les portes s'ouvrent un peu
        
        // 2. Glisser le joueur vers l'intérieur + rotation 180°
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        
        // Rotation finale : regarder vers la porte avec offset ajustable
        Quaternion baseRot = Quaternion.LookRotation(-transform.forward);
        Quaternion endRot = baseRot * Quaternion.Euler(0f, lookRotationOffset, 0f);
        
        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / enterDuration;
            
            // Courbe smooth (ease in-out)
            float smoothT = t * t * (3f - 2f * t);
            
            // Déplacer le joueur
            player.position = Vector3.Lerp(startPos, hidePosition.position, smoothT);
            
            // Tourner le joueur
            player.rotation = Quaternion.Slerp(startRot, endRot, smoothT);
            
            // Reset la rotation locale de la caméra (pour éviter les conflits)
            playerCamera.transform.localRotation = Quaternion.Slerp(savedCameraRotation, Quaternion.identity, smoothT);
            
            yield return null;
        }
        
        // S'assurer qu'on est exactement à la position finale
        player.position = hidePosition.position;
        player.rotation = endRot;
        playerCamera.transform.localRotation = Quaternion.identity;
        
        // 3. Fermer les portes
        yield return new WaitForSeconds(0.2f);
        isOpen = false;
        animator.SetBool("IsOpen", false);
        
        // 4. Le joueur est maintenant caché
        isPlayerInside = true;
        IsPlayerHiding = true;
        isAnimating = false;
    }
    
    IEnumerator ExitLockerSequence()
    {
        isAnimating = true;
        
        // Cacher le prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // 1. Ouvrir les portes
        isOpen = true;
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.3f);
        
        // 2. Sortir du casier (juste avancer, pas de rotation)
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        
        // Direction de sortie basée sur l'angle
        Vector3 exitDirection = Quaternion.Euler(0f, exitDirectionAngle, 0f) * transform.forward;
        Vector3 exitPos = transform.position + exitDirection * exitDistance;
        exitPos.y = savedPlayerPosition.y;
        
        float elapsed = 0f;
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            float smoothT = t * t * (3f - 2f * t);
            
            player.position = Vector3.Lerp(startPos, exitPos, smoothT);
            // Garde la même rotation, pas de changement
            
            yield return null;
        }
        
        player.position = exitPos;
        
        // 3. Réactiver le contrôle du joueur
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
        
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = true;
        
        // 4. Fermer les portes
        yield return new WaitForSeconds(0.3f);
        isOpen = false;
        animator.SetBool("IsOpen", false);
        
        // 5. Le joueur n'est plus caché
        isPlayerInside = false;
        IsPlayerHiding = false;
        isAnimating = false;
    }
}