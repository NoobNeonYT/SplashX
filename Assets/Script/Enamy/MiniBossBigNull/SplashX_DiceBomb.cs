using UnityEngine;

public class SplashX_DiceBomb : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 8f;       // Vertical descent speed (slower than Smash attack)

    [Header("Explosion Settings")]
    public float explosionRadius = 2f; // Area of effect radius
    public int damage = 1;
    public GameObject explosionEffect; // Visual effect prefab for the explosion

    [Header("Layer Detection")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    private bool isExploding = false;

    void Update()
    {
        if (isExploding) return;

        // Move downward at a constant velocity
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Simple raycast to detect impact with the ground layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);
        if (hit.collider != null)
        {
            Explode();
        }
    }

    void Explode()
    {
        isExploding = true;

        // Check for player within the explosion radius using an overlap circle
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (playerHit != null)
        {
            SplashX_PlayerStats stats = playerHit.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
        }

        // Spawn visual feedback if assigned
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Cleanup the projectile object
        Destroy(gameObject);
    }

    // Visualize the explosion radius in the Scene View when the object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0); // Orange color for visual clarity
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}