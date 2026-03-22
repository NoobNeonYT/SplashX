using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicController : MonoBehaviour
{
    [Header("Select Panels")]
    public GameObject[] comicPanels;

    [Header("Time")]
    public float delayBetweenPanels = 1.0f;

    [Header("Popup")]
    public float popupDuration = 0.5f;
    public AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Prefab Setting")]
    public float delayBeforeEnd = 2.0f;
    public GameObject nextPrefab;
    public bool hideComicWhenFinished = true;

    [Header("Transition")]
    public GameObject transitionPanel;
    public float transitionDelay = 2f;

    private Vector3[] _originalScales;
    private bool isTransitioning = false;

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

        Debug.Log("โชว์ครบแล้ว กำลังรอ...");
        yield return new WaitForSeconds(delayBeforeEnd);

        // แสดง prefab ถัดไป
        if (nextPrefab != null)
        {
            Instantiate(nextPrefab);
        }

        // ซ่อน comic
        if (hideComicWhenFinished)
        {
            gameObject.SetActive(false);
        }

        // 🔥 เรียก transition แบบ Invoke
        StartTransition();
    }

    void StartTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }

        Invoke(nameof(LoadGame), transitionDelay);
    }

    void LoadGame()
    {
        SceneManager.LoadScene("MapLv2");
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
}