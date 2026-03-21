using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
    // 🔥 ระบบคุมเสียงเดิน
    private void HandleWalkSound()
    {
        if (audioSource == null || Walk == null) return;

        // เช็คเงื่อนไข: ต้องอยู่บนพื้น + มีการกดปุ่มเดิน + ไม่ได้กำลังตี/พุ่ง/เจ็บ/ตาย
        bool isWalking = isGrounded && Mathf.Abs(moveInput) > 0 && !isAttacking && !isDashing && !isHurt && (playerStats != null && playerStats.currentHp > 0);

        if (isWalking)
        {
            // ถ้ายังไม่ได้เล่นเสียงเดิน ให้เริ่มเล่น (และตั้งค่าให้มันเล่นวนซ้ำ Loop)
            if (!audioSource.isPlaying || audioSource.clip != Walk)
            {
                audioSource.clip = Walk;
                audioSource.loop = true; // เปิด Loop ให้เสียงเดินเล่นต่อเนื่อง
                audioSource.Play();
            }
        }
        else
        {
            // ถ้าหยุดเดิน หรือกระโดดลอยฟ้า ให้หยุดเสียงเดินทันที
            if (audioSource.clip == Walk && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false; // ปิด Loop เผื่อเสียงอื่นมาใช้ต่อ
            }
        }
    }
}