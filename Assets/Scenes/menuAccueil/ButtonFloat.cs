using UnityEngine;
using System.Collections;

public class ButtonFloat : MonoBehaviour
{
    [Header("Mouvement")]
    public float vitesseX = 0.5f;
    public float vitesseY = 0.7f;
    public float vitesseZ = 0.3f;

    public float amplitudeX = 10f;
    public float amplitudeY = 8f;
    public float amplitudeZ = 5f;

    [Header("Décalage aléatoire")]
    // Evite que tous les boutons bougent en même temps
    private float offsetX;
    private float offsetY;
    private float offsetZ;

    private Vector3 positionDepart;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        positionDepart = rectTransform.anchoredPosition3D;

        // Décalage aléatoire différent pour chaque bouton
        offsetX = Random.Range(0f, 100f);
        offsetY = Random.Range(0f, 100f);
        offsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        float x = Mathf.Sin((Time.time + offsetX) * vitesseX) * amplitudeX;
        float y = Mathf.Sin((Time.time + offsetY) * vitesseY) * amplitudeY;
        float z = Mathf.Sin((Time.time + offsetZ) * vitesseZ) * amplitudeZ;

        rectTransform.anchoredPosition3D = positionDepart + new Vector3(x, y, z);
    }
}