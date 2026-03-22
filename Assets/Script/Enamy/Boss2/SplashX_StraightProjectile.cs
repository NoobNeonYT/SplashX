using UnityEngine;

public class SplashX_StraightProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    [Tooltip("ทิศทางที่พุ่ง (1 = ขึ้น, -1 = ลง)")]
    public float directionY = -1f;

    [Header("Cleanup")]
    public float lifetime = 5f; // พุ่งไป 5 วินาทีแล้วลบทิ้ง (พอที่จะทะลุจอ)

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // สั่งให้พุ่งไปตามแกน Y (ขึ้นหรือลง ตามที่ตั้งค่าไว้)
        transform.Translate(new Vector3(0, directionY * speed * Time.deltaTime, 0));
    }
}