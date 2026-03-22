using UnityEngine;

// 🛑 บังคับให้ Object ตัวนี้ต้องมี Collider แบบ Is Trigger เสมอ
[RequireComponent(typeof(Collider2D))]
public class SplashX_HarmfulObject : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 20; // จำนวนความเสียหาย (ปรับได้ตามชอบ)
    [Tooltip("ใส่ Tag ของผู้เล่น (ปกติคือ Player)")]
    public string playerTag = "Player";

    // ฟังก์ชันนี้จะทำงานเมื่อ "เริ่มชน" กับวัตถุอื่น
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. เช็คว่าวัตถุที่ชนใช่ "ผู้เล่น" ไหม โดยดูจาก Tag
        if (other.CompareTag(playerTag))
        {
            Debug.Log("🔥 อาวุธบอสโดนผู้เล่น! สร้างดาเมจ: " + damageAmount);

            // 2. ดึงสคริปต์ SplashX_PlayerStats ของผู้เล่นออกมา
            SplashX_PlayerStats playerStats = other.GetComponent<SplashX_PlayerStats>();

            // 3. ถ้าเจอสคริปต์เลือด สั่งหักเลือดทันที!
            if (playerStats != null)
            {
                playerStats.TakeDamage(damageAmount);
            }
        }
    }
}