using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_GroundLaserAI : MonoBehaviour
{
    [Header("Activation & Aggro")]
    public float activationRange = 20f;
    public float deactivationRange = 25f;
    private bool isAwake = false;

    [Header("Movement & Positioning")]
    public float moveSpeed = 3f;
    public float preferredDistance = 10f; // เลเซอร์ควรยืนยิงไกลหน่อย
    public float retreatDistance = 5f;

    [Header("Environment Checks")]
    public float wallCheckDistance = 0.6f;
    public float edgeCheckDistance = 1.5f;
    public float raycastOffsetY = 0.5f;
    public float edgeCheckOffsetX = 0.8f;
    public LayerMask groundLayer;

    [Header("Laser Attack Settings")]
    public Transform firePoint;            // จุดปล่อยเลเซอร์
    public GameObject chargeVFX;           // เอฟเฟกต์แสงเตือนก่อนยิง (ลาก Object ลูกมาใส่)
    public LineRenderer laserLine;         // ตัววาดเส้นเลเซอร์
    public GameObject hitVFX;              // สะเก็ดไฟตอนเลเซอร์กระแทกเป้าหมาย

    public int laserDamage = 20;
    public float laserDistance = 20f;      // ความยาวสูงสุดของเลเซอร์
    public float chargeTime = 1f;          // เวลาชาร์จแสงก่อนยิง
    public float fireCooldown = 3f;
    public LayerMask hitLayers;            // เลเยอร์ที่เลเซอร์ชนได้ (ตั้งเป็น Player + Ground)
    private float fireTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private SplashX_Enemy enemyStats;
    private Transform player;

    private bool isAttacking = false;
    private bool facingRight = true;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyStats = GetComponent<SplashX_Enemy>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        fireTimer = fireCooldown;

        // ปิดเส้นเลเซอร์และแสงชาร์จไว้ก่อนตอนเริ่มเกม
        if (laserLine != null) laserLine.enabled = false;
        if (chargeVFX != null) chargeVFX.SetActive(false);
    }

    void Update()
    {
        if (enemyStats.currentHealth <= 0 && !isDead)
        {
            Die();
            return;
        }

        if (isDead || player == null) return;

        if (enemyStats.isStunned)
        {
            anim.Play("Hurt");
            StopLaserIfStunned(); // ถ้าโดนตีตอนชาร์จ ให้ยกเลิกการยิง
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAwake)
        {
            if (distanceToPlayer <= activationRange) isAwake = true;
            else return;
        }
        else if (distanceToPlayer > deactivationRange)
        {
            isAwake = false;
            StopMoving();
            return;
        }

        fireTimer -= Time.deltaTime;

        if (!isAttacking)
        {
            FlipTowardsPlayer();

            if (distanceToPlayer > preferredDistance)
            {
                Move(1f);
            }
            else if (distanceToPlayer < retreatDistance)
            {
                Move(-1f);
            }
            else
            {
                StopMoving();
                bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, edgeCheckDistance, groundLayer);
                if (fireTimer <= 0 && isGrounded)
                {
                    StartCoroutine(LaserRoutine());
                }
            }
        }
    }

    void Move(float directionMultiplier)
    {
        float dirX = (facingRight ? 1f : -1f) * directionMultiplier;
        Vector2 rayStartPos = new Vector2(transform.position.x, transform.position.y + raycastOffsetY);
        bool hitWall = Physics2D.Raycast(rayStartPos, Vector2.right * dirX, wallCheckDistance, groundLayer);
        Vector2 edgeCheckPos = new Vector2(transform.position.x + (dirX * edgeCheckOffsetX), transform.position.y + raycastOffsetY);
        bool hasGroundAhead = Physics2D.Raycast(edgeCheckPos, Vector2.down, edgeCheckDistance, groundLayer);

        if (hitWall || !hasGroundAhead) StopMoving();
        else
        {
            rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
            anim.SetBool("isWalking", true);
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isWalking", false);
    }

    IEnumerator LaserRoutine()
    {
        isAttacking = true;
        StopMoving();

        anim.SetTrigger("Laser");

        if (chargeVFX != null) chargeVFX.SetActive(true);

        yield return new WaitForSeconds(chargeTime);

        if (chargeVFX != null) chargeVFX.SetActive(false);

        if (!enemyStats.isStunned && laserLine != null && firePoint != null)
        {
            // 🌟 [โค้ดพระเจ้า บังคับข่มขืน Inspector แก้ปัญหาภาพหาย 100%]
            laserLine.gameObject.SetActive(true); // ปลุก Object
            laserLine.enabled = true;             // เปิด Component
            laserLine.useWorldSpace = true;       // บังคับอิงพิกัดโลก
            laserLine.positionCount = 2;          // บังคับมี 2 จุดเสมอ
            laserLine.startWidth = 0.5f;          // บังคับความหนา
            laserLine.endWidth = 0.5f;
            laserLine.sortingOrder = 9999;        // บังคับอยู่หน้าสุดทะลุมิติ

            // 🔥 บังคับเสก Material และสีแดงสดทึบแสง (กันปัญหาลืมดึง Alpha หรือ Material บั๊ก)
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = Color.red;

            // บังคับแกน Z เป็น 0 
            Vector3 startPos = new Vector3(firePoint.position.x, firePoint.position.y, 0f);
            laserLine.SetPosition(0, startPos);

            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, laserDistance, hitLayers);

            if (hit.collider != null)
            {
                Vector3 endPos = new Vector3(hit.point.x, hit.point.y, 0f);
                laserLine.SetPosition(1, endPos);

                if (hit.collider.CompareTag("Player"))
                {
                    SplashX_PlayerStats pStats = hit.collider.GetComponent<SplashX_PlayerStats>();
                    if (pStats != null) pStats.TakeDamage(laserDamage);
                }

                if (hitVFX != null) Instantiate(hitVFX, hit.point, Quaternion.identity);
            }
            else
            {
                Vector3 endPos = firePoint.position + firePoint.right * laserDistance;
                endPos.z = 0f;
                laserLine.SetPosition(1, endPos);
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (laserLine != null) laserLine.enabled = false;

        yield return new WaitForSeconds(fireCooldown);
        isAttacking = false;
    }

    void StopLaserIfStunned()
    {
        if (isAttacking)
        {
            StopAllCoroutines(); // หยุดการชาร์จ/ยิงทันที
            if (chargeVFX != null) chargeVFX.SetActive(false);
            if (laserLine != null) laserLine.enabled = false;
            isAttacking = false;
            fireTimer = fireCooldown; // รีเซ็ตคูลดาวน์ใหม่
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        StopLaserIfStunned();
        anim.SetTrigger("Death");
    }

    void FlipTowardsPlayer()
    {
        if (player.position.x > transform.position.x && !facingRight)
        {
            facingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            facingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }
}