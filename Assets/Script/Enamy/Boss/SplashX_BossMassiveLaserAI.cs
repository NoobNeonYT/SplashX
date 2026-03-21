using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SplashX_BossNode))]
public class SplashX_BossMassiveLaserAI : MonoBehaviour
{
    [Header("Laser Objects")]
    public GameObject massiveLaserObj; // ลาก Object เลเซอร์ยักษ์มาใส่
    public GameObject warningVFX;      // ลากแสงเตือน (เส้นเล็กๆ ก่อนยิงจริง) มาใส่

    [Header("Timing Settings")]
    public float startDelay = 2f;      // ให้เวลาผู้เล่นตั้งตัวตอนเข้าห้องบอส
    public float cooldownTime = 4f;    // พักคูลดาวน์ก่อนยิงรอบต่อไป
    public float warningTime = 1.5f;   // เวลาชาร์จเตือนก่อนยิง
    public float firingTime = 2.5f;    // เวลาที่เลเซอร์แช่ค้างไว้ในฉาก

    [Header("Shake Effect")]
    public float shakeIntensity = 0.2f; // ความแรงในการสั่น (แกน Y)

    private bool isFiring = false;
    private Vector3 originalLaserPos;
    private SplashX_BossNode nodeStats;

    void Start()
    {
        nodeStats = GetComponent<SplashX_BossNode>();

        if (massiveLaserObj != null)
        {
            // จำพิกัดดั้งเดิมไว้ จะได้ดึงกลับมาถูกที่หลังสั่นเสร็จ
            originalLaserPos = massiveLaserObj.transform.localPosition;
            massiveLaserObj.SetActive(false);
        }
        if (warningVFX != null) warningVFX.SetActive(false);

        StartCoroutine(LaserCycleRoutine());
    }

    IEnumerator LaserCycleRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        // ยิงวนไปเรื่อยๆ ตราบใดที่จุดอ่อนยังไม่พัง
        while (nodeStats != null && !nodeStats.isDestroyed)
        {
            // 1. ชาร์จพลัง! เปิดเส้นเตือน
            if (warningVFX != null) warningVFX.SetActive(true);
            yield return new WaitForSeconds(warningTime);
            if (warningVFX != null) warningVFX.SetActive(false);

            // เช็คอีกรอบ เผื่อป้อมโดนตีแตกตอนกำลังชาร์จพอดี
            if (nodeStats.isDestroyed) break;

            // 2. ยิงเลเซอร์มหึมา!
            isFiring = true;
            if (massiveLaserObj != null) massiveLaserObj.SetActive(true);

            yield return new WaitForSeconds(firingTime);

            // 3. ปิดเลเซอร์ พักคูลดาวน์
            isFiring = false;
            if (massiveLaserObj != null)
            {
                massiveLaserObj.transform.localPosition = originalLaserPos; // จัดทรงให้ตรง
                massiveLaserObj.SetActive(false);
            }

            yield return new WaitForSeconds(cooldownTime);
        }

        // ถ้าหลุดลูป (ป้อมพังแล้ว) ให้ปิดทุกอย่างทิ้ง
        isFiring = false;
        if (massiveLaserObj != null) massiveLaserObj.SetActive(false);
        if (warningVFX != null) warningVFX.SetActive(false);
    }

    void Update()
    {
        // 💥 ระบบสั่นเลเซอร์ตอนกำลังยิง
        if (isFiring && massiveLaserObj != null && !nodeStats.isDestroyed)
        {
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            // ขยับเฉพาะแกน Y ขึ้นลงรัวๆ ให้ดูเหมือนพลังงานกระชาก
            massiveLaserObj.transform.localPosition = originalLaserPos + new Vector3(0, shakeY, 0);
        }
    }
}