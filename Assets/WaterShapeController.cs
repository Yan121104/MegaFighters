using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class WaterShapeController : MonoBehaviour
{
    private int CornersCount = 2;

    [SerializeField] private SpriteShapeController spriteShapeController;
    [SerializeField] private GameObject wavePointPref;
    [SerializeField] private GameObject wavePoints;

    [SerializeField, Range(1, 100)]
    private int WavesCount = 10;

    private List<WaterSpring> springs = new();

    [Header("Propiedades del resorte")]
    public float springStiffness = 0.05f;
    public float dampening = 0.04f;
    public float spread = 0.003f;

    // Evita ejecución en editor
    void OnValidate()
    {
        if (!Application.isPlaying) return; // Solo actualizar en Play
        SafeRebuildWaves();
    }

    private void SafeRebuildWaves()
    {
        StopAllCoroutines();
        StartCoroutine(CreateWaves());
    }

    IEnumerator CreateWaves()
    {
        if (spriteShapeController == null || wavePoints == null) yield break;

        // Espera un frame para evitar conflictos con física o validación
        yield return null;

        foreach (Transform child in wavePoints.transform)
        {
            Destroy(child.gameObject); // ✅ Usa Destroy, no DestroyImmediate
        }

        yield return null;
        SetWaves();
    }

    private void SetWaves()
    {
        Spline waterSpline = spriteShapeController.spline;
        int waterPointsCount = waterSpline.GetPointCount();

        // Remover puntos del medio
        for (int i = CornersCount; i < waterPointsCount - CornersCount; i++)
        {
            waterSpline.RemovePointAt(CornersCount);
        }

        Vector3 waterTopLeftCorner = waterSpline.GetPosition(1);
        Vector3 waterTopRightCorner = waterSpline.GetPosition(2);
        float waterWidth = waterTopRightCorner.x - waterTopLeftCorner.x;

        float spacingPerWave = waterWidth / (WavesCount + 1);
        if (spacingPerWave < 0.01f)
        {
            Debug.LogWarning("Espaciado entre olas demasiado pequeño. Reduce WavesCount o aumenta el ancho del agua.");
            return;
        }

        // Insertar puntos de ola
        for (int i = WavesCount; i > 0; i--)
        {
            int index = CornersCount;
            float xPosition = waterTopLeftCorner.x + (spacingPerWave * i);
            Vector3 wavePoint = new Vector3(xPosition, waterTopLeftCorner.y, waterTopLeftCorner.z);
            waterSpline.InsertPointAt(index, wavePoint);
            waterSpline.SetHeight(index, 0.1f);
            waterSpline.SetCorner(index, false);
            waterSpline.SetTangentMode(index, ShapeTangentMode.Continuous);
        }

        springs.Clear();
        for (int i = 0; i <= WavesCount + 1; i++)
        {
            int index = i + 1;
            Smoothen(waterSpline, index);

            GameObject wavePoint = Instantiate(wavePointPref, wavePoints.transform, false);
            wavePoint.transform.localPosition = waterSpline.GetPosition(index);

            WaterSpring waterSpring = wavePoint.GetComponent<WaterSpring>();
            if (waterSpring != null)
                waterSpring.Init(spriteShapeController);

            springs.Add(waterSpring);
        }
    }

    private void Smoothen(Spline waterSpline, int index)
    {
        Vector3 position = waterSpline.GetPosition(index);
        Vector3 positionPrev = index > 1 ? waterSpline.GetPosition(index - 1) : position;
        Vector3 positionNext = (index - 1 <= WavesCount) ? waterSpline.GetPosition(index + 1) : position;
        Vector3 forward = transform.forward;

        float scale = Mathf.Min((positionNext - position).magnitude, (positionPrev - position).magnitude) * 0.33f;
        Vector3 leftTangent, rightTangent;
        SplineUtility.CalculateTangents(position, positionPrev, positionNext, forward, scale, out rightTangent, out leftTangent);

        waterSpline.SetLeftTangent(index, leftTangent);
        waterSpline.SetRightTangent(index, rightTangent);
    }

    void FixedUpdate()
    {
        if (springs == null || springs.Count == 0) return;

        foreach (WaterSpring waterSpringComponent in springs)
        {
            if (waterSpringComponent == null) continue;
            waterSpringComponent.WaveSpringUpdate(springStiffness, dampening);
            waterSpringComponent.WavePointUpdate();
        }

        UpdateSprings();
    }

    private void UpdateSprings()
    {
        int count = springs.Count;
        if (count == 0) return;

        float[] left_deltas = new float[count];
        float[] right_deltas = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                left_deltas[i] = spread * (springs[i].height - springs[i - 1].height);
                springs[i - 1].velocity += left_deltas[i];
            }
            if (i < count - 1)
            {
                right_deltas[i] = spread * (springs[i].height - springs[i + 1].height);
                springs[i + 1].velocity += right_deltas[i];
            }
        }
    }

    private void Splash(int index, float speed)
    {
        if (index >= 0 && index < springs.Count)
        {
            springs[index].velocity += speed;
        }
    }

    public void MakeSplashAtPosition(float xPosition, float speed)
    {
        if (springs == null || springs.Count == 0) return;

        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < springs.Count; i++)
        {
            float dist = Mathf.Abs(springs[i].transform.position.x - xPosition);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestIndex = i;
            }
        }

        Splash(nearestIndex, speed * 0.1f);
    }
}
