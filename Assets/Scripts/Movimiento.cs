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

    [Header("Salto continuo")]
    public float tiempoEntreSaltos = 0.4f;
    private float temporizadorSalto = 0f;

    [Header("Sonido de salto")]
    public AudioSource audioSource;
    public AudioClip saltoSound;
    public float volumenSalto = 0.8f;

    // --- Para plataformas móviles ---
    private Vector2 plataformaVelocidad;
    public string nombreLayerPlataforma = "Suelo";

    private Vector2 ultimaInerciaPlataforma = Vector2.zero;

    // --- Dirección del personaje ---
    public int direccion = 1;

    // --- Movimiento forzado por portal ---
    [HideInInspector] public bool forzarMovimiento = false;

    [Header("Escaleras")]
    public LayerMask escaleraLayer;
    public float velocidadEscalera = 3f;
    private bool enEscalera = false;

    private Vector3 escalaOriginal;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        escalaOriginal = transform.localScale;

        if (animator == null)
            Debug.LogError("⚠️ No se encontró Animator en " + gameObject.name);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 🔥 AÑADE ESTO 🔥
        enSuelo = Physics2D.OverlapCircle(checkSuelo.position, radioSuelo, sueloLayer);
        animator.SetBool("Saltando", !enSuelo);
    }

    void FixedUpdate()
    {
        enSuelo = Physics2D.OverlapCircle(checkSuelo.position, radioSuelo, sueloLayer);
    }

    void Update()
    {

        // Esperar a que el juego inicie
        if (!InicioNivel.banderaJuegoIniciado)
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("Velocidad", 0f);
            animator.SetBool("Saltando", false);
            return; // no hacer nada más
        }

        // --- Movimiento horizontal ---
        float inputHorizontal = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) inputHorizontal = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) inputHorizontal = -1f;

        if (forzarMovimiento)
            movimiento = direccion;
        else
            movimiento = inputHorizontal;

        if (inputHorizontal == 0)
            forzarMovimiento = false;

        velocidadActual = Input.GetKey(KeyCode.Z) ? velocidadCorrer : velocidadNormal;

        Vector2 direccionMovimiento = movimiento > 0 ? Vector2.right :
                                      (movimiento < 0 ? Vector2.left : Vector2.zero);
        Vector2 velJugador = direccionMovimiento * (Mathf.Abs(movimiento) * velocidadActual);
        Vector2 velPlataforma = enSuelo ? plataformaVelocidad : ultimaInerciaPlataforma;

        rb.velocity = new Vector2(velJugador.x + velPlataforma.x, rb.velocity.y);

        // --- Dirección del personaje ---
        if (movimiento > 0.01f) direccion = 1;
        else if (movimiento < -0.01f) direccion = -1;

        // Mantener escala normal del personaje
        transform.localScale = new Vector3(
            direccion * Mathf.Abs(escalaOriginal.x),
            Mathf.Abs(escalaOriginal.y),
            escalaOriginal.z
        );

        // --- Salto continuo ---
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (enSuelo && temporizadorSalto <= 0f)
            {
                ultimaInerciaPlataforma = plataformaVelocidad;
                rb.velocity = new Vector2(rb.velocity.x + ultimaInerciaPlataforma.x, 0)
                              + Vector2.up * fuerzaSalto;

                if (audioSource != null && saltoSound != null)
                    audioSource.PlayOneShot(saltoSound, volumenSalto);

                temporizadorSalto = tiempoEntreSaltos;
            }
        }
        else
        {
            temporizadorSalto = 0f;
        }

        if (temporizadorSalto > 0f)
            temporizadorSalto -= Time.deltaTime;

        // --- Escaleras ---
        RaycastHit2D escaleraHitArriba = Physics2D.Raycast(transform.position, Vector2.up, 1f, escaleraLayer);
        RaycastHit2D escaleraHitAbajo = Physics2D.Raycast(transform.position, Vector2.down, 1f, escaleraLayer);

        bool hayEscaleraArriba = escaleraHitArriba.collider != null;
        bool hayEscaleraAbajo = escaleraHitAbajo.collider != null;

        // Entrar a la escalera
        if (!enEscalera)
        {
            if ((hayEscaleraArriba && Input.GetKey(KeyCode.UpArrow)) ||
                (hayEscaleraAbajo && Input.GetKey(KeyCode.DownArrow)))
            {
                enEscalera = true;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
        }

        // Movimiento en escalera
        if (enEscalera)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(rb.velocity.x, vertical * velocidadEscalera);
            rb.gravityScale = 0f;

            // Animación de escalera
            animator.SetBool("Saltando", false);

            if (Mathf.Abs(vertical) > 0.01f)
            {
                animator.SetBool("EnEscalera", true);
                animator.speed = 1f;
            }
            else
            {
                animator.speed = 0f;
            }

            // Salir de escalera
            if (enSuelo || (!Physics2D.Raycast(transform.position, Vector2.up, 1f, escaleraLayer).collider &&
                            !Physics2D.Raycast(transform.position, Vector2.down, 1f, escaleraLayer).collider))
            {
                enEscalera = false;
                rb.gravityScale = 2f;
                animator.speed = 1f;
                animator.SetBool("EnEscalera", false);
            }
        }
        else
        {
            animator.SetBool("EnEscalera", false);
            animator.SetBool("Saltando", !enSuelo);
        }

        // --- Animación de velocidad horizontal ---
        animator.SetFloat("Velocidad", Mathf.Abs(movimiento * velocidadActual));
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & escaleraLayer) != 0)
        {
            enEscalera = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & escaleraLayer) != 0)
        {
            enEscalera = false;
            rb.gravityScale = 2f;
            animator.SetBool("EnEscalera", false);
        }
    }
}
