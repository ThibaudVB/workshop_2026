using UnityEngine;
using UnityEngine.InputSystem;

public class LockerDoor : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private bool isOpen = false;
    
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            animator.SetBool("IsOpen", isOpen);
        }
    }
}