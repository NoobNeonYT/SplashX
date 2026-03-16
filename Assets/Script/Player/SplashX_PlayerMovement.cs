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
    public float fastFallSpeed = 60f;           // ⚡ ความเร็วดิ่งพสุธา (อัดให้เยอะๆ จะได้ถึงพื้นทันที)
    public float doubleTapTime = 0.25f;         // ⏱️ เวลาที่ยอมให้กด S เบิ้ล (วินาที)
    private float lastDownTime = -1f;           // เก็บเวลาที่กด S ครั้งล่าสุด

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
    public KeyCode jumpKey = KeyCode.J;
    public KeyCode dashKey = KeyCode.K;
    public KeyCode downKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.Y;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundAndPlatformLayer;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Combat Settings")]
    public float attackAnimTime = 0.5f;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 20;
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
    public GameObject boneModel;      // ลากก้อน MainCharacter มาใส่
    public Animator boneAnim;         // ลากก้อน MainCharacter มาใส่ (เพื่อส่งค่าเดิน/วิ่ง)

    public GameObject fbfAttackModel; // ลากก้อน FbF_Attack มาใส่
    public Animator fbfAnim;          // ลากก้อน FbF_Attack มาใส่ (เพื่อสั่งเล่นท่าตี)

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

        moveInput = Input.GetAxisRaw("Horizontal");
        

        if (Input.GetKeyDown(jumpKey))
        {
            if (Input.GetKey(downKey) && isGrounded)
            {
                DropFromPlatform();
            }
            else if (jumpsLeft > 0)
            {
                ExecuteJump();
            }
        }

        // --- ระบบกด S เบิ้ล (Double Tap) เพื่อดิ่งพสุธา ---
        if (!isGrounded && Input.GetKeyDown(downKey))
        {
            // ถ้ากดครั้งที่ 2 ภายในเวลาที่กำหนด
            if (Time.time - lastDownTime <= doubleTapTime)
            {
                isFastFalling = true;
            }
            lastDownTime = Time.time; // อัปเดตเวลาที่กดล่าสุดไว้เสมอ
        }

        if (Input.GetKeyDown(dashKey) && canDash && !isAttacking)
        {
            if (playerStats == null || playerStats.UseStamina(dashStaminaCost))
            {
                StartCoroutine(DashRoutine());
            }
        }

        if (Time.time >= nextAttackTime && Input.GetKeyDown(attackKey) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + 1f / attackRate;
        }

        Flip();
        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (!isGrounded)
        {
            // 1. ดิ่ง Tetris (ความเร็วพุ่งปรี๊ด)
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
                if (isFastFalling)
                {
                    if (anim != null) anim.SetTrigger("LandHeavy");
                    PlaySFX(heavyLandSFX);
                }
                else
                {
                    if (anim != null) anim.SetTrigger("LandNormal");
                    PlaySFX(landSFX);
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

        if (anim != null) anim.SetTrigger("Jump");
        PlaySFX(jumpSFX);
    }

    void UpdateAnimatorParameters()
    {
        // ส่งค่าฟิสิกส์ให้ Animator ของร่างกระดูกเท่านั้น
        if (boneAnim == null) return;
        boneAnim.SetBool("isGrounded", isGrounded);
        boneAnim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        boneAnim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        isFastFalling = false;

        if (anim != null) anim.SetTrigger("Dash");
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

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. สลับร่าง! (ปิดกระดูก เปิดภาพวาด)
        if (boneModel != null) boneModel.SetActive(false);
        if (fbfAttackModel != null) fbfAttackModel.SetActive(true);

        // 2. สั่งเล่นอนิเมชันโจมตีที่ร่างภาพวาด
        if (fbfAnim != null) fbfAnim.SetTrigger("Attack");
        PlaySFX(attackSFX);

        // ล็อกฟิสิกส์ตอนตี
        float prevVelocityY = rb.linearVelocity.y;
        if (!isGrounded && !isFastFalling) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // คำนวณดาเมจ
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackAnimTime);

        // สลับร่างกลับ! (ปิดภาพวาด เปิดกระดูก)
        if (fbfAttackModel != null) fbfAttackModel.SetActive(false);
        if (boneModel != null) boneModel.SetActive(true);

        isAttacking = false;

        // คืนค่าฟิสิกส์
        if (!isGrounded && !isFastFalling) rb.linearVelocity = new Vector2(rb.linearVelocity.x, prevVelocityY);
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
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}