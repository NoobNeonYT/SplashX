using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashX_DebugMode : MonoBehaviour
{
    // ทำให้ Object นี้เป็นอมตะข้ามซีน
    public static SplashX_DebugMode instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // บินตามไปทุกฉาก!
        }
        else
        {
            Destroy(gameObject); // ถ้าเจอตัวซ้ำซ้อนให้ลบทิ้ง
        }
    }

    void Update()
    {
        // ==========================================
        // 🚀 โซนวาร์ปข้ามฉาก (ปุ่ม NumPad 1 - 6)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.Keypad1)) LoadScene("MapLv2");
        if (Input.GetKeyDown(KeyCode.Keypad2)) LoadScene("Background2_Boss");
        if (Input.GetKeyDown(KeyCode.Keypad3)) LoadScene("MapLv3");
        if (Input.GetKeyDown(KeyCode.Keypad4)) LoadScene("Final_Boss_Phase_0");
        if (Input.GetKeyDown(KeyCode.Keypad5)) LoadScene("Final_Boss_Phase_1");
        if (Input.GetKeyDown(KeyCode.Keypad6)) LoadScene("Final_Boss_Phase_2");

        // ==========================================
        // 🔥 โซนสูตรโกงพลัง (ปุ่ม NumPad 7 - 8)
        // ==========================================

        // NumPad 7: ลดค่า MaxPerk = 0 (ใช้ Perk รัวๆ)
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            SplashX_PerkSystem perkSys = FindObjectOfType<SplashX_PerkSystem>();
            if (perkSys != null)
            {
                perkSys.maxPerk = 0;
                Debug.Log("🔥 [Debug] สูตรติด! MaxPerk = 0");
            }
            else Debug.LogWarning("[Debug] หา SplashX_PerkSystem ในฉากนี้ไม่เจอ!");
        }

        // NumPad 8: อัลติพร้อมใช้ (Ultimate Charge = 0)
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            SplashX_PlayerMovement playerMove = FindObjectOfType<SplashX_PlayerMovement>();
            if (playerMove != null)
            {
                playerMove.ultimateChargeRequired = 0;
                Debug.Log("🌟 [Debug] สูตรติด! Ultimate Charge Required = 0");
            }
            else Debug.LogWarning("[Debug] หา SplashX_PlayerMovement ในฉากนี้ไม่เจอ!");
        }
    }

    // ฟังก์ชันจัดการตอนเปลี่ยนฉาก
    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // รีเซ็ตเวลาให้เดินปกติ (เผื่อเผลอกดตอนเกมหยุด)
        SceneManager.LoadScene(sceneName);
        Debug.Log("🚀 [Debug] กำลังวาร์ปไปฉาก: " + sceneName);
    }
}