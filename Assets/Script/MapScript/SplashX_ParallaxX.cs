using UnityEngine;

public class ParallaxX : MonoBehaviour
{
    public Transform player;
    public float parallaxFactor = 0.5f;

    private float startX;
    private float startPlayerX;

    void Start()
    {
        startX = transform.position.x;
        startPlayerX = player.position.x;
    }

    void Update()
    {
        float deltaX = player.position.x - startPlayerX;

        transform.position = new Vector3(
            startX + deltaX * parallaxFactor,
            transform.position.y,
            transform.position.z
        );
    }
}