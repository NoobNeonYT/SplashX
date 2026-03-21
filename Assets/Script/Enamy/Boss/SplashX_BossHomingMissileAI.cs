using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SplashX_BossNode))]
public class SplashX_BossHomingMissileAI : MonoBehaviour
{
    [Header("Missile Settings")]
    [Tooltip("ใส่ Prefab มิสไซล์ที่มีสคริปต์ SplashX_HomingMissile")]
    public GameObject missilePrefab;
    public Transform firePoint;

    [Header("Firing Rhythm")]
    public float cooldownTime = 4f; // ความถี่ในการปล่อยมิสไซล์ (ยิ่งน้อยยิ่งป่วน)
    public float startDelay = 2f;   // หน่วงเวลาตอนเริ่มเฟส

    private SplashX_BossNode nodeStats;
    private Transform player;

    void Start()
    {
        nodeStats = GetComponent<SplashX_BossNode>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(FireMissileRoutine());
    }

    IEnumerator FireMissileRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        // ทำงานวนไปตราบใดที่ป้อมยังไม่โดนตีแตก
        while (nodeStats != null && !nodeStats.isDestroyed)
        {
            if (player == null) break;

            if (missilePrefab != null && firePoint != null)
            {
                // เสกมิสไซล์ออกมาตามองศาของ FirePoint
                Instantiate(missilePrefab, firePoint.position, firePoint.rotation);
            }

            // พักรอก่อนปล่อยลูกถัดไป
            yield return new WaitForSeconds(cooldownTime);
        }
    }
}