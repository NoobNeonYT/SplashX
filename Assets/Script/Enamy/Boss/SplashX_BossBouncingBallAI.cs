using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SplashX_BossNode))]
public class SplashX_BossBouncingBallAI : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject bouncingBallPrefab;
    public Transform firePoint;
    public float cooldownTime = 3.5f;   // ระยะเวลาพักก่อนยิงลูกต่อไป
    public float startDelay = 1f;

    [Header("Random Force (สุ่มแรงกระเด้ง)")]
    public float minForceX = 5f;        // แรงผลักไปข้างหน้าน้อยสุด
    public float maxForceX = 15f;       // แรงผลักไปข้างหน้ามากสุด
    public float minForceY = 2f;        // แรงโยนขึ้นฟ้าน้อยสุด
    public float maxForceY = 8f;        // แรงโยนขึ้นฟ้ามากสุด

    [Tooltip("ทิศทางการยิง: ถ้าป้อมอยู่ซ้ายยิงไปขวาใส่ 1 / ถ้าป้อมอยู่ขวายิงไปซ้ายใส่ -1")]
    public float shootDirectionX = 1f;

    private SplashX_BossNode nodeStats;

    void OnEnable()
    {
        nodeStats = GetComponent<SplashX_BossNode>();
        StartCoroutine(SpawnBallRoutine());
    }

    IEnumerator SpawnBallRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (nodeStats != null && !nodeStats.isDestroyed)
        {
            if (bouncingBallPrefab != null && firePoint != null)
            {
                // 1. เสกลูกบอลออกมา
                GameObject ball = Instantiate(bouncingBallPrefab, firePoint.position, Quaternion.identity);
                Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // 2. สุ่มแรงผลัก X และ Y
                    float randX = Random.Range(minForceX, maxForceX) * shootDirectionX;
                    float randY = Random.Range(minForceY, maxForceY);

                    // 3. เตะลูกบอลออกไปด้วยแรงที่สุ่มได้
                    rb.AddForce(new Vector2(randX, randY), ForceMode2D.Impulse);
                }
            }

            yield return new WaitForSeconds(cooldownTime);
        }
    }
}                   