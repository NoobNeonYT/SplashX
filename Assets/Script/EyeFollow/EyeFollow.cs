using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    public Transform player;

    [Header("Eye Settings")]
    [Tooltip("ติ๊กช่องนี้สำหรับตาข้างขวา (ที่ต้องการให้หันสลับทิศ)")]
    public bool isRightEye = false;

    private float currentVelocity = 0f;

    void Update()
    {
        if (player != null)
        {

            Vector3 direction = player.position - transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            float targetAngle = angle;
            if (isRightEye)
            {
                targetAngle += 180f;
            }

            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref currentVelocity, 0.05f);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
    }
}