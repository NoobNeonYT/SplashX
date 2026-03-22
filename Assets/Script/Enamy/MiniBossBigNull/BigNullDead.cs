using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashX_BigNullManager : MonoBehaviour
{

    [Header("Boss References (ลากมือ 2 ข้างมาใส่)")]
    public SplashX_BigNullHand leftHand;          // แขนซ้ายรับสคริปต์แบบเงา
    public SplashX_BigNullGroundArm rightHand;    // แขนขวารับสคริปต์แบบกวาดพื้น

    [Header("Transition Settings")]
    public string nextSceneName = "Phase3_Scene"; // ชื่อฉากต่อไปที่จะให้โหลด
    public float delayBeforeTransition = 3f;      // หน่วงเวลากี่วินาทีหลังบอสตาย ก่อนตัดฉาก

    private bool isTransitioning = false;

    void Update()
    {
        // ถ้ากำลังเปลี่ยนฉากอยู่ ให้หยุดทำงานจะได้ไม่รันซ้ำ
        if (isTransitioning) return;

        // 1. เช็คว่ามือซ้ายตายหรือยัง? (เรียกนามสกุล HandState)
        bool isLeftDead = (leftHand == null || leftHand.currentState == SplashX_BigNullHand.HandState.Dead);

        // 2. เช็คว่ามือขวาตายหรือยัง? (🔥 เปลี่ยนมาเรียกนามสกุล GroundArmState)
        bool isRightDead = (rightHand == null || rightHand.currentState == SplashX_BigNullGroundArm.GroundArmState.Dead);

        // 3. ถ้าตายครบ 2 ข้าง ให้เริ่มกระบวนการเปลี่ยนฉาก
        if (isLeftDead && isRightDead)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextScene());
        }
    }

    IEnumerator TransitionToNextScene()
    {
        Debug.Log("บอสมือทั้ง 2 ข้างตายหมดแล้ว! เตรียมเปลี่ยนฉาก...");

        // รอให้แอนิเมชันตาย / ระเบิด ของบอสเล่นให้เสร็จก่อน
        yield return new WaitForSeconds(delayBeforeTransition);

        // (แถม) สร้างจอดำ Fade Out แบบอัตโนมัติให้ฉากตัดเนียนๆ
        yield return StartCoroutine(FadeOut());

        // โหลดซีนต่อไป
        SceneManager.LoadScene(nextSceneName);
    }

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