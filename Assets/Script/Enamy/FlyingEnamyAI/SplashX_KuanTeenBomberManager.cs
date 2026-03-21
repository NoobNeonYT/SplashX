using System.Collections;
using UnityEngine;

public class SplashX_KuanTeenBomberManager : MonoBehaviour
{
    [Header("Bomber Settings")]
    public GameObject bomberPrefab; // เอา Prefab ของ SplashX_KuanTeenBomberAI มาใส่ช่องนี้

    [Tooltip("เวลาสุ่มต่ำสุดที่จะโผล่มา (วินาที) เช่น 60 = 1 นาที")]
    public float minSpawnTime = 60f;

    [Tooltip("เวลาสุ่มสูงสุดที่จะโผล่มา (วินาที) เช่น 150 = 2 นาทีครึ่ง")]
    public float maxSpawnTime = 150f;

    [Header("Spawn Position (จุดเกิดนอกจอ)")]
    public float offScreenDistanceX = 30f;
    public float offScreenHeightY = 15f;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // เริ่มวงจรการนับเวลาสุ่มเสกตัวกวนตีน
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true) // วนลูปทำงานไปตลอดทั้งด่าน
        {
            // 1. สุ่มเวลารอ
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // 2. ถ้าผู้เล่นยังมีชีวิตอยู่ ให้เริ่มกระบวนการเสก
            if (player != null && bomberPrefab != null)
            {
                // สุ่มว่าจะบินมาจากฝั่งซ้าย หรือ ฝั่งขวา
                float side = Random.value > 0.5f ? 1f : -1f;

                // คำนวณพิกัดเกิดให้อยู่นอกจอ
                Vector2 spawnPos = new Vector2(player.position.x + (offScreenDistanceX * side), player.position.y + offScreenHeightY);

                // เสกออกมาป่วน!
                Instantiate(bomberPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}