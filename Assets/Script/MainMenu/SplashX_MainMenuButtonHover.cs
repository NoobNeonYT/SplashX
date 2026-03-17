using UnityEngine;
using UnityEngine.EventSystems;

public class SplashX_MainMenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public float scale = 1.1f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = startScale * scale;

        UIAudioManager.instance.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = startScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.instance.PlayClick();
    }
}