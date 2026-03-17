using System.Collections;
using UnityEngine;

public class SplashX_PlayerMovement : MonoBehaviour
{
    // 🔥 ระบบจำประเภทปุ่มที่กดล่วงหน้า
    private enum QueuedAttack { None, Normal, Skill }
    private QueuedAttack queuedAttack = QueuedAttack.None;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;

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

    [Header("Skill Combo 1: Mid-Range (U -> J)")]
    public Vector2 skill1HitBox = new Vector2(4f, 1f);
    public int skill1Damage = 25;
    public float skill1AnimTime = 0.5f;
    public AudioClip skill1SFX;
    public GameObject skill1VFX;

    private bool isAttacking = false;
    private bool isShotgunKnockback = false;
    private int comboStep = 0;

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

        if (isHurt || isDashing) return;

        if (Input.GetKeyDown(attackKey))
        {
            if (!isAttacking) StartCoroutine(ComboAttackRoutine());
            else queuedAttack = QueuedAttack.Normal;
        }
        else if (Input.GetKeyDown(skillKey))
        {
            if (!isAttacking) StartCoroutine(ShotgunRoutine());
            else queuedAttack = QueuedAttack.Skill;
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
            if (!isShotgunKnockback)
            {
                rb.linearVelocity = new Vector2(0f, isGrounded ? rb.linearVelocity.y : 0f);
            }
            rb.gravityScale = isGrounded ? defaultGravity : 0f;
            return;
        }

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
                        PlaySFX(heavyLandSFX);
                        TriggerFastFallImpact();
                    }
                    else
                    {
                        if (boneAnim != null) boneAnim.SetTrigger("LandNormal");
                        PlaySFX(landSFX);
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
        PlaySFX(jumpSFX);
    }

    void UpdateAnimatorParameters()
    {
        if (boneAnim == null || (boneModel != null && !boneModel.activeSelf)) return;

        boneAnim.SetBool("isGrounded", isGrounded);
        boneAnim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        boneAnim.SetBool("isDashing", isDashing);

        if (isDashing || isAttacking || isHurt)
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
        PlaySFX(shotgunSFX);

        if (shotgunProjectilePrefab != null && attackPoint != null)
        {
            Instantiate(shotgunProjectilePrefab, attackPoint.position, attackPoint.rotation);
        }

        float recoilDir = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(recoilDir * shotgunKnockbackForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(shotgunAnimTime);

        isShotgunKnockback = false;
        ResetAttackState();
    }

    private IEnumerator ComboAttackRoutine()
    {
        isAttacking = true;
        comboStep = 1;
        queuedAttack = QueuedAttack.None;

        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        ExecuteHit1();
        yield return new WaitForSeconds(hit1AnimTime);

        if (queuedAttack == QueuedAttack.Normal)
        {
            comboStep = 2;
            queuedAttack = QueuedAttack.None;

            ExecuteHit2();
            yield return new WaitForSeconds(hit2AnimTime);

            if (queuedAttack == QueuedAttack.Normal)
            {
                comboStep = 3;
                queuedAttack = QueuedAttack.None;

                ExecuteHit3();
                yield return new WaitForSeconds(hit3AnimTime);
            }
        }
        else if (queuedAttack == QueuedAttack.Skill)
        {
            comboStep = 2;
            queuedAttack = QueuedAttack.None;

            ExecuteSkill1();
            yield return new WaitForSeconds(skill1AnimTime);
        }

        ResetAttackState();
    }

    void ExecuteHit1()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack1");
        PlaySFX(hit1SFX);
        ProcessBoxHit(hit1HitBox, hit1Damage);
    }

    void ExecuteHit2()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack2");
        PlaySFX(hit2SFX);
        ProcessBoxHit(hit2HitBox, hit2Damage);
    }

    void ExecuteHit3()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack3");
        PlaySFX(hit3SFX);
        if (hit3VFX != null && attackPoint != null) Instantiate(hit3VFX, attackPoint.position, Quaternion.identity);

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, hit3Radius, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(hit3Damage);
            }
        }
    }

    void ExecuteSkill1()
    {
        if (fbfAnim != null) fbfAnim.SetTrigger("Skill1");
        PlaySFX(skill1SFX);
        if (skill1VFX != null && attackPoint != null) Instantiate(skill1VFX, attackPoint.position, Quaternion.identity);
        ProcessBoxHit(skill1HitBox, skill1Damage);
    }

    void ProcessBoxHit(Vector2 hitBoxSize, int damage)
    {
        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, hitBoxSize, 0f, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
            if (enemyScript != null) enemyScript.TakeDamage(damage);
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
        comboStep = 0;
        queuedAttack = QueuedAttack.None;
        isShotgunKnockback = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
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
        PlaySFX(dashSFX);

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
        isHurt = true;

        isAttacking = false;
        isDashing = false;
        canDash = true;
        isFastFalling = false;
        comboStep = 0;
        queuedAttack = QueuedAttack.None;

        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

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

    // 🔥 ฟังก์ชันนี้เอาไว้ล้างสถานะที่ค้างอยู่ตอนตื่นจากการตาย
    public void ResetAllStatesForRevive()
    {
        // 1. ล้างตัวแปรค้างคาทั้งหมด
        isAttacking = false;
        isDashing = false;
        canDash = true;
        isFastFalling = false;
        isShotgunKnockback = false;
        isHurt = false;
        comboStep = 0;
        queuedAttack = QueuedAttack.None;

        // 2. บังคับสลับกลับมาร่างปกติ
        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        // 3. รีเซ็ตคำสั่งแอนิเมชันของ "ร่างกระดูก" (อันนี้ไม่มี Attack)
        if (boneAnim != null)
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

        // 🔥 4. รีเซ็ตคำสั่งแอนิเมชันของ "ร่างฟันดาบ" (แก้ Error ตรงนี้!)
        if (fbfAnim != null)
        {
            fbfAnim.ResetTrigger("Attack1");
            fbfAnim.ResetTrigger("Attack2");
            fbfAnim.ResetTrigger("Attack3");
            fbfAnim.ResetTrigger("Skill1"); // เผื่อตายตอนกดท่า J
        }

        // คืนค่าแรงโน้มถ่วงกลับเป็นปกติ
        if (rb != null) rb.gravityScale = defaultGravity;
    }
}