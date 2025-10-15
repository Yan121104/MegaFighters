using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, GameObject attacker)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibi� {amount} de da�o de {attacker.name} (Vida: {currentHealth})");

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    void Die(GameObject killer)
    {
        Debug.Log($"{gameObject.name} fue eliminado por {killer.name}");
        Destroy(gameObject);
    }
}
