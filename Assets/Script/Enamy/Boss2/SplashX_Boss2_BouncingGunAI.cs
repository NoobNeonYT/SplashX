using System.Collections;
using UnityEngine;

public class SplashX_Boss2_BouncingGunAI : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bouncingBulletPrefab;

    [Header("Settings")]
    public float fireRate = 2f;      // ยิงทุกๆ กี่วินาที
    public float bulletSpeed = 15f;  // ความเร็วพุ่ง
    public float rotationSpeed = 5f;

    private Transform player;

    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(ShootRoutine());
    }

    void Update()
    {
        // หันปากกระบอกปืนตามผู้เล่นตลอดเวลา
        if (player != null)
        {
            Vector2 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireRate);

            if (bouncingBulletPrefab != null && firePoint != null)
            {
                // เสกกระสุน
                GameObject bullet = Instantiate(bouncingBulletPrefab, firePoint.position, firePoint.rotation);

                // ดันกระสุนให้พุ่งไปข้างหน้าอย่างแรง!
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = firePoint.right * bulletSpeed;
                }

                // สั่งทำลายกระสุนทิ้งหลังผ่านไป 10 วินาที จะได้ไม่รกฉาก
                Destroy(bullet, 10f);
            }
        }
    }
}