using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager instance;

    public AudioSource audioSource;

    public AudioClip hoverSound;
    public AudioClip clickSound;

    void Awake()
    {
        instance = this;
    }

    public void PlayHover()
    {
        audioSource.PlayOneShot(hoverSound);
    }

    public void PlayClick()
    {
        audioSource.PlayOneShot(clickSound);
    }
}
