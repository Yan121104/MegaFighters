using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterCollision : MonoBehaviour
{
    public WaterShapeController waterController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Jugador o bot
        {
            Rigidbody2D rb = collision.attachedRigidbody;
            if (rb != null)
            {
                float xPos = collision.transform.position.x;
                waterController.MakeSplashAtPosition(xPos, rb.velocity.y * 0.5f);
            }

            // 💧 Obtener el sistema de vida del que cayó al agua
            HealthSystem hs = collision.GetComponent<HealthSystem>();
            if (hs != null && !hs.isDead)
            {
                Debug.Log($"🌊 {collision.name} cayó al agua y murió instantáneamente.");
                hs.TakeDamage(hs.maxHealth, gameObject); // Mata instantáneamente
            }
        }
    }
}
