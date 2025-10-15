using UnityEngine;

public class BulletLife : MonoBehaviour
{
    public float lifetime = 2f;
    [HideInInspector] public GameObject shooter; // 🔫 el que disparó
    public int damage = 10; // daño base

    void Start()
    {
        Destroy(gameObject, lifetime); // seguridad si nunca choca
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Evitar colisión con quien disparó
        if (collision.gameObject == shooter) return;

        // Solo daña a los objetos con tag "Player"
        if (collision.CompareTag("Player"))
        {
            HealthSystem health = collision.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage, shooter);
            }
        }

        // Se destruye al impactar
        Destroy(gameObject);
    }
}
