using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Canvas Group para el fade")]
    public CanvasGroup fadeImage;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener todo el FadeManager entre escenas
            if (fadeImage != null)
            {
                DontDestroyOnLoad(fadeImage.gameObject); // Mantener el CanvasGroup también
                fadeImage.alpha = 0f;
                fadeImage.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void FadeToScene(string sceneName, float fadeInDuration = 1f, float fadeOutDuration = 1.5f, float delayBeforeLoad = 0.2f)
    {
        StartCoroutine(FadeCoroutine(sceneName, fadeInDuration, fadeOutDuration, delayBeforeLoad));
    }

    private IEnumerator FadeCoroutine(string sceneName, float fadeInDuration, float fadeOutDuration, float delayBeforeLoad)
    {
        if (fadeImage == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        // FADE IN
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            fadeImage.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        fadeImage.alpha = 1f;

        yield return new WaitForSeconds(delayBeforeLoad);

        // Cargar escena
        SceneManager.LoadScene(sceneName);
        yield return null;

        // FADE OUT
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            fadeImage.alpha = Mathf.SmoothStep(1f, 0f, t);
            yield return null;
        }

        fadeImage.alpha = 0f;
        fadeImage.gameObject.SetActive(false);
    }
}
