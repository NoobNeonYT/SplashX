using System.Collections;
using UnityEngine;

public class SplashX_SceneSpawnPoint : MonoBehaviour
{
    // 🔥 เปลี่ยนมาใช้ IEnumerator เพื่อให้มัน "รอ" ได้
    IEnumerator Start()
    {
        // 1. รอให้ Unity โหลดทุกอย่างในฉาก และดึงตัวผู้เล่นข้ามมาให้เสร็จสมบูรณ์ก่อน (รอจบเฟรม)
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. เบรกฟิสิกส์! ล้างค่าความเร็วที่ค้างมาจากฉากที่แล้วให้หมด จะได้ไม่สไลด์เด้งกลับ
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // 3. บังคับจับวางตำแหน่ง
            player.transform.position = transform.position;
            Debug.Log("📍 [SpawnPoint] โหลดฉากเสร็จ! ล้างฟิสิกส์และบังคับวาร์ปสำเร็จ");
        }
        else
        {
            Debug.LogWarning("⚠️ [SpawnPoint] หาตัวผู้เล่นไม่เจอ!");
        }
    }
}