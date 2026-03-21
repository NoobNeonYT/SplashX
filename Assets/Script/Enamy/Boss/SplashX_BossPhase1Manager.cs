using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 🔥 ตัวนี้แหละที่แก้ Error คำว่า Image

public class SplashX_BossPhase1Manager : MonoBehaviour
{
    [Header("Right Wall Nodes (ตีฝั่งนี้ก่อน)")]
    public SplashX_BossNode[] rightNodes;

    [Header("Left Wall Nodes (เปิดเมื่อฝั่งขวาพังหมด)")]
    public SplashX_BossNode[] leftNodes;

    [Header("Phase 1 Outro Cinematic")]
    public Transform farawayBoss;     // บอสที่อยู่ไกลๆ 
    public Transform abyssTarget;     // จุดที่บอสจะร่วงลงเหว
    public Transform ufoObject;       // UFO ที่จะลอยลงมา
    public Transform ufoTarget;       // จุดที่ UFO ลอยมาหยุดตรงกลางจอ

    private bool phase1Complete = false;
    private bool rightWallDestroyed = false;

    [Header("Next Phase Transition")]
    public string nextPhaseSceneName = "BossRoom_Phase2";

    void Start()
    {
        // เริ่มเกมมา ปิดฝั่งซ้ายไว้ก่อน (เป็นอมตะและยังไม่ยิง)
        foreach (var node in leftNodes)
        {
            node.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (phase1Complete) return;

        // 1. เช็คว่าฝั่งขวาพังหมดหรือยัง
        if (!rightWallDestroyed && CheckAllNodesDestroyed(rightNodes))
        {
            rightWallDestroyed = true;
            ActivateLeftWall();
        }

        // 2. ถ้าซ้ายเปิดแล้ว เช็คว่าฝั่งซ้ายพังหมดหรือยัง
        if (rightWallDestroyed && CheckAllNodesDestroyed(leftNodes))
        {
            phase1Complete = true;
            StartCoroutine(PlayOutroCinematic());
        }
    }

    bool CheckAllNodesDestroyed(SplashX_BossNode[] nodes)
    {
        foreach (var node in nodes)
        {
            if (!node.isDestroyed) return false; // ถ้ามีแม้แต่ 1 อันที่ยังไม่พัง = ถือว่ายังไม่หมด
        }
        return true;
    }

    void ActivateLeftWall()
    {
        Debug.Log("ฝั่งขวาถูกทำลายหมดแล้ว! เปิดระบบฝั่งซ้าย!!");
        // เปิดใช้งานฝั่งซ้าย ผู้เล่นต้องกระโดดไปตีต่อ
        foreach (var node in leftNodes)
        {
            node.gameObject.SetActive(true);
        }
    }

    IEnumerator PlayOutroCinematic()
    {
        Debug.Log("ชนะเฟส 1! เริ่มเล่นฉาก Cinematic...");

        // 1. เลื่อนบอสไกลๆ (Moon) ตกลงไปในเหว
        if (farawayBoss != null && abyssTarget != null)
        {
            while (Vector2.Distance(farawayBoss.position, abyssTarget.position) > 0.1f)
            {
                farawayBoss.position = Vector3.MoveTowards(farawayBoss.position, abyssTarget.position, 5f * Time.deltaTime);
                yield return null;
            }
        }

        // 2. รอซักแป๊บนึง ให้คนเล่นตายใจ
        yield return new WaitForSeconds(1.5f);

        // 3. UFO ยักษ์ ลอยลงมาตรงกลางจอ
        if (ufoObject != null && ufoTarget != null)
        {
            while (Vector2.Distance(ufoObject.position, ufoTarget.position) > 0.1f)
            {
                ufoObject.position = Vector3.MoveTowards(ufoObject.position, ufoTarget.position, 6f * Time.deltaTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 4. ไฮไลต์เด็ด: UFO ดูด Moon กลับขึ้นมาประกอบร่าง! 
        // (เลื่อนดวงจันทร์จากก้นเหว กลับขึ้นมาหาตำแหน่ง UFO แบบพุ่งปรี๊ด)
        if (farawayBoss != null && ufoObject != null)
        {
            Debug.Log("🛸 UFO ลำแสงแทรกเตอร์ทำงาน! กำลังดูดดวงจันทร์ขึ้นมา...");
            while (Vector2.Distance(farawayBoss.position, ufoObject.position) > 0.1f)
            {
                // ความเร็ว 15f คือดูดขึ้นมาอย่างไว
                farawayBoss.position = Vector3.MoveTowards(farawayBoss.position, ufoObject.position, 15f * Time.deltaTime);
                yield return null;
            }

            // พอตัวมันชนกับ UFO ปุ๊บ ปิดรูปดวงจันทร์ทิ้งให้เหมือนประกอบร่างเสร็จแล้ว
            farawayBoss.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeAndLoadNextPhase());
    }

    // สร้างระบบเฟดจอดำแบบเขียนโค้ดสด ไม่ต้องง้อ Canvas
    IEnumerator FadeAndLoadNextPhase()
    {
        Debug.Log("🎬 กำลังโหลดฉาก Phase 2...");

        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // ค่อยๆ มืดลง 1.5 วินาที
        float fadeDuration = 1.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;

        // ย้ายไปฉากเฟส 2
        SceneManager.LoadScene(nextPhaseSceneName);
    }
}
