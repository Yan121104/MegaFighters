using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Jugador a seguir
    public float smoothSpeed = 0.125f; // Suavizado del movimiento
    public Vector3 offset; // Desplazamiento de cámara
    public float minX, maxX, minY, maxY; // Límites del escenario

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 🔍 Zoom (aumenta o reduce el tamaño del área visible)
        cam.orthographicSize = 2.5f; // Valor menor = más cerca; mayor = más alejado
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calcula posición deseada con desplazamiento
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Limita la cámara dentro del escenario
        float clampedX = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        // Actualiza posición
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
