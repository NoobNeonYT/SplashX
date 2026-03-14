using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour
{
    public float scale = 1.1f;

    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = startScale * scale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = startScale;
    }
}
