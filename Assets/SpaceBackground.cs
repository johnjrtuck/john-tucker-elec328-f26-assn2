using UnityEngine;

// Environment prop (not one of your 3 required deformations - just backdrop).
// hw1_space.unity has no Camera of its own (Reality Roost supplies one at
// runtime, and its Clear Flags aren't something this project controls), so a
// Lighting-window skybox material isn't reliable here. This builds actual
// background GEOMETRY instead: a big sphere with its winding flipped inward
// so you see its inside surface from wherever you're standing, plus a
// scatter of tiny bright quads for stars. Renders correctly regardless of
// what camera/clear-flags Reality Roost uses.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpaceBackground : MonoBehaviour
{
    [Header("Sky dome")]
    public float skyRadius = 60f;
    public int meridians = 20;
    public int parallels = 14;
    public Color skyColor = new Color(0.01f, 0.01f, 0.03f);

    [Header("Stars")]
    public int starCount = 400;
    public float starRadius = 55f; // slightly inside the sky dome
    public float starSize = 0.4f;
    public Color starColor = new Color(0.9f, 0.95f, 1f);
    [Range(0f, 1f)] public float starColorVariation = 0.15f;

    void Start()
    {
        BuildSkyDome();
        BuildStars();
    }

    void BuildSkyDome()
    {
        Vector3[] vertices = ProceduralMeshUtility.GenerateSphereVertices(meridians, parallels, skyRadius);
        int[] outwardTriangles = ProceduralMeshUtility.GenerateSphereTriangles(meridians, parallels);

        // Flip winding so the INSIDE of the sphere faces the camera - a
        // normal sphere is built to be seen from outside, and we're
        // standing inside this one.
        int[] triangles = new int[outwardTriangles.Length];
        for (int i = 0; i < outwardTriangles.Length; i += 3)
        {
            triangles[i] = outwardTriangles[i];
            triangles[i + 1] = outwardTriangles[i + 2];
            triangles[i + 2] = outwardTriangles[i + 1];
        }

        Mesh mesh = new Mesh();
        mesh.name = "SkyDome";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        Renderer rend = GetComponent<MeshRenderer>();
        rend.material = MakeUnlitMaterial(skyColor);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    void BuildStars()
    {
        GameObject starsObj = new GameObject("Stars");
        starsObj.transform.SetParent(transform, false);

        MeshFilter mf = starsObj.AddComponent<MeshFilter>();
        MeshRenderer mr = starsObj.AddComponent<MeshRenderer>();

        Vector3[] vertices = new Vector3[starCount * 4];
        // 4 triangles per star (front + back of the quad, both windings) so
        // it's visible no matter which way the material culls faces.
        int[] triangles = new int[starCount * 12];
        Color[] colors = new Color[starCount * 4];

        for (int i = 0; i < starCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            Vector3 center = dir * starRadius;

            Vector3 up = Vector3.Cross(dir, Vector3.up).sqrMagnitude > 0.001f
                ? Vector3.Cross(dir, Vector3.up).normalized
                : Vector3.Cross(dir, Vector3.right).normalized;
            Vector3 right = Vector3.Cross(dir, up).normalized;

            float size = starSize * Random.Range(0.5f, 1.5f);

            int vi = i * 4;
            vertices[vi + 0] = center - right * size - up * size;
            vertices[vi + 1] = center + right * size - up * size;
            vertices[vi + 2] = center + right * size + up * size;
            vertices[vi + 3] = center - right * size + up * size;

            Color c = Color.Lerp(
                starColor,
                Color.white,
                Random.Range(-starColorVariation, starColorVariation)
            );
            colors[vi + 0] = c;
            colors[vi + 1] = c;
            colors[vi + 2] = c;
            colors[vi + 3] = c;

            int ti = i * 12;
            // front winding
            triangles[ti + 0] = vi + 0;
            triangles[ti + 1] = vi + 2;
            triangles[ti + 2] = vi + 1;
            triangles[ti + 3] = vi + 0;
            triangles[ti + 4] = vi + 3;
            triangles[ti + 5] = vi + 2;
            // back winding (so the quad shows from either side)
            triangles[ti + 6] = vi + 0;
            triangles[ti + 7] = vi + 1;
            triangles[ti + 8] = vi + 2;
            triangles[ti + 9] = vi + 0;
            triangles[ti + 10] = vi + 2;
            triangles[ti + 11] = vi + 3;
        }

        Mesh starMesh = new Mesh();
        starMesh.name = "Stars";
        starMesh.vertices = vertices;
        starMesh.triangles = triangles;
        starMesh.colors = colors;
        starMesh.RecalculateNormals();

        mf.mesh = starMesh;

        mr.material = MakeUnlitMaterial(Color.white);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    Material MakeUnlitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }
}
