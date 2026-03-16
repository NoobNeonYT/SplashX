using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashX_GameManager : MonoBehaviour
{
    public static SplashX_GameManager instance;

    [Header("Death & Checkpoint System")]
    public int deathCount = 0;
    public int maxDeathsBeforeReset = 5;

    public Vector2 majorCheckpointPos;
    public Vector2 minorCheckpointPos;
    public bool hasMinorCheckpoint = false;
    private bool hasMajorCheckpoint = false;

    void Awake()
    {
        // ทำให้ GameManager อยู่ยงคงกระพัน
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame(); // โหลดเซฟตอนเปิดเกม
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันเก็บจุด Checkpoint
    public void RegisterCheckpoint(Vector2 pos, bool isMajor)
    {
        if (isMajor)
        {
            majorCheckpointPos = pos;
            hasMajorCheckpoint = true;
            hasMinorCheckpoint = false; // เหยียบจุดหลัก ให้ล้างจุดรองทิ้ง
            SaveGame(); // เซฟเกมลงเครื่องเฉพาะจุดหลัก
            Debug.Log("🔥 Major Checkpoint Saved!");
        }
        else
        {
            minorCheckpointPos = pos;
            hasMinorCheckpoint = true;
            Debug.Log("🚩 Minor Checkpoint Reached!");
        }
    }

    // ฟังก์ชันจัดการตอนตาย
    public void HandlePlayerDeath()
    {
        deathCount++;
        Debug.Log("💀 Player Died! Death Count: " + deathCount);

        if (deathCount >= maxDeathsBeforeReset)
        {
            // ตายครบ 5 ครั้ง -> รีเซ็ตแมพและกลับจุดหลัก
            deathCount = 0;
            hasMinorCheckpoint = false;
            Debug.Log("🔄 5 Deaths Reached! Resetting Map to Major Checkpoint...");

            // โหลด Scene เดิมใหม่เพื่อรีเซ็ตแมพ (มอนสเตอร์เกิดใหม่ ของกลับมาที่เดิม)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // ยังไม่ถึง 5 ครั้ง -> แค่วาร์ปกลับจุดล่าสุด
            RespawnPlayerWithoutReset();
        }
    }

    void RespawnPlayerWithoutReset()
    {
        // สั่งหาตัวผู้เล่นในฉากแล้วจับเทเลพอร์ต
        SplashX_PlayerStats player = FindFirstObjectByType<SplashX_PlayerStats>();
        if (player != null)
        {
            Vector2 respawnPos = hasMinorCheckpoint ? minorCheckpointPos : majorCheckpointPos;
            player.transform.position = respawnPos;
            player.Revive(); // สั่งชุบชีวิต
        }
    }

    // ฟังก์ชันเช็คตำแหน่งเกิด (เอาไว้ให้ Player เรียกใช้ตอนเปิดเกม/โหลดแมพใหม่)
    public Vector2 GetRespawnPosition(Vector2 defaultPos)
    {
        if (hasMinorCheckpoint) return minorCheckpointPos;
        if (hasMajorCheckpoint) return majorCheckpointPos;
        return defaultPos; // ถ้าไม่เคยเหยียบเลยให้เกิดจุดเริ่มต้นเดิม
    }

    // ระบบเซฟเกมด้วย PlayerPrefs (จำค่าลงเครื่อง)
    public void SaveGame()
    {
        PlayerPrefs.SetFloat("MajorPosX", majorCheckpointPos.x);
        PlayerPrefs.SetFloat("MajorPosY", majorCheckpointPos.y);
        PlayerPrefs.SetInt("HasMajor", 1);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.GetInt("HasMajor", 0) == 1)
        {
            majorCheckpointPos.x = PlayerPrefs.GetFloat("MajorPosX");
            majorCheckpointPos.y = PlayerPrefs.GetFloat("MajorPosY");
            hasMajorCheckpoint = true;
        }
    }
}