using UnityEngine;
using System.Collections;

public class CameraFOV : MonoBehaviour
{
    [Header("Paramètres FOV")]
    public float startFOV = 179f;
    public float endFOV = 60f;
    public float duration = 2f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = startFOV;
        StartCoroutine(AnimateFOV());
    }

    IEnumerator AnimateFOV()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth pour un effet cinématique
            t = t * t * (3f - 2f * t);

            cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);
            yield return null;
        }

        cam.fieldOfView = endFOV;
    }
}