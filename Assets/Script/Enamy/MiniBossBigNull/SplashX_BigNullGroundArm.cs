using System.Collections;
using UnityEngine;

public class SplashX_BigNullGroundArm : MonoBehaviour
{
    // 🔥 เติม Dead เข้ามาให้ตรงนี้เรียบร้อยแล้วครับ
    public enum GroundArmState { Inactive, Resting, Warning, Dashing, Stuck, Retracting, Dead }

    [Header("Current State")]
    public GroundArmState currentState = GroundArmState.Inactive;

    [Header("Phase 2 Trigger Settings")]
    public SplashX_Enemy hoverArmStats;
    public float triggerHpPercent = 0.6f;
    private bool hasWokenUp = false;

    [Header("Positions (จุดเริ่มต้นและจุดพุ่ง)")]
    public Transform rightStartPos;
    public Transform leftEndPos;

    [Header("Attack Settings")]
    public float attackCooldown = 4f;
    public float warningDuration = 1.5f;
    public float maxShakeForce = 0.5f;
    public float dashSpeed = 35f;
    public float stuckDuration = 2f;
    public float retractSpeed = 10f;
    public int dashDamage = 2;

    [Header("Effects")]
    public ParticleSystem warningParticles;

    private float attackTimer;
    private int hoverArmMaxHp;
    private bool hasDamagedThisDash = false;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        if (hoverArmStats != null)
        {
            hoverArmMaxHp = hoverArmStats.maxHealth;
        }
    }

    void Update()
    {
        if (currentState == GroundArmState.Inactive)
        {
            CheckPhase2Trigger();
            return;
        }

        if (currentState == GroundArmState.Resting)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    void CheckPhase2Trigger()
    {
        if (hoverArmStats == null || hasWokenUp || hoverArmMaxHp <= 0) return;

        float currentHpPercent = (float)hoverArmStats.currentHealth / hoverArmMaxHp;

        if (currentHpPercent <= triggerHpPercent)
        {
            WakeUpArm();
        }
    }

    public void WakeUpArm()
    {
        hasWokenUp = true;

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;

        transform.position = rightStartPos.position;
        attackTimer = attackCooldown;
        currentState = GroundArmState.Resting;

        Debug.Log("บอสเข้า Phase 2! แขนกวาดพื้นโผล่มาแล้ว!");
    }

    IEnumerator AttackRoutine()
    {
        currentState = GroundArmState.Warning;
        if (warningParticles != null) warningParticles.Play();

        float timer = 0f;
        Vector3 basePos = transform.position;

        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            float currentShake = Mathf.Lerp(0f, maxShakeForce, timer / warningDuration);
            transform.position = basePos + (Vector3)Random.insideUnitCircle * currentShake;
            yield return null;
        }

        transform.position = basePos;
        if (warningParticles != null) warningParticles.Stop();

        currentState = GroundArmState.Dashing;
        hasDamagedThisDash = false;

        while (Vector2.Distance(transform.position, leftEndPos.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, leftEndPos.position, dashSpeed * Time.deltaTime);
            yield return null;
        }

        currentState = GroundArmState.Stuck;
        yield return new WaitForSeconds(stuckDuration);

        currentState = GroundArmState.Retracting;
        while (Vector2.Distance(transform.position, rightStartPos.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, rightStartPos.position, retractSpeed * Time.deltaTime);
            yield return null;
        }

        attackTimer = attackCooldown;
        currentState = GroundArmState.Resting;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == GroundArmState.Dashing && collision.CompareTag("Player") && !hasDamagedThisDash)
        {
            hasDamagedThisDash = true;
            SplashX_PlayerStats stats = collision.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(dashDamage);
        }
    }
}