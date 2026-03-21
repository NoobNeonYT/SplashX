using UnityEngine;
using UnityEngine.UI; // ต้องเรียกใช้ UI เสมอเวลาทำหลอดเลือด
using System.Collections.Generic;

public class SplashX_BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bossBarContainer; // ลาก BossHealthBar_Container มาใส่ (เพื่อเปิด/ปิดหลอดเลือด)
    public Image redFillImage;          // ลาก Red_Fill มาใส่ (เพื่อคุมการลดของหลอดแดง)

    private List<SplashX_Enemy> bossParts = new List<SplashX_Enemy>();
    private float totalMaxHealth = 0f;

    void Start()
    {
        // 1. ค้นหาศัตรู "ทุกตัว" ในฉากตอนเริ่มเกม
        SplashX_Enemy[] allEnemies = FindObjectsByType<SplashX_Enemy>(FindObjectsSortMode.None);

        // 2. คัดกรองเอาเฉพาะตัวที่ติ๊ก isBoss = true
        foreach (var enemy in allEnemies)
        {
            if (enemy.isBoss)
            {
                bossParts.Add(enemy);
                totalMaxHealth += enemy.maxHealth; // เอา Max HP มารวมกัน
            }
        }

        // 3. ถ้าด่านนี้มีบอส ให้โชว์หลอดเลือด ถ้าไม่มีให้ซ่อนทิ้ง
        if (bossParts.Count > 0)
        {
            bossBarContainer.SetActive(true);
            redFillImage.fillAmount = 1f; // หลอดเต็ม 100%
        }
        else
        {
            bossBarContainer.SetActive(false);
        }
    }

    void Update()
    {
        // ถ้าไม่มีบอส หรือบอสพังหมดแล้ว ไม่ต้องคำนวณต่อ
        if (bossParts.Count == 0 || totalMaxHealth <= 0) return;

        float currentTotalHealth = 0f;

        // ล้างรายชื่อบอสที่โดนทำลาย (Destroy) ออกจากระบบ กันบัคโค้ดพัง
        bossParts.RemoveAll(part => part == null);

        // 4. คำนวณเลือดปัจจุบันของบอสทุกชิ้นรวมกัน
        foreach (var boss in bossParts)
        {
            if (boss != null)
            {
                // ใช้ Mathf.Max เพื่อกันไม่ให้เลือดติดลบมาหักล้างกัน (เช่น ตีเพลินจนเลือดป้อมติดลบ 10)
                currentTotalHealth += Mathf.Max(0, boss.currentHealth);
            }
        }

        // 5. อัปเดต UI หลอดแดง (สูตร: เลือดปัจจุบัน หารด้วย เลือดเต็ม)
        redFillImage.fillAmount = currentTotalHealth / totalMaxHealth;

        // 6. ถ้าเลือดรวมเหลือ 0 (บอสพังทุกชิ้น) ให้ซ่อนหลอดเลือด
        if (currentTotalHealth <= 0)
        {
            bossBarContainer.SetActive(false);
        }
    }
}