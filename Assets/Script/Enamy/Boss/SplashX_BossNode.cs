using UnityEngine;

[RequireComponent(typeof(SplashX_Enemy))]
[RequireComponent(typeof(SpriteRenderer))]
public class SplashX_BossNode : MonoBehaviour
{
    [Header("Sprite States")]
    public Sprite activeSprite;    // ภาพตอนติดไฟ
    public Sprite destroyedSprite; // ภาพตอนดับ

    [HideInInspector] public bool isDestroyed = false;
    private SplashX_Enemy enemyStats;
    private SpriteRenderer sr;

    void Start()
    {
        enemyStats = GetComponent<SplashX_Enemy>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = activeSprite;
    }

    void Update()
    {
        // ถ้าเลือดยังไม่หมด แต่พังไปแล้ว (กันบัครันซ้ำ)
        if (!isDestroyed && enemyStats.currentHealth <= 0)
        {
            DestroyNode();
        }
    }

    void DestroyNode()
    {
        isDestroyed = true;
        sr.sprite = destroyedSprite;

        // 🔥 ปิดสคริปต์อาวุธ (เดี๋ยวเราจะมาเขียนเพิ่มทีหลังให้มันหยุดยิงตอนพัง)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this && script != enemyStats) script.enabled = false;
        }
    }
}