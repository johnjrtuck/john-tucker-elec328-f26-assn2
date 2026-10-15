using UnityEngine;

// Shared helper for building a procedural UV sphere.
// All three deformation scripts below start from this same technique
// (the same box-walking triangle pattern you used for the flat grid mesh,
// just wrapped around a sphere instead of laid flat) so that any two
// vertex arrays built with the same meridians/parallels line up index-for-index
// and can be lerped vertex-by-vertex. That's what makes the morphing work.
public static class ProceduralMeshUtility
{
    // meridians = vertical slices (longitude), parallels = horizontal rings (latitude)
    public static Vector3[] GenerateSphereVertices(int meridians, int parallels, float radius)
    {
        Vector3[] vertices = new Vector3[(meridians + 1) * (parallels + 1)];
        int vi = 0;
        for (int y = 0; y <= parallels; y++)
        {
            float v = (float)y / parallels;      // 0 (top) to 1 (bottom)
            float phi = v * Mathf.PI;             // 0 to PI

            for (int x = 0; x <= meridians; x++)
            {
                float u = (float)x / meridians;   // 0 to 1 around
                float theta = u * Mathf.PI * 2f;  // 0 to 2*PI

                float sinPhi = Mathf.Sin(phi);
                float px = sinPhi * Mathf.Cos(theta);
                float py = Mathf.Cos(phi);
                float pz = sinPhi * Mathf.Sin(theta);

                vertices[vi] = new Vector3(px, py, pz) * radius;
                vi++;
            }
        }
        return vertices;
    }

    public static int[] GenerateSphereTriangles(int meridians, int parallels)
    {
        int[] triangles = new int[meridians * parallels * 6];
        int ti = 0;
        int rowLength = meridians + 1;

        for (int y = 0; y < parallels; y++)
        {
            for (int x = 0; x < meridians; x++)
            {
                int a = y * rowLength + x;
                int b = (y + 1) * rowLength + x;
                int c = (y + 1) * rowLength + x + 1;
                int d = y * rowLength + x + 1;

                triangles[ti++] = a;
                triangles[ti++] = b;
                triangles[ti++] = c;

                triangles[ti++] = a;
                triangles[ti++] = c;
                triangles[ti++] = d;
            }
        }
        return triangles;
    }
}
