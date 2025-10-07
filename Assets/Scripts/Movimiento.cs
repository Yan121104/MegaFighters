using UnityEngine;

public class Movimiento : MonoBehaviour
{
    [Header("Velocidad y salto")]
    public float velocidadNormal = 5f;
    public float velocidadCorrer = 8f;
    public float fuerzaSalto = 10f;
    private float velocidadActual;

    [Header("Detección de suelo")]
    public Transform checkSuelo;
    public float radioSuelo = 0.2f;
    public LayerMask sueloLayer;
    private bool enSuelo;

    private Rigidbody2D rb;
    private Animator animator;
    private float movimiento;

    // --- Para plataformas móviles ---
    private Vector2 plataformaVelocidad;
    public string nombreLayerPlataforma = "Suelo";

    private Vector2 ultimaInerciaPlataforma = Vector2.zero;

    // --- Dirección del personaje ---
    public int direccion = 1;  // 1 = derecha, -1 = izquierda

    // --- Movimiento forzado por portal ---
    [HideInInspector] public bool forzarMovimiento = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("⚠️ No se encontró Animator en " + gameObject.name);
    }

    void FixedUpdate()
    {
        enSuelo = Physics2D.OverlapCircle(checkSuelo.position, radioSuelo, sueloLayer);
    }

    void Update()
    {
        float inputHorizontal = 0f;

        if (Input.GetKey(KeyCode.RightArrow))
            inputHorizontal = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow))
            inputHorizontal = -1f;

        // Si está forzado por el portal, ignora el input
        if (forzarMovimiento)
        {
            movimiento = direccion; // sigue moviéndose en la dirección actual
        }
        else
        {
            movimiento = inputHorizontal;
        }

        // Si el jugador suelta las teclas de movimiento, desactivar forzado
        if (inputHorizontal == 0)
        {
            forzarMovimiento = false;
        }

        // Ajustar velocidad
        velocidadActual = Input.GetKey(KeyCode.Z) ? velocidadCorrer : velocidadNormal;

        // --- Movimiento y velocidad ---
        Vector2 direccionMovimiento = movimiento > 0 ? Vector2.right :
                                      (movimiento < 0 ? Vector2.left : Vector2.zero);

        Vector2 velJugador = direccionMovimiento * (Mathf.Abs(movimiento) * velocidadActual);

        // 🔹 Si está en suelo/plataforma -> usar velocidad de la plataforma
        // 🔹 Si está en el aire -> mantener la última inercia de la plataforma
        Vector2 velPlataforma = enSuelo ? plataformaVelocidad : ultimaInerciaPlataforma;

        // Asignar velocidad final (respetando velocidad Y actual)
        rb.velocity = new Vector2(velJugador.x + velPlataforma.x, rb.velocity.y);

        // Animaciones
        // 🔹 Usar solo el input del jugador para la animación de caminar/correr
        animator.SetFloat("Velocidad", Mathf.Abs(movimiento * velocidadActual));

        // 🔹 Estado de salto
        animator.SetBool("Saltando", !enSuelo);


        // Cambiar dirección SOLO si hay input o si está forzado
        if (movimiento > 0.01f)
            direccion = 1;
        else if (movimiento < -0.01f)
            direccion = -1;

        // Aplicar flip visual según dirección
        transform.localScale = new Vector3(direccion * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Salto
        if (Input.GetKeyDown(KeyCode.UpArrow) && enSuelo)
        {
            // Guardamos la inercia de la plataforma
            ultimaInerciaPlataforma = plataformaVelocidad;

            // Aplicamos salto + inercia horizontal
            rb.velocity = new Vector2(rb.velocity.x + ultimaInerciaPlataforma.x, 0)
                          + Vector2.up * fuerzaSalto;
        }

    }

    // --- Plataformas móviles ---
    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(nombreLayerPlataforma))
        {
            PlataformaMovil pm = other.gameObject.GetComponent<PlataformaMovil>();
            if (pm != null)
                plataformaVelocidad = pm.VelocidadActual;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(nombreLayerPlataforma))
        {
            // Guardamos la última velocidad de la plataforma al salir
            ultimaInerciaPlataforma = plataformaVelocidad;
            plataformaVelocidad = Vector2.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkSuelo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkSuelo.position, radioSuelo);
        }
    }
}
