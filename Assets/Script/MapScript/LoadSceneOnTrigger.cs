using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnTrigger : MonoBehaviour
{
    [Header("Scene")]
    public string sceneName = "Background2_Boss";

    [Header("Transition")]
    public GameObject transitionPanel;
    public float delay = 2f;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLoading) return;

        if (collision.CompareTag("Player"))
        {
            isLoading = true;

            // 🔥 เปิดหน้าโหลด
            if (transitionPanel != null)
            {
                transitionPanel.SetActive(true);
            }

            // 🔥 รอแล้วค่อยโหลด
            Invoke(nameof(LoadScene), delay);
        }
    }

    void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}