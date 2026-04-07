using UnityEngine;

/// <summary>
/// Système de peur basé sur la proximité du monstre.
/// À placer sur le Player.
/// </summary>
public class FearSystem : MonoBehaviour
{
    [Header("=== RÉFÉRENCE MONSTRE ===")]
    public Transform monster;

    [Header("=== DISTANCES ===")]
    public float maxFearDistance = 30f;   // Distance où la peur commence
    public float minFearDistance = 1.5f;  // Distance de peur maximale

    [Header("=== DEBUG ===")]
    public bool showDebug = true;

    // Niveau de peur actuel (0 à 1)
    private float currentFear = 0f;
    private float targetFear = 0f;
    private float fearSmoothSpeed = 3f;

    // Properties publiques
    public float CurrentFear => currentFear;
    public float DistanceToMonster => monster != null ? Vector3.Distance(transform.position, monster.position) : float.MaxValue;

    void Start()
    {
        // Trouver le monstre automatiquement si pas assigné
        if (monster == null)
        {
            AlienMonster alien = FindAnyObjectByType<AlienMonster>();
            if (alien != null)
            {
                monster = alien.transform;
            }
        }
    }

    void Update()
    {
        if (monster == null) return;

        CalculateFear();
    }

    void CalculateFear()
    {
        float distance = DistanceToMonster;

        // Calculer le niveau de peur selon la distance
        if (distance >= maxFearDistance)
        {
            targetFear = 0f;
        }
        else if (distance <= minFearDistance)
        {
            targetFear = 1f;
        }
        else
        {
            // Interpolation inverse : plus proche = plus de peur
            targetFear = 1f - ((distance - minFearDistance) / (maxFearDistance - minFearDistance));
        }

        // Smooth transition
        currentFear = Mathf.Lerp(currentFear, targetFear, fearSmoothSpeed * Time.deltaTime);
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.Lerp(Color.white, Color.red, currentFear);

        GUI.Label(new Rect(10, 10, 300, 30), $"Peur: {(currentFear * 100f):F0}%", style);
        GUI.Label(new Rect(10, 35, 300, 30), $"Distance: {DistanceToMonster:F1}m", style);
    }
}
