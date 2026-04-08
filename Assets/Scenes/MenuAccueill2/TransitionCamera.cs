using UnityEngine;
using System.Collections;

public class TransitionCamera : MonoBehaviour
{
    [Header("Positions de la caméra")]
    public Transform positionMenuPrincipal;   // Position devant la télé principale
    public Transform positionParametres;      // Position devant la télé paramètres

    [Header("Réglages")]
    public float dureeTransition = 1.5f;      // Durée du déplacement en secondes
    public AnimationCurve courbe;             // Courbe d'animation (ease in/out)

    private bool estDansParametres = false;
    private Coroutine transitionEnCours;

    public void AllerParametres()
    {
        if (transitionEnCours != null) StopCoroutine(transitionEnCours);
        transitionEnCours = StartCoroutine(Deplacer(positionParametres));
        estDansParametres = true;
    }

    public void Retour()
    {
        if (transitionEnCours != null) StopCoroutine(transitionEnCours);
        transitionEnCours = StartCoroutine(Deplacer(positionMenuPrincipal));
        estDansParametres = false;
    }

    private IEnumerator Deplacer(Transform cible)
    {
        Vector3 departPos = transform.position;
        Quaternion departRot = transform.rotation;

        float temps = 0f;

        while (temps < dureeTransition)
        {
            temps += Time.deltaTime;
            float t = courbe.Evaluate(temps / dureeTransition);

            transform.position = Vector3.Lerp(departPos, cible.position, t);
            transform.rotation = Quaternion.Lerp(departRot, cible.rotation, t);

            yield return null;
        }

        transform.position = cible.position;
        transform.rotation = cible.rotation;
    }
}