using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("Chute ?");
        }
    }
}