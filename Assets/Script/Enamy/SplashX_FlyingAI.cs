using UnityEngine;

public class SplashX_FlyingAI : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public float followRange = 10f;
    public float stopDistance = 1.5f;
    public float moveSpeed = 3f;

    [Header("Visuals")]
    public bool flipGraphic = true;
    private bool facingRight = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < followRange && distanceToPlayer > stopDistance)
        {
            FollowPlayer();
        }

        if (flipGraphic)
        {
            FlipTowardsPlayer();
        }
    }

    void FollowPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void FlipTowardsPlayer()
    {
        if (transform.position.x < player.position.x && !facingRight)
        {
            Flip();
        }
        else if (transform.position.x > player.position.x && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);
    }
}