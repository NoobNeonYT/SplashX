using UnityEngine;

public class CreditsCameraPath : MonoBehaviour
{
    [Header("Waypoints (A → B → C → D)")]
    public Transform[] waypoints;

    [Header("Move Speed")]
    public float moveSpeed = 3f;

    [Header("Zigzag")]
    public float zigzagAmount = 1.5f;
    public float zigzagSpeed = 2f;

    private int currentIndex = 0;
    private float time;

    void Update()
    {
        if (waypoints.Length == 0) return;
        if (currentIndex >= waypoints.Length) return;

        time += Time.deltaTime;

        Transform target = waypoints[currentIndex];

        // คำนวณตำแหน่งเป้าหมาย
        Vector3 targetPos = target.position;

        // เพิ่ม zigzag (แกน X)
        float xOffset = Mathf.Sin(time * zigzagSpeed) * zigzagAmount;
        targetPos.x += xOffset;

        // เคลื่อนที่ไปหาเป้าหมาย
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // ถึงจุดแล้ว → ไปจุดถัดไป
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            currentIndex++;
        }
    }
}