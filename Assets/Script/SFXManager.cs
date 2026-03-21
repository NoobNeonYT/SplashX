using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransfrom, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransfrom.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
    public void RandomPlaySoundFXClip(AudioClip[] audioClip, Transform spawnTransfrom, float volume)
    {
        int rand = Random.Range(0, audioClip.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransfrom.position, Quaternion.identity);

        audioSource.clip = audioClip[rand];

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
