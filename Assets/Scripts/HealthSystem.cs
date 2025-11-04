using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class HealthSystem : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    private int currentHealth;

    private HealthBar healthBar;
    private Rigidbody2D rb;

    [Header("Jugador Principal")]
    [Tooltip("Asigna aquí la barra de vida del Canvas solo para el jugador principal.")]
    public HealthBar playerCanvasHealthBar;

    [Header("Daño por Caída")]
    [Tooltip("Velocidad mínima (negativa) para comenzar a recibir daño por caída.")]
    public float fallDamageThreshold = -4f;
    [Tooltip("Multiplicador del daño basado en la velocidad de caída.")]
    public float damageMultiplier = 6f;
    [Tooltip("Layer del suelo para detectar aterrizajes.")]
    public LayerMask groundLayer;

    private bool isGrounded;
    private float lastYVelocity;

    [Header("Efectos opcionales")]
    public AudioSource damageSound;
    public AudioSource deathSound;
    public Animator animator;

    [Header("UI Game Over")]
    [Tooltip("Texto que se mostrará cuando el jugador muera (usar TextMeshProUGUI).")]
    public TextMeshProUGUI gameOverText;

    public bool isDead { get; private set; } = false;
    private bool controlsDisabled = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        // Determinar qué barra de vida usar
        healthBar = playerCanvasHealthBar != null
            ? playerCanvasHealthBar
            : GetComponentInChildren<HealthBar>();

        if (healthBar != null)
            healthBar.Initialize(maxHealth);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        lastYVelocity = rb.velocity.y;
        bool groundedNow = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);

        // Daño por caída
        if (!isGrounded && groundedNow && lastYVelocity < fallDamageThreshold)
        {
            int fallDamage = Mathf.RoundToInt(Mathf.Abs(lastYVelocity) * damageMultiplier);
            TakeDamage(fallDamage, gameObject);
            Debug.Log($"💥 {gameObject.name} recibió {fallDamage} de daño por caída (Velocidad: {lastYVelocity:F2})");
        }

        // Daño manual de prueba
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10, gameObject);

        isGrounded = groundedNow;
    }

    public void TakeDamage(int amount, GameObject attacker)
    {
        if (amount <= 0 || isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (damageSound != null)
            damageSound.Play();

        if (animator != null)
            animator.SetTrigger("Hurt");

        Debug.Log($"🩸 {gameObject.name} recibió {amount} de daño de {attacker.name} (Vida: {currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
            Die(attacker);
    }

    public void RestoreHealth()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        Debug.Log($"💚 {gameObject.name} restauró su vida al máximo ({maxHealth}).");
    }

    public void Die(GameObject killer)
    {
        if (isDead) return;
        isDead = true;
        controlsDisabled = true;

        if (playerCanvasHealthBar != null)
        {
            Debug.Log("💀 El jugador principal ha muerto. (GAME OVER)");

            // Desactivar scripts de movimiento
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this && script.enabled && script.GetType().Name.Contains("Movimiento"))
                    script.enabled = false;
            }

            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER\nPresiona ENTER para reiniciar";
                gameOverText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"🤖 Bot eliminado: {gameObject.name}");
            CheckAllBotsDead();
            Destroy(gameObject, 0.5f);
        }

        if (deathSound != null)
            deathSound.Play();

        if (animator != null)
            animator.SetTrigger("Die");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDead && collision.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log("🌊 El jugador cayó al agua. GAME OVER");
            TakeDamage(currentHealth, gameObject);
        }
    }

    public bool ControlsDisabled() => controlsDisabled;

    private void CheckAllBotsDead()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        int botsVivos = 0;

        foreach (GameObject p in allPlayers)
        {
            HealthSystem hs = p.GetComponent<HealthSystem>();
            if (hs != null && !hs.isDead && hs.playerCanvasHealthBar == null)
                botsVivos++;
        }

        if (botsVivos == 0)
        {
            Debug.Log("🎯 Todos los bots han sido eliminados. Iniciando transición a Nivel2...");

            // Usar FadeManager global para cargar escena
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene("Nivel2", 2f, 2f, 0.5f);
            }
            else
            {
                SceneManager.LoadScene("Nivel2");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}
