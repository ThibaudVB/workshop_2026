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
    public GameObject CurrentModel => currentModel;

    void Start()
    {
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
        if (currentModel != null)
            Destroy(currentModel);

        currentModel = Instantiate(data.modelPrefab, transform);
        currentModel.transform.localPosition = new Vector3(0f, data.yOffset, 0f);
        currentModel.transform.localRotation = Quaternion.identity;

        Animator anim = currentModel.GetComponent<Animator>();
        if (anim != null)
        {
            if (data.animatorOverride != null)
                anim.runtimeAnimatorController = data.animatorOverride;
            else
                anim.runtimeAnimatorController = baseAnimatorController;

            anim.applyRootMotion = false;
        }

        PlayerAnimatorBridge bridge = currentModel.GetComponent<PlayerAnimatorBridge>();
        if (bridge == null)
            bridge = currentModel.AddComponent<PlayerAnimatorBridge>();

        bridge.playerRb = GetComponent<Rigidbody>();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            CameraOffset camOffset = cam.GetComponent<CameraOffset>();
            if (camOffset == null)
                camOffset = cam.gameObject.AddComponent<CameraOffset>();

            camOffset.offset = data.cameraOffset;
        }
    }
}