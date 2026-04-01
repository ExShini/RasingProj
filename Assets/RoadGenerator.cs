using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SmoothRoadGenerator : MonoBehaviour
{
    public List<Transform> waypoints;
    public float roadWidth = 2f;
    public float curbHeight = 0.3f;
    [Range(2, 50)] public int resolution = 15;

    [Header("Generated Data")]
    public Vector3[] generatedPoints; // Центральные точки плавного пути

    [Header("Environment Settings")]
    public GameObject[] envPrefabs; // Список префабов (деревья, камни, знаки)
    public float spawnProbability = 0.2f; // Вероятность спавна в каждой точке (0..1)
    public float distanceFromCenter = 1.5f; // На каком расстоянии от центра дороги ставить
    public Transform propsContainer; // Сюда будут складываться объекты


    private void OnValidate() => Build(); // Авто-обновление при изменении параметров



    [ContextMenu("Clear & Spawn Environment")]
    public void SpawnEnvironment()
    {
        // Очистка старых объектов
        if (propsContainer == null)
        {
            propsContainer = new GameObject("Props").transform;
            propsContainer.SetParent(transform);
        }
        for (int i = propsContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(propsContainer.GetChild(i).gameObject);

        if (envPrefabs == null || envPrefabs.Length == 0 || generatedPoints == null) return;

        int count = waypoints.Count;
        int totalSteps = count * resolution;

        for (int i = 0; i < totalSteps; i++)
        {
            if (Random.value > spawnProbability) continue;

            // Вычисляем позицию и ориентацию
            Vector3 pos = generatedPoints[i];

            // Находим направление (тангенс) для текущей точки
            int nextIdx = (i + 1) % totalSteps;
            Vector3 tangent = (generatedPoints[nextIdx] - pos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            // Выбираем сторону (лево/право) и случайный префаб
            float side = Random.value > 0.5f ? 1f : -1f;
            GameObject prefab = envPrefabs[Random.Range(0, envPrefabs.Length)];

            // Спавним объект
            Vector3 spawnPos = pos + right * (distanceFromCenter * side);
            GameObject prop = Instantiate(prefab, spawnPos, Quaternion.LookRotation(tangent), propsContainer);

            // Небольшой рандом поворота и размера для естественности
            prop.transform.Rotate(0, Random.Range(0, 360f), 0);
            prop.transform.localScale *= Random.Range(0.8f, 1.2f);
        }
    }



    [ContextMenu("Generate Smooth Road")]
    public void Build()
    {
        if (waypoints == null || waypoints.Count < 3) return;

        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> centerPath = new List<Vector3>();

        int count = waypoints.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 p0 = waypoints[(i + count - 1) % count].position;
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[(i + 1) % count].position;
            Vector3 p3 = waypoints[(i + 2) % count].position;

            for (int step = 0; step < resolution; step++)
            {
                float t = (float)step / resolution;
                Vector3 posWorld = GetCatmullRomPoint(p0, p1, p2, p3, t);
                Vector3 posLocal = posWorld - transform.position;

                centerPath.Add(posWorld); // Сохраняем мировую позицию центра

                Vector3 tangent = GetCatmullRomTangent(p0, p1, p2, p3, t);
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

                // Вершины
                verts.Add(posLocal - right * roadWidth);                   // 0: Лево низ
                verts.Add(posLocal + right * roadWidth);                   // 1: Право низ
                verts.Add(posLocal - right * roadWidth + Vector3.up * curbHeight); // 2: Лево верх
                verts.Add(posLocal + right * roadWidth + Vector3.up * curbHeight); // 3: Право верх

                if (verts.Count > 4)
                {
                    int curr = verts.Count - 4;
                    int prev = curr - 4;
                    AddQuad(tris, prev + 0, prev + 1, curr + 0, curr + 1); // Полотно
                    AddQuad(tris, prev + 2, prev + 3, curr + 2, curr + 3); // Верх бордюра
                    AddQuad(tris, prev + 0, prev + 2, curr + 0, curr + 2); // Стенка лево
                    AddQuad(tris, prev + 3, prev + 1, curr + 3, curr + 1); // Стенка право
                }
            }
        }

        // Замыкание кольца
        int last = verts.Count - 4;
        AddQuad(tris, last + 0, last + 1, 0, 1);
        AddQuad(tris, last + 2, last + 3, 2, 3);
        AddQuad(tris, last + 0, last + 2, 0, 2);
        AddQuad(tris, last + 3, last + 1, 3, 1);

        generatedPoints = centerPath.ToArray();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void AddQuad(List<int> tris, int v0, int v1, int v2, int v3)
    {
        tris.Add(v0); tris.Add(v2); tris.Add(v1);
        tris.Add(v1); tris.Add(v2); tris.Add(v3);
    }

    Vector3 GetCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) =>
        0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);

    Vector3 GetCatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) =>
        (0.5f * (-p0 + p2 + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t)).normalized;

    // Отрисовка пути в редакторе (зеленая линия)
    private void OnDrawGizmos()
    {
        if (generatedPoints == null || generatedPoints.Length < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < generatedPoints.Length - 1; i++)
            Gizmos.DrawLine(generatedPoints[i], generatedPoints[i + 1]);
        Gizmos.DrawLine(generatedPoints[generatedPoints.Length - 1], generatedPoints[0]);
    }
}
