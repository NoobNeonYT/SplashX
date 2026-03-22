using System.Collections;
using UnityEngine;

// 🛑 บังคับให้ Object ตัวนี้ต้องมี Component ชื่อ LineRenderer เสมอ
[RequireComponent(typeof(LineRenderer))]
public class SplashX_Boss2_TrackingLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public Transform firePoint;
    public GameObject laserPrefab;

    [Header("Aim Line (เส้นเล็งเป้า)")]
    public float aimLineWidth = 0.05f;
    public Color aimColor = new Color(1f, 0f, 0f, 0.4f);
    public LayerMask groundLayer;

    [Header("Timing (วินาที)")]
    public float aimDuration = 3f;
    public float freezeTime = 0.5f;
    public float fireDuration = 1f;
    public float cooldown = 2f;
    public float rotationSpeed = 5f;

    private Transform player;
    private bool isAiming = false;
    private bool isFreezing = false;
    private LineRenderer aimLine;

    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        aimLine = GetComponent<LineRenderer>();
        aimLine.startWidth = aimLineWidth;
        aimLine.endWidth = aimLineWidth;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = aimColor;
        aimLine.endColor = aimColor;
        aimLine.positionCount = 2;
        aimLine.enabled = false;

        StartCoroutine(LaserRoutine());
    }

    void Update()
    {
        if (player == null) return;

        // 1. ระบบหันหน้า: 🔥 หันตามผู้เล่นเฉพาะช่วงกำลังเล็ง (isAiming) เท่านั้น! 
        // พอเข้าช่วง isFreezing ปืนจะหยุดขยับทันที (ให้คนเล่นมีเวลาหลบ)
        if (isAiming)
        {
            Vector2 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 2. ระบบวาดเส้นเล็งเป้า: (เส้นยังคงโชว์อยู่ทั้งตอนกำลังเล็งและตอนหยุดนิ่ง)
        if (aimLine.enabled && firePoint != null)
        {
            aimLine.SetPosition(0, firePoint.position);

            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, 50f, groundLayer);

            if (hit.collider != null)
            {
                aimLine.SetPosition(1, hit.point);
            }
            else
            {
                aimLine.SetPosition(1, firePoint.position + firePoint.right * 50f);
            }
        }
    }

    IEnumerator LaserRoutine()
    {
        while (true)
        {
            // 1. ช่วงเล็งเป้า: หันตาม + เปิดเส้นเล็งเตือน
            isAiming = true;
            if (aimLine != null) aimLine.enabled = true;
            yield return new WaitForSeconds(aimDuration);

            // 2. หยุดเล็ง (ล็อกเป้าหมาย): 0.5 วินาที -> แช่เส้นเล็งไว้ ปืนหยุดขยับ
            isAiming = false;
            isFreezing = true;
            yield return new WaitForSeconds(freezeTime);

            // 3. ยิงเลเซอร์: ปิดเส้นเล็งเตือน แล้วเสกเลเซอร์ของจริงออกมา
            isFreezing = false;
            if (aimLine != null) aimLine.enabled = false;

            if (laserPrefab != null && firePoint != null)
            {
                GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation, firePoint);
                Destroy(laser, fireDuration);
            }
            yield return new WaitForSeconds(fireDuration);

            // 4. พักคูลดาวน์
            yield return new WaitForSeconds(cooldown);
        }
    }
}