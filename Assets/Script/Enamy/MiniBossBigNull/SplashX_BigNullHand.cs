using UnityEngine;
using System.Collections;

public class SplashX_BigNullHand : MonoBehaviour
{
    public enum HandState { Hover, SmashWindUp, SmashDown, Stuck, PullUp, DropDice }
    [Header("Current State")]
    public HandState currentState = HandState.Hover;

    [Header("Hover Settings (ลอยตามผู้เล่น)")]
    public float hoverHeight = 6f;      // ลอยสูงจากผู้เล่นแค่ไหน
    public float followSpeed = 5f;      // ความเร็วตอนบินตาม

    [Header("Smash Attack (ท่าทุบพื้น)")]
    public float smashSpeed = 25f;      // ความเร็วตอนพุ่งทุบ (เร็วกว่าลูกเต๋า)
    public float pullUpSpeed = 8f;      // ความเร็วดึงมือกลับ
    public float stuckDuration = 3f;    // เวลาที่มือติดพื้น (ให้ผู้เล่นรุมตี)
    public int smashDamage = 3;         // ดาเมจทุบ (เจ็บหนัก)

    [Header("Dice Attack (ท่ายิงลูกเต๋า)")]
    public GameObject dicePrefab;       // ลาก Prefab ลูกเต๋ามาใส่
    public Transform diceSpawnPoint;    // จุดปล่อยลูกเต๋า (เช่น ปลายนิ้ว)

    [Header("Attack Flow")]
    public float attackCooldown = 2.5f; // เวลารอระหว่างโจมตี
    private float attackTimer;

    [Header("References & Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1f; // ระยะเช็คพื้นตอนทุบลงมา

    private Transform player;
    private Vector2 targetPos;
    private bool hasDamagedThisSmash = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        attackTimer = attackCooldown;
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case HandState.Hover:
                HoverLogic();
                break;
            case HandState.SmashDown:
                SmashDownLogic();
                break;
            case HandState.Stuck:
                // ติดพื้น รอนิ่งๆ ใน Coroutine
                break;
            case HandState.PullUp:
                PullUpLogic();
                break;
        }
    }

    // --- 1. สถานะลอยตามผู้เล่น ---
    void HoverLogic()
    {
        // คำนวณจุดลอยตัว (X ตรงกับผู้เล่น, Y ลอยสูงขึ้นไป)
        targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            // สุ่มท่าโจมตี (0 = ทุบ, 1 = ปล่อยลูกเต๋า)
            int randomAttack = Random.Range(0, 2);
            if (randomAttack == 0) StartCoroutine(SmashRoutine());
            else StartCoroutine(DropDiceRoutine());
        }
    }

    // --- 2. ท่าทุบพื้น (Smash) ---
    IEnumerator SmashRoutine()
    {
        currentState = HandState.SmashWindUp;
        hasDamagedThisSmash = false;

        // ชะงักเพื่อล็อคเป้าและเตือนผู้เล่นแปปนึง
        yield return new WaitForSeconds(0.5f);

        currentState = HandState.SmashDown;
    }

    void SmashDownLogic()
    {
        // พุ่งลงดิ่งๆ แกน Y
        transform.Translate(Vector3.down * smashSpeed * Time.deltaTime);

        // เช็คว่าชนพื้นหรือยัง
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider != null)
        {
            // ชนพื้นแล้ว! เข้าสถานะติดพื้น
            StartCoroutine(StuckRoutine());
        }
    }

    IEnumerator StuckRoutine()
    {
        currentState = HandState.Stuck;
        Debug.Log("มือติดพื้น! โอกาสทองของผู้เล่นตีได้เลย!");

        // รอเวลาดึงมือออก
        yield return new WaitForSeconds(stuckDuration);

        currentState = HandState.PullUp;
    }

    void PullUpLogic()
    {
        // ดึงมือกลับไปที่ระยะ Hover
        targetPos = new Vector2(transform.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, pullUpSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            attackTimer = attackCooldown;
            currentState = HandState.Hover;
        }
    }

    // --- 3. ท่าปล่อยลูกเต๋า (Dice) ---
    IEnumerator DropDiceRoutine()
    {
        currentState = HandState.DropDice;

        // หยุดนิ่งแปปนึงเป็นอนิเมชั่นปล่อยของ
        yield return new WaitForSeconds(0.3f);

        if (dicePrefab != null && diceSpawnPoint != null)
        {
            Instantiate(dicePrefab, diceSpawnPoint.position, Quaternion.identity);
        }

        attackTimer = attackCooldown;
        currentState = HandState.Hover;
    }

    // --- ระบบทำดาเมจตอนทุบโดน ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == HandState.SmashDown && collision.CompareTag("Player") && !hasDamagedThisSmash)
        {
            hasDamagedThisSmash = true;
            SplashX_PlayerStats stats = collision.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(smashDamage);
        }
    }
}