using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public Animator anim;
    public Transform player;
    public Transform door;

    private bool isOpen = false;
    private bool isAnimating = false;
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static readonly int CloseHash = Animator.StringToHash("Close");

    void Update()
    {
        if (isAnimating) return;

        float distance = Vector3.Distance(player.position, door.position);

        if (distance < 4f && !isOpen)
        {
            isOpen = true;
            isAnimating = true;
            anim.ResetTrigger(CloseHash);
            anim.SetTrigger(OpenHash);
            StartCoroutine(WaitForState("Open"));
        }
        else if (distance > 6f && isOpen)
        {
            isOpen = false;
            isAnimating = true;
            anim.ResetTrigger(OpenHash);
            anim.SetTrigger(CloseHash);
            StartCoroutine(WaitForState("Closed"));
        }
    }

    IEnumerator WaitForState(string stateName)
    {
        yield return null;
        yield return null;

        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        isAnimating = false;
    }
}