using UnityEngine;
using Unity.Cinemachine;

public class SplashX_CameraControl : MonoBehaviour
{
    public CinemachineCamera vcam;

    [Header("Zoom Levels")]
    public float explorationSize = 7.5f; // เดินปกติ
    public float scoutSize = 9.0f;       // ซูมออกเล็กน้อยเมื่อใกล้ศัตรู (ให้เห็นก่อน)
    public float combatSize = 5.5f;      // ซูมเข้าตอนปะทะ
    public float zoomSpeed = 3f;

    [Header("Detection Zones")]
    public float scoutRadius = 12f;      // รัศมีมองไกล
    public float combatRadius = 6f;       // รัศมีระยะประชิด
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

        // 1. เช็คระยะประชิด (Combat)
        Collider2D[] combatEnemies = Physics2D.OverlapCircleAll(playerTransform.position, combatRadius, enemyLayer);
        // 2. เช็คระยะไกล (Scout)
        Collider2D[] scoutEnemies = Physics2D.OverlapCircleAll(playerTransform.position, scoutRadius, enemyLayer);

        if (combatEnemies.Length > 0)
        {
            targetSize = combatSize; // ซูมเข้าโหมดโหด
        }
        else if (scoutEnemies.Length > 0)
        {
            targetSize = scoutSize; // ซูมออกให้มองเห็นศัตรูก่อน
        }
        else
        {
            targetSize = explorationSize; // กลับสู่สภาวะปกติ
        }

        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow; // Scout Zone
            Gizmos.DrawWireSphere(playerTransform.position, scoutRadius);
            Gizmos.color = Color.red;    // Combat Zone
            Gizmos.DrawWireSphere(playerTransform.position, combatRadius);
        }
    }
}