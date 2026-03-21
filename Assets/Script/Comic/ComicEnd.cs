using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public class ComicEnd : MonoBehaviour
{
    [Header("Select 4 Panels")]
    public GameObject[] comicPanels;

    [Header("Time")]
    public float delayBetweenPanels = 1.0f;

    [Header("Popup")]
    [Tooltip("Pop Time")]
    public float popupDuration = 0.5f;
    public AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene Transition Setting")]
    [Tooltip("Out Time (เวลาหน่วงก่อนเริ่มเฟดจอดำ)")]
    public float delayBeforeEnd = 2.0f;

    [Tooltip("ชื่อของ Scene ถัดไปที่ต้องการให้โหลด")]
    public string nextSceneName;

    [Header("Fade Setting")]
    [Tooltip("ลาก UI Image สีดำที่ขยายเต็มจอ มาใส่ตรงนี้")]
    public Image fadeImage;
    [Tooltip("ความเร็วในการเฟดจอดำ (วินาที)")]
    public float fadeDuration = 1.0f;

    private Vector3[] _originalScales;

    void Start()
    {
        _originalScales = new Vector3[comicPanels.Length];

        
        for (int i = 0; i < comicPanels.Length; i++)
        {
            if (comicPanels[i] != null)
            {
                _originalScales[i] = comicPanels[i].transform.localScale;
                comicPanels[i].SetActive(false);
                comicPanels[i].transform.localScale = Vector3.zero;
            }
        }

        
        if (fadeImage != null)
        {
            Color startColor = fadeImage.color;
            startColor.a = 0f;
            fadeImage.color = startColor;
            fadeImage.gameObject.SetActive(true); 
        }

        StartCoroutine(ShowComicPanelsWithPopup());
    }

    IEnumerator ShowComicPanelsWithPopup()
    {
        yield return new WaitForSeconds(0.5f);

       
        for (int i = 0; i < comicPanels.Length; i++)
        {
            if (comicPanels[i] != null)
            {
                comicPanels[i].SetActive(true);
                StartCoroutine(AnimatePopup(i));
                yield return new WaitForSeconds(delayBetweenPanels);
            }
        }

        Debug.Log("โชว์คอมมิคครบแล้ว กำลังรอเพื่อเฟดจอดำ...");
        yield return new WaitForSeconds(delayBeforeEnd);

        
        StartCoroutine(FadeToBlackAndLoadScene());
    }

    IEnumerator AnimatePopup(int panelIndex)
    {
        float timer = 0f;
        GameObject panel = comicPanels[panelIndex];
        Vector3 originalScale = _originalScales[panelIndex];

        while (timer < popupDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / popupDuration;
            float curveValue = popupCurve.Evaluate(normalizedTime);
            panel.transform.localScale = originalScale * curveValue;
            yield return null;
        }

        panel.transform.localScale = originalScale;
    }

    
    IEnumerator FadeToBlackAndLoadScene()
    {
        if (fadeImage != null)
        {
            float timer = 0f;
            Color color = fadeImage.color;

           
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            
            color.a = 1f;
            fadeImage.color = color;
        }
        else
        {
            Debug.LogWarning("ไม่ได้ใส่ UI Image จอดำเอาไว้ ภาพเลยตัดฉับไปซีนใหม่เลยนะครับ!");
        }

        
        yield return new WaitForSeconds(0.2f);

        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("ยังไม่ได้ตั้งชื่อ Scene ถัดไปใน Inspector!");
        }
    }
}