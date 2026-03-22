using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashX_DebugMode : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("ลาก Panel ที่ใช้เป็นพื้นหลังหน้าต่าง Debug มาใส่")]
    public GameObject debugPanel;
    [Tooltip("ลาก InputField ที่ให้ผู้เล่นพิมพ์คำสั่งมาใส่")]
    public InputField commandInput;

    void Start()
    {
        // ปิดหน้าต่าง UI ไว้ก่อนตอนเริ่มเกม
        if (debugPanel != null) debugPanel.SetActive(false);

        // ดักจับเมื่อผู้เล่นพิมพ์เสร็จแล้วกด Enter ให้รันคำสั่งทันที
        if (commandInput != null)
        {
            commandInput.onEndEdit.AddListener(ProcessCommand);
        }
    }

    void Update()
    {
        // กดปุ่ม / (Slash) เพื่อเปิด/ปิด Debug UI
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            ToggleDebugUI();
        }
    }

    void ToggleDebugUI()
    {
        if (debugPanel == null || commandInput == null) return;

        bool isActive = !debugPanel.activeSelf;
        debugPanel.SetActive(isActive);

        if (isActive)
        {
            // เปิด UI -> เคลียร์ช่องพิมพ์ และเอาเมาส์ไปโฟกัสให้พร้อมพิมพ์ทันที
            commandInput.text = "";
            commandInput.ActivateInputField();

            // หยุดเวลาในเกมไว้ชั่วคราวตอนพิมพ์ (ถ้าไม่อยากให้หยุด ลบ Time.timeScale ออกได้ครับ)
            Time.timeScale = 0f;
        }
        else
        {
            // ปิด UI -> ให้เวลาเดินตามปกติ
            commandInput.DeactivateInputField();
            Time.timeScale = 1f;
        }
    }

    void ProcessCommand(string command)
    {
        // ถ้าช่องว่างเปล่า หรือเพิ่งปิด UI ไป ไม่ต้องทำอะไร
        if (string.IsNullOrEmpty(command) || !debugPanel.activeSelf) return;

        // ลบช่องว่างหน้า-หลังเผื่อเผลอเคาะสเปซบาร์
        string finalCommand = command.Trim();
        Debug.Log("💻 Debug Command: " + finalCommand);

        switch (finalCommand)
        {
            case "s1":
                LoadScene("MapLv2");
                break;
            case "s2":
                LoadScene("Background2_Boss");
                break;
            case "s3":
                LoadScene("MapLv3");
                break;
            case "s4":
                LoadScene("Final_Boss_Phase_0");
                break;
            case "s5":
                LoadScene("Final_Boss_Phase_1");
                break;
            case "s6":
                LoadScene("Final_Boss_Phase_2");
                break;

            case "NloobNeon":
                // ค้นหาสคริปต์ Perk ในฉาก แล้วปรับ MaxPerk = 0
                SplashX_PerkSystem perkSys = FindObjectOfType<SplashX_PerkSystem>();
                if (perkSys != null)
                {
                    perkSys.maxPerk = 0;
                    Debug.Log("🔥 สูตรติด! MaxPerk = 0 (ใช้สกิล Perk ได้รัวๆ)");
                }
                else Debug.LogWarning("ไม่เจอ SplashX_PerkSystem ในฉาก");
                break;

            case "HanaHa":
                // ค้นหาสคริปต์ Player ในฉาก แล้วปรับเกจเป้าหมาย = 0
                SplashX_PlayerMovement playerMove = FindObjectOfType<SplashX_PlayerMovement>();
                if (playerMove != null)
                {
                    playerMove.ultimateChargeRequired = 0;
                    Debug.Log("🌟 สูตรติด! Ultimate Charge = 0 (อัลติพร้อมใช้ตลอดเวลา)");
                }
                else Debug.LogWarning("ไม่เจอ SplashX_PlayerMovement ในฉาก");
                break;

            default:
                Debug.LogWarning("❌ ไม่มีสูตรนี้ในระบบ: " + finalCommand);
                break;
        }

        // รันคำสั่งเสร็จแล้วปิด UI
        ToggleDebugUI();
    }

    private void LoadScene(string sceneName)
    {
        // คืนค่าเวลาให้เป็น 1 ก่อนย้ายฉาก ไม่งั้นฉากใหม่จะค้าง
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}