using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;               // Velocidad de la plataforma
    public Transform limiteIzquierdo;          // Punto límite izquierdo
    public Transform limiteDerecho;            // Punto límite derecho

    private int direccion = 1;                 // Dirección actual (1 = derecha, -1 = izquierda)
    private Rigidbody2D rb;

    // Exponer la velocidad actual para que el jugador pueda leerla
    public Vector2 VelocidadActual { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Aseguramos que la plataforma tenga Rigidbody2D en modo Kinematic
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogError("⚠️ La plataforma " + gameObject.name + " necesita un Rigidbody2D");
        }
    }

    void FixedUpdate()
    {
        // Movimiento en la dirección actual
        Vector2 movimiento = Vector2.right * direccion * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movimiento);

        // Guardar velocidad real (para el jugador)
        VelocidadActual = movimiento / Time.fixedDeltaTime;

        // Cambio de dirección en los límites
        if (direccion == 1 && rb.position.x >= limiteDerecho.position.x)
        {
            direccion = -1;
        }
        else if (direccion == -1 && rb.position.x <= limiteIzquierdo.position.x)
        {
            direccion = 1;
        }
    }

    void OnDrawGizmos()
    {
        if (limiteIzquierdo != null && limiteDerecho != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(limiteIzquierdo.position, limiteDerecho.position);
        }
    }
}
