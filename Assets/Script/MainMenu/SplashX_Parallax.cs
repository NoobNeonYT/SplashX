using UnityEngine;

public class SplashX_Parallax_UI : MonoBehaviour
{
    public float offsetMultiplier = 1f;
    public float smoothTime = .3f;

    public float maxMoveX = 1f;
    public float maxMoveY = 1f;

    private Vector3 startPosition;
    private Vector3 velocity;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        Vector2 offset = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        Vector3 targetPosition = startPosition + new Vector3(
            offset.x * offsetMultiplier,
            offset.y * offsetMultiplier,
            0f
        );

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            startPosition.x - maxMoveX,
            startPosition.x + maxMoveX
        );

        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            startPosition.y - maxMoveY,
            startPosition.y + maxMoveY
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}