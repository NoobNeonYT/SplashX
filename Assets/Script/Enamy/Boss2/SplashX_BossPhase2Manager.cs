using UnityEngine;

public class SplashX_BossPhase2Manager : MonoBehaviour
{
    [Header("Boss Core (ตัวบอส)")]
    [Tooltip("ลาก Object บอสที่มีสคริปต์ SplashX_Enemy มาใส่")]
    public SplashX_Enemy bossStats;

    [Header("Unlockable Weapons (อาวุธที่รอเปิด)")]
    public GameObject torpedoSystem; // ตอร์ปิโด (เปิดตอน 60%)
    public GameObject laserSystem;   // เลเซอร์ (เปิดตอน 40%)

    // ตัวแปรเช็คว่าเปิดไปหรือยัง จะได้ไม่สั่งเปิดซ้ำทุกเฟรม
    private bool unlocked60 = false;
    private bool unlocked40 = false;

    void Start()
    {
        // 1. ตอนเริ่มเกม (เลือด 100%) บังคับปิดอาวุธ 2 ชิ้นนี้ไว้ก่อนทันที
        if (torpedoSystem != null) torpedoSystem.SetActive(false);
        if (laserSystem != null) laserSystem.SetActive(false);
    }

    void Update()
    {
        if (bossStats == null) return;

        // 2. คำนวณเลือดเป็นเปอร์เซ็นต์ (เอาเลือดปัจจุบัน หาร เลือดเต็ม)
        // ผลลัพธ์จะได้ตั้งแต่ 0.0 (ตาย) ถึง 1.0 (เต็ม)
        float hpPercent = (float)bossStats.currentHealth / bossStats.maxHealth;

        // 3. เช็คเงื่อนไข: เลือด <= 60% (0.6f) และยังไม่ได้เปิดตอร์ปิโด
        if (hpPercent <= 0.60f && !unlocked60)
        {
            unlocked60 = true; // ล็อกเป้าว่าเปิดแล้ว
            if (torpedoSystem != null) torpedoSystem.SetActive(true);
            Debug.Log("⚠️ บอสเลือดเหลือ 60%! ระบบตอร์ปิโดทำงาน!");
        }

        // 4. เช็คเงื่อนไข: เลือด <= 40% (0.4f) และยังไม่ได้เปิดเลเซอร์
        if (hpPercent <= 0.40f && !unlocked40)
        {
            unlocked40 = true; // ล็อกเป้าว่าเปิดแล้ว
            if (laserSystem != null) laserSystem.SetActive(true);
            Debug.Log("🚨 บอสเลือดเหลือ 40%! ระบบเลเซอร์ล็อกเป้าทำงาน!");
        }
    }
}