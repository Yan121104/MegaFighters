using UnityEngine;

public class BulletLife : MonoBehaviour
{
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime); // seguridad por si nunca choca
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject); // 💥 se destruye al instante
    }
}
