using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Header("Référence caméra")]
    public TransitionCamera transitionCamera; // Glisse la Main Camera ici

    public void Lancer()
    {
        SceneManager.LoadScene("NomDeTaScene");
    }

    public void Parametres()
    {
        transitionCamera.AllerParametres();
    }

    public void Quitter()
    {
        Application.Quit();
    }

    public void Retour()
    {
        transitionCamera.Retour();
    }
}