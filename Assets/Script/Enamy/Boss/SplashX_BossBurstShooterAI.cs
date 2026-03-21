using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SplashX_BossNode))]
public class SplashX_BossBurstShooterAI : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // ใช้ Prefab กระสุนศัตรูอันเดิม (SplashX_EnemyProjectile) ได้เลย
    public Transform firePoint;

    [Header("Burst Settings")]
    public int minShots = 3;           // ขั้นต่ำ 3 นัด
    public int maxShots = 7;           // สูงสุด 7 นัด
    public float timeBetweenShots = 0.15f; // ความรัวของกระสุนใน 1 ชุด (ยิ่งน้อยยิ่งรัว)
    public float cooldownTime = 2.5f;      // เวลาพักเหนื่อยก่อนยิงชุดต่อไป
    public float startDelay = 1f;          // หน่วงเวลาตอนเริ่มเกมเล็กน้อย

    private SplashX_BossNode nodeStats;
    private Transform player;

    void Start()
    {
        nodeStats = GetComponent<SplashX_BossNode>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(BurstShootRoutine());
    }

    IEnumerator BurstShootRoutine()
    {
        // รอแป๊บนึงตอนเริ่มเฟส ให้ผู้เล่นตั้งตัว
        yield return new WaitForSeconds(startDelay);

        // ทำงานวนไปเรื่อยๆ ตราบใดที่ป้อมยังไม่พัง
        while (nodeStats != null && !nodeStats.isDestroyed)
        {
            if (player == null) break;

            // 1. สุ่มจำนวนกระสุนในชุดนี้ (3 ถึง 7 นัด)
            int shotsThisBurst = Random.Range(minShots, maxShots + 1);

            // 2. กราดยิงทีละนัดจนครบโควตา
            for (int i = 0; i < shotsThisBurst; i++)
            {
                // ถ้าป้อมโดนตีแตกกลางคัน หรือผู้เล่นตาย ให้เบรกการยิงทันที
                if (nodeStats.isDestroyed || player == null) break;

                ShootAtPlayer();

                // รอเสี้ยววิก่อนยิงนัดต่อไปในชุดเดียวกัน
                yield return new WaitForSeconds(timeBetweenShots);
            }

            // 3. พักคูลดาวน์ปืน ก่อนจะเริ่มสุ่มยิงชุดใหม่
            yield return new WaitForSeconds(cooldownTime);
        }
    }

    void ShootAtPlayer()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // คำนวณองศาให้ปลายกระบอกหันไปเล็งที่ผู้เล่นเป๊ะๆ
            Vector2 dirToPlayer = player.position - firePoint.position;
            float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

            // หมุนภาพกระสุนให้พุ่งไปตามองศาที่เล็งไว้
            Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

            // เสกกระสุนออกมา!
            Instantiate(projectilePrefab, firePoint.position, bulletRotation);
        }
    }
} 