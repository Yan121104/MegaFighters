using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class AnimarOlas : MonoBehaviour
{
    [Header("Configuración de la ola")]
    public float amplitud = 0.5f;       // Altura de las olas
    public float longitudOnda = 2f;     // Distancia entre crestas
    public float velocidad = 1f;        // Velocidad del movimiento

    private SpriteShapeController shape;
    private float[] faseInicial;

    void Start()
    {
        shape = GetComponent<SpriteShapeController>();

        // Guardar fase inicial de cada punto
        faseInicial = new float[shape.spline.GetPointCount()];
        for (int i = 0; i < faseInicial.Length; i++)
            faseInicial[i] = Random.Range(0f, Mathf.PI * 2);
    }

    void Update()
    {
        int puntos = shape.spline.GetPointCount();

        for (int i = 0; i < puntos; i++)
        {
            Vector3 pos = shape.spline.GetPosition(i);
            float offsetY = Mathf.Sin(Time.time * velocidad + (pos.x / longitudOnda) + faseInicial[i]) * amplitud;
            shape.spline.SetPosition(i, new Vector3(pos.x, offsetY, pos.z));
        }

        shape.BakeCollider(); // Opcional si usas collider con el agua
    }
}
