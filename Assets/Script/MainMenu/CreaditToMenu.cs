using UnityEngine;
using UnityEngine.SceneManagement; // 🔥 สำคัญมาก! ต้องมีบรรทัดนี้ถึงจะสั่งย้ายซีนได้

public class CreaditToMenu : MonoBehaviour
{
    // ฟังก์ชันนี้เปิดเป็น public เพื่อให้ปุ่ม UI มองเห็นและเรียกใช้ได้
    public void LoadSceneByName(string sceneName)
    {
        Debug.Log("🔄 กำลังโหลดซีน: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // (แถม) ฟังก์ชันสำหรับปุ่ม Quit ออกจากเกม
    public void QuitGame()
    {
        Debug.Log("❌ ปิดเกม");
        Application.Quit();
    }
}