using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform rightStartPos;
    public Transform leftEndPos;
    public float speed = 2.0f;

    private Vector3 targetPos;

    void Start()
    {

        if (leftEndPos != null)
        {
            targetPos = leftEndPos.position;
        }
    }

    void FixedUpdate()
    {
        if (rightStartPos == null || leftEndPos == null) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (targetPos == leftEndPos.position)
                targetPos = rightStartPos.position;
            else
                targetPos = leftEndPos.position;
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
            Debug.Log("Player is now stuck to platform!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
            Debug.Log("Player left platform!");
        }
    }
}