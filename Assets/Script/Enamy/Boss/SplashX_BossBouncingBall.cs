using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SplashX_BossBouncingBall : MonoBehaviour
{
    [Header("Ball Stats")]
    public int damage = 20;
    public int maxBounces = 4;           // จำนวนครั้งที่เด้งได้ก่อนระเบิด
    public GameObject explosionVFX;      // เอฟเฟกต์ระเบิด
    public LayerMask groundLayer;        // เลเยอร์พื้นและกำแพง

    private int currentBounces = 0;
    private bool isExploding = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploding) return;

        // 1. ถ้าชนผู้เล่น -> ระเบิดและทำดาเมจทันที
        if (collision.gameObject.CompareTag("Player"))
        {
            SplashX_PlayerStats pStats = collision.gameObject.GetComponent<SplashX_PlayerStats>();
            if (pStats != null) pStats.TakeDamage(damage);

            Explode();
            return;
        }

        // 2. ถ้าชนพื้น/กำแพง -> ให้นับจำนวนครั้งที่เด้ง
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            currentBounces++;

            // ถ้าเด้งครบกำหนดแล้ว ให้ระเบิด
            if (currentBounces >= maxBounces)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        isExploding = true;

        // เสกเอฟเฟกต์ระเบิด (ถ้ามี)
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}