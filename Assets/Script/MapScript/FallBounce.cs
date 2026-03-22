using UnityEngine;

public class FallBounce : MonoBehaviour
{
    public float bounceForce = 15f;
    public int damage = 10;
    public float bounceCooldown = 0.5f;

    private bool canBounce = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBounce) return;

        if (collision.CompareTag("Player"))
        {
            canBounce = false;

            SplashX_PlayerStats player = collision.GetComponent<SplashX_PlayerStats>();

            if (player != null)
            {
                player.Bounce(bounceForce);
                player.TakeDamage(damage);
            }

            Invoke(nameof(ResetBounce), bounceCooldown);
        }
    }

    void ResetBounce()
    {
        canBounce = true;
    }
}