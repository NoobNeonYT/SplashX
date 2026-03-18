using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashX_MainMenu : MonoBehaviour
{
    public GameObject buttons;
    public GameObject Mainbuttons;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject logo;
    public GameObject startPanel;

    public Image soundImage;
    public Sprite soundOn;
    public Sprite soundOff;
    public AudioSource music;

    public GameObject transitionPanel;

    bool muted = false;

    public void StartGame()
    {
        transitionPanel.SetActive(true);
        Invoke("LoadGame", 2f);
    }

    void LoadGame()
    {
        SceneManager.LoadScene("all_system_here");
    }

    public void Play()
    {
        Mainbuttons.SetActive(false);
        startPanel.SetActive(true);
    }

    public void CloseStartPanel()
    {
        startPanel.SetActive(false);
        Mainbuttons.SetActive(true);

    }

    public void OpenCredits()
    {
        buttons.SetActive(false);
        logo.SetActive(false);

        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);

        buttons.SetActive(true);
        logo.SetActive(true);
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ToggleSound()
    {
        muted = !muted;
        music.mute = muted;

        soundImage.sprite = muted ? soundOff : soundOn;
    }

    public void SetVolume(float volume)
    {
        music.volume = volume;
    }
}
