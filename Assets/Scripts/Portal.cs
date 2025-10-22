using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Destino del portal")]
    public Transform destino;

    [Header("Tiempo de espera para evitar loop")]
    public float cooldown = 0.5f;

    [Header("Fuerza de empuje al salir")]
    public float fuerzaEmpuje = 3f;

    private bool puedeTeletransportar = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!puedeTeletransportar) return;

        Movimiento jugador = other.GetComponent<Movimiento>();
        MovimientoBotInteligente bot = other.GetComponent<MovimientoBotInteligente>();
        Rigidbody2D rb = other.attachedRigidbody;

        if (jugador != null || bot != null)
        {
            // Teletransportar al destino
            other.transform.position = destino.position;

            int nuevaDireccion = -1; // dirección por defecto

            if (jugador != null)
            {
                // Invertir dirección del jugador
                jugador.direccion *= -1;
                jugador.forzarMovimiento = true;
                nuevaDireccion = jugador.direccion;
            }
            else if (bot != null)
            {
                // Reiniciar su estado de movimiento después del portal
                int nuevaDir = -bot.Direccion; // invierte la dirección
                bot.ActivarPortal(nuevaDir);
                nuevaDireccion = nuevaDir;

                // Reanudar su comportamiento normal tras breve pausa
                bot.ReanudarMovimientoDespuesDe(0.2f);
            }

            // Aplicar empuje
            if (rb != null)
            {
                rb.velocity = new Vector2(nuevaDireccion * fuerzaEmpuje, rb.velocity.y);
            }

            // Activar cooldown en ambos portales
            StartCoroutine(DesactivarPorUnMomento());
            Portal portalDestino = destino.GetComponent<Portal>();
            if (portalDestino != null)
                portalDestino.StartCoroutine(portalDestino.DesactivarPorUnMomento());
        }
    }

    private System.Collections.IEnumerator DesactivarPorUnMomento()
    {
        puedeTeletransportar = false;
        yield return new WaitForSeconds(cooldown);
        puedeTeletransportar = true;
    }
}
