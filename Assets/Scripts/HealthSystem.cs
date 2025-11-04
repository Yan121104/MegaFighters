using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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

    [Header("Transición de Nivel")]
    [Tooltip("Imagen negra para el fundido al cambiar de nivel.")]
    public CanvasGroup fadeImage;

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
        // Si está muerto, solo escucha ENTER para reiniciar
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

            // 🔒 Desactivar control del jugador
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this && script.enabled && script.GetType().Name.Contains("Movimiento"))
                {
                    script.enabled = false; // Desactiva scripts de movimiento
                }
            }

            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER\nPresiona ENTER para reiniciar";
                gameOverText.gameObject.SetActive(true);
            }
        }


        Debug.Log($"☠️ {gameObject.name} fue eliminado por {killer.name}");

        if (deathSound != null)
            deathSound.Play();

        if (animator != null)
            animator.SetTrigger("Die");

        if (playerCanvasHealthBar != null)
        {
            Debug.Log("💀 El jugador principal ha muerto. (GAME OVER)");

            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER\nPresiona ENTER para reiniciar";
                gameOverText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"🤖 Bot eliminado: {gameObject.name}");

            CheckAllBotsDead(); // ✅ Se ejecuta mientras el bot aún existe

            Destroy(gameObject, 0.5f); // luego lo destruyes
        }
    }

    // ← Detección del agua (Layer Water)
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
            Debug.Log("🎯 Todos los bots han sido eliminados. Iniciando fundido antes de cargar 'Nivel2'...");

            // 🧩 Buscar al jugador principal (el que tiene la barra en el Canvas)
            HealthSystem player = null;
            foreach (GameObject p in allPlayers)
            {
                HealthSystem hs = p.GetComponent<HealthSystem>();
                if (hs != null && hs.playerCanvasHealthBar != null)
                {
                    player = hs;
                    break;
                }
            }

            // 🩵 Si se encontró el jugador, usa su fadeImage
            if (player != null && player.fadeImage != null)
            {
                player.StartCoroutine(player.FadeAndLoadScene("Nivel2"));
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró la imagen de fundido en el jugador. Cargando escena directamente.");
                SceneManager.LoadScene("Nivel2");
            }
        }
    }


    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("⚠️ No se asignó la imagen de fundido (fadeImage). Cargando escena directamente.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        // 👇 Evita que se destruya el fade al cambiar de escena
        DontDestroyOnLoad(fadeImage.transform.root.gameObject);

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeImage.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneName);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}
