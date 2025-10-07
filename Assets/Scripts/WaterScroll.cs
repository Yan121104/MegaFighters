using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public float speed = 0.5f; // Velocidad del movimiento
    private Vector3 startPos;
    private float length;

    void Start()
    {
        // Guardamos la posición inicial
        startPos = transform.position;

        // Calculamos el ancho del sprite (basado en el tamaño visible)
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Calculamos el movimiento horizontal
        float temp = Mathf.Repeat(Time.time * speed, length);
        transform.position = startPos + Vector3.left * temp;
    }
}
