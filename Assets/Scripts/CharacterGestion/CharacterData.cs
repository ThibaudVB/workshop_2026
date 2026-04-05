using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Infos")]
    public string characterName;

    [Header("Modèle 3D")]
    [Tooltip("Le prefab du modèle 3D (le mesh importé de Mixamo)")]
    public GameObject modelPrefab;

    [Header("Animations")]
    [Tooltip("L'Animator Override Controller de ce personnage")]
    public AnimatorOverrideController animatorOverride;

    [Header("Ajustements")]
    [Tooltip("Décalage Y pour que les pieds touchent le sol")]
    public float yOffset = -1f;
}