using UnityEngine;
using System.Collections;

public class SplashX_EyeServantAI : MonoBehaviour
{
    public enum State { Idle, Hover, Charging, Dashing }
    public State currentState = State.Idle;

    [Header("Detection")]
    public float detectRange = 12f;
    public float hoverDistance = 5f; // ระยะที่มันจะบินวนดูเชิง

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float dashSpeed = 15f;    // ความเร็วตอนพุ่งชน
    public float dashDuration = 0.5f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

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
        // บินวนไปรอบๆ ผู้เล่นในระยะที่กำหนด
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // ถ้าอยู่ใกล้ในระยะที่พร้อมพุ่งชน และไม่ได้กำลังคูลดาวน์
        if (dist < hoverDistance)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = State.Charging;

        // 1. จังหวะ Wind-up: หยุดนิ่งครู่หนึ่งเพื่อบอกใบ้คนเล่น (Cinematic Feel)
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.6f);

        // 2. จังหวะ Dash: พุ่งตรงไปยังตำแหน่งล่าสุดของ Player
        currentState = State.Dashing;
        Vector2 attackDir = (player.position - transform.position).normalized;
        rb.linearVelocity = attackDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 3. จังหวะพัก: ค่อยๆ เบรกและกลับไป Hover
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
        currentState = State.Hover;
    }
}