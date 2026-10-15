using UnityEngine;
using UnityEngine.InputSystem;
// Deformation 1: INPUT-RESPONSIVE
// Starts as a smooth "water world" sphere. Left click raises procedural
// continents out of the noise-quiet areas, so it visibly becomes more
// Earth-like each click; right click erodes it back toward pure ocean.
// Color shifts from ocean blue toward a greener/earthier tone as it goes.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClickMorphFace : MonoBehaviour
{
    [Header("Resolution (vertices per cube edge)")]
    public int resolution = 24;
    public float radius = 1f;

    [Header("Morph")]
    [Range(0f, 1f)] public float morphAmount = 0f; // 0 = all water, 1 = full continents
    public float stepPerClick = 0.12f;

    [Header("Continent shape")]
    public float continentScale = 1.5f;
    [Range(0f, 1f)] public float continentThreshold = 0.55f;
    public float landHeight = 0.12f;

    [Header("Color tint while morphing")]
    public Color waterColor = new Color(0.05f, 0.25f, 0.55f);
    public Color earthColor = new Color(0.35f, 0.55f, 0.25f);

    [Header("VR Trigger Input (optional - drag the 'Activate' action from " +
        "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/" +
        "XRI Default Input Actions.inputactions)")]
    [Tooltip("Right controller trigger: morphs toward Earth, same as left mouse click.")]
    public InputActionReference rightTriggerAction;
    [Tooltip("Left controller trigger (optional): morphs back toward water, same as right mouse click.")]
    public InputActionReference leftTriggerAction;

    Mesh mesh;
    Vector3[] waterVertices;
    Vector3[] earthVertices;
    Vector3[] currentVertices;
    Renderer rend;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "WaterToEarth";

        Vector3[] cubeVertices = CubeSphereUtility.GenerateCubeVertices(resolution, radius);
        waterVertices = CubeSphereUtility.GenerateSphereFromCube(cubeVertices, radius);
        earthVertices = BuildEarthShape(waterVertices);
        int[] triangles = CubeSphereUtility.GenerateCubeTriangles(resolution);

        currentVertices = new Vector3[waterVertices.Length];

        mesh.vertices = waterVertices;
        mesh.triangles = triangles;
        mesh.uv = CubeSphereUtility.GenerateCubeUVs(resolution);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        rend = GetComponent<MeshRenderer>();

        // Set the object to world position (0, 0, 5)
        transform.position = new Vector3(0f, 0f, 5f);

        ApplyMorph();
    }

    void OnEnable()
    {
        if (rightTriggerAction != null) rightTriggerAction.action.Enable();
        if (leftTriggerAction != null) leftTriggerAction.action.Enable();
    }

    void OnDisable()
    {
        if (rightTriggerAction != null) rightTriggerAction.action.Disable();
        if (leftTriggerAction != null) leftTriggerAction.action.Disable();
    }

    void Update()
    {
        // Works with either a desktop mouse (Editor testing) or a VR
        // controller trigger (in the headset, where there is no mouse).
        bool forwardPressed =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (rightTriggerAction != null && rightTriggerAction.action.WasPressedThisFrame());

        bool backwardPressed =
            (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
            (leftTriggerAction != null && leftTriggerAction.action.WasPressedThisFrame());

        if (forwardPressed) // forward in time: raise continents
        {
            morphAmount = Mathf.Clamp01(morphAmount + stepPerClick);
            ApplyMorph();
        }
        else if (backwardPressed) // backward in time: erode back to water
        {
            morphAmount = Mathf.Clamp01(morphAmount - stepPerClick);
            ApplyMorph();
        }
    }

    void ApplyMorph()
    {
        for (int i = 0; i < currentVertices.Length; i++)
        {
            currentVertices[i] = Vector3.Lerp(
                waterVertices[i],
                earthVertices[i],
                morphAmount
            );
        }

        mesh.vertices = currentVertices;
        mesh.RecalculateNormals();

        if (rend != null)
        {
            rend.material.color = Color.Lerp(
                waterColor,
                earthColor,
                morphAmount
            );
        }
    }

    // Raises land wherever a cheap pseudo-3D noise value clears a threshold,
    // leaving everything else at the water sphere's base radius.
    Vector3[] BuildEarthShape(Vector3[] baseVerts)
    {
        Vector3[] result = new Vector3[baseVerts.Length];

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Vector3 dir = baseVerts[i].normalized;
            float noise = Sample3DNoise(dir * continentScale);

            float elevation = 0f;

            if (noise > continentThreshold)
            {
                elevation =
                    (noise - continentThreshold) /
                    (1f - continentThreshold) *
                    landHeight;
            }

            result[i] = dir * (radius + elevation);
        }

        return result;
    }

    float Sample3DNoise(Vector3 p)
    {
        float n1 = Mathf.PerlinNoise(p.x, p.y);
        float n2 = Mathf.PerlinNoise(p.y, p.z);
        float n3 = Mathf.PerlinNoise(p.z, p.x);

        return (n1 + n2 + n3) / 3f;
    }
}
