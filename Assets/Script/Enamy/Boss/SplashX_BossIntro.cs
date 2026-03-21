using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashX_BossIntro : MonoBehaviour
{
    [Header("Cinematic Objects")]
    public Transform bigMoon;
    public Transform bigMoonTarget;
    // ลบ Small Moon ออกไปแล้ว!

    [Header("Camera Control")]
    public float shakeDuration = 4f;
    public float shakeMagnitude = 0.3f;
    public float zoomOutSize = 8f;
    public float normalZoomSize = 5f;
    public Transform lockedCameraTarget;

    [Header("Optional: ปิดกล้องตามผู้เล่น")]
    [Tooltip("ลากสคริปต์ที่คุมกล้องของคุณมาใส่ตรงนี้ (ถ้ามี) เพื่อปิดมันชั่วคราวให้กล้องสั่นได้เต็มที่")]
    public MonoBehaviour cameraFollowScript;

    [Header("Timing Settings")]
    public float bigMoonSpeed = 3f;
    public string bossSceneName = "BossRoom_Phase1";

    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(PlayIntroCinematic());
        }
    }

    IEnumerator PlayIntroCinematic()
    {
        Camera mainCam = Camera.main;

        // 🔥 บังคับปิดสคริปต์กล้องตามผู้เล่น (ถ้าลากมาใส่ไว้) กล้องจะได้ไม่โดนดึงกลับไปหาเคียวตอนกำลังสั่น
        if (cameraFollowScript != null) cameraFollowScript.enabled = false;

        Vector3 originalCamPos = mainCam.transform.position;
        float originalSize = mainCam.orthographicSize;

        // --- 1. กล้องสั่น 4 วินาที ---
        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCam.transform.position = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // คืนตำแหน่งเดิมก่อนซูม
        mainCam.transform.position = originalCamPos;

        // --- 2. Zoom out ดูความใหญ่ของดวงจันทร์ ---
        elapsed = 0f;
        while (elapsed < 1.5f)
        {
            mainCam.orthographicSize = Mathf.Lerp(originalSize, zoomOutSize, elapsed / 1.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        // --- 3. Zoom กลับไปที่เป้าหมายที่ล็อคไว้ (ขยับกล้องไปดูดวงจันทร์เต็มๆ) ---
        elapsed = 0f;
        Vector3 lockPos = lockedCameraTarget != null ? lockedCameraTarget.position : originalCamPos;
        lockPos.z = originalCamPos.z;

        while (elapsed < 1.5f)
        {
            mainCam.orthographicSize = Mathf.Lerp(zoomOutSize, normalZoomSize, elapsed / 1.5f);
            mainCam.transform.position = Vector3.Lerp(originalCamPos, lockPos, elapsed / 1.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- 4. Big Moon ลอยขึ้นไปแล้วทำลายทิ้ง ---
        if (bigMoon != null && bigMoonTarget != null)
        {
            while (Vector2.Distance(bigMoon.position, bigMoonTarget.position) > 0.1f)
            {
                bigMoon.position = Vector3.MoveTowards(bigMoon.position, bigMoonTarget.position, bigMoonSpeed * Time.deltaTime);
                yield return null;
            }
            Destroy(bigMoon.gameObject);
        }

        // --- 5. Fade เข้า Scene ห้องบอสอัตโนมัติ ---
        yield return StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        // สร้าง UI จอดำด้วยโค้ด ล้วนๆ
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

        // ค่อยๆ มืดลง 1 วินาที
        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;

        // ย้ายฉาก!
        SceneManager.LoadScene(bossSceneName);
    }
}