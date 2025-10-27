using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletLife : MonoBehaviour
{
    public float lifetime = 2f;
    [HideInInspector] public GameObject shooter;
    public int damage = 10;
    public bool isFlamethrower = false;

    private bool hasDealtDamage = false;
    private bool isOnGround = false;
    private Rigidbody2D rb;
    private ParticleSystem ps;

    // 🔥 Control de daño periódico
    private float burnInterval = 0.5f; // cada cuánto tiempo causa daño
    private List<GameObject> playersInside = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ps = GetComponent<ParticleSystem>();

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Evita dañar al propio jugador que disparó
        if (collision.gameObject == shooter) return;

        // Si toca un jugador
        if (collision.CompareTag("Player"))
        {
            // Si aún está volando (no en el suelo)
            if (!isOnGround && !hasDealtDamage)
            {
                HealthSystem health = collision.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(damage, shooter);
                    hasDealtDamage = true;
                }

                // No se destruye si es lanzallamas
                if (isFlamethrower) return;
            }

            // Si ya está ardiendo en el suelo, empieza el daño periódico
            if (isOnGround && !playersInside.Contains(collision.gameObject))
            {
                playersInside.Add(collision.gameObject);
                StartCoroutine(DamageOverTime(collision.gameObject));
            }
        }

        // Si no es lanzallamas, se destruye al chocar
        if (!isFlamethrower)
            Destroy(gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playersInside.Contains(collision.gameObject))
        {
            playersInside.Remove(collision.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 🧱 Si toca el suelo (layer "Suelo"), se queda ardiendo
        if (isFlamethrower && collision.gameObject.layer == LayerMask.NameToLayer("Suelo") && !isOnGround)
        {
            isOnGround = true;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.isKinematic = true;
            }

            StartCoroutine(BurnOnGround());
        }
    }

    IEnumerator BurnOnGround()
    {
        float burnTime = Random.Range(2.5f, 4f); // cuánto dura ardiendo

        if (ps != null)
        {
            var main = ps.main;
            main.startSize = Random.Range(0.12f, 0.2f);
            main.startLifetime = burnTime;
            ps.Play();
        }

        yield return new WaitForSeconds(burnTime);
        Destroy(gameObject);
    }

    IEnumerator DamageOverTime(GameObject player)
    {
        while (playersInside.Contains(player))
        {
            if (player != null)
            {
                HealthSystem health = player.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(Mathf.RoundToInt(damage * 0.5f), shooter);
                }
            }
            yield return new WaitForSeconds(burnInterval);
        }
    }
}
