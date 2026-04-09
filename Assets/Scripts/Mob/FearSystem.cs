using UnityEngine;

public class FearSystem : MonoBehaviour
{
    [Header("=== RÉFÉRENCE MONSTRE ===")]
    public Transform monster;

    [Header("=== DISTANCES ===")]
    public float maxFearDistance = 30f;
    public float minFearDistance = 1.5f;

    [Header("=== DEBUG ===")]
    public bool showDebug = true;

    private float currentFear = 0f;
    private float targetFear = 0f;
    private float fearSmoothSpeed = 3f;

    public float CurrentFear => currentFear;
    public float DistanceToMonster => monster != null && monster.gameObject.activeInHierarchy
        ? Vector3.Distance(transform.position, monster.position)
        : float.MaxValue;

    void Start()
    {
        if (monster == null)
        {
            AlienMonster alien = FindAnyObjectByType<AlienMonster>();
            if (alien != null)
                monster = alien.transform;
        }
    }

    void Update()
    {
        if (monster == null) return;
        CalculateFear();
    }

    public void ForceMaxFear()
    {
        currentFear = 1f;
        targetFear = 1f;
    }

    public void ResetFear()
    {
        currentFear = 0f;
        targetFear = 0f;
    }

    void CalculateFear()
    {
        if (!monster.gameObject.activeInHierarchy)
        {
            targetFear = 0f;
            currentFear = Mathf.Lerp(currentFear, 0f, fearSmoothSpeed * Time.deltaTime);
            return;
        }

        float distance = DistanceToMonster;

        if (distance >= maxFearDistance)
            targetFear = 0f;
        else if (distance <= minFearDistance)
            targetFear = 1f;
        else
            targetFear = 1f - ((distance - minFearDistance) / (maxFearDistance - minFearDistance));

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