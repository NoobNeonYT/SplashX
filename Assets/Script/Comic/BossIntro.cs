using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ต้องใช้สำหรับเรียกคำสั่ง Image

public class BossIntro : MonoBehaviour
{
    [Header("Boss")]
    [Tooltip("ลากรูปบอสมาใส่ตรงนี้")]
    public Image bossImage;

    [Tooltip("Bossimage")]
    public CanvasGroup bossCanvasGroup;

    [Header("ตั้งค่าเวลาแอนิเมชัน")]
    [Tooltip("เวลาที่ค่อยๆ ปาดภาพจากซ้ายไปขวา (วินาที)")]
    public float wipeInDuration = 1.0f;
    [Tooltip("เวลาโชว์ค้างไว้ (วินาที)")]
    public float displayDuration = 1.5f;
    [Tooltip("เวลาที่ค่อยๆ เฟดจางหายไป (วินาที)")]
    public float fadeOutDuration = 0.8f;

    void Start()
    {
        if (bossImage == null || bossCanvasGroup == null)
        {
            Debug.LogError("ใส่รูปบอสใน Inspector ให้ครบทั้ง 2 ช่องด้วยนะครับ!");
            return;
        }

       
        bossImage.gameObject.SetActive(true);
        bossCanvasGroup.alpha = 1f;
        bossImage.fillAmount = 0f;

        
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        
        float timer = 0f;
        while (timer < wipeInDuration)
        {
            timer += Time.deltaTime;
           
            bossImage.fillAmount = Mathf.Lerp(0f, 1f, timer / wipeInDuration);
            yield return null;
        }
        bossImage.fillAmount = 1f; 

        
        yield return new WaitForSeconds(displayDuration);

       
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
           
            bossCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }

        
        bossCanvasGroup.alpha = 0f;
        bossImage.gameObject.SetActive(false);

       
    }
}