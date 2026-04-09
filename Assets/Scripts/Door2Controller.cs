using UnityEngine;
using System.Collections;

public class Door2Controller : MonoBehaviour
{
    public Animator anim;
    public Transform player;
    public AudioSource audioSource;
    public AudioClip doorSound;

    private bool isOpen = false;
    private bool isAnimating = false;
    private static readonly int NearbyHash = Animator.StringToHash("character_nearby");

    void Update()
    {
        if (isAnimating) return;

        float distance = Vector3.Distance(player.position, this.transform.position);

        if (distance < 3f && !isOpen)
        {
            isOpen = true;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);
            anim.SetBool(NearbyHash, true);
            StartCoroutine(OpenThenWait());
        }
        else if (distance > 4f && isOpen)
        {
            isOpen = false;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);
            anim.SetBool(NearbyHash, false);
            StartCoroutine(WaitForAnimation(2f));
        }
    }

    IEnumerator OpenThenWait()
    {
        yield return new WaitForSeconds(5f);
        isAnimating = false;
    }

    IEnumerator WaitForAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAnimating = false;
    }
}