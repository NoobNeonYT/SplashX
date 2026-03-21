using UnityEngine;

public class SplashX_BossLaserHitbox : MonoBehaviour
{
    public int damage = 25;
    [Tooltip("ระยะเวลาหน่วงก่อนจะโดนดาเมจซ้ำ ถ้าผู้เล่นยืนแช่ในเลเซอร์")]
    public float damageTickRate = 0.5f;

    private float nextDamageTime = 0f;

    // ใช้ OnTriggerStay2D เพราะเลเซอร์มันแช่ค้างไว้
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            SplashX_PlayerStats pStats = collision.GetComponent<SplashX_PlayerStats>();
            if (pStats != null)
            {
                pStats.TakeDamage(damage);
                nextDamageTime = Time.time + damageTickRate;
            }
        }
    }
}