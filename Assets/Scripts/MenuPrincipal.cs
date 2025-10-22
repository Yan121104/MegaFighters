using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void IniciarJuego()
    {
        // Carga la escena del juego (asegúrate de añadirla en Build Settings)
        SceneManager.LoadScene("Nivel1");
    }

    public void AbrirOpciones()
    {
        // Aquí puedes abrir un panel de opciones
        Debug.Log("Opciones abiertas");
    }

    public void VerCreditos()
    {
        // Muestra los créditos o carga otra escena
        Debug.Log("Mostrando créditos...");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
