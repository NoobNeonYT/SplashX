using UnityEngine;

public class SplashX_SceneSpawnPoint : MonoBehaviour
{
    void Start()
    {
        // 1. ตามหาตัวผู้เล่นในฉาก (ต้องแน่ใจว่าตัวผู้เล่นเซ็ต Tag เป็น "Player" ไว้แล้ว)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 2. จับผู้เล่นวาร์ปมาที่ตำแหน่งของจุดเกิดนี้ทันที
            player.transform.position = transform.position;
            Debug.Log("📍 โหลดฉากใหม่! ย้ายผู้เล่นมาที่จุด Spawn เรียบร้อย");

            // (ถ้ามีระบบล็อกกล้อง ก็สามารถสั่งกล้องให้รีเซ็ตมาที่ผู้เล่นตรงนี้ได้ด้วย)
        }
        else
        {
            Debug.LogWarning("⚠️ หาตัวผู้เล่นไม่เจอ! เช็คว่าผู้เล่นติด Tag 'Player' หรือยัง?");
        }
    }
}