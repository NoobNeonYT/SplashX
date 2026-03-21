using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashX_BossRoomIntro : MonoBehaviour
{
    [Header("Small Moon Settings")]
    public Transform smallMoon;       // ดวงจันทร์เล็ก
    public Transform smallMoonTarget; // จุดเป้าหมาย
    public float moonSpeed = 4f;

    [Header("Darken Background")]
    [Tooltip("ความมืดที่ต้องการ (0 = สว่างปกติ, 1 = มืดสนิท)")]
    public float targetDarkness = 0.6f;
    public float darkenDuration = 2f;
    [Tooltip("เลเยอร์ของความมืด (ติดลบคืออยู่หลังสุด, ถ้าอยากให้มืดทับผู้เล่นด้วยให้ใส่ค่าบวกเยอะๆ)")]
    public int sortingOrder = -10;

    void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // 1. สร้าง UI แผ่นฟิล์มสีดำมาบังฉาก (เขียนโค้ดสร้างให้ จะได้ไม่ต้องไปกาง Canvas เอง)
        GameObject canvasObj = new GameObject("DarkOverlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        GameObject imgObj = new GameObject("DarkImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        Image darkImage = imgObj.AddComponent<Image>();
        darkImage.color = new Color(0, 0, 0, 0); // เริ่มที่โปร่งใส

        RectTransform rect = darkImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float elapsed = 0f;

        // 2. เลื่อนดวงจันทร์ลงมา และค่อยๆ ปรับให้จอมืดลงไปพร้อมๆ กัน
        while (smallMoon != null && smallMoonTarget != null && Vector2.Distance(smallMoon.position, smallMoonTarget.position) > 0.01f)
        {
            smallMoon.position = Vector3.MoveTowards(smallMoon.position, smallMoonTarget.position, moonSpeed * Time.deltaTime);

            if (elapsed < darkenDuration)
            {
                float alpha = Mathf.Lerp(0f, targetDarkness, elapsed / darkenDuration);
                darkImage.color = new Color(0, 0, 0, alpha);
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        // ทำให้ชัวร์ว่ามืดถึงระดับที่ตั้งไว้เป๊ะๆ ตอนดวงจันทร์จอดสนิท
        darkImage.color = new Color(0, 0, 0, targetDarkness);
    }
}