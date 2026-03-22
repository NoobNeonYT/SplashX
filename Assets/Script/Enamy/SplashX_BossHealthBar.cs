using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SplashX_BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bossBarContainer;
    public Image redFillImage;

    private List<SplashX_Enemy> bossParts = new List<SplashX_Enemy>();
    private float totalMaxHealth = 0f;

    void Start()
    {
        // 1. ค้นหาศัตรู "ทุกตัว" ในฉาก
        SplashX_Enemy[] allEnemies = FindObjectsByType<SplashX_Enemy>(FindObjectsSortMode.None);

        // 2. คัดกรองเอาเฉพาะตัวที่เป็นบอส (เช็คจาก isBoss หรือ isBossDontDestroy ก็ได้)
        foreach (var enemy in allEnemies)
        {
            if (enemy.isBoss || enemy.isBossDontDestroy)
            {
                bossParts.Add(enemy);
                totalMaxHealth += enemy.maxHealth; // รวม Max HP
            }
        }

        // 3. จัดการเปิด/ปิด UI หลอดเลือด
        if (bossParts.Count > 0)
        {
            bossBarContainer.SetActive(true);
            redFillImage.fillAmount = 1f;
        }
        else
        {
            bossBarContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (bossParts.Count == 0 || totalMaxHealth <= 0) return;

        float currentTotalHealth = 0f;

        // ล้างรายชื่อบอสที่โดนทำลาย (เผื่อมีบอสบางตัวที่ไม่ได้ติ๊ก isBossDontDestroy แล้วโดนลบทิ้ง)
        bossParts.RemoveAll(part => part == null);

        // 4. คำนวณเลือดปัจจุบันรวมกัน
        foreach (var boss in bossParts)
        {
            if (boss != null)
            {
                // ใช้ Mathf.Max ดักไว้ เผื่อเลือดป้อมที่พังไปแล้วติดลบ จะได้ไม่เอาค่าลบมาหักหลอดเลือด
                currentTotalHealth += Mathf.Max(0, boss.currentHealth);
            }
        }

        // 5. อัปเดตความยาวหลอดแดง
        redFillImage.fillAmount = currentTotalHealth / totalMaxHealth;

        // 6. ถ้าเลือดรวมเหลือ 0 ให้ซ่อนหลอดเลือด (บอสพังหมดแล้ว)
        if (currentTotalHealth <= 0)
        {
            bossBarContainer.SetActive(false);
        }
    }
}