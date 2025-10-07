using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Tiempo de vida del arma (segundos)")]
    public float tiempoVida = 10f;

    void Start()
    {
        // Destruir el objeto despu�s de 'tiempoVida' segundos
        Destroy(gameObject, tiempoVida);
    }
}
