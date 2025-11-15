using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollThreshold = 0.001f;
    public float inputDelay;
    public Image fadeImage;
    public float fadeDuration;

    private bool returning = false;
    private float timeElapsed = 0f;

    void Start()
    {
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (!returning)
        {
            if (timeElapsed >= inputDelay && Input.anyKeyDown)
            {
                StartCoroutine(FadeAndLoadScene());
            }

            if (timeElapsed >= 1.5f && scrollRect != null &&
                scrollRect.verticalNormalizedPosition <= scrollThreshold)
            {
                StartCoroutine(FadeAndLoadScene());
            }
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        returning = true;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        SceneManager.LoadScene("_MainMenu");
    }
}