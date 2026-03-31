using UnityEngine;
using System.Collections;

public class Door2Controller : MonoBehaviour
{
    public Animator anim;
    public Transform player;

    private bool isOpen = false;
    private bool isAnimating = false;
    private static readonly int NearbyHash = Animator.StringToHash("character_nearby");

    void Update()
    {
        if (isAnimating) return;

        float distance = Vector3.Distance(player.position, this.transform.position);

        if (distance < 2.5f && !isOpen)
        {
            isOpen = true;
            isAnimating = true;
            anim.SetBool(NearbyHash, true);
            StartCoroutine(WaitForState("door_2_opened"));
        }
        else if (distance > 3f && isOpen)
        {
            isOpen = false;
            isAnimating = true;
            anim.SetBool(NearbyHash, false);
            StartCoroutine(WaitForState("door_2_closed"));
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