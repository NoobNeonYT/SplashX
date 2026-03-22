using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicEnd : MonoBehaviour
{
    [Header("Select Panels")]
    public GameObject[] comicPanels;

    [Header("Time")]
    public float delayBetweenPanels = 1.0f;

    [Header("Popup")]
    public float popupDuration = 0.5f;
    public AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("End Delay")]
    public float delayBeforeEnd = 2.0f;

    [Header("Transition")]
    public GameObject transitionPanel;
    public float transitionDelay = 2f;

    private Vector3[] _originalScales;
    private bool isLoading = false;

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

        // ปิด transition ไว้ก่อน
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
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

        Debug.Log("Comic จบแล้ว...");
        yield return new WaitForSeconds(delayBeforeEnd);

        // 🔥 เริ่มโหลด
        StartTransition();
    }

    IEnumerator AnimatePopup(int panelIndex)
    {
        float timer = 0f;
        GameObject panel = comicPanels[panelIndex];
        Vector3 originalScale = _originalScales[panelIndex];

        while (timer < popupDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popupDuration;
            float curveValue = popupCurve.Evaluate(t);
            panel.transform.localScale = originalScale * curveValue;
            yield return null;
        }

        panel.transform.localScale = originalScale;
    }

    void StartTransition()
    {
        if (isLoading) return;
        isLoading = true;

        // 🔥 เปิดหน้าโหลด
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }

        // 🔥 รอแล้วโหลด Credit
        Invoke(nameof(LoadScene), transitionDelay);
    }

    void LoadScene()
    {
        SceneManager.LoadScene("Credit");
    }
}