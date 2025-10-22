using UnityEngine;
using System.Collections;

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

    [Header("Detección de otros bots")]
    public LayerMask botLayer;
    public float distanciaBot = 0.7f;

    [Header("Persecución del jugador")]
    public float rangoDeteccion = 6f;
    public float rangoAtaque = 1.2f;
    private Transform jugadorObjetivo;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Dirección del bot")]
    [SerializeField] private int direccion = 1; // 1 = derecha, -1 = izquierda
    public int Direccion
    {
        get { return direccion; }
        set { direccion = Mathf.Clamp(value, -1, 1); }
    }

    private float movimiento;
    private float proximoSalto = 0f;
    private float cooldownSalto = 2f;

    private Vector2 plataformaVelocidad;
    public string nombreLayerPlataforma = "Suelo";
    private Vector2 ultimaInerciaPlataforma = Vector2.zero;

    [HideInInspector] public bool forzarMovimiento = false;
    private bool enPortal = false;

    // --- Antiatasco ---
    private float tiempoAtascado = 0f;
    private float tiempoMaxAtascado = 1.5f;
    private Vector2 ultimaPosicion;
    private float tiempoProximoIntento = 0f;

    [Header("Plataformas Móviles")]
    public LayerMask plataformaMovilLayer;
    public float rangoSaltoPlataforma = 2.5f;

    [Header("Seguridad ante vacío")]
    public float distanciaSeguridadVacio = 1.5f; // Si el suelo está más lejos que esto, no avanza
    private bool evitarCaida = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning($"⚠️ No se encontró Animator en {gameObject.name}");

        velocidadActual = velocidadNormal;
        ultimaPosicion = transform.position;

        BuscarJugadorPrincipal();
    }

    void BuscarJugadorPrincipal()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (p != gameObject)
            {
                jugadorObjetivo = p.transform;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (checkSuelo != null)
            enSuelo = Physics2D.OverlapCircle(checkSuelo.position, radioSuelo, sueloLayer);
    }

    void Update()
    {
        if (rb == null || enPortal) return;

        if (jugadorObjetivo == null)
        {
            BuscarJugadorPrincipal();
            return;
        }

        float distanciaX = Mathf.Abs(jugadorObjetivo.position.x - transform.position.x);
        bool jugadorEnRango = distanciaX <= rangoDeteccion;

        // --- Evaluar si hay suelo delante antes de avanzar ---
        evitarCaida = !HaySueloSeguroDelante();

        if (jugadorEnRango)
        {
            // --- PERSECUCIÓN INTELIGENTE ---
            if (distanciaX > rangoAtaque)
            {
                int dirJugador = (jugadorObjetivo.position.x > transform.position.x) ? 1 : -1;

                // Solo persigue si no hay riesgo de caer al vacío
                if (!evitarCaida)
                {
                    direccion = dirJugador;
                    movimiento = direccion;
                    velocidadActual = velocidadCorrer;
                }
                else
                {
                    movimiento = 0; // Detenerse si hay vacío
                }
            }
            else
            {
                movimiento = 0;
            }
        }
        else
        {
            // --- PATRULLA NORMAL ---
            velocidadActual = velocidadNormal;
            movimiento = direccion;

            RaycastHit2D obstaculo = Physics2D.Raycast(checkFrente.position, Vector2.right * direccion, distanciaObstaculo, obstaculoLayer);
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Vector3 bordeOffset = new Vector3((col.bounds.extents.x + 0.05f) * direccion, 0, 0);
            bool bordeSeguro = Physics2D.Raycast(checkSuelo.position + bordeOffset, Vector2.down, distanciaBorde, sueloLayer);

            if (!forzarMovimiento)
            {
                if (!bordeSeguro && enSuelo)
                {
                    if (!IntentarSaltarHaciaPlataforma())
                        CambiarDireccion();
                }

                if (obstaculo.collider != null && enSuelo)
                {
                    Vector2 normal = obstaculo.normal;
                    if (Mathf.Abs(normal.x) > 0.9f && Mathf.Abs(normal.y) < 0.2f)
                    {
                        if (Time.time > proximoSalto)
                        {
                            Saltar();
                            proximoSalto = Time.time + cooldownSalto + Random.Range(0f, 2f);
                        }
                    }
                }

                DetectarYSepararBots();
            }
        }

        // --- Movimiento final ---
        Vector2 velBase = new Vector2(movimiento * velocidadActual, rb.velocity.y);
        Vector2 velPlataforma = enSuelo ? plataformaVelocidad : ultimaInerciaPlataforma;
        rb.velocity = new Vector2(velBase.x + velPlataforma.x, velBase.y);

        if (animator != null)
        {
            animator.SetFloat("Velocidad", Mathf.Abs(rb.velocity.x));
            animator.SetBool("Saltando", !enSuelo);
        }

        transform.localScale = new Vector3(
            direccion * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );

        VerificarAtasco();
    }

    private bool IntentarSaltarHaciaPlataforma()
    {
        RaycastHit2D plataformaCercana = Physics2D.Raycast(
            checkBorde.position,
            Vector2.down + Vector2.right * direccion * 0.5f,
            rangoSaltoPlataforma,
            plataformaMovilLayer
        );

        if (plataformaCercana.collider != null && Time.time > proximoSalto)
        {
            PlataformaMovil plataforma = plataformaCercana.collider.GetComponent<PlataformaMovil>();
            if (plataforma != null)
            {
                float alturaRelativa = plataforma.transform.position.y - transform.position.y;
                if (alturaRelativa < 1.2f && Mathf.Abs(plataforma.transform.position.x - transform.position.x) < 3f)
                {
                    Saltar();
                    proximoSalto = Time.time + cooldownSalto + Random.Range(0.5f, 1.5f);
                    return true;
                }
            }
        }

        return false;
    }

    private bool HaySueloSeguroDelante()
    {
        if (checkSuelo == null) return true;

        Vector3 origen = checkSuelo.position + Vector3.right * direccion * 0.6f;
        RaycastHit2D sueloDetectado = Physics2D.Raycast(origen, Vector2.down, distanciaSeguridadVacio, sueloLayer);

        // Si no hay suelo o el suelo está demasiado lejos, no es seguro
        return sueloDetectado.collider != null;
    }

    private void Saltar()
    {
        ultimaInerciaPlataforma = plataformaVelocidad;
        rb.velocity = new Vector2(rb.velocity.x + ultimaInerciaPlataforma.x, 0)
                      + Vector2.up * fuerzaSalto;
    }

    private void CambiarDireccion()
    {
        direccion *= -1;
    }

    private void DetectarYSepararBots()
    {
        Collider2D botCercano = Physics2D.OverlapCircle(transform.position, distanciaBot, botLayer);
        if (botCercano != null && botCercano.gameObject != gameObject)
        {
            if (Time.time > tiempoProximoIntento)
            {
                CambiarDireccion();
                tiempoProximoIntento = Time.time + 0.8f;
            }
        }
    }

    private void VerificarAtasco()
    {
        float distancia = Vector2.Distance(transform.position, ultimaPosicion);

        if (distancia < 0.05f)
        {
            tiempoAtascado += Time.deltaTime;
            if (tiempoAtascado > tiempoMaxAtascado)
            {
                if (enSuelo)
                {
                    if (Random.value < 0.7f)
                        CambiarDireccion();
                    else if (Time.time > proximoSalto)
                    {
                        Saltar();
                        proximoSalto = Time.time + cooldownSalto + Random.Range(0.5f, 1.5f);
                    }
                }
                tiempoAtascado = 0f;
            }
        }
        else
        {
            tiempoAtascado = 0f;
        }

        ultimaPosicion = transform.position;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!forzarMovimiento && ((1 << other.gameObject.layer) & botLayer) != 0)
        {
            foreach (ContactPoint2D contacto in other.contacts)
            {
                if (direccion == 1 && contacto.normal.x < -0.5f)
                {
                    CambiarDireccion();
                    break;
                }
                else if (direccion == -1 && contacto.normal.x > 0.5f)
                {
                    CambiarDireccion();
                    break;
                }
            }
        }
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(checkSuelo.position, radioSuelo);
        }

        if (checkFrente != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(checkFrente.position,
                            checkFrente.position + Vector3.right * direccion * distanciaObstaculo);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaBot);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangoDeteccion * 2, 1f, 0));

        Gizmos.color = Color.cyan;
        if (checkBorde != null)
        {
            Gizmos.DrawLine(checkBorde.position,
                            checkBorde.position + (Vector3)(Vector2.down + Vector2.right * direccion * 0.5f) * rangoSaltoPlataforma);
        }

        Gizmos.color = Color.magenta;
        Vector3 origen = checkSuelo != null ? checkSuelo.position + Vector3.right * direccion * 0.6f : transform.position;
        Gizmos.DrawLine(origen, origen + Vector3.down * distanciaSeguridadVacio);
    }

    // --- Portales ---
    public void ActivarPortal(int nuevaDireccion)
    {
        forzarMovimiento = true;
        enPortal = true;
        Direccion = nuevaDireccion;
        rb.velocity = Vector2.zero;
    }

    public void DesactivarPortal()
    {
        forzarMovimiento = false;
        enPortal = false;
    }

    public void ReanudarMovimientoDespuesDe(float tiempo)
    {
        StartCoroutine(ReanudarDespuesDeTiempo(tiempo));
    }

    private IEnumerator ReanudarDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        DesactivarPortal();
    }
}
