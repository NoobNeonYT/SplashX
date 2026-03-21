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
        // 1. สร้าง UI แผ่นฟิล์มสีดำมาบังฉาก (โค้ดเดิม)
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

        // 2. ลูปที่ 1: เลื่อนดวงจันทร์ลงมาให้ถึงจุดหมาย "ก่อน"
        if (smallMoon != null && smallMoonTarget != null)
        {
            while (Vector2.Distance(smallMoon.position, smallMoonTarget.position) > 0.01f)
            {
                smallMoon.position = Vector3.MoveTowards(smallMoon.position, smallMoonTarget.position, moonSpeed * Time.deltaTime);
                yield return null;
            }
            // จับวางให้ตรงเป๊ะกันบัคขยับไม่สุด
            smallMoon.position = smallMoonTarget.position;
        }

        // 3. ลูปที่ 2: พอดวงจันทร์ถึงเป้าหมายแล้ว... ค่อยเริ่มปรับให้จอมืดลง
        float elapsed = 0f;
        while (elapsed < darkenDuration)
        {
            float alpha = Mathf.Lerp(0f, targetDarkness, elapsed / darkenDuration);
            darkImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ทำให้ชัวร์ว่ามืดถึงระดับที่ตั้งไว้เป๊ะๆ
        darkImage.color = new Color(0, 0, 0, targetDarkness);
    }
}