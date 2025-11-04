using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // mantener entre escenas
        if (fadeCanvas != null)
            StartCoroutine(FadeIn());
    }

    public void FadeToSceneByIndex(int sceneIndex)
    {
        StartCoroutine(FadeOut(sceneIndex));
    }

    private IEnumerator FadeIn()
    {
        float t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }

    private IEnumerator FadeOut(int sceneIndex)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneIndex);
    }
}
