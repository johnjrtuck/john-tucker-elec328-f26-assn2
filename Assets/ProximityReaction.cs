using UnityEngine;

// Deformation 3: DISTINCT THIRD DEFORMATION (proximity-responsive)
// A stylized head that puffs up the closer the viewer gets to it.
// This reacts to a CONDITION (distance to the camera/VR viewer) rather
// than a click or pure time, so it reads as clearly different in both
// behavior and visual style from the other two scripts.
//
// The geometry below is a generic stylized head on purpose -- swap in
// your own name, texture, and material for whatever character concept
// you land on. Just make sure any texture/logo you apply is your own
// work or something you can properly cite, since this ships in your
// public report and demo video.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProximityReaction : MonoBehaviour
{
    [Header("Sphere resolution")]
    public int meridians = 24;
    public int parallels = 16;
    public float radius = 0.5f;

    [Header("Proximity reaction")]
    public Transform viewer;           // leave empty to auto-use Camera.main
    public float maxDistance = 3f;     // beyond this, no reaction at all
    public float maxBulge = 0.5f;      // how much it puffs up at point-blank range

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] currentVertices;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "ProximityReaction";

        baseVertices = ProceduralMeshUtility.GenerateSphereVertices(meridians, parallels, radius);
        int[] triangles = ProceduralMeshUtility.GenerateSphereTriangles(meridians, parallels);
        currentVertices = new Vector3[baseVertices.Length];

        mesh.vertices = baseVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;

        if (viewer == null && Camera.main != null)
        {
            viewer = Camera.main.transform;
        }
    }

    void Update()
    {
        if (viewer == null) return;

        float distance = Vector3.Distance(transform.position, viewer.position);
        float proximity = 1f - Mathf.Clamp01(distance / maxDistance); // 0 far, 1 close
        float bulge = proximity * maxBulge;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 dir = baseVertices[i].normalized;
            currentVertices[i] = baseVertices[i] + dir * bulge;
        }

        mesh.vertices = currentVertices;
        mesh.RecalculateNormals();
    }
}
