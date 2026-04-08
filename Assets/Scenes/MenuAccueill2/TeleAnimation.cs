using UnityEngine;

public class TeleAnimation : MonoBehaviour
{
    [Header("Flottement")]
    public float amplitude = 0.2f;
    public float vitesse = 1.2f;

    [Header("Rotation")]
    public float rotationMax = 5f;
    public float rotationVitesse = 0.8f;

    private Vector3 positionDepart;
    private Quaternion rotationDepart; // ← sauvegarde la rotation initiale

    void Start()
    {
        positionDepart = transform.localPosition;
        rotationDepart = transform.localRotation; // ← on mémorise ta rotation
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * vitesse) * amplitude;
        transform.localPosition = positionDepart + new Vector3(0f, offsetY, 0f);

        float rotZ = Mathf.Sin(Time.time * rotationVitesse) * rotationMax;
        // On ajoute le balancement PAR DESSUS ta rotation de départ
        transform.localRotation = rotationDepart * Quaternion.Euler(0f, 0f, rotZ);
    }
}