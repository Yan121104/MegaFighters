using UnityEngine;
using TMPro;

public class InicioNivel : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject textoInicio;       // Texto UI (TMP o Text)
    public GameObject jugador;           // Objeto jugador que tiene el script Movimiento
    public AudioSource audioSource;      // AudioSource en este objeto

    [Header("Sonidos")]
    public AudioClip sonidoEspera;       // Loop mientras se espera
    public AudioClip sonidoInicio;       // Sonido al iniciar

    [Header("Efectos visuales")]
    public bool parpadeoTexto = true;
    public float velocidadParpadeo = 2f;

    private bool juegoIniciado = false;
    private TextMeshProUGUI tmp;
    private UnityEngine.UI.Text uiText;
    private Animator animatorJugador;

    public static bool banderaJuegoIniciado = false; // 🔥 bandera global

    void Awake()
    {
        // Detectar tipo de texto
        if (textoInicio != null)
        {
            tmp = textoInicio.GetComponent<TextMeshProUGUI>();
            uiText = textoInicio.GetComponent<UnityEngine.UI.Text>();
        }

        // Obtener animador del jugador
        if (jugador != null)
            animatorJugador = jugador.GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Mostrar texto
        if (textoInicio) textoInicio.SetActive(true);

        // 🔒 Desactivar movimiento del jugador
        if (jugador)
        {
            var scriptMovimiento = jugador.GetComponent("Movimiento") as MonoBehaviour;
            if (scriptMovimiento != null) scriptMovimiento.enabled = false;
        }

        // 🔊 Sonido de espera
        if (audioSource != null && sonidoEspera != null)
        {
            audioSource.clip = sonidoEspera;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 🧍‍♂️ Forzar animación Idle
        if (animatorJugador != null)
        {
            animatorJugador.SetFloat("Velocidad", 0f);
            animatorJugador.SetBool("Saltando", false);
            animatorJugador.SetBool("EnEscalera", false);
        }
    }

    void Update()
    {
        if (!juegoIniciado && Input.GetKeyDown(KeyCode.Return))
        {
            IniciarJuego();
        }

        // ✨ Parpadeo
        if (!juegoIniciado && parpadeoTexto && textoInicio != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * velocidadParpadeo));
            if (tmp != null)
            {
                var c = tmp.color; c.a = alpha; tmp.color = c;
            }
            else if (uiText != null)
            {
                var c = uiText.color; c.a = alpha; uiText.color = c;
            }
        }
    }

    void IniciarJuego()
    {
        juegoIniciado = true;
        banderaJuegoIniciado = true; // 🔥 activa la bandera global

        if (textoInicio) textoInicio.SetActive(false);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (audioSource != null && sonidoInicio != null)
            audioSource.PlayOneShot(sonidoInicio);

        if (jugador)
        {
            var scriptMovimiento = jugador.GetComponent("Movimiento") as MonoBehaviour;
            if (scriptMovimiento != null) scriptMovimiento.enabled = true;
        }

        Debug.Log("🎮 Juego iniciado.");
    }
}
