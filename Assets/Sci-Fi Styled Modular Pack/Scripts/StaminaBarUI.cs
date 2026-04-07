using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Références")]
    public PlayerController playerController;
    public Image fillImage;
    public Image backgroundImage;

    [Header("Couleurs")]
    public Color highStaminaColor = new Color(0.2f, 0.8f, 0.3f);   // Vert
    public Color mediumStaminaColor = new Color(0.9f, 0.8f, 0.2f); // Jaune
    public Color lowStaminaColor = new Color(0.8f, 0.2f, 0.2f);    // Rouge

    [Header("Options")]
    public bool hideWhenFull = true;
    public float fadeSpeed = 3f;

    private CanvasGroup canvasGroup;
    private float targetAlpha = 1f;

    void Start()
    {
        // Trouver le PlayerController si pas assigné
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        // Créer le CanvasGroup pour le fade
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (playerController == null || fillImage == null) return;

        // Mettre à jour le remplissage
        float percent = playerController.StaminaPercent;
        fillImage.fillAmount = percent;

        // Mettre à jour la couleur
        if (percent > 0.5f)
        {
            fillImage.color = highStaminaColor;
        }
        else if (percent > 0.25f)
        {
            fillImage.color = mediumStaminaColor;
        }
        else
        {
            fillImage.color = lowStaminaColor;
        }

        // Gérer la visibilité
        if (hideWhenFull)
        {
            targetAlpha = (percent < 0.99f) ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }
}
