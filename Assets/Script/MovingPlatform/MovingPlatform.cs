using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform rightStartPos;
    public Transform leftEndPos;
    public float speed = 2.0f;

    private Vector3 targetPos;

    void Start()
    {
        targetPos = leftEndPos.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (targetPos == leftEndPos.position)
                targetPos = rightStartPos.position;
            else
                targetPos = leftEndPos.position;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // เมื่อตัวละครมาเหยียบ ให้กลายเป็นลูกของ Platform (จะขยับตามกัน)
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // เมื่อตัวละครกระโดดออก ให้ยกเลิกการเป็นลูก
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}