using UnityEngine;
using Unity.Cinemachine;

public class SplashX_CameraControl : MonoBehaviour
{
    public CinemachineCamera vcam;

    [Header("Zoom Levels")]
    public float explorationSize = 7.5f; 
    public float scoutSize = 9.0f;     
    public float combatSize = 5.5f;   
    public float zoomSpeed = 3f;

    [Header("Detection Zones")]
    public float scoutRadius = 12f;  
    public float combatRadius = 6f; 
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

        Collider2D[] combatEnemies = Physics2D.OverlapCircleAll(playerTransform.position, combatRadius, enemyLayer);
   
        Collider2D[] scoutEnemies = Physics2D.OverlapCircleAll(playerTransform.position, scoutRadius, enemyLayer);

        if (combatEnemies.Length > 0)
        {
            targetSize = combatSize; 
        }
        else if (scoutEnemies.Length > 0)
        {
            targetSize = scoutSize; 
        }
        else
        {
            targetSize = explorationSize; 
        }

        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow; 
            Gizmos.DrawWireSphere(playerTransform.position, scoutRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, combatRadius);
        }
    }
}