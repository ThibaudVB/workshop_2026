using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Caméra")]
    public Camera mainCamera;
    public float fovDuration = 0.5f;

    [Header("Panels")]
    public GameObject panelMenu;
    public GameObject panelParametres;

    [Header("Audio")]
    public Slider sliderVolume;

    void Start()
    {
        // Affiche le menu, cache les paramètres
        panelMenu.SetActive(true);
        panelParametres.SetActive(false);

        // Initialise le slider avec le volume actuel
        if (sliderVolume != null)
        {
            sliderVolume.minValue = 0f;
            sliderVolume.maxValue = 1f;
            sliderVolume.value = AudioListener.volume;
            sliderVolume.onValueChanged.AddListener(ChangerVolume);
        }
    }

    // Bouton Jouer
    public void NouvellePartie()
    {
        StartCoroutine(ZoomEtCharger("Maintenance"));
    }

    // Bouton Paramètres
    public void OuvrirParametres()
    {
        panelMenu.SetActive(false);
        panelParametres.SetActive(true);
    }

    // Bouton Retour dans les paramètres
    public void FermerParametres()
    {
        panelParametres.SetActive(false);
        panelMenu.SetActive(true);
    }

    // Bouton Quitter
    public void Quitter()
    {
        Application.Quit();
    }

    // Slider volume
    public void ChangerVolume(float valeur)
    {
        AudioListener.volume = valeur;
    }

    IEnumerator ZoomEtCharger(string nomScene)
    {
        float tempsEcoule = 0f;
        float fovDepart = mainCamera.fieldOfView;

        while (tempsEcoule < fovDuration)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / fovDuration;
            mainCamera.fieldOfView = Mathf.Lerp(fovDepart, 0f, progression);
            yield return null;
        }

        mainCamera.fieldOfView = 0f;
        SceneManager.LoadScene(nomScene);
    }
}