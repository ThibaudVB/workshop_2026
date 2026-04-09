using UnityEngine;
using DG.Tweening;

public class PortePrincipale : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;
    public Transform player;
    public Transform monster;
    public AudioSource audioSource;
    public AudioClip doorSound;

    public float slideDistance = 2f;
    public float animDuration = 1f;
    public float openDistance = 3f;
    public float closeDistance = 4f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 leftClosed;
    private Vector3 rightClosed;

    void Start()
    {
        leftClosed = doorLeft.localPosition;
        rightClosed = doorRight.localPosition;
    }

    float GetClosestDistance()
    {
        float d = Vector3.Distance(player.position, transform.position);
        if (monster != null && monster.gameObject.activeInHierarchy)
        {
            float dm = Vector3.Distance(monster.position, transform.position);
            d = Mathf.Min(d, dm);
        }
        return d;
    }

    void Update()
    {
        if (isAnimating) return;

        float distance = GetClosestDistance();

        if (distance < openDistance && !isOpen)
        {
            isOpen = true;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);

            doorLeft.DOLocalMoveX(leftClosed.x + slideDistance, animDuration).SetEase(Ease.InOutSine);
            doorRight.DOLocalMoveX(rightClosed.x - slideDistance, animDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => isAnimating = false);
        }
        else if (distance > closeDistance && isOpen)
        {
            isOpen = false;
            isAnimating = true;
            audioSource.PlayOneShot(doorSound);

            doorLeft.DOLocalMoveX(leftClosed.x, animDuration).SetEase(Ease.InOutSine);
            doorRight.DOLocalMoveX(rightClosed.x, animDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => isAnimating = false);
        }
    }
}