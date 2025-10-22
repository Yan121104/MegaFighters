using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void IniciarJuego()
    {
        // Carga la escena del juego (aseg�rate de a�adirla en Build Settings)
        SceneManager.LoadScene("Nivel1");
    }

    public void AbrirOpciones()
    {
        // Aqu� puedes abrir un panel de opciones
        Debug.Log("Opciones abiertas");
    }

    public void VerCreditos()
    {
        // Muestra los cr�ditos o carga otra escena
        Debug.Log("Mostrando cr�ditos...");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
