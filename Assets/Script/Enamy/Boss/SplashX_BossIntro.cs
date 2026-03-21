using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashX_BossIntro : MonoBehaviour
{
    [Header("Cinematic Objects")]
    public Transform bigMoon;       // ดวงจันทร์ดวงใหญ่ (ฉากหลัง)
    public Transform bigMoonTarget; // จุดที่ดวงจันทร์ใหญ่จะลอยขึ้นไป (สร้าง Empty Object ทิ้งไว้นอกจอ)
    public Transform smallMoon;     // ดวงจันทร์ดวงเล็ก
    public Transform smallMoonTarget; // จุดที่ดวงจันทร์เล็กจะลอยลงมา

    [Header("Settings")]
    public float moonMoveSpeed = 2f;
    public float shakeDuration = 1.5f;
    public float shakeMagnitude = 0.3f;
    public string bossSceneName = "BossRoom_Phase1"; // ชื่อฉากห้องบอสที่จะโหลดไป

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
        // 1. กล้องสั่น! (ถอดการควบคุมผู้เล่นด้วยถ้ามีสคริปต์คุม)
        Vector3 originalCamPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            Camera.main.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalCamPos; // คืนค่ากล้อง

        // 2. ดวงจันทร์ใหญ่ลอยขึ้น และดวงจันทร์เล็กลอยลงมา
        while (Vector2.Distance(bigMoon.position, bigMoonTarget.position) > 0.1f ||
               Vector2.Distance(smallMoon.position, smallMoonTarget.position) > 0.1f)
        {
            bigMoon.position = Vector3.MoveTowards(bigMoon.position, bigMoonTarget.position, moonMoveSpeed * Time.deltaTime);
            smallMoon.position = Vector3.MoveTowards(smallMoon.position, smallMoonTarget.position, (moonMoveSpeed * 0.8f) * Time.deltaTime);
            yield return null;
        }

        // ทำลายดวงจันทร์ใหญ่ทิ้ง
        Destroy(bigMoon.gameObject);

        // รอจังหวะแป๊บนึงให้ผู้เล่นเห็นว่ามันลงมาแล้ว
        yield return new WaitForSeconds(1f);

        // 3. ย้ายซีนไปห้องบอส!
        SceneManager.LoadScene(bossSceneName);
    }
}