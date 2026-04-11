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
        // 🔥 แก้ชื่อให้ตรงกับด้านบนแล้ว (ใช้คำว่า hoverSound)
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
        else
        {
            Debug.LogWarning("⚠️ [UIManager] ลืมใส่ไฟล์เสียง Hover นะ!");
        }
    }

    public void PlayClick()
    {
        // 🔥 แถมเกราะกันพังให้ปุ่ม Click ด้วยครับ!
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogWarning("⚠️ [UIManager] ลืมใส่ไฟล์เสียง Click นะ!");
        }
    }
}