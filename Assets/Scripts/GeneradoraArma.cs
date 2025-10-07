using UnityEngine;
using System.Collections;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Prefabs de armas")]
    public GameObject[] armasPrefabs;

    [Header("Puntos de spawn")]
    public Transform[] plataformas;

    [Header("Configuración de tiempo")]
    public float tiempoMin = 3f;
    public float tiempoMax = 7f;

    // Array para guardar el arma que está en cada plataforma
    private GameObject[] armasEnPlataformas;

    private void Start()
    {
        armasEnPlataformas = new GameObject[plataformas.Length];
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float espera = Random.Range(tiempoMin, tiempoMax);
            yield return new WaitForSeconds(espera);

            // Elige una plataforma aleatoria
            int indicePlataforma = Random.Range(0, plataformas.Length);

            // Si ya hay un arma en esa plataforma → no hacer nada
            if (armasEnPlataformas[indicePlataforma] != null)
                continue;

            // Elegir un arma aleatoria
            GameObject armaPrefab = armasPrefabs[Random.Range(0, armasPrefabs.Length)];

            // Crear arma
            Vector3 posicionSpawn = plataformas[indicePlataforma].position + Vector3.up * 0.5f;
            GameObject nuevaArma = Instantiate(armaPrefab, posicionSpawn, Quaternion.identity);

            // Guardar referencia en el array
            armasEnPlataformas[indicePlataforma] = nuevaArma;

            // Cuando el arma desaparezca (destruida o recogida), liberar el slot
            StartCoroutine(LiberarPlataformaCuandoDestruida(indicePlataforma, nuevaArma));
        }
    }

    private IEnumerator LiberarPlataformaCuandoDestruida(int indice, GameObject arma)
    {
        // Espera hasta que el arma sea destruida
        while (arma != null)
        {
            yield return null;
        }
        armasEnPlataformas[indice] = null;
    }
}
