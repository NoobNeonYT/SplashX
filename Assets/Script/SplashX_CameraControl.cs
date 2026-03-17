using UnityEngine;
using Unity.Cinemachine;

public class SplashX_CameraControl : MonoBehaviour
{
    public CinemachineCamera vcam;

    [Header("Zoom Levels")]
    public float explorationSize = 7.5f;
    public float scoutSize = 9.0f;
    public float combatSize = 5.5f;

    // 🔥 เพิ่มระดับการซูมสำหรับบอส
    public float bossSize = 12.0f;       // ซูมออกกว้างๆ ให้เห็นบอสเต็มตัว
    public float megaBossSize = 16.0f;   // ซูมออกสุดกู่ สำหรับบอสขนาดยักษ์ (ที่กำลังจะทำ)
    public float zoomSpeed = 3f;

    [Header("Detection Zones")]
    public float scoutRadius = 12f;
    public float combatRadius = 6f;
    public float bossDetectRadius = 20f; // 🔥 รัศมีตรวจจับบอส (ตั้งให้กว้างกว่าปกติมากๆ)
    public LayerMask enemyLayer;

    private float targetSize;
    private Transform playerTransform;

    void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
        targetSize = explorationSize;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 1. กวาดเรดาร์หาศัตรูในระยะต่างๆ
        Collider2D[] bossEnemies = Physics2D.OverlapCircleAll(playerTransform.position, bossDetectRadius, enemyLayer);
        Collider2D[] combatEnemies = Physics2D.OverlapCircleAll(playerTransform.position, combatRadius, enemyLayer);
        Collider2D[] scoutEnemies = Physics2D.OverlapCircleAll(playerTransform.position, scoutRadius, enemyLayer);

        bool foundMegaBoss = false;
        bool foundBoss = false;

        // 2. เช็คว่าในรัศมีวงกว้าง มีบอสหรือบอสยักษ์ผสมอยู่ไหม
        foreach (Collider2D col in bossEnemies)
        {
            if (col.CompareTag("MegaBoss"))
            {
                foundMegaBoss = true;
                break; // เจอตัวใหญ่สุดแล้ว หยุดค้นหาได้เลย (Priority สูงสุด)
            }
            else if (col.CompareTag("Boss"))
            {
                foundBoss = true;
            }
        }

        // 3. ลำดับความสำคัญการซูม (เช็คจากสเกลใหญ่สุด -> ไปเล็กสุด)
        if (foundMegaBoss)
        {
            targetSize = megaBossSize; // อลังการขั้นสุด
        }
        else if (foundBoss)
        {
            targetSize = bossSize;     // อลังการปกติ
        }
        else if (combatEnemies.Length > 0)
        {
            targetSize = combatSize;   // นัวเนียระยะประชิด ซูมใกล้ๆ ให้แอคชันมันส์ๆ
        }
        else if (scoutEnemies.Length > 0)
        {
            targetSize = scoutSize;    // เห็นศัตรูไกลๆ ซูมออกนิดนึง
        }
        else
        {
            targetSize = explorationSize; // เดินเล่นปกติ
        }

        // 4. สั่งกล้องให้ค่อยๆ ปรับขนาด (Smooth Zoom)
        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // วาดวงกลมสีม่วง โชว์รัศมีตรวจจับบอส
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerTransform.position, bossDetectRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, scoutRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, combatRadius);
        }
    }
}