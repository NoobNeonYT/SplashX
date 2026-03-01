using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashX_MainMenu : MonoBehaviour
{
    public GameObject playSplash;

    public void Play()
    {
        playSplash.SetActive(true);
        Invoke("LoadScene", 1f);

        SceneManager.LoadScene("all_system_here");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
