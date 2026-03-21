using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashX_BossIntro : MonoBehaviour
{
    [Header("Cinematic Objects")]
    public Transform bigMoon;
    public Transform bigMoonTarget;
    public Transform smallMoon;
    public Transform smallMoonTarget; 

    [Header("Settings")]
    public float moonMoveSpeed = 2f;
    public float shakeDuration = 1.5f;
    public float shakeMagnitude = 0.3f;
    public string bossSceneName = "BossRoom_Phase1"; 

    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(PlayIntroCinematic());
        }
    }

    IEnumerator PlayIntroCinematic()
    {
        Vector3 originalCamPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            Camera.main.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalCamPos; 

        while (Vector2.Distance(bigMoon.position, bigMoonTarget.position) > 0.1f ||
               Vector2.Distance(smallMoon.position, smallMoonTarget.position) > 0.1f)
        {
            bigMoon.position = Vector3.MoveTowards(bigMoon.position, bigMoonTarget.position, moonMoveSpeed * Time.deltaTime);
            smallMoon.position = Vector3.MoveTowards(smallMoon.position, smallMoonTarget.position, (moonMoveSpeed * 0.8f) * Time.deltaTime);
            yield return null;
        }

        Destroy(bigMoon.gameObject);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(bossSceneName);
    }
}