using System.Collections;
using UnityEngine;

public class SplashX_PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;

    [Header("Anime Jump Physics (Custom Gravity)")]
    public float jumpForce = 25f;
    public float upwardGravityMult = 4f;
    public float downwardGravityMult = 5f;

    [Header("Hang Time (ค้างกลางอากาศ)")]
    public float hangTimeVelocityThreshold = 0.5f;
    public float hangTimeGravityMult = 0.1f;

    [Header("Tetris Drop (Double Tap)")]
    public float fastFallSpeed = 60f;
    public float doubleTapTime = 0.25f;
    private float lastDownTime = -1f;

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
    // (ลบ attackKey ของเดิมทิ้งไปแล้ว เพราะเราแยกเป็น Normal กับ Heavy ด้านล่างแทน)

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundAndPlatformLayer;
    private bool isGrounded;
    private bool wasGrounded;

    // 🔥 โซนที่เพิ่มเข้ามาใหม่: แยกตัวแปรให้ท่าโจมตีแต่ละแบบ
    [Header("Combat - Normal Attack (ท่าฟันปกติ)")]
    public KeyCode normalAttackKey = KeyCode.U;
    public Vector2 normalHitBox = new Vector2(2f, 1f);
    public int normalDamage = 20;
    public float normalAnimTime = 0.5f;

    [Header("Combat - Heavy Attack (ท่าฟันหนัก)")]
    public KeyCode heavyAttackKey = KeyCode.I; // สมมติให้เป็นปุ่ม I
    public Vector2 heavyHitBox = new Vector2(4f, 3f);
    public int heavyDamage = 50;
    public float heavyAnimTime = 0.8f;

    [Header("Combat - System")]
    public Transform attackPoint;
    public Vector2 debugHitBoxSize = new Vector2(2f, 1f); // ตลับเมตรเอาไว้วัดระยะเฉยๆ
    public LayerMask enemyLayers;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Components & Effects")]
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip dashSFX;
    public AudioClip attackSFX;
    public AudioClip landSFX;
    public AudioClip heavyLandSFX;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SplashX_PlayerStats playerStats;
    private float moveInput;
    private bool facingRight = true;
    private float defaultGravity;

    [Header("Character Models (ระบบสลับร่าง)")]
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
        if (isDashing) return;

        // ล็อกไม่ให้เดินหรือกระโดดตอนกำลังฟันดาบ
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
            moveInput = 0f; // ถ้ากำลังตี บังคับให้ความเร็วแกน X เป็น 0
        }

        if (Input.GetKeyDown(dashKey) && canDash && !isAttacking)
        {
            if (playerStats == null || playerStats.UseStamina(dashStaminaCost)) StartCoroutine(DashRoutine());
        }

        // ระบบเช็คการโจมตี (แยกท่ากด)
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            if (Input.GetKeyDown(normalAttackKey))
            {
                StartCoroutine(AttackRoutine(normalHitBox, normalDamage, normalAnimTime));
                nextAttackTime = Time.time + 1f / attackRate;
            }
            else if (Input.GetKeyDown(heavyAttackKey))
            {
                StartCoroutine(AttackRoutine(heavyHitBox, heavyDamage, heavyAnimTime));
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        Flip();
        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // 🔥 แก้ตรงนี้: แยกระบบแช่แข็งฟิสิกส์ให้ชัดเจน
        if (isAttacking)
        {
            // ถ้าทำ A (ยืนตีบนพื้น) -> ล็อกความเร็วแกน X เป็น 0 ไม่ให้ไถล แต่ปล่อยแรงโน้มถ่วงไว้ให้เท้ากดติดพื้นแน่นๆ
            // ถ้าทำ B (ตีกลางอากาศ) -> ล็อกทั้ง X, Y และแรงโน้มถ่วงเป็น 0 ให้ลอยค้างอยู่กับที่
            rb.linearVelocity = new Vector2(0f, isGrounded ? rb.linearVelocity.y : 0f);
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
                // บล็อคไม่ให้เล่นท่าลงพื้นตอนกำลังฟันดาบ
                if (!isAttacking && (boneModel != null && boneModel.activeSelf))
                {
                    if (isFastFalling)
                    {
                        if (boneAnim != null) boneAnim.SetTrigger("LandHeavy");
                        PlaySFX(heavyLandSFX);
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

        // 🔥 ไม้ตายแก้บั๊ก Fall: ถ้าอยู่บนพื้น บังคับส่งเลข 0 ไปให้ Animator เลย 
        // ตัดปัญหาเศษฟิสิกส์ติดลบ (-0.001) ไปกวนประสาท Animator
        boneAnim.SetFloat("yVelocity", isGrounded ? 0f : rb.linearVelocity.y);
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        isFastFalling = false;

        if (boneAnim != null) boneAnim.SetTrigger("Dash");
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

    private IEnumerator AttackRoutine(Vector2 hitBoxSize, int damage, float animDuration)
    {
        isAttacking = true;

        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        if (fbfAnim != null) fbfAnim.SetTrigger("Attack");
        PlaySFX(attackSFX);

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, hitBoxSize, 0f, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(damage);
            }
        }

        // ... โค้ดด้านบนของ AttackRoutine เหมือนเดิม ...

        // ... โค้ดด้านบนของ AttackRoutine เหมือนเดิม ...

        yield return new WaitForSeconds(animDuration);

        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null)
        {
            boneModel.SetActive(true);

            if (boneAnim != null)
            {
                // 🔥 1. ยัดข้อมูลใส่สมองมันทันทีในเสี้ยววินาทีที่ตื่น! ตัดปัญหาตื่นมาเบลอ
                boneAnim.SetBool("isGrounded", isGrounded);
                boneAnim.SetFloat("yVelocity", 0f);
                boneAnim.SetFloat("Speed", 0f);

                // 🔥 2. ล้างคำสั่งตกค้างให้หมด
                boneAnim.ResetTrigger("LandNormal");
                boneAnim.ResetTrigger("LandHeavy");
                boneAnim.ResetTrigger("Dash");
                boneAnim.ResetTrigger("Jump");

                // 🔥 3. บังคับยัดเข้าท่า Idle แบบฮาร์ดคอร์ (-1, 0f คือการบังคับเล่นเฟรมแรกทันที)
                if (isGrounded)
                {
                    boneAnim.Play("Player_idle", -1, 0f);
                }
            }
        }

        isAttacking = false;
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

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(attackPoint.position, debugHitBoxSize);
        }
    }
}