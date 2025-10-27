using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    private int currentHealth;

    private HealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;

        // Buscar la barra de vida dentro del prefab del bot
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
            healthBar.Initialize(maxHealth);
    }

    /// <summary>
    /// Aplica daño al bot y actualiza su barra de vida.
    /// </summary>
    public void TakeDamage(int amount, GameObject attacker)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{gameObject.name} recibió {amount} de daño de {attacker.name} (Vida: {currentHealth}/{maxHealth})");

        // Actualizar barra de vida
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    /// <summary>
    /// Restaura completamente la salud del bot (útil al respawnear).
    /// </summary>
    public void RestoreHealth()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }

    void Die(GameObject killer)
    {
        Debug.Log($"{gameObject.name} fue eliminado por {killer.name}");
        Destroy(gameObject);
    }
}
