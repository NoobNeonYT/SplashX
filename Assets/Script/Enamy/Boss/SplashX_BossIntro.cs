using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Cinemachine;

public class SplashX_BossIntro : MonoBehaviour
{
    [Header("Cinematic Objects")]
    public Transform bigMoon;
    public Transform bigMoonTarget;

    [Header("Cameras & Control")]
    public SplashX_CameraControl playerCamScript;
    public CinemachineCamera cinematicVcam; // กล้องเป้าหมายที่ตั้งค่าซูมและตำแหน่งไว้แล้ว

    [Header("Shake Settings")]
    public float shakeDuration = 5f;
    public float shakeAmplitude = 3f;

    [Header("Timing Settings")]
    public float bigMoonSpeed = 3f;
    public string bossSceneName = "BossRoom_Phase1";

    private bool isTriggered = false;
    private CinemachineBasicMultiChannelPerlin playerNoise;

    void Start()
    {
        // เริ่มเกมมา ปิดกล้องคัตซีนไว้ก่อน
        if (cinematicVcam != null) cinematicVcam.gameObject.SetActive(false);
    }

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
        if (playerCamScript != null && playerCamScript.vcam != null)
        {
            playerNoise = playerCamScript.vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // ล็อกไม่ให้สคริปต์ผู้เล่นมาแย่งซูม
        if (playerCamScript != null) playerCamScript.isLockedZ = true;

        // --- 1. สั่นกล้องผู้เล่น 5 วินาที ---
        if (playerNoise != null)
        {
            playerNoise.AmplitudeGain = shakeAmplitude;
            yield return new WaitForSeconds(shakeDuration);
            playerNoise.AmplitudeGain = 0f;
        }

        // --- 2. ตัดสลับไปกล้องคัตซีน ---
        // ปิดสคริปต์คุมกล้องเดิมของผู้เล่นทิ้งไปเลย
        if (playerCamScript != null) playerCamScript.enabled = false;

        if (cinematicVcam != null)
        {
            // 🔥 แค่สั่งเปิดกล้องคัตซีน Cinemachine จะทำการแพนและซูมภาพไปหากล้องคัตซีนให้เองแบบสมูทสุดๆ!
            cinematicVcam.gameObject.SetActive(true);

            // ให้เวลา Cinemachine เบลนด์ภาพไปหากล้องคัตซีน (ปกติใช้เวลา 2 วินาที)
            yield return new WaitForSeconds(2.5f);
        }

        // --- 3. ดวงจันทร์ใหญ่ลอยขึ้นไปแล้ว "หยุด" ---
        if (bigMoon != null && bigMoonTarget != null)
        {
            while (Vector2.Distance(bigMoon.position, bigMoonTarget.position) > 0.1f)
            {
                bigMoon.position = Vector3.MoveTowards(bigMoon.position, bigMoonTarget.position, bigMoonSpeed * Time.deltaTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.5f);

        // --- 4. Fade เข้า Scene ห้องบอส ---
        yield return StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
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

        SceneManager.LoadScene(bossSceneName);
    }
}