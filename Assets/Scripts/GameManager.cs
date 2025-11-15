using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Victory")]
    public TextMeshProUGUI victoryText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (victoryText != null)
            victoryText.gameObject.SetActive(false);
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (currentSceneIndex + 1 < totalScenes)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            // Último nivel alcanzado -> Mostrar victoria
            if (victoryText != null)
            {
                victoryText.text = "¡GANASTE!";
                victoryText.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("¡GANASTE!");
            }
        }
    }
}
