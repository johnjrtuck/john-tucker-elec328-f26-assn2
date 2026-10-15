using UnityEngine;

// Deformation 2: CONTINUOUS / TIME-BASED
// A single sphere continuously blends its actual VERTEX geometry between
// a sun shape and a moon shape over time.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SunMoonCycle : MonoBehaviour
{
    [Header("Sphere resolution")]
    public int meridians = 28;
    public int parallels = 18;
    public float radius = 1f;

    [Header("Cycle")]
    public float cycleSpeed = 0.5f;

    [Header("Sun tendrils")]
    public int tendrilCount = 10;
    public float tendrilLength = 0.4f;
    [Range(0.5f, 0.98f)] public float tendrilSharpness = 0.85f;

    [Header("Moon crescent")]
    public float crescentOffset = 0.55f;
    public float crescentBiteRadius = 1f;

    [Header("Optional color tint")]
    public bool tintMaterial = true;
    public Color sunColor = new Color(1f, 0.8f, 0.3f);
    public Color moonColor = new Color(0.6f, 0.65f, 0.75f);

    [Header("Orbit")]
    public Transform orbitCenter; // Leave empty to orbit world origin
    public float orbitRadius = 10f; // Distance from the orbit center
    public float orbitSpeed = 30f; // Degrees per second
    [Tooltip("Axis to orbit around. Vector3.up (0,1,0) = flat circle around " +
        "the player at constant height (old behavior). Vector3.right (1,0,0) " +
        "or Vector3.forward (0,0,1) = vertical circle that swings overhead " +
        "and underfoot instead.")]
    public Vector3 orbitAxis = Vector3.right;

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] sunVertices;
    Vector3[] moonVertices;
    Vector3[] currentVertices;
    Renderer rend;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "SunMoonCycle";

        baseVertices = ProceduralMeshUtility.GenerateSphereVertices(
            meridians, parallels, radius
        );

        int[] triangles = ProceduralMeshUtility.GenerateSphereTriangles(
            meridians, parallels
        );

        sunVertices = BuildSunTendrilShape(baseVertices);
        moonVertices = BuildMoonCrescentShape(baseVertices);
        currentVertices = new Vector3[baseVertices.Length];

        mesh.vertices = baseVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        rend = GetComponent<MeshRenderer>();

        // NOTE: previously this forced transform.position to
        // (orbitCenter + orbitRadius, 0, 0) in WORLD space, which silently
        // teleported this object away from wherever it was placed in the
        // Editor/scene (it was placed near the other props, but this line
        // snapped it out to world x=10, far outside the play space - that's
        // why it never appeared during testing). Keep whatever position was
        // authored in the scene instead; RotateAround in Update() will still
        // orbit correctly from here.
    }

    void Update()
    {
        float cycle = (Mathf.Sin(Time.time * cycleSpeed) + 1f) * 0.5f;

        // Morph the actual vertex positions between moon and sun.
        for (int i = 0; i < currentVertices.Length; i++)
        {
            currentVertices[i] = Vector3.Lerp(
                moonVertices[i],
                sunVertices[i],
                cycle
            );
        }

        mesh.vertices = currentVertices;
        mesh.RecalculateNormals();

        if (tintMaterial && rend != null)
        {
            rend.material.color = Color.Lerp(
                moonColor,
                sunColor,
                cycle
            );
        }

        // Orbit around (0,0,0), or the assigned orbitCenter.
        Vector3 pivot = orbitCenter != null
            ? orbitCenter.position
            : Vector3.zero;

        transform.RotateAround(
            pivot,
            orbitAxis,
            orbitSpeed * Time.deltaTime
        );
    }

    Vector3[] BuildSunTendrilShape(Vector3[] baseVerts)
    {
        Vector3[] flareDirections = GenerateEvenSphereDirections(tendrilCount);
        Vector3[] result = new Vector3[baseVerts.Length];

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Vector3 dir = baseVerts[i].normalized;

            float bestDot = -1f;

            for (int f = 0; f < flareDirections.Length; f++)
            {
                float d = Vector3.Dot(
                    dir,
                    flareDirections[f]
                );

                if (d > bestDot)
                    bestDot = d;
            }

            float falloff = Mathf.Clamp01(
                (bestDot - tendrilSharpness) /
                (1f - tendrilSharpness)
            );

            float spike = Mathf.Pow(
                falloff,
                3f
            ) * tendrilLength;

            result[i] = baseVerts[i] + dir * spike;
        }

        return result;
    }

    Vector3[] GenerateEvenSphereDirections(int count)
    {
        Vector3[] points = new Vector3[count];

        float goldenAngle =
            Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < count; i++)
        {
            float y =
                1f -
                (i / (float)(count - 1)) * 2f;

            float radiusAtY =
                Mathf.Sqrt(
                    Mathf.Max(0f, 1f - y * y)
                );

            float theta = goldenAngle * i;

            points[i] = new Vector3(
                Mathf.Cos(theta) * radiusAtY,
                y,
                Mathf.Sin(theta) * radiusAtY
            );
        }

        return points;
    }

    Vector3[] BuildMoonCrescentShape(Vector3[] baseVerts)
    {
        Vector3[] result = new Vector3[baseVerts.Length];

        Vector3 biteCenter =
            new Vector3(
                radius * crescentOffset,
                0f,
                0f
            );

        float biteRadius =
            radius * crescentBiteRadius;

        float biteCenterSqrMag =
            biteCenter.sqrMagnitude;

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Vector3 dir = baseVerts[i].normalized;

            float dirDotCenter =
                Vector3.Dot(
                    dir,
                    biteCenter
                );

            float discriminant =
                dirDotCenter * dirDotCenter -
                (biteCenterSqrMag -
                biteRadius * biteRadius);

            float finalDistance = radius;

            if (discriminant >= 0f)
            {
                float tNear =
                    dirDotCenter -
                    Mathf.Sqrt(discriminant);

                if (tNear > 0f && tNear < radius)
                {
                    finalDistance = tNear;
                }
            }

            result[i] = dir * finalDistance;
        }

        return result;
    }
}
