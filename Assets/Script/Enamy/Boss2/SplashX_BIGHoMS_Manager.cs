using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashX_BIGHoMS_Manager : MonoBehaviour
{
    [Header("Boss Reference")]
    [Tooltip("ลากบอส BIGHoMS จากในฉากมาใส่ช่องนี้")]
    public SplashX_Enemy bigHomsStats;

    [Header("Scene Transition")]
    public string nextSceneName = "ComicEnd"; // ชื่อซีนที่จะให้ย้ายไป
    public float delayBeforeTransition = 3f;  // รอให้บอสระเบิดโชว์ความเท่กี่วินาที ค่อยตัดฉาก

    private bool isTransitioning = false;

    void Update()
    {
        if (isTransitioning) return;

        // เช็ค 2 เงื่อนไข: 
        // 1. ถ้าเลือดบอส <= 0 (ตายแล้ว)
        // 2. หรือถ้า bigHomsStats กลายเป็น null (แปลว่าโดนสคริปต์ Enemy สั่ง Destroy ลบทิ้งไปแล้ว)
        if (bigHomsStats == null || bigHomsStats.currentHealth <= 0)
        {
            isTransitioning = true;
            StartCoroutine(EndSceneRoutine());
        }
    }

    IEnumerator EndSceneRoutine()
    {
        Debug.Log("💀 BIGHoMS ถูกจัดการแล้ว! รอเปลี่ยนซีนไปที่: " + nextSceneName);

        // 1. รอเวลาให้บอสเล่นแอนิเมชันตาย หรือระเบิดให้เสร็จก่อน
        yield return new WaitForSeconds(delayBeforeTransition);

        // 2. เฟดจอดำแบบหล่อๆ 
        yield return StartCoroutine(FadeOut());

        // 3. ย้ายซีนไปตอนจบ!
        SceneManager.LoadScene(nextSceneName);
    }

    // ระบบสร้างจอดำอัตโนมัติด้วยโค้ด (จะได้ไม่ต้องทำ UI เอง)
    IEnumerator FadeOut()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float fadeDuration = 1.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;
    }
}