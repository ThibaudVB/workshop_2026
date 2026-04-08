using UnityEngine;
using UnityEngine.UI;


public class VignetteProximity : MonoBehaviour
{
    [Header("Référence UI")]
    [Tooltip("L'Image UI qui couvre tout l'écran (vignette noire, centre transparent)")]
    public Image vignetteImage;

    [Header("Distances")]
    [Tooltip("Distance en dessous de laquelle la vignette est à son maximum")]
    public float distanceMin = 1f;
    [Tooltip("Distance au-delà de laquelle la vignette est invisible (doit coïncider avec le radius du SphereCollider du RadarScript)")]
    public float distanceMax = 15f;

    [Header("Intensité")]
    [Range(0f, 1f)]
    [Tooltip("Alpha maximal de la vignette (1 = complètement noir en périphérie)")]
    public float maxAlpha = 0.85f;

    [Header("Lissage")]
    [Tooltip("Vitesse de transition de l'effet (plus grand = plus réactif)")]
    public float smoothSpeed = 5f;


    private Transform   _currentTarget;
    private bool        _targetInZone;
    private float       _currentAlpha;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            _currentTarget = other.transform;
            _targetInZone  = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            _targetInZone  = false;
            _currentTarget = null;
        }
    }

    void Update()
    {
        float targetAlpha = 0f;

        if (_targetInZone && _currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.position);

            float t = Mathf.InverseLerp(distanceMin, distanceMax, dist);
            targetAlpha = Mathf.Lerp(maxAlpha, 0f, t);
        }

        _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, Time.deltaTime * smoothSpeed);

        ApplyVignette(_currentAlpha);
    }

    void ApplyVignette(float alpha)
    {
        if (vignetteImage == null) return;

        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }

    public void GenerateVignetteTexture(int resolution = 512)
    {
        if (vignetteImage == null) return;

        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(0.5f, 0.5f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 uv   = new Vector2(x / (float)resolution, y / (float)resolution);
                float   dist = Vector2.Distance(uv, center) * 2f; // 0=centre, 1=bord

                // Courbe douce : le centre reste parfaitement transparent
                float alpha = Mathf.Pow(Mathf.Clamp01(dist), 2f);

                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        tex.Apply();

        vignetteImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f)
        );
    }

    void Start()
    {
        GenerateVignetteTexture();

        if (vignetteImage != null)
        {
            RectTransform rt = vignetteImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        _currentAlpha = 0f;
    }
}
