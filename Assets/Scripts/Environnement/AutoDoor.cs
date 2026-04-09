using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [SerializeField] private float detectionDistance = 3f;
    private Animator animator;
    private Transform player;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionDistance)
        {
            if (!isOpen)
            {
                isOpen = true;
                animator.SetBool("IsOpen", true);
            }
        }
        else
        {
            if (isOpen)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("OpenedDoor"))
                {
                    isOpen = false;
                    animator.SetBool("IsOpen", false);
                }
            }
        }
    }
}