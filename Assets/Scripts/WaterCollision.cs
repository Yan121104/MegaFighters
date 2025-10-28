using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterCollision : MonoBehaviour
{
    public WaterShapeController waterController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Aseg�rate que tu personaje tenga el tag Player
        {
            Rigidbody2D rb = collision.attachedRigidbody;
            if (rb != null)
            {
                // Tomamos la posici�n horizontal del impacto
                float xPos = collision.transform.position.x;

                // Llamamos al splash en ese punto con fuerza proporcional a la velocidad de ca�da
                waterController.MakeSplashAtPosition(xPos, rb.velocity.y * 0.5f);
            }
        }
    }
}
