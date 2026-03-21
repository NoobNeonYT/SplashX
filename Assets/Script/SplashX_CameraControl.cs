using UnityEngine;
using Unity.Cinemachine;

public class SplashX_CameraControl : MonoBehaviour
{
    public CinemachineCamera vcam;

    [Header("Zoom Levels")]
    public float explorationSize = 7.5f;
    public float scoutSize = 9.0f;
    public float combatSize = 5.5f;

    public float bossSize = 12.0f;
    public float megaBossSize = 16.0f;
    public float zoomSpeed = 3f;

    [Header("Map Boundaries (กันกล้องทะลุพื้น)")]
    [Tooltip("ใส่พิกัดแกน Y ของพื้นล่างสุดของฉาก (ดูเส้นสีแดงใน Scene)")]
    public float mapBottomY = -5f; // 🔥 จุดนี้สำคัญ! ปรับเลขนี้เพื่อยกขอบกล้องล่างสุด

    [Header("Detection Zones")]
    public float scoutRadius = 12f;
    public float combatRadius = 6f;
    public float bossDetectRadius = 20f;
    public LayerMask enemyLayer;

    private float targetSize;
    private Transform playerTransform;

    // 🔥 ตัวแทน (Proxy) ให้กล้องตามแทนผู้เล่น จะได้เขียนโค้ดยกกล้องได้โดยไม่ตีกับ Cinemachine
    private GameObject proxyTarget;

    [HideInInspector] public bool isLockedZ = false;

    void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        targetSize = explorationSize;

        // 🔥 สร้าง Object ตัวแทนล่องหนขึ้นมา แล้วสั่งให้กล้องหันมามองตัวนี้แทนเคียว
        proxyTarget = new GameObject("CameraProxyTarget");
        proxyTarget.transform.position = playerTransform.position;
        vcam.Follow = proxyTarget.transform;
    }

    void Update()
    {
        if (isLockedZ) return;
        if (playerTransform == null) return;

        // 1. กวาดเรดาร์หาศัตรูในระยะต่างๆ
        Collider2D[] bossEnemies = Physics2D.OverlapCircleAll(playerTransform.position, bossDetectRadius, enemyLayer);
        Collider2D[] combatEnemies = Physics2D.OverlapCircleAll(playerTransform.position, combatRadius, enemyLayer);
        Collider2D[] scoutEnemies = Physics2D.OverlapCircleAll(playerTransform.position, scoutRadius, enemyLayer);

        bool foundMegaBoss = false;
        bool foundBoss = false;

        foreach (Collider2D col in bossEnemies)
        {
            if (col.CompareTag("MegaBoss"))
            {
                foundMegaBoss = true;
                break;
            }
            else if (col.CompareTag("Boss"))
            {
                foundBoss = true;
            }
        }

        // 2. ลำดับความสำคัญการซูม
        if (foundMegaBoss) targetSize = megaBossSize;
        else if (foundBoss) targetSize = bossSize;
        else if (combatEnemies.Length > 0) targetSize = combatSize;
        else if (scoutEnemies.Length > 0) targetSize = scoutSize;
        else targetSize = explorationSize;

        // 3. สั่งกล้องให้ค่อยๆ ปรับขนาด (Smooth Zoom)
        float currentZoom = Mathf.Lerp(vcam.Lens.OrthographicSize, targetSize, zoomSpeed * Time.deltaTime);
        vcam.Lens.OrthographicSize = currentZoom;

        // 4. 🔥 ระบบยกกล้องหนีพื้น (Clamp Camera Y)
        Vector3 newProxyPos = playerTransform.position;

        // คำนวณว่า "จุดกึ่งกลางกล้องต้องอยู่สูงแค่ไหน ขอบล่างถึงจะแตะ mapBottomY พอดี"
        // ยิ่งซูมออกมาก กล้องยิ่งต้องยกตัวสูงขึ้น
        float minAllowedY = mapBottomY + currentZoom;

        // ถ้าตำแหน่งของเคียวมันเตี้ยกว่าที่กล้องควรอยู่ ให้ดันกล้องขึ้น (กล้องไม่มุดดินตามเคียว)
        if (newProxyPos.y < minAllowedY)
        {
            newProxyPos.y = minAllowedY;
        }

        // อัปเดตตำแหน่งให้กล้องตามมา
        proxyTarget.transform.position = newProxyPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerTransform.position, bossDetectRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, scoutRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, combatRadius);
        }

        // 🔥 วาดเส้นแนวนอนสีแดงยาวๆ ในหน้า Scene เพื่อให้คุณกะระยะขอบล่างของกล้องได้ง่ายๆ
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, mapBottomY, 0), new Vector3(50, mapBottomY, 0));
    }
}