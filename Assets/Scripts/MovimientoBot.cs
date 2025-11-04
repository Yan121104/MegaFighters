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
        get => direccion;
        set => direccion = Mathf.Clamp(value, -1, 1);
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
    private float tiempoMaxAtascado = 1.2f;
    private Vector2 ultimaPosicion;
    private float tiempoProximoIntento = 0f;

    [Header("Plataformas Móviles")]
    public LayerMask plataformaMovilLayer;
    public float rangoSaltoPlataforma = 2.5f;

    [Header("Seguridad ante vacío")]
    public float distanciaSeguridadVacio = 1.5f;
    private bool evitarCaida = false;

    [Header("Exploración Automática")]
    public float tiempoCambioDireccion = 3f;
    private float tiempoUltimoCambioDireccion = 0f;
    private float tiempoQuietoMax = 2.5f;
    private float tiempoQuietoActual = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning($"⚠️ No se encontró Animator en {gameObject.name}");

        velocidadActual = velocidadNormal;
        ultimaPosicion = transform.position;

        BuscarJugadorPrincipal();

        tiempoCambioDireccion += Random.Range(-1f, 1f);
        tiempoQuietoMax += Random.Range(-1f, 0.5f);
    }

    void BuscarJugadorPrincipal()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player != gameObject)
            jugadorObjetivo = player.transform;
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
        evitarCaida = !HaySueloSeguroDelante();

        if (jugadorEnRango)
            PerseguirJugador(distanciaX);
        else
            Patrullar();

        AplicarMovimiento();
        ActualizarAnimaciones();
        VerificarAtasco();
    }

    // --- PERSECUCIÓN ---
    private void PerseguirJugador(float distanciaX)
    {
        if (distanciaX > rangoAtaque)
        {
            int dirJugador = (jugadorObjetivo.position.x > transform.position.x) ? 1 : -1;

            if (!evitarCaida)
            {
                direccion = dirJugador;
                movimiento = direccion;
                velocidadActual = velocidadCorrer;
            }
            else
                movimiento = 0;
        }
        else
            movimiento = 0;
    }

    private void Patrullar()
    {
        velocidadActual = velocidadNormal;
        movimiento = direccion;

        RaycastHit2D obstaculo = Physics2D.Raycast(checkFrente.position, Vector2.right * direccion, distanciaObstaculo, obstaculoLayer);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector3 bordeOffset = new Vector3((col.bounds.extents.x + 0.05f) * direccion, 0, 0);
        bool bordeSeguro = Physics2D.Raycast(checkSuelo.position + bordeOffset, Vector2.down, distanciaBorde, sueloLayer);

        // Si no hay suelo o hay un obstáculo, decide qué hacer
        if (!forzarMovimiento)
        {
            if (!bordeSeguro && enSuelo)
            {
                if (!IntentarSaltarHaciaPlataforma())
                    CambiarDireccion();
            }

            if (obstaculo.collider != null && enSuelo)
            {
                if (Time.time > proximoSalto)
                {
                    Saltar();
                    proximoSalto = Time.time + cooldownSalto + Random.Range(0f, 2f);
                }
            }

            // Si se topa con otro bot
            DetectarYSepararBots();

            // --- 🧠 Exploración aleatoria ---
            if (Time.time - tiempoUltimoCambioDireccion > tiempoCambioDireccion)
            {
                // 30% de probabilidad de cambiar dirección aunque no haya obstáculo
                if (Random.value < 0.3f)
                    CambiarDireccion();

                tiempoUltimoCambioDireccion = Time.time;
            }

            // --- Si está demasiado quieto, forzar movimiento ---
            if (Mathf.Abs(rb.velocity.x) < 0.1f && enSuelo)
            {
                tiempoQuietoActual += Time.deltaTime;
                if (tiempoQuietoActual > tiempoQuietoMax)
                {
                    // Cambiar dirección o saltar aleatoriamente
                    if (Random.value < 0.5f)
                        CambiarDireccion();
                    else
                        Saltar();

                    tiempoQuietoActual = 0f;
                }
            }
            else
            {
                tiempoQuietoActual = 0f;
            }
        }
    }

    private void AplicarMovimiento()
    {
        Vector2 velBase = new Vector2(movimiento * velocidadActual, rb.velocity.y);
        Vector2 velPlataforma = enSuelo ? plataformaVelocidad : ultimaInerciaPlataforma;
        rb.velocity = new Vector2(velBase.x + velPlataforma.x, velBase.y);

        transform.localScale = new Vector3(
            direccion * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        animator.SetFloat("Velocidad", Mathf.Abs(rb.velocity.x));
        animator.SetBool("Saltando", !enSuelo);
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
        return sueloDetectado.collider != null;
    }

    private void Saltar()
    {
        ultimaInerciaPlataforma = plataformaVelocidad;
        rb.velocity = new Vector2(rb.velocity.x + ultimaInerciaPlataforma.x, 0)
                      + Vector2.up * fuerzaSalto;
    }

    private void CambiarDireccion() => direccion *= -1;

    private void DetectarYSepararBots()
    {
        Collider2D botCercano = Physics2D.OverlapCircle(transform.position, distanciaBot, botLayer);
        if (botCercano != null && botCercano.gameObject != gameObject && Time.time > tiempoProximoIntento)
        {
            CambiarDireccion();
            tiempoProximoIntento = Time.time + 0.8f;
        }
    }

    private void VerificarAtasco()
    {
        float distancia = Vector2.Distance(transform.position, ultimaPosicion);

        if (distancia < 0.05f)
        {
            tiempoAtascado += Time.deltaTime;
            if (tiempoAtascado > tiempoMaxAtascado && enSuelo)
            {
                if (Random.value < 0.7f)
                    CambiarDireccion();
                else if (Time.time > proximoSalto)
                {
                    Saltar();
                    proximoSalto = Time.time + cooldownSalto + Random.Range(0.5f, 1.5f);
                }

                tiempoAtascado = 0f;
            }
        }
        else
            tiempoAtascado = 0f;

        ultimaPosicion = transform.position;
    }

    // --- Colisiones ---
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!forzarMovimiento && ((1 << other.gameObject.layer) & botLayer) != 0)
        {
            foreach (ContactPoint2D contacto in other.contacts)
            {
                if ((direccion == 1 && contacto.normal.x < -0.5f) ||
                    (direccion == -1 && contacto.normal.x > 0.5f))
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

    // --- Gizmos ---
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
            Gizmos.DrawLine(checkBorde.position,
                checkBorde.position + (Vector3)(Vector2.down + Vector2.right * direccion * 0.5f) * rangoSaltoPlataforma);

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

    // --- RECOGER ARMAS AUTOMÁTICAMENTE ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si el objeto es un arma
        if (collision.gameObject.GetComponent<WeaponPickup>() != null)
        {
            RecogerArmaBot(collision.gameObject);
        }
    }

    private void RecogerArmaBot(GameObject arma)
    {
        BotWeaponController wc = GetComponent<BotWeaponController>();
        if (wc == null) return;

        string nombreArma = arma.name.ToLower();

        // --- Detectar tipo de arma (usa el enum del bot) ---
        if (nombreArma.Contains("hacha"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Hacha);
        else if (nombreArma.Contains("sable"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Sable);
        else if (nombreArma.Contains("machete"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Machete);
        else if (nombreArma.Contains("basuca"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Basuca);
        else if (nombreArma.Contains("desert"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Desert);
        else if (nombreArma.Contains("escopeta"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Escopeta);
        else if (nombreArma.Contains("lanzallamas"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Lanzallamas);
        else if (nombreArma.Contains("m4a1"))
            wc.EquipWeapon(BotWeaponController.WeaponType.M4a1);
        else if (nombreArma.Contains("miniusi"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Miniusi);
        else if (nombreArma.Contains("usp"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Usp);
        else if (nombreArma.Contains("snyper"))
            wc.EquipWeapon(BotWeaponController.WeaponType.Snyper);

        // --- Efecto de sonido (opcional) ---
        WeaponPickup pickup = arma.GetComponent<WeaponPickup>();
        if (pickup != null && pickup.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickup.pickupSound, transform.position, 1f);
        }

        // Destruye el arma recogida
        Destroy(arma, 0.3f);

        Debug.Log($"🤖 {gameObject.name} recogió {arma.name}");
    }


}
