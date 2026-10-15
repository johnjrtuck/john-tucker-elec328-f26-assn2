using System.Collections.Generic;
using UnityEngine;

// Decorative background flourish -- NOT one of your three required
// deformations, just atmosphere. Spawns lumpy, randomly-shaped rocks that
// tumble in place and drift slowly across the background, looping back
// around once they drift far enough away.
public class AsteroidField : MonoBehaviour
{
    [Header("Field shape")]
    public int asteroidCount = 20;
    public float minDistance = 15f;
    public float maxDistance = 30f;

    [Header("Asteroid look")]
    public int resolution = 8;
    public float minRadius = 0.3f;
    public float maxRadius = 1.2f;
    public float bumpiness = 0.35f;
    public Material asteroidMaterial;

    [Header("Motion")]
    public float minDriftSpeed = 0.2f;
    public float maxDriftSpeed = 1f;
    public float minSpinSpeed = 10f;
    public float maxSpinSpeed = 60f;

    [Header("Crumbling")]
    public float minCrumbleDuration = 10f;
    public float maxCrumbleDuration = 25f;
    [Range(0.05f, 0.9f)] public float collapseFraction = 0.25f; // how much of its timeline a vertex takes to fully collapse once its turn starts

    class Asteroid
    {
        public Transform transform;
        public Vector3 driftDirection;
        public float driftSpeed;
        public Vector3 spinAxis;
        public float spinSpeed;

        // Crumbling
        public MeshFilter meshFilter;
        public Mesh mesh;
        public Vector3[] baseVerts;       // full, uncrumbled shape
        public float[] crumbleThreshold;  // per-vertex, 0-1, when that vertex starts collapsing
        public float radius;
        public float age;
        public float crumbleDuration;     // seconds until fully crumbled
    }

    List<Asteroid> asteroids = new List<Asteroid>();

    void Start()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            SpawnAsteroid();
        }
    }

    void SpawnAsteroid()
    {
        GameObject go = new GameObject("Asteroid");
        go.transform.SetParent(transform);
        go.transform.position = RandomPointInShell();

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        if (asteroidMaterial != null) mr.material = asteroidMaterial;

        Asteroid a = new Asteroid();
        a.transform = go.transform;
        a.meshFilter = mf;
        a.driftDirection = Random.onUnitSphere;
        a.driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed);
        a.spinAxis = Random.onUnitSphere;
        a.spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);

        InitAsteroidShape(a);
        asteroids.Add(a);
    }

    void InitAsteroidShape(Asteroid a)
    {
        a.radius = Random.Range(minRadius, maxRadius);
        Vector3[] cubeVerts = CubeSphereUtility.GenerateCubeVertices(resolution, a.radius);
        Vector3[] sphereVerts = CubeSphereUtility.GenerateSphereFromCube(cubeVerts, a.radius);
        a.baseVerts = MakeLumpy(sphereVerts, a.radius);
        int[] triangles = CubeSphereUtility.GenerateCubeTriangles(resolution);

        a.crumbleThreshold = new float[a.baseVerts.Length];
        for (int i = 0; i < a.crumbleThreshold.Length; i++)
            a.crumbleThreshold[i] = Random.Range(0f, 1f - collapseFraction);

        a.age = 0f;
        a.crumbleDuration = Random.Range(minCrumbleDuration, maxCrumbleDuration);

        Mesh mesh = new Mesh();
        mesh.name = "Asteroid";
        mesh.vertices = a.baseVerts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        a.mesh = mesh;
        a.meshFilter.mesh = mesh;
    }

    Vector3[] MakeLumpy(Vector3[] baseVerts, float radius)
    {
        Vector3[] result = new Vector3[baseVerts.Length];
        Vector3 seedOffset = new Vector3(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Vector3 dir = baseVerts[i].normalized;
            Vector3 samplePoint = dir * 2f + seedOffset;
            float n1 = Mathf.PerlinNoise(samplePoint.x, samplePoint.y);
            float n2 = Mathf.PerlinNoise(samplePoint.y, samplePoint.z);
            float noise = (n1 + n2) * 0.5f - 0.5f;
            result[i] = dir * (radius + noise * radius * bumpiness);
        }

        return result;
    }

    Vector3 RandomPointInShell()
    {
        Vector3 dir = Random.onUnitSphere;
        float dist = Random.Range(minDistance, maxDistance);
        return transform.position + dir * dist;
    }

    void Update()
    {
        foreach (Asteroid a in asteroids)
        {
            a.transform.position += a.driftDirection * a.driftSpeed * Time.deltaTime;
            a.transform.Rotate(a.spinAxis, a.spinSpeed * Time.deltaTime, Space.World);

            a.age += Time.deltaTime;
            ApplyCrumble(a);

            float dist = Vector3.Distance(a.transform.position, transform.position);
            bool driftedAway = dist > maxDistance * 1.5f;
            bool fullyCrumbled = a.age >= a.crumbleDuration;

            if (driftedAway) a.transform.position = RandomPointInShell();
            if (fullyCrumbled) InitAsteroidShape(a); // regrow a fresh asteroid in place
        }
    }

    void ApplyCrumble(Asteroid a)
    {
        float t = Mathf.Clamp01(a.age / a.crumbleDuration);
        Vector3[] verts = new Vector3[a.baseVerts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            float localT = Mathf.Clamp01((t - a.crumbleThreshold[i]) / collapseFraction);
            // collapsed target: pulled in toward the core along its own normal
            Vector3 dir = a.baseVerts[i].normalized;
            Vector3 collapsedPos = dir * (a.radius * 0.05f);
            verts[i] = Vector3.Lerp(a.baseVerts[i], collapsedPos, localT);
        }

        a.mesh.vertices = verts;
        a.mesh.RecalculateNormals();
    }
}