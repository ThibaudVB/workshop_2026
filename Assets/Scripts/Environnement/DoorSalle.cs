using UnityEngine;
using DG.Tweening;

public class DoorSalle : MonoBehaviour
{
    public Transform door;
    public Transform player;
    public AudioSource audioSource;
    public AudioClip doorSound;

    public Vector3 slideOffset = new Vector3(2f, 0, 0);
    public float animDuration = 1f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 closedPosition;

    void Start()
    {
        closedPosition = door.localPosition;
    }

    void Update()
    {
        if (isAnimating) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < 5f && !isOpen)
        {
            isOpen = true;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);
            door.DOLocalMove(closedPosition + slideOffset, animDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => isAnimating = false);
        }
        else if (distance > 7f && isOpen)
        {
            isOpen = false;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);
            door.DOLocalMove(closedPosition, animDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => isAnimating = false);
        }
    }
}