using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplashX_BossNode))]
public class SplashX_BossMonsterSpawnerAI : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("ใส่ Prefab มอนสเตอร์กี่ชนิดก็ได้ ระบบจะสุ่มเสกออกมา")]
    public GameObject[] monsterPrefabs;
    public Transform spawnPoint;
    public float spawnCooldown = 6f; // ระยะเวลาเสกแต่ละตัว
    public float startDelay = 2f;
    public GameObject spawnVFX; // เอฟเฟกต์ตอนมอนสเตอร์เกิด (เช่น ควันดำๆ)

    [Header("Limit Settings")]
    [Tooltip("จำกัดจำนวนลูกน้องบนจอ (ป้องกันเสกจนล้นแล้วเกมพัง)")]
    public int maxMonstersAtOnce = 3;

    private SplashX_BossNode nodeStats;
    private List<GameObject> activeMonsters = new List<GameObject>(); // จัดเก็บรายชื่อมอนสเตอร์ที่ยังมีชีวิต

    void OnEnable()
    {
        nodeStats = GetComponent<SplashX_BossNode>();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (nodeStats != null && !nodeStats.isDestroyed)
        {
            // 1. กวาดล้างรายชื่อมอนสเตอร์ที่โดนเคียวฟันตายไปแล้วออกจากระบบ
            activeMonsters.RemoveAll(monster => monster == null);

            // 2. ถ้าลูกน้องบนจอยังไม่เกินโควตา ถึงจะยอมเสกเพิ่ม
            if (activeMonsters.Count < maxMonstersAtOnce && monsterPrefabs.Length > 0 && spawnPoint != null)
            {
                // เปิดควัน/แสงเตือนก่อนมอนสเตอร์โผล่
                if (spawnVFX != null) Instantiate(spawnVFX, spawnPoint.position, Quaternion.identity);

                // สุ่มเรียกมอนสเตอร์ 1 ชนิดจากอาเรย์
                int randomIndex = Random.Range(0, monsterPrefabs.Length);
                GameObject spawnedMonster = Instantiate(monsterPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);

                // จดชื่อลงบัญชีไว้ว่าตัวนี้ยังมีชีวิตอยู่
                activeMonsters.Add(spawnedMonster);
            }

            // 3. พักเครื่องก่อนเสกรอบต่อไป (ต่อให้โควตาเต็มก็ต้องรอคูลดาวน์ใหม่)
            yield return new WaitForSeconds(spawnCooldown);
        }
    }

}