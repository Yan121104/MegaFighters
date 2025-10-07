using UnityEngine;

public class MovimientoBotInteligente : MonoBehaviour
{
    [Header("Velocidad y salto")]
    public float velocidadNormal = 5f;
    public float velocidadCorrer = 8f;
    public float fuerzaSalto = 10f;
    private float velocidadActual;

    [Header("Detección de suelo y obstáculos")]
    public Transform checkSuelo;
    public Transform checkFrente;
    public Transform checkBorde;
    public float radioSuelo = 0.2f;
    public float distanciaObstaculo = 0.5f;
    public float distanciaBorde = 0.6f;
    public LayerMask sueloLayer;
    public LayerMask obstaculoLayer;
    private bool enSuelo;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Dirección del bot")]
    [SerializeField] private int direccion = 1; // 1 = derecha, -1 = izquierda

    // Getter/Setter público para acceder desde otros scripts
    public int Direccion
    {
        get { return direccion; }
        set { direccion = value; }
    }

    private float movimiento;
    private float proximoSalto = 0f;
    private float cooldownSalto = 2f;

    private Vector2 plataformaVelocidad;
    public string nombreLayerPlataforma = "Suelo";
    private Vector2 ultimaInerciaPlataforma = Vector2.zero;

    // --- Movimiento forzado por portal ---
    [HideInInspector] public bool forzarMovimiento = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("⚠️ No se encontró Animator en " + gameObject.name);

        velocidadActual = velocidadNormal;
    }

    void FixedUpdate()
    {
        enSuelo = Physics2D.OverlapCircle(checkSuelo.position, radioSuelo, sueloLayer);
    }

    void Update()
    {
        // --- Detección de entorno ---
        RaycastHit2D obstaculo = Physics2D.Raycast(checkFrente.position, Vector2.right * direccion, distanciaObstaculo, obstaculoLayer);

        // Detectar borde usando el tamaño del collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector3 bordeOffset = new Vector3((col.bounds.extents.x + 0.05f) * direccion, 0, 0);
        bool bordeSeguro = Physics2D.Raycast(checkSuelo.position + bordeOffset, Vector2.down, distanciaBorde, sueloLayer);

        // --- Movimiento base ---
        if (forzarMovimiento)
        {
            // Si está en movimiento forzado → ignorar la IA normal
            movimiento = direccion;
        }
        else
        {
            movimiento = direccion;

            // Si no hay suelo → girar
            if (!bordeSeguro && enSuelo)
            {
                direccion *= -1;
            }

            // --- Lógica con obstáculos ---
            if (obstaculo.collider != null && enSuelo)
            {
                Vector2 normal = obstaculo.normal;

                // Si el obstáculo es casi vertical → muro
                if (Mathf.Abs(normal.x) > 0.9f && Mathf.Abs(normal.y) < 0.2f)
                {
                    if (Time.time > proximoSalto)
                    {
                        Saltar();
                        proximoSalto = Time.time + cooldownSalto + Random.Range(0f, 2f);
                    }
                    else
                    {
                        direccion *= -1; // si no puede saltar → girar
                    }
                }
            }
        }

        // --- Movimiento final ---
        Vector2 velJugador = new Vector2(movimiento * velocidadActual, rb.velocity.y);
        Vector2 velPlataforma = enSuelo ? plataformaVelocidad : ultimaInerciaPlataforma;

        rb.velocity = new Vector2(velJugador.x + velPlataforma.x, rb.velocity.y);

        // --- Animaciones ---
        animator.SetFloat("Velocidad", Mathf.Abs(movimiento * velocidadActual));
        animator.SetBool("Saltando", !enSuelo);

        // Flip visual
        transform.localScale = new Vector3(direccion * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);
    }

    private void Saltar()
    {
        ultimaInerciaPlataforma = plataformaVelocidad;
        rb.velocity = new Vector2(rb.velocity.x + ultimaInerciaPlataforma.x, 0)
                      + Vector2.up * fuerzaSalto;
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
            ultimaInerciaPlataforma = plataformaVelocidad;
            plataformaVelocidad = Vector2.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkSuelo != null)
        {
            Gizmos.color = Color.blue;
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Vector3 bordeOffset = new Vector3((col.bounds.extents.x + 0.05f) * direccion, 0, 0);
            Gizmos.DrawLine(checkSuelo.position + bordeOffset,
                            checkSuelo.position + bordeOffset + Vector3.down * distanciaBorde);
        }

        if (checkFrente != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(checkFrente.position, checkFrente.position + Vector3.right * direccion * distanciaObstaculo);
        }

        if (checkBorde != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(checkBorde.position, checkBorde.position + Vector3.down * distanciaBorde);
        }
    }

    // --- Método para activar movimiento forzado desde el portal ---
    public void ActivarPortal(int nuevaDireccion)
    {
        forzarMovimiento = true;
        Direccion = nuevaDireccion; // usa getter/setter
    }

    // --- Método para desactivar movimiento forzado ---
    public void DesactivarPortal()
    {
        forzarMovimiento = false;
    }
}
