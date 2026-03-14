using UnityEngine;
using UnityEngine.UI; // ต้องมีบรรทัดนี้เพื่อสั่งงาน UI

public class SplashX_UIManager : MonoBehaviour
{
    [Header("Player Reference")]
    public SplashX_PlayerStats playerStats; // ลากตัวละคร Kyo มาใส่ช่องนี้

    [Header("UI Fill Images")]
    public Image hpFill;       // ลาก Image HP_Fill มาใส่
    public Image staminaFill;  // ลาก Image Stam_Fill มาใส่

    void Update()
    {
        if (playerStats == null) return;

        // --- อัปเดตหลอดเลือด HP ---
        // ต้องแปลงเป็น float ก่อนหาร ไม่งั้นค่าจะเป็น 0 หรือ 1 เท่านั้น
        float hpPercentage = (float)playerStats.currentHp / playerStats.maxHp;
        hpFill.fillAmount = hpPercentage;

        // --- อัปเดตหลอด Stamina ---
        float staminaPercentage = playerStats.currentStamina / playerStats.maxStamina;
        staminaFill.fillAmount = staminaPercentage;
    }
}