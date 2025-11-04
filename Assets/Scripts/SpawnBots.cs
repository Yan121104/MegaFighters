using UnityEngine;
using System.Collections.Generic;

public class SpawnBots : MonoBehaviour
{
    [Header("Prefab del Bot")]
    public GameObject botPrefab;

    [Header("Cantidad de Bots")]
    [Range(1, 20)]
    public int cantidadBots = 5;

    [Header("Configuración de plataformas")]
    public LayerMask sueloLayer;
    public float alturaOffset = 0.6f; // Cuánto encima del suelo aparece el bot
    public float margenBorde = 0.5f;  // Margen para no caer del borde
    public Vector2 rangoBusqueda = new Vector2(100f, 100f); // Área donde buscar plataformas

    private readonly Collider2D[] bufferPlataformas = new Collider2D[100]; // buffer fijo (sin asignación dinámica)

    void Start()
    {
        GenerarBotsEnPlataformas();
    }

    void GenerarBotsEnPlataformas()
    {
        int numPlataformas = Physics2D.OverlapBoxNonAlloc(Vector2.zero, rangoBusqueda, 0f, bufferPlataformas, sueloLayer);

        if (numPlataformas == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron plataformas en el layer 'Suelo'.");
            return;
        }

        for (int i = 0; i < cantidadBots; i++)
        {
            Collider2D plataforma = bufferPlataformas[Random.Range(0, numPlataformas)];
            if (plataforma == null) continue;

            Bounds b = plataforma.bounds;
            float x = Random.Range(b.min.x + margenBorde, b.max.x - margenBorde);
            float y = b.max.y + alturaOffset;

            Vector2 spawnPos = new Vector2(x, y);

            // Instanciar el bot
            GameObject nuevoBot = Instantiate(botPrefab, spawnPos, Quaternion.identity);

            // Asignar etiqueta
            BotIdentifier id = nuevoBot.GetComponent<BotIdentifier>();
            if (id != null)
            {
                string etiqueta = "B" + (i + 1); // Ejemplo: B1, B2, B3...
                id.SetEtiqueta(etiqueta);
            }
        }

        Debug.Log($"✅ Se generaron {cantidadBots} bots en {numPlataformas} plataformas detectadas.");
    }

    // Mostrar las plataformas detectadas en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector2.zero, rangoBusqueda);

        Collider2D[] plataformas = Physics2D.OverlapBoxAll(Vector2.zero, rangoBusqueda, 0f, sueloLayer);
        foreach (Collider2D c in plataformas)
        {
            if (c != null)
                Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
        }
    }
}
