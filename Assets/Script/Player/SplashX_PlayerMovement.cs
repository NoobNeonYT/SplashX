using System.Collections;
using UnityEngine;

public class SplashX_PlayerMovement : MonoBehaviour
{
    // 🔥 ระบบจำประเภทปุ่มที่กดล่วงหน้า
    private enum QueuedAttack { None, Normal, Skill }
    private QueuedAttack queuedAttack = QueuedAttack.None;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public AudioClip Walk;

    [Header("Air Combo Settings")]
    public float airHangTimeOnHit = 0.4f;
    private float currentHitHangTimer = 0f;

    [Header("Anime Jump Physics")]
    public float jumpForce = 25f;
    public float upwardGravityMult = 4f;
    public float downwardGravityMult = 5f;

    [Header("Hang Time & Tetris Drop")]
    public float hangTimeVelocityThreshold = 0.5f;
    public float hangTimeGravityMult = 0.1f;
    public float fastFallSpeed = 60f;
    public float doubleTapTime = 0.25f;
    private float lastDownTime = -1f;
    public int fastFallDamage = 30;
    public float fastFallHitRadius = 3f;
    public GameObject heavyLandVFX;

    public int maxJumps = 2;
    private int jumpsLeft;
    private bool isFastFalling = false;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float dashStaminaCost = 20f;
    private bool isDashing;
    private bool canDash = true;

    [Header("Controls")]
    public KeyCode jumpKey = KeyCode.K;
    public KeyCode dashKey = KeyCode.L;
    public KeyCode downKey = KeyCode.S;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundAndPlatformLayer;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Combat - Combo System")]
    public KeyCode attackKey = KeyCode.U;
    public KeyCode skillKey = KeyCode.J;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [Header("Hit 1: Normal (U)")]
    public Vector2 hit1HitBox = new Vector2(2f, 1f);
    public int hit1Damage = 15;
    public float hit1AnimTime = 0.4f;
    public AudioClip hit1SFX;

    [Header("Hit 2: Upward Slash (U -> U)")]
    public Vector2 hit2HitBox = new Vector2(2f, 2.5f);
    public int hit2Damage = 20;
    public float hit2AnimTime = 0.4f;
    public AudioClip hit2SFX;

    [Header("Hit 3: Ground Smash (U -> U -> U)")]
    public float hit3Radius = 3f;
    public int hit3Damage = 20;
    public float hit3AnimTime = 0.6f;
    public AudioClip hit3SFX;
    public GameObject hit3VFX;

    [Header("Skill: Shotgun (J) - ร่างกระดูก")]
    public GameObject shotgunProjectilePrefab;
    public float shotgunKnockbackForce = 5f;
    public float shotgunAnimTime = 0.5f;
    public AudioClip shotgunSFX;
    public float shotgunCooldown = 2f; // 🔥 เพิ่มเวลา Cooldown (วินาที)
    private float nextShotgunTime = 0f; // ตัวนับเวลาหลังฉาก

    [Header("Skill Combo 1: Finisher (U -> U -> U -> J)")] // เปลี่ยนชื่อให้ตรงกับของใหม่
    public Vector2 skill1HitBox = new Vector2(4f, 1f);
    public int skill1Damage = 25;
    public float skill1AnimTime = 0.5f;
    public AudioClip skill1SFX;
    public GameObject skill1VFX;
    public GameObject skill1ProjectilePrefab;

    private bool isAttacking = false;
    private bool isShotgunKnockback = false;

    [Header("Skill: Heavy Smash (I)")]
    public KeyCode smashKey = KeyCode.I;
    public float smashStaminaCost = 100f;
    public int smashDamage = 50;
    public float smashRadius = 4f;
    public float smashJumpForce = 15f;
    public float smashForwardSpeed = 30f;
    public float smashHangTime = 0.2f;
    public float smashDownSpeed = 50f;
    public float smashRecoverTime = 0.5f;
    public AudioClip smashSFX;
    public GameObject smashVFX;
    private bool isHeavySmashing = false;

    [Header("Ultimate: Star Burst (S + I)")]
    public int ultimateChargeRequired = 500;
    public int currentUltimateCharge = 0;
    public GameObject ultimateReadyAura;
    public float ultimateDashSpeed = 50f;
    public float ultimateDashDuration = 0.5f;
    public int starDamage = 7;
    public float starDashOffset = 3f;
    public float starDashSpeed = 40f;
    public TrailRenderer dashTrail;

    public AudioClip ultimateDashSFX;
    public AudioClip starSlashSFX;
    public GameObject ultimateSmashVFX;

    public float ultDropHeight = 2f;       // ระยะกระโดดขึ้นไปบนหัวมอนก่อนทุ่ม
    public float ultDropSpeed = 80f;       // ความเร็วดิ่งพสุธาตอนจบท่า
    public int ultThrowDamage = 20;        // ดาเมจเป้าหมายที่โดนจับทุ่ม
    public int ultAoeDamage = 50;          // ดาเมจคลื่นระเบิดรอบๆ
    public float ultAoeRadius = 4f;        // รัศมีคลื่นระเบิด


    [Header("Components & Effects")]
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip dashSFX;
    public AudioClip landSFX;
    public AudioClip heavyLandSFX;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SplashX_PlayerStats playerStats;
    private float moveInput;
    private bool facingRight = true;
    private float defaultGravity;

    [Header("Hurt Settings")]
    public float hurtDuration = 0.3f;
    private bool isHurt = false;

    [Header("Character Models")]
    public GameObject boneModel;
    public Animator boneAnim;
    public GameObject fbfAttackModel;
    public Animator fbfAnim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerStats = GetComponent<SplashX_PlayerStats>();

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        CheckGrounded();
        UpdateAnimatorParameters();

        if (currentHitHangTimer > 0) currentHitHangTimer -= Time.unscaledDeltaTime;

        if (boneAnim != null && boneModel != null && boneModel.activeSelf)
        {
            boneAnim.SetBool("isAttacking", isAttacking);
        }

        if (isHurt || isDashing) return;

        // 🔥 คิวปุ่มกด: อัลติ (S+I), Smash (I), Normal (U), Shotgun (J)
        if (Input.GetKey(downKey) && Input.GetKeyDown(smashKey))
        {
            // เช็คว่าไม่ได้ตีอยู่ และ "เกจดาเมจครบ 500 แล้ว"
            if (!isAttacking && currentUltimateCharge >= ultimateChargeRequired)
            {
                StartCoroutine(UltimateStarRoutine());
            }
        }
        else if (Input.GetKeyDown(smashKey))
        {
            if (!isAttacking && playerStats != null && playerStats.currentStamina >= smashStaminaCost)
            {
                playerStats.UseStamina(smashStaminaCost);
                StartCoroutine(HeavySmashRoutine());
            }
        }
        else if (Input.GetKeyDown(attackKey))
        {
            if (!isAttacking) StartCoroutine(ComboAttackRoutine());
            else queuedAttack = QueuedAttack.Normal;
        }
        else if (Input.GetKeyDown(skillKey))
        {
            if (!isAttacking)
            {
                if (Time.time >= nextShotgunTime)
                {
                    StartCoroutine(ShotgunRoutine());
                    nextShotgunTime = Time.time + shotgunCooldown;
                }
            }
            else
            {
                queuedAttack = QueuedAttack.Skill;
            }
        }

        if (!isAttacking)
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(jumpKey))
            {
                if (Input.GetKey(downKey) && isGrounded) DropFromPlatform();
                else if (jumpsLeft > 0) ExecuteJump();
            }

            if (!isGrounded && Input.GetKeyDown(downKey))
            {
                if (Time.time - lastDownTime <= doubleTapTime) isFastFalling = true;
                lastDownTime = Time.time;
            }
        }
        else
        {
            moveInput = 0f;
        }

        if (Input.GetKeyDown(dashKey) && canDash && !isAttacking)
        {
            if (playerStats == null || playerStats.UseStamina(dashStaminaCost)) StartCoroutine(DashRoutine());
        }

        Flip();
    }

    void FixedUpdate()
    {
        if (isHurt || isDashing) return;

        if (isAttacking)
        {
            // 🔥 ถ้าเป็นท่า Smash ให้ "return" ออกไปเลย ปล่อยให้ Coroutine จัดการความเร็วเอง!
            if (isHeavySmashing) return;

            if (!isShotgunKnockback)
            {
                if (currentHitHangTimer > 0 && !isGrounded)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }
                else
                {
                    // อันนี้คือตัวการที่ล็อกแกน X ไว้สำหรับท่าโจมตีปกติ
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    rb.gravityScale = defaultGravity;
                }
            }
            else
            {
                rb.gravityScale = defaultGravity;
            }
            return;
        }

        ApplyNormalPhysics();
    }

    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundAndPlatformLayer);

        if (isGrounded)
        {
            jumpsLeft = maxJumps;

            if (!wasGrounded)
            {
                if (!isAttacking && (boneModel != null && boneModel.activeSelf))
                {
                    if (isFastFalling)
                    {
                        if (boneAnim != null) boneAnim.SetTrigger("LandHeavy");
                        SFXManager.Instance.PlaySoundFXClip(heavyLandSFX, transform, 1f);
                        TriggerFastFallImpact();
                    }
                    else
                    {
                        if (boneAnim != null) boneAnim.SetTrigger("LandNormal");
                        SFXManager.Instance.PlaySoundFXClip(landSFX, transform, 1f);      
                    }
                }
                isFastFalling = false;
            }
        }
    }

    void TriggerFastFallImpact()
    {
        if (heavyLandVFX != null && groundCheck != null) Instantiate(heavyLandVFX, groundCheck.position, Quaternion.identity);

        if (groundCheck != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheck.position, fastFallHitRadius, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(fastFallDamage);
            }
        }
    }

    void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsLeft--;
        isFastFalling = false;

        if (boneAnim != null) boneAnim.SetTrigger("Jump");
        SFXManager.Instance.PlaySoundFXClip(jumpSFX, transform, 1f);
    }

    void UpdateAnimatorParameters()
    {
        if (boneAnim == null || (boneModel != null && !boneModel.activeSelf)) return;

        bool isDead = (playerStats != null && playerStats.currentHp <= 0);

        boneAnim.SetBool("isGrounded", isDead ? true : isGrounded);
        boneAnim.SetFloat("Speed", isDead ? 0f : Mathf.Abs(rb.linearVelocity.x));

        // 🔥 กู้คืนบรรทัดนี้กลับมา! (ส่งสวิตช์ Dash ให้ Animator รับรู้)
        boneAnim.SetBool("isDashing", isDashing);

        boneAnim.SetBool("isAttacking", isAttacking);

        // 🔥 อนิเมชันจะนิ่ง (yVelocity = 0) เฉพาะตอนที่ "ตีโดน" (Hang) เท่านั้น
        // ถ้าตีวืด ปล่อยให้ค่า yVelocity ไหลตาม Rigidbody เพื่อให้แอนิเมชัน Fall ทำงาน
        if (isDashing || isHurt || isDead || (isAttacking && currentHitHangTimer > 0))
        {
            boneAnim.SetFloat("yVelocity", 0f);
        }
        else
        {
            boneAnim.SetFloat("yVelocity", isGrounded ? 0f : rb.linearVelocity.y);
        }
    }

    private IEnumerator ShotgunRoutine()
    {
        isAttacking = true;
        isShotgunKnockback = true;
        queuedAttack = QueuedAttack.None;

        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        if (boneAnim != null) boneAnim.SetTrigger("Shotgun");
        SFXManager.Instance.PlaySoundFXClip(shotgunSFX, transform, 1f);

        if (shotgunProjectilePrefab != null && attackPoint != null)
        {
            Instantiate(shotgunProjectilePrefab, attackPoint.position, attackPoint.rotation);
        }

        float recoilDir = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(recoilDir * shotgunKnockbackForce, rb.linearVelocity.y);

        yield return new WaitForSecondsRealtime(shotgunAnimTime); // 🔥 Realtime

        isShotgunKnockback = false;
        ResetAttackState();
    }

    private IEnumerator ComboAttackRoutine()
    {
        isAttacking = true;
        queuedAttack = QueuedAttack.None;

        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        ExecuteHit1();
        yield return new WaitForSecondsRealtime(hit1AnimTime); // 🔥 Realtime

        if (queuedAttack == QueuedAttack.Normal)
        {
            queuedAttack = QueuedAttack.None;
            ExecuteHit2();
            yield return new WaitForSecondsRealtime(hit2AnimTime); // 🔥 Realtime

            if (queuedAttack == QueuedAttack.Normal)
            {
                queuedAttack = QueuedAttack.None;
                ExecuteHit3();
                yield return new WaitForSecondsRealtime(hit3AnimTime); // 🔥 Realtime

                if (queuedAttack == QueuedAttack.Skill)
                {
                    queuedAttack = QueuedAttack.None;
                    ExecuteSkill1();
                    yield return new WaitForSecondsRealtime(skill1AnimTime); // 🔥 Realtime
                }
            }
        }
        ResetAttackState();
    }

    private IEnumerator UltimateStarRoutine()
    {
        isAttacking = true;
        isHeavySmashing = true;
        queuedAttack = QueuedAttack.None;
        currentHitHangTimer = 0f;

        // 🛡️ รีเซ็ตเกจและเปิดอมตะ
        currentUltimateCharge = 0;
        if (ultimateReadyAura != null) ultimateReadyAura.SetActive(false);
        if (playerStats != null) playerStats.SetInvincible(true);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // 🔥 สลับเป็นร่างเฟรมบายเฟรมทันทีที่กดใช้!
        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        // 🌟 รันอนิเมชันพุ่ง (UltSkill)
        if (fbfAnim != null) fbfAnim.SetTrigger("UltSkill");

        // 🔊 เล่นเสียงท่าพุ่ง
        SFXManager.Instance.PlaySoundFXClip(ultimateDashSFX, transform, 1f);

        // 🚀 พุ่งตรงไปข้างหน้าแบบความเร็วแสง
        float dashTimer = 0f;
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * ultimateDashSpeed, 0f);

        Collider2D target = null;

        while (dashTimer < ultimateDashDuration)
        {
            dashTimer += Time.deltaTime;
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, hit1HitBox, 0f, enemyLayers);
            if (hits.Length > 0)
            {
                target = hits[0];
                break; // ชนปุ๊บ หยุดพุ่งปั๊บ
            }
            yield return null;
        }

        rb.linearVelocity = Vector2.zero; // เบรก

        // 💥 ตัดสินผล: โดนเป้า หรือ วืด?
        if (target != null)
        {
            SplashX_Enemy enemyScript = target.GetComponent<SplashX_Enemy>();
            Rigidbody2D enemyRb = target.GetComponent<Rigidbody2D>();
            bool isBoss = (enemyScript != null) && enemyScript.isBoss;

            // รันอนิเมชันแปลงร่างเป็นดาว (PlayerStar)
            if (fbfAnim != null) fbfAnim.SetTrigger("PlayerStar");

            // ลุยคอมโบ 13 ดาบต่อเลย!
            yield return StartCoroutine(StarDashSequence(enemyScript, enemyRb, target.transform, isBoss));
        }
        else
        {
            // ❌ พุ่งวืด: จบท่า
            if (playerStats != null) playerStats.SetInvincible(false);
            isHeavySmashing = false;
            rb.gravityScale = defaultGravity;
            ResetAttackState();
        }
    }

    private IEnumerator StarDashSequence(SplashX_Enemy enemyScript, Rigidbody2D enemyRb, Transform targetTrans, bool isBoss)
    {
        if (dashTrail != null) dashTrail.emitting = true;

        if (!isBoss && enemyRb != null)
        {
            enemyRb.linearVelocity = new Vector2(0f, 15f);
            rb.linearVelocity = new Vector2(0f, 15f);
            yield return new WaitForSeconds(0.3f);

            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        // 🌟 ลุย 13 ดาบ! (ใช่ครับ 13 ทีเป๊ะๆ)
        for (int i = 0; i < 13; i++)
        {
            if (enemyScript == null || enemyScript.currentHealth <= 0 || !enemyScript.gameObject.activeInHierarchy) break;

            bool LtoR = Random.value > 0.5f;
            Vector2 targetPos = targetTrans.position;
            Vector2 startPos = targetPos + (LtoR ? Vector2.left : Vector2.right) * starDashOffset;
            Vector2 endPos = targetPos + (LtoR ? Vector2.right : Vector2.left) * starDashOffset;

            transform.position = startPos;
            if ((LtoR && !facingRight) || (!LtoR && facingRight)) Flip();

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * starDashSpeed;
                transform.position = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            enemyScript.TakeDamage(starDamage);
            SFXManager.Instance.PlaySoundFXClip(starSlashSFX, transform, 1f);

            yield return new WaitForSeconds(0.05f);
        }

        if (dashTrail != null) dashTrail.emitting = false;

        // 🔥 สลับกลับเป็นร่างคนกลางอากาศทันทีที่ครบ 13 ที!
        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null)
        {
            boneModel.SetActive(true);
            if (boneAnim != null)
            {
                boneAnim.SetBool("isGrounded", false);
                boneAnim.SetFloat("yVelocity", -10f);
            }
        }

        bool targetStillAlive = enemyScript != null && enemyScript.currentHealth > 0 && enemyScript.gameObject.activeInHierarchy;

        // 🌟 ช็อตลงพื้น (Finisher)
        if (!isBoss && targetStillAlive)
        {
            // เป้าหมายยังรอด -> กระโดดไปบนหัวแล้วทุ่มลงพื้น!
            transform.position = targetTrans.position + Vector3.up * ultDropHeight; // ใช้ ultDropHeight

            rb.linearVelocity = new Vector2(0f, -ultDropSpeed); // ใช้ ultDropSpeed
            if (enemyRb != null) enemyRb.linearVelocity = new Vector2(0f, -ultDropSpeed);

            while (!isGrounded) yield return null;

            rb.linearVelocity = Vector2.zero;
            enemyScript.TakeDamage(ultThrowDamage); // ดาเมจพิเศษโดนจับทุ่ม
            if (enemyRb != null) enemyRb.gravityScale = 1f;
        }
        else
        {
            // เป้าหมายตาย หรือเป็นบอส -> ร่วงลงมากระแทกพื้นเปล่าๆ 
            if (enemyRb != null && !isBoss) enemyRb.gravityScale = 1f;

            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(0f, -ultDropSpeed); // ใช้ ultDropSpeed

            while (!isGrounded) yield return null;

            rb.linearVelocity = Vector2.zero;
        }

        // 💥 ทันทีที่เท้าแตะพื้น 
        if (boneAnim != null) boneAnim.SetTrigger("LandHeavy");
        SFXManager.Instance.PlaySoundFXClip(heavyLandSFX, transform, 1f);
        SFXManager.Instance.PlaySoundFXClip(smashSFX, transform, 1f);
        if (ultimateSmashVFX != null && groundCheck != null) Instantiate(ultimateSmashVFX, groundCheck.position, Quaternion.identity);

        // 💥 ระเบิดดาเมจ AOE รอบตัว! (ใช้ ultAoeRadius และ ultAoeDamage)
        if (groundCheck != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheck.position, ultAoeRadius, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy hitEnemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (hitEnemyScript != null)
                {
                    hitEnemyScript.TakeDamage(ultAoeDamage);
                }
            }
        }

        // 🛡️ จบท่า
        yield return new WaitForSeconds(0.5f);
        if (playerStats != null) playerStats.SetInvincible(false);
        isHeavySmashing = false;
        ResetAttackState();
    }

    private IEnumerator HeavySmashRoutine()
    {
        isAttacking = true;
        isHeavySmashing = true;
        queuedAttack = QueuedAttack.None;
        currentHitHangTimer = 0f;

        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        if (fbfAnim != null) fbfAnim.SetTrigger("HeavySmash");

        float dir = facingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(dir * smashForwardSpeed, isGrounded ? smashJumpForce : smashJumpForce * 0.5f);

        yield return new WaitForSecondsRealtime(0.35f); // 🔥 Realtime

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        yield return new WaitForSecondsRealtime(smashHangTime); // 🔥 Realtime

        rb.gravityScale = defaultGravity;
        rb.linearVelocity = new Vector2(dir * smashForwardSpeed, -smashDownSpeed);

        while (!isGrounded)
        {
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        SFXManager.Instance.PlaySoundFXClip(smashSFX, transform, 1f);
        ExecuteSmashHit();

        yield return new WaitForSecondsRealtime(smashRecoverTime); // 🔥 Realtime

        isHeavySmashing = false;
        ResetAttackState();
    }

    void ExecuteSmashHit()
    {
        if (smashVFX != null && groundCheck != null)
        {
            Instantiate(smashVFX, groundCheck.position, Quaternion.identity);
        }

        if (groundCheck != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(groundCheck.position, smashRadius, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(smashDamage);
                    AddUltimateCharge(smashDamage); // 🔥 เติมเกจตรงนี้!
                }
            }
        }
    }
    void ExecuteHit1()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack1");
        SFXManager.Instance.PlaySoundFXClip(hit1SFX, transform, 1f);
        ProcessBoxHit(hit1HitBox, hit1Damage);
    }

    void ExecuteHit2()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack2");
        SFXManager.Instance.PlaySoundFXClip(hit2SFX, transform, 1f);
        ProcessBoxHit(hit2HitBox, hit2Damage);
    }

    void ExecuteHit3()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack3");
        SFXManager.Instance.PlaySoundFXClip(hit3SFX, transform, 1f);
        if (hit3VFX != null && attackPoint != null) Instantiate(hit3VFX, attackPoint.position, Quaternion.identity);

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, hit3Radius, enemyLayers);
            bool hitSuccess = false;
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(hit3Damage);
                    AddUltimateCharge(hit3Damage); // 🔥 เติมเกจตรงนี้!
                    hitSuccess = true;
                }
            }
            if (hitSuccess && !isGrounded) currentHitHangTimer = airHangTimeOnHit;
        }
    }

    void ExecuteSkill1()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Skill1");
        SFXManager.Instance.PlaySoundFXClip(skill1SFX, transform, 1f);

        // 1. สร้างองศาหักหัวลง 20 องศา
        Quaternion downwardRotation = attackPoint.rotation * Quaternion.Euler(0f, 0f, -20f);

        // 🔥 2. ยิง Prefab กระสุนคลื่นดาบออกไปตามองศาที่คำนวณไว้
        if (skill1ProjectilePrefab != null && attackPoint != null)
        {
            Instantiate(skill1ProjectilePrefab, attackPoint.position, downwardRotation);
        }

        // (ถ้ามี Effect ฟันลมตอนง้างดาบ ก็ให้มันเล่นตรงตำแหน่งตัวละครปกติ)
        if (skill1VFX != null && attackPoint != null)
        {
            Instantiate(skill1VFX, attackPoint.position, Quaternion.identity);
        }
    }

    void ProcessBoxHit(Vector2 hitBoxSize, int damage)
    {
        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, hitBoxSize, 0f, enemyLayers);

        bool hitSuccess = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                AddUltimateCharge(damage); // 🔥 เติมเกจตรงนี้!
                hitSuccess = true;
            }
        }

        if (hitSuccess && !isGrounded)
        {
            currentHitHangTimer = airHangTimeOnHit;
        }
    }

    void ResetAttackState()
    {
        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null)
        {
            boneModel.SetActive(true);
            if (boneAnim != null)
            {
                boneAnim.SetBool("isGrounded", isGrounded);
                boneAnim.SetFloat("yVelocity", 0f);
                boneAnim.SetFloat("Speed", 0f);
                boneAnim.ResetTrigger("Dash");
                boneAnim.ResetTrigger("Jump");
                if (isGrounded) boneAnim.Play("Player_idle", -1, 0f);
            }
        }
        isAttacking = false;
        queuedAttack = QueuedAttack.None;
        isShotgunKnockback = false;
        isHeavySmashing = false;
    }

    private void Flip()
    {
        if (moveInput > 0 && !facingRight)
        {
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // 🔥 กู้คืนฟังก์ชันที่โดนตัดทิ้งกลับมาให้หมด!
    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        isFastFalling = false;

        if (boneAnim != null)
        {
            boneAnim.SetFloat("yVelocity", 0f);
            boneAnim.SetTrigger("Dash");
        }
        SFXManager.Instance.PlaySoundFXClip(dashSFX, transform, 1f);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2((facingRight ? 1 : -1) * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void DropFromPlatform()
    {
        if (groundCheck == null) return;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundAndPlatformLayer);
        foreach (Collider2D col in colliders)
        {
            if (col.GetComponent<PlatformEffector2D>() != null)
            {
                StartCoroutine(DisableCollisionTemporary(col));
                break;
            }
        }
    }

    private IEnumerator DisableCollisionTemporary(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    public void TriggerHurt()
    {
        StopAllCoroutines();
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHeavySmashing = false;
        isAttacking = false;
        isDashing = false;
        canDash = true;
        isFastFalling = false;
        queuedAttack = QueuedAttack.None;
        isShotgunKnockback = false;

        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (playerStats != null && playerStats.currentHp <= 0)
        {
            isHurt = false;

            // 🔥 ย้ำอีกรอบก่อนที่สคริปต์จะโดนปิด! บังคับยัดค่า 0 ใส่หัว Animator ทันที
            if (boneAnim != null)
            {
                boneAnim.SetFloat("yVelocity", 0f);
                boneAnim.SetFloat("Speed", 0f);
            }

            yield break;
        }

        isHurt = true;

        if (boneAnim != null)
        {
            boneAnim.ResetTrigger("Attack");
            boneAnim.ResetTrigger("Dash");
            boneAnim.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(hurtDuration);

        isHurt = false;

        if (isGrounded && boneAnim != null)
        {
            boneAnim.Play("Player_idle", -1, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, fastFallHitRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(groundCheck.position, smashRadius);

            // 🔥 วงกลมระเบิดของท่า Ultimate (สีส้ม)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(groundCheck.position, ultAoeRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(attackPoint.position, hit1HitBox);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(attackPoint.position, hit2HitBox);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, hit3Radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(attackPoint.position, skill1HitBox);
        }
    }

    public void ResetAllStatesForRevive()
    {
        // 1. ล้างตัวแปรค้างคาทั้งหมด
        isHeavySmashing = false;
        isAttacking = false;
        isDashing = false;
        canDash = true;
        isFastFalling = false;
        isShotgunKnockback = false;
        isHurt = false;
        queuedAttack = QueuedAttack.None;

        // 🔥 2. เคลียร์สมอง "ร่างฟันดาบ" (ทำก่อนที่จะปิดตัวมัน และทำเฉพาะตอนที่มันยังเปิดอยู่เท่านั้น!)
        if (fbfAnim != null && fbfAttackModel != null && fbfAttackModel.activeSelf)
        {
            fbfAnim.ResetTrigger("Attack1");
            fbfAnim.ResetTrigger("Attack2");
            fbfAnim.ResetTrigger("Attack3");
            fbfAnim.ResetTrigger("Skill1");
        }

        // 3. บังคับสลับกลับมาร่างปกติ
        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        // 🔥 4. เคลียร์สมอง "ร่างกระดูก" (ตอนนี้เปิดอยู่แน่นอน เพราะเพิ่งสั่ง SetActive(true) ไป)
        if (boneAnim != null && boneModel != null && boneModel.activeSelf)
        {
            boneAnim.ResetTrigger("Death");
            boneAnim.ResetTrigger("Dash");
            boneAnim.ResetTrigger("Hurt");
            boneAnim.ResetTrigger("Shotgun");
            boneAnim.ResetTrigger("Jump");
            boneAnim.ResetTrigger("LandNormal");
            boneAnim.ResetTrigger("LandHeavy");

            // บังคับยืนท่าปกติทันที
            boneAnim.Play("Player_idle", -1, 0f);
        }

        // คืนค่าแรงโน้มถ่วงกลับเป็นปกติ
        if (rb != null) rb.gravityScale = defaultGravity;
    }

    // 🔥 ฟังก์ชันฟิสิกส์การเดิน/กระโดด ปกติ (ยกยอดมาจาก FixedUpdate เดิม)
    private void ApplyNormalPhysics()
    {
        if (!isGrounded)
        {
            if (isFastFalling)
            {
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, -fastFallSpeed);
                rb.gravityScale = 0f;
            }
            else if (Mathf.Abs(rb.linearVelocity.y) < hangTimeVelocityThreshold)
            {
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
                rb.gravityScale = defaultGravity * hangTimeGravityMult;
            }
            else if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
                rb.gravityScale = defaultGravity * downwardGravityMult;
            }
            else if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
                rb.gravityScale = defaultGravity * upwardGravityMult;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            rb.gravityScale = defaultGravity;
        }
    }

    public void AddUltimateCharge(int damageDealt)
    {
        if (currentUltimateCharge >= ultimateChargeRequired) return;

        currentUltimateCharge += damageDealt;

        if (currentUltimateCharge >= ultimateChargeRequired)
        {
            currentUltimateCharge = ultimateChargeRequired;
            if (ultimateReadyAura != null) ultimateReadyAura.SetActive(true);
        }
    }
    // 🔥 ฟังก์ชันพิเศษสำหรับรับบัพเร่งเวลาจากสกิลฮะ ฮ่า ช้าชะมัด
    public void ApplyTimeScaleMultiplier(float multiplier)
    {
        moveSpeed *= multiplier;
        jumpForce *= multiplier;
        dashSpeed *= multiplier;
        fastFallSpeed *= multiplier;
        shotgunKnockbackForce *= multiplier;
        smashForwardSpeed *= multiplier;
        smashDownSpeed *= multiplier;
        ultimateDashSpeed *= multiplier;
        starDashSpeed *= multiplier;

        // ⚠️ แรงโน้มถ่วงต้องคูณยกกำลังสอง ตัวละครถึงจะตกถึงพื้นในเวลาเท่าเดิมเป๊ะๆ
        float gravityMult = multiplier * multiplier;
        defaultGravity *= gravityMult;
        if (rb != null) rb.gravityScale = defaultGravity;
    }
}