using UnityEngine;

public class SplashX_HarmfulObject : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 20;
    public string playerTag = "Player";

    // 1. สำหรับอาวุธประเภท "ทะลุผ่าน" (Is Trigger = true) เช่น เลเซอร์, ตอร์ปิโด
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            ApplyDamage(other.gameObject);
        }
    }

    // 2. 🔥 เพิ่มใหม่! สำหรับอาวุธประเภท "ของแข็งเด้งได้" (Is Trigger = false) เช่น กระสุนเด้ง
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            ApplyDamage(collision.gameObject);
        }
    }

    // ฟังก์ชันหักเลือด (ยุบรวมไว้ตรงนี้จะได้ไม่เขียนโค้ดซ้ำ)
    private void ApplyDamage(GameObject target)
    {
        Debug.Log("🔥 อาวุธบอสโดนผู้เล่น! สร้างดาเมจ: " + damageAmount);
        SplashX_PlayerStats playerStats = target.GetComponent<SplashX_PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damageAmount);
        }
    }
}