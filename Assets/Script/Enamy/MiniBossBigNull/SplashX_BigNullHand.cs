using UnityEngine;
using System.Collections;

public class SplashX_BigNullHand : MonoBehaviour
{
    public enum HandState { Hover, SmashWindUp, SmashDown, Stuck, PullUp, DropDice }

    [Header("Current State")]
    public HandState currentState = HandState.Hover;

    [Header("Hover Settings (Follow Player)")]
    public float hoverHeight = 6f;
    public float followSpeed = 5f;

    [Header("Smash Attack Settings")]
    public float smashSpeed = 25f;      // Downward velocity (faster than projectiles)
    public float pullUpSpeed = 8f;      // Speed when returning to hover position
    public float stuckDuration = 3f;    // Vulnerable window for player to attack
    public int smashDamage = 3;

    [Header("Dice Attack Settings")]
    public GameObject dicePrefab;
    public Transform diceSpawnPoint;    // E.g., Fingertips or palm center

    [Header("Attack Flow")]
    public float attackCooldown = 2.5f;
    private float attackTimer;

    [Header("References & Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1f;

    private Transform player;
    private Vector2 targetPos;
    private bool hasDamagedThisSmash = false;

    void Start()
    {
        // Locate player via Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        attackTimer = attackCooldown;
    }

    void Update()
    {
        if (player == null) return;

        // State Machine execution
        switch (currentState)
        {
            case HandState.Hover:
                HoverLogic();
                break;
            case HandState.SmashDown:
                SmashDownLogic();
                break;
            case HandState.Stuck:
                // Logic handled via StuckRoutine coroutine
                break;
            case HandState.PullUp:
                PullUpLogic();
                break;
        }
    }

    // --- 1. Hover Logic (Follow Player on X-axis) ---
    void HoverLogic()
    {
        // Smoothly follow player's X position while maintaining hover height
        targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            // Randomize between Smash or Dice projectile attacks
            int randomAttack = Random.Range(0, 2);
            if (randomAttack == 0) StartCoroutine(SmashRoutine());
            else StartCoroutine(DropDiceRoutine());
        }
    }

    // --- 2. Smash Attack Sequence ---
    IEnumerator SmashRoutine()
    {
        currentState = HandState.SmashWindUp;
        hasDamagedThisSmash = false;

        // Brief telegraph/anticipation before striking
        yield return new WaitForSeconds(0.5f);

        currentState = HandState.SmashDown;
    }

    void SmashDownLogic()
    {
        // Vertical descent
        transform.Translate(Vector3.down * smashSpeed * Time.deltaTime);

        // Ground detection
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider != null)
        {
            StartCoroutine(StuckRoutine());
        }
    }

    IEnumerator StuckRoutine()
    {
        currentState = HandState.Stuck;
        Debug.Log("Boss hand stuck! Opportunity for player to strike.");

        // Wait for recovery window
        yield return new WaitForSeconds(stuckDuration);

        currentState = HandState.PullUp;
    }

    void PullUpLogic()
    {
        // Return to the hover offset position
        targetPos = new Vector2(transform.position.x, player.position.y + hoverHeight);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, pullUpSpeed * Time.deltaTime);

        // Reset to Hover state once reached
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            attackTimer = attackCooldown;
            currentState = HandState.Hover;
        }
    }

    // --- 3. Dice Projectile Attack ---
    IEnumerator DropDiceRoutine()
    {
        currentState = HandState.DropDice;

        // Small delay for spawning animation/timing
        yield return new WaitForSeconds(0.3f);

        if (dicePrefab != null && diceSpawnPoint != null)
        {
            Instantiate(dicePrefab, diceSpawnPoint.position, Quaternion.identity);
        }

        attackTimer = attackCooldown;
        currentState = HandState.Hover;
    }

    // --- Collision & Damage Handling ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Damage player only during SmashDown state
        if (currentState == HandState.SmashDown && collision.CompareTag("Player") && !hasDamagedThisSmash)
        {
            hasDamagedThisSmash = true;
            SplashX_PlayerStats stats = collision.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(smashDamage);
        }
    }
}