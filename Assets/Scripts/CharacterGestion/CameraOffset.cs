using UnityEngine;

/// <summary>
/// Applique un décalage à la caméra APRÈS que le PlayerController ait fait son travail.
/// Se met sur le même GameObject que la Camera (enfant du Player).
/// Le CharacterLoader remplit l'offset automatiquement.
/// </summary>
public class CameraOffset : MonoBehaviour
{
    [HideInInspector]
    public Vector3 offset = Vector3.zero;

    void LateUpdate()
    {
        // Ajoute l'offset par dessus ce que le PlayerController a déjà fait
        // On ne touche pas au Y (le PlayerController gère la hauteur avec le crouch)
        // On applique juste X et Z
        Vector3 pos = transform.localPosition;
        pos.x = offset.x;
        pos.z = offset.z;
        transform.localPosition = pos;
    }
}