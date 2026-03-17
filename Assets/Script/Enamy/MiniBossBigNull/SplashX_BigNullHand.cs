using UnityEngine;
using System.Collections;

public class SplashX_BigNullHand : MonoBehaviour
{
    public enum HandState { Hover, SmashWindUp, SmashDown, Stuck, PullUp, ShootHandDown, ShootHandPullUp, Dead }

    [Header("Current State")]
    public HandState currentState = HandState.Hover;

    [Header("Boss Stats")]
    public int maxHp = 100;
    public int currentHp;

    [Header("Visual References (ชิ้นส่วนบอส)")]
    public Transform armVisual;  // ภาพแขนท่อนบน
    public Transform handVisual; // ภาพมือ (แยกชิ้นเพื่อยิงลงมาได้)
    private Vector3 handStartLocalPos; // จุดเชื่อมต่อมือกับแขน

    [Header("Hover Settings (ตามผู้เล่น)")]
    public float hoverHeight = 6f;
    public float followSpeed = 5f;

    [Header("Smash Attack (ทุบทั้งแขน)")]
    public float smashSpeed = 25f;
    public float pullUpSpeed = 8f;
    public float stuckDuration = 3f;
    public int smashDamage = 30;

    [Header("Shoot Hand Attack (ยิงมือระเบิด)")]
    public float shootSpeed = 30f;
    public float handPullUpSpeed = 15f;
    public float explosionRadius = 3f;
    public int explosionDamage = 40;
    public GameObject explosionVFX; // เอฟเฟกต์ระเบิดตอนมือกระแทกพื้น

    [Header("Attack Flow")]
    public float attackCooldown = 2.5f;
    private float attackTimer;

    [Header("Death Sequence (ฉากตาย)")]
    public float deathShakeIntensity = 0.5f;
    public float deathDuration = 3f;
    public GameObject smallExplosionVFX; // ระเบิดเล็กๆ ตามแขนตอนพัง
    public GameObject smokeVFX;          // ควันตอนจางหาย

    [Header("References & Ground Check")]
    public LayerMask groundLayer;
    public LayerMask playerLayer; // เอาไว้เช็คตอนระเบิดโดนผู้เล่น
    public float groundCheckDistance = 1f;

    private Transform player;
    private Vector2 targetPos;
    private bool hasDamagedThisSmash = false;

    void Start()
    {
        currentHp = maxHp;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // จำตำแหน่งเดิมของมือตอนที่ยังติดกับแขนไว้
        if (handVisual != null) handStartLocalPos = handVisual.localPosition;

        attackTimer = attackCooldown;
    }

    void Update()
    {
        if (player == null || currentState == HandState.Dead) return;

        switch (currentState)
        {
            case HandState.Hover:
                HoverLogic();
                break;
            case HandState.SmashDown:
                SmashDownLogic();
                break;
            case HandState.Stuck:
                // รอใน Coroutine
                break;
            case HandState.PullUp:
                PullUpLogic();
                break;
            case HandState.ShootHandDown:
                ShootHandDownLogic();
                break;
            case HandState.ShootHandPullUp:
                ShootHandPullUpLogic();
                break;
        }
    }

    // --- 1. Hover Logic ---
    void HoverLogic()
    {
        targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            int randomAttack = Random.Range(0, 2);
            if (randomAttack == 0) StartCoroutine(SmashRoutine());
            else StartCoroutine(ShootHandRoutine());
        }
    }

    // --- 2. Smash Attack (ทุบทั้งแขน) ---
    IEnumerator SmashRoutine()
    {
        currentState = HandState.SmashWindUp;
        hasDamagedThisSmash = false;
        yield return new WaitForSeconds(0.5f);
        currentState = HandState.SmashDown;
    }

    void SmashDownLogic()
    {
        // ขยับลงมา "ทั้งตัว" (แขน + มือ)
        transform.Translate(Vector3.down * smashSpeed * Time.deltaTime);

        RaycastHit2D groundHit = Physics2D.Raycast(handVisual.position, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider != null)
        {
            StartCoroutine(StuckRoutine());
        }
    }

    IEnumerator StuckRoutine()
    {
        currentState = HandState.Stuck;
        yield return new WaitForSeconds(stuckDuration);
        currentState = HandState.PullUp;
    }

    void PullUpLogic()
    {
        targetPos = new Vector2(transform.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, pullUpSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            attackTimer = attackCooldown;
            currentState = HandState.Hover;
        }
    }

    // --- 3. Shoot Hand Attack (ยิงมือระเบิด) ---
    IEnumerator ShootHandRoutine()
    {
        currentState = HandState.SmashWindUp; // ชาร์จพลัง
        yield return new WaitForSeconds(0.5f);
        currentState = HandState.ShootHandDown;
    }

    void ShootHandDownLogic()
    {
        // 🔥 ขยับลงมา "แค่มือ" (แขนอยู่ที่เดิม)
        handVisual.Translate(Vector3.down * shootSpeed * Time.deltaTime, Space.World);

        RaycastHit2D groundHit = Physics2D.Raycast(handVisual.position, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider != null)
        {
            TriggerHandExplosion();
            currentState = HandState.ShootHandPullUp;
        }
    }

    void TriggerHandExplosion()
    {
        // 1. เสกเอฟเฟกต์ระเบิดตรงตำแหน่งมือ
        if (explosionVFX != null) Instantiate(explosionVFX, handVisual.position, Quaternion.identity);

        // 2. เช็คดาเมจรอบๆ มือเป็นวงกลม (ใครหลบไม่พ้นโดนตู้ม!)
        Collider2D[] hits = Physics2D.OverlapCircleAll(handVisual.position, explosionRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            SplashX_PlayerStats stats = hit.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(explosionDamage);
        }

        // (ใส่เสียงระเบิดตรงนี้ได้)
    }

    void ShootHandPullUpLogic()
    {
        // 🔥 ดึงมือกลับไปหาแขน (เลื่อนกลับไปที่ Local Position เดิม)
        handVisual.localPosition = Vector3.MoveTowards(handVisual.localPosition, handStartLocalPos, handPullUpSpeed * Time.deltaTime);

        if (Vector2.Distance(handVisual.localPosition, handStartLocalPos) < 0.01f)
        {
            handVisual.localPosition = handStartLocalPos; // ล็อกให้เป๊ะ
            attackTimer = attackCooldown;
            currentState = HandState.Hover;
        }
    }

    // --- Collision Damage (สำหรับท่าทุบโดนตัวตรงๆ) ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == HandState.SmashDown && collision.CompareTag("Player") && !hasDamagedThisSmash)
        {
            hasDamagedThisSmash = true;
            SplashX_PlayerStats stats = collision.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(smashDamage);
        }
    }

    // --- ระบบรับดาเมจ และ ฉากตาย ---
    public void TakeDamage(int damage)
    {
        if (currentState == HandState.Dead) return;

        currentHp -= damage;
        if (currentHp <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        currentState = HandState.Dead; // หยุดการทำงานทุกอย่าง

        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < deathDuration)
        {
            // 1. สั่นทั้งแขน (Shake)
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * deathShakeIntensity;

            // 2. สุ่มเสกเอฟเฟกต์ระเบิดเล็กๆ ตามแขนและมือ
            if (Time.frameCount % 10 == 0 && smallExplosionVFX != null) // ไม่ให้ระเบิดถี่เกินไป
            {
                Vector3 randomPoint = transform.position + (Vector3)Random.insideUnitCircle * 3f; // ปรับรัศมีระเบิดได้
                Instantiate(smallExplosionVFX, randomPoint, Quaternion.identity);
            }

            elapsed += Time.deltaTime;
            yield return null; // รอเฟรมถัดไป
        }

        // 3. ปิดภาพแขนและมือทิ้ง
        if (armVisual != null) armVisual.gameObject.SetActive(false);
        if (handVisual != null) handVisual.gameObject.SetActive(false);

        // 4. เสกควันก้อนใหญ่
        if (smokeVFX != null) Instantiate(smokeVFX, transform.position, Quaternion.identity);

        // 5. ทำลายทิ้งหลังควันจาง (สมมติให้เวลาควันโชว์ 2 วินาที)
        Destroy(gameObject, 2f);
    }

    // เอาไว้วาดวงกลมรัศมีระเบิดดูในฉาก Editor
    private void OnDrawGizmosSelected()
    {
        if (handVisual != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(handVisual.position, explosionRadius);
        }
    }
}