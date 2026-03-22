using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashX_Boss2_TorpedoAI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject smallWarningPrefab; // ลูกเล็กเตือน
    public GameObject realTorpedoPrefab;  // ของจริงดาเมจ 50

    [Header("Spawn Points")]
    public Transform bossFirePoint;       // จุดที่บอสยิงลูกเตือนขึ้นฟ้า
    public Transform[] skyDropPoints;     // จุดสุ่ม 5 จุดบนฟ้า (ลากมาใส่ในนี้)

    [Header("Settings")]
    public int dropCount = 3;             // จำนวนที่ร่วงลงมาต่อ 1 เวฟ (สุ่ม 3 จุด จาก 5 จุด)
    public float warningDelay = 1.5f;     // เวลาหน่วงหลังจากยิงลูกเตือน ก่อนของจริงร่วง
    public float cooldown = 4f;           // พักเหนื่อยก่อนยิงเวฟต่อไป

    void OnEnable()
    {
        StartCoroutine(TorpedoRoutine());
    }

    IEnumerator TorpedoRoutine()
    {
        while (true)
        {
            // 1. 🔥 ยิงลูกเตือน 3 นัดขึ้นฟ้าพร้อมกัน (ยิงจากตัวบอส)
            if (smallWarningPrefab != null && bossFirePoint != null)
            {
                // เสกแบบเฉียงนิดๆ หรือซ้อนกันก็ได้ (ในที่นี้เสกซ้อนกันให้พุ่งขึ้นไปเป็นกระจุก)
                for (int i = 0; i < 3; i++)
                {
                    Instantiate(smallWarningPrefab, bossFirePoint.position, Quaternion.identity);
                }
            }

            // 2. ⏱️ รอเวลาให้ผู้เล่นเตรียมตัวหลบ
            yield return new WaitForSeconds(warningDelay);

            // 3. 💥 สุ่มจุดตกและปล่อยของจริงลงมา!
            if (realTorpedoPrefab != null && skyDropPoints.Length > 0)
            {
                // สร้างลิสต์จำลองเพื่อเอาไว้สุ่มจุดแบบไม่ซ้ำกัน
                List<Transform> availablePoints = new List<Transform>(skyDropPoints);
                int actualDropCount = Mathf.Min(dropCount, availablePoints.Count);

                for (int i = 0; i < actualDropCount; i++)
                {
                    // สุ่มเลือก 1 จุดจากที่เหลืออยู่
                    int randomIndex = Random.Range(0, availablePoints.Count);
                    Transform selectedPoint = availablePoints[randomIndex];

                    // เสกตอร์ปิโดของจริง
                    Instantiate(realTorpedoPrefab, selectedPoint.position, selectedPoint.rotation);

                    // เอาจุดนี้ออกจากลิสต์ชั่วคราว เพื่อไม่ให้ลูกถัดไปสุ่มตกจุดเดียวกัน
                    availablePoints.RemoveAt(randomIndex);
                }
            }

            // 4. 💤 พักคูลดาวน์
            yield return new WaitForSeconds(cooldown);
        }
    }
}