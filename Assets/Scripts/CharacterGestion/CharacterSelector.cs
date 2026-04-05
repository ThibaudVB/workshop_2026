using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    [Header("Personnages disponibles")]
    [Tooltip("Glisse ici tous tes CharacterData")]
    public CharacterData[] characters;

    [Header("Preview")]
    [Tooltip("L'endroit où le modèle preview apparaît dans la scène de sélection")]
    public Transform previewSpawnPoint;

    [Header("UI")]
    [Tooltip("Le texte qui affiche le nom du personnage")]
    public TMPro.TextMeshProUGUI characterNameText;

    [Header("Scène de jeu")]
    public string gameSceneName = "GameScene";

    private int currentIndex = 0;
    private GameObject currentPreview;

    // Variable statique pour transmettre le choix entre les scènes
    public static CharacterData SelectedCharacter { get; private set; }

    void Start()
    {
        ShowCharacter(0);
    }

    /// <summary>
    /// Appelé par le bouton "Suivant" de ton UI
    /// </summary>
    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % characters.Length;
        ShowCharacter(currentIndex);
    }

    /// <summary>
    /// Appelé par le bouton "Précédent" de ton UI
    /// </summary>
    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = characters.Length - 1;
        ShowCharacter(currentIndex);
    }

    /// <summary>
    /// Appelé par le bouton "Jouer" / "Confirmer"
    /// </summary>
    public void ConfirmSelection()
    {
        SelectedCharacter = characters[currentIndex];
        SceneManager.LoadScene(gameSceneName);
    }

    void ShowCharacter(int index)
    {
        // Détruit l'ancien preview
        if (currentPreview != null)
            Destroy(currentPreview);

        // Affiche le nouveau
        CharacterData data = characters[index];
        if (data.modelPrefab != null && previewSpawnPoint != null)
        {
            currentPreview = Instantiate(data.modelPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);

            // Joue l'animation Idle en preview
            Animator anim = currentPreview.GetComponent<Animator>();
            if (anim != null && data.animatorOverride != null)
            {
                anim.runtimeAnimatorController = data.animatorOverride;
            }
        }
        if (characterNameText != null)
    characterNameText.text = data.characterName;
    }
}