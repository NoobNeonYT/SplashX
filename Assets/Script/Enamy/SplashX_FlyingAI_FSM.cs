using UnityEngine;
using System.Collections;

public class SplashX_EyeServantAI : MonoBehaviour
{
    public enum State { Idle, Hover, Charging, Dashing }

    [Header("State Control")]
    public State currentState = State.Idle;

    [Header("Detection Settings")]
    public float detectRange = 12f;
    public float hoverDistance = 5f; // Maintain this distance to circle/observe the player

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float dashSpeed = 15f;    // Velocity during the lunge attack
    public float dashDuration = 0.5f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find player by tag - ensure player object is tagged correctly
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Simple State Machine logic
        switch (currentState)
        {
            case State.Idle:
                if (dist < detectRange) currentState = State.Hover;
                break;

            case State.Hover:
                HoverLogic(dist);
                break;
        }
    }

    void HoverLogic(float dist)
    {
        // Move towards the player until within hover range
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // Trigger attack sequence if player is within strike distance
        if (dist < hoverDistance)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = State.Charging;

        // 1. Wind-up phase: Stop movement and telegraph the attack (Cinematic Feel)
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.6f);

        // 2. Dash phase: Lunge towards the player's last known position
        currentState = State.Dashing;
        Vector2 attackDir = (player.position - transform.position).normalized;
        rb.linearVelocity = attackDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 3. Recovery phase: Reset velocity and return to Hover state
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        currentState = State.Hover;
    }
}