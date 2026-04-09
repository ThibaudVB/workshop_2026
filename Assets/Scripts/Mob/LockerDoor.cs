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
    [SerializeField] private Transform hidePosition;
    [SerializeField] private float enterDuration = 1f;
    [SerializeField] private float exitDuration = 0.5f;
    [SerializeField] private float lookRotationOffset = 0f;
    [SerializeField] private float exitDirectionAngle = 0f;
    [SerializeField] private float exitDistance = 1.5f;
    
    private bool isPlayerInside = false;
    private bool isPlayerNear = false;
    private bool isAnimating = false;
    private Transform player;
    private Camera playerCamera;
    
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Quaternion savedCameraRotation;
    
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
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(isPlayerNear && !isAnimating);
        
        if (isPlayerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPlayerInside)
                StartCoroutine(EnterLockerSequence());
            else
                StartCoroutine(ExitLockerSequence());
        }
    }
    
    IEnumerator EnterLockerSequence()
    {
        isAnimating = true;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        savedPlayerPosition = player.position;
        savedPlayerRotation = player.rotation;
        savedCameraRotation = playerCamera.transform.localRotation;
        
        PlayerController.blockMovement = true;
        PlayerController.blockCamera = true;
        
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
        
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.3f);
        
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        
        Quaternion baseRot = Quaternion.LookRotation(-transform.forward);
        Quaternion endRot = baseRot * Quaternion.Euler(0f, lookRotationOffset, 0f);
        
        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / enterDuration;
            float smoothT = t * t * (3f - 2f * t);
            
            player.position = Vector3.Lerp(startPos, hidePosition.position, smoothT);
            player.rotation = Quaternion.Slerp(startRot, endRot, smoothT);
            playerCamera.transform.localRotation = Quaternion.Slerp(savedCameraRotation, Quaternion.identity, smoothT);
            
            yield return null;
        }
        
        player.position = hidePosition.position;
        player.rotation = endRot;
        playerCamera.transform.localRotation = Quaternion.identity;
        
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("IsOpen", false);
        
        isPlayerInside = true;
        IsPlayerHiding = true;
        isAnimating = false;
    }
    
    IEnumerator ExitLockerSequence()
    {
        isAnimating = true;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.3f);
        
        Vector3 startPos = player.position;
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
            yield return null;
        }
        
        player.position = exitPos;
        
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
        
        PlayerController.blockMovement = false;
        PlayerController.blockCamera = false;
        
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("IsOpen", false);
        
        isPlayerInside = false;
        IsPlayerHiding = false;
        isAnimating = false;
    }
}