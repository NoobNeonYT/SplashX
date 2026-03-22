using System.Collections;
using UnityEngine;

public class SplashX_Boss2_SweepShooter : MonoBehaviour
{
    [Header("Object Parts (ชิ้นส่วนปืน)")]
    [Tooltip("ตัวฐานปืน ที่จะหันหน้าหาผู้เล่นเสมอ")]
    public Transform baseTransform;
    [Tooltip("ปลายกระบอกปืน ที่จะส่ายไปมา")]
    public Transform barrelTransform;
    [Tooltip("จุดปล่อยกระสุน (เอาไปใส่เป็นลูกของ barrelTransform)")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Sweep Settings (การกวาด)")]
    public float sweepAngle = 60f; // กวาดซ้าย 60 ขวา 60 (รวมเป็น 120 องศา)
    public float sweepSpeed = 3f;  // ความเร็วในการส่ายพัดลม

    [Header("Shoot Settings")]
    public int minBullets = 9;
    public int maxBullets = 15;
    public float fireRate = 0.1f;  // ความรัวของกระสุนแต่ละนัด
    public float cooldown = 3f;    // พักรีโหลด

    private Transform player;

    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(ShootRoutine());
    }

    void Update()
    {
        // 1. ฐานปืน: หันหาผู้เล่นแบบ 360 องศาตลอดเวลา
        if (player != null && baseTransform != null)
        {
            Vector2 direction = player.position - baseTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            baseTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 2. ปลายกระบอกปืน: ส่ายไปส่ายมา (ใช้ฟังก์ชันคณิตศาสตร์ Sin เวฟ เพื่อให้มันแกว่งซ้ายขวา)
        if (barrelTransform != null)
        {
            float currentAngle = Mathf.Sin(Time.time * sweepSpeed) * sweepAngle;
            // ใช้ localRotation เพราะเราต้องการให้มันหมุนอิงจากฐานปืน ไม่ใช่อิงจากโลก
            barrelTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            // สุ่มจำนวนกระสุน 9 ถึง 15 นัด
            int bulletCount = Random.Range(minBullets, maxBullets + 1);

            for (int i = 0; i < bulletCount; i++)
            {
                if (bulletPrefab != null && firePoint != null)
                {
                    Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                }
                yield return new WaitForSeconds(fireRate); // เว้นระยะยิงแต่ละนัด
            }

            yield return new WaitForSeconds(cooldown);
        }
    }
}