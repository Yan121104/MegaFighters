using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public static void CargarNivel2()
    {
        Debug.Log("🔄 Cargando escena Nivel2 desde GameManager...");
        SceneManager.LoadScene("Nivel2");
    }
}
