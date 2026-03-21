using System.Collections;
using UnityEngine;

public class ComicController : MonoBehaviour
{
    [Header("Select 4 Panels")]
    public GameObject[] comicPanels;

    [Header("Time")]
    public float delayBetweenPanels = 1.0f;

    [Header("Popup")]
    [Tooltip("Pop Time")]
    public float popupDuration = 0.5f;
    public AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Prefab Setting")]
    [Tooltip("Out Time")]
    public float delayBeforeEnd = 2.0f;

    [Tooltip(" Prefab ถัดไปจากหน้าต่าง Project มาใส่ตรงนี้ได้เลย!")]
    public GameObject nextPrefab;

    [Tooltip("ต้องการให้ปิดหน้าคอมมิคนี้ทิ้งไปเลยไหมเมื่อโชว์จบ?")]
    public bool hideComicWhenFinished = true;

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

        Debug.Log("โชว์ครบแล้ว กำลังรอเพื่อแสดง Prefab ถัดไป...");
        yield return new WaitForSeconds(delayBeforeEnd);

        
        if (nextPrefab != null)
        {
            
            Instantiate(nextPrefab);
        }

       
        if (hideComicWhenFinished)
        {
            gameObject.SetActive(false);
        }
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
}