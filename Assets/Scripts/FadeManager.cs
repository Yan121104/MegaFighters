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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inicializar fade invisible
        if (fadeImage != null)
        {
            fadeImage.alpha = 0f;
            fadeImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Llama para hacer la transición a otra escena con fade cinematográfico.
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar</param>
    /// <param name="fadeInDuration">Tiempo para oscurecer</param>
    /// <param name="fadeOutDuration">Tiempo para revelar</param>
    /// <param name="delayBeforeLoad">Delay antes de cargar la escena</param>
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

        // -----------------------------
        // FADE IN (oscurecer)
        // -----------------------------
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            fadeImage.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return new WaitForEndOfFrame();
        }

        fadeImage.alpha = 1f;

        // Pequeño delay para que el fade in se vea completo
        yield return new WaitForSeconds(delayBeforeLoad);

        // -----------------------------
        // Cargar escena
        // -----------------------------
        SceneManager.LoadScene(sceneName);
        yield return null; // esperar un frame para asegurar carga

        // -----------------------------
        // FADE OUT (revelar escena)
        // -----------------------------
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            fadeImage.alpha = Mathf.SmoothStep(1f, 0f, t);
            yield return new WaitForEndOfFrame();
        }

        fadeImage.alpha = 0f;
        fadeImage.gameObject.SetActive(false);
    }
}
