using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public void Rejouer()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AlienMonster.IsDead = false;
        AlienMonster.cinematicMode = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}