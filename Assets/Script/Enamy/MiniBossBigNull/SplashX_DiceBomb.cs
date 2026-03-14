using UnityEngine;

public class SplashX_DiceBomb : MonoBehaviour
{
    public float fallSpeed = 8f;       // ความเร็วตอนตก (ต้องช้ากว่าท่าทุบ)
    public float explosionRadius = 2f; // รัศมีระเบิด
    public int damage = 1;             // ดาเมจลูกเต๋า
    public LayerMask groundLayer;      // เลเยอร์พื้น
    public LayerMask playerLayer;      // เลเยอร์ผู้เล่น
    public GameObject explosionEffect; // เอฟเฟกต์ตอนระเบิด (ถ้ามี)

    private bool isExploding = false;

    void Update()
    {
        if (isExploding) return;

        // ตกลงมาตรงๆ
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // เช็คว่าแตะพื้นหรือยัง
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);
        if (hit.collider != null)
        {
            Explode();
        }
    }

    void Explode()
    {
        isExploding = true;

        // วงกลมเช็คว่าผู้เล่นอยู่ในรัศมีระเบิดไหม
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (playerHit != null)
        {
            SplashX_PlayerStats stats = playerHit.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
        }

        // เสกเอฟเฟกต์ระเบิด (ถ้าใส่ไว้)
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // ทำลายลูกเต๋าทิ้ง
        Destroy(gameObject);
    }

    // วาดเส้นรัศมีระเบิดสีส้มให้เห็นตอนเทส
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}