using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Tiempo de vida del arma (segundos)")]
    public float tiempoVida = 10f;

    void Start()
    {
        // Destruir el objeto después de 'tiempoVida' segundos
        Destroy(gameObject, tiempoVida);
    }
}
