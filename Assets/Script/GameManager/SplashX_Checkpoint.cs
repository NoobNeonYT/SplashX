using UnityEngine;

public class SplashX_Checkpoint : MonoBehaviour
{
    public bool isMajorCheckpoint = false; // ติ๊กถูกถ้าเป็นจุดหลักที่ใช้เซฟเกม
    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้าผู้เล่นเดินมาชนและยังไม่เคยเก็บจุดนี้
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            // ส่งตำแหน่งตัวเองไปให้ GameManager จำไว้
            SplashX_GameManager.instance.RegisterCheckpoint(transform.position, isMajorCheckpoint);

            // (ใส่ Effect แสงสว่าง หรือเสียงเซฟเกมตรงนี้ได้)
        }
    }
}