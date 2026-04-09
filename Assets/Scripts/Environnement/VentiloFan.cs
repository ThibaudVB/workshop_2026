using UnityEngine;
using DG.Tweening;

public class VentiloFan : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float accelerationTime = 2f;
    [SerializeField] private float decelerationTime = 3f;

    private Tween fanTween;
    private bool isRunning = false;
    private float currentSpeed = 0f;

    void Start()
    {
        currentSpeed = rotationSpeed;
        TurnOn();
    }

    public void TurnOn()
    {
        if (isRunning) return;
        isRunning = true;

        DOTween.To(() => currentSpeed, x =>
        {
            currentSpeed = x;
            fanTween?.Kill();
            fanTween = transform.DOLocalRotate(
                new Vector3(360f, 0f, 0f),
                60f / currentSpeed,
                RotateMode.LocalAxisAdd
            )
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
        }, rotationSpeed, accelerationTime).SetEase(Ease.OutQuad);
    }

    public void TurnOff()
    {
        if (!isRunning) return;
        isRunning = false;

        DOTween.To(() => currentSpeed, x =>
        {
            currentSpeed = x;
            fanTween?.Kill();
            if (currentSpeed > 0.1f)
            {
                fanTween = transform.DOLocalRotate(
                    new Vector3(360f, 0f, 0f),
                    60f / currentSpeed,
                    RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
            }
        }, 0f, decelerationTime)
        .SetEase(Ease.InQuad)
        .OnComplete(() =>
        {
            fanTween?.Kill();
            currentSpeed = rotationSpeed;
        });
    }
}