using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    [Header("Fallback (si on lance la scène de jeu directement sans passer par la sélection)")]
    [Tooltip("Le personnage par défaut si aucun n'a été choisi")]
    public CharacterData defaultCharacter;

    [Header("Réglages")]
    [Tooltip("L'Animator Controller de base (celui avec toutes les transitions)")]
    public RuntimeAnimatorController baseAnimatorController;

    private GameObject currentModel;

    void Start()
    {
        // Récupère le choix du joueur, ou le perso par défaut
        CharacterData chosen = CharacterSelector.SelectedCharacter;
        if (chosen == null)
            chosen = defaultCharacter;

        if (chosen == null)
        {
            Debug.LogError("CharacterLoader : Aucun personnage assigné !");
            return;
        }

        SpawnCharacter(chosen);
    }

    void SpawnCharacter(CharacterData data)
    {
        // Supprime l'ancien modèle s'il y en a un
        if (currentModel != null)
            Destroy(currentModel);

        // Instancie le modèle comme enfant du Player
        currentModel = Instantiate(data.modelPrefab, transform);

        // Positionne correctement (pieds au sol)
        currentModel.transform.localPosition = new Vector3(0f, data.yOffset, 0f);
        currentModel.transform.localRotation = Quaternion.identity;

        // Configure l'Animator
        Animator anim = currentModel.GetComponent<Animator>();
        if (anim != null)
        {
            // Utilise l'override si dispo, sinon le controller de base
            if (data.animatorOverride != null)
                anim.runtimeAnimatorController = data.animatorOverride;
            else
                anim.runtimeAnimatorController = baseAnimatorController;

            // IMPORTANT : pas de Root Motion
            anim.applyRootMotion = false;
        }

        // Configure le bridge (le script qui envoie Speed et Direction)
        PlayerAnimatorBridge bridge = currentModel.GetComponent<PlayerAnimatorBridge>();
        if (bridge == null)
            bridge = currentModel.AddComponent<PlayerAnimatorBridge>();

        bridge.playerRb = GetComponent<Rigidbody>();
    }
}