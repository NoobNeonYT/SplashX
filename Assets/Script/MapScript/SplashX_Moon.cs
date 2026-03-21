using UnityEngine;

public class SplashX_Moon : MonoBehaviour
{
    public Transform player;

    public float offsetX = 5f;
    public float smoothSpeed = 2f;

    void Update()
    {
        float targetX = player.position.x + offsetX;

        float newX = Mathf.Lerp(
            transform.position.x,
            targetX,
            Time.deltaTime * smoothSpeed
        );

        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );
    }
}
