using UnityEngine;

// Builds a "cube sphere": six flat grids arranged into a cube, whose
// vertices can either stay on the cube's flat faces (the cube shape)
// or be normalized onto a sphere (the sphere shape). Because both
// shapes come from the exact same grid layout, vertex counts and order
// match perfectly, so they lerp together cleanly -- with no pinching
// at the poles, which is the problem a plain UV sphere runs into when
// you try to reproject it into a cube.
public static class CubeSphereUtility
{
    static readonly Vector3[] faceUpDirections =
    {
        Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
    };

    // resolution = vertices per edge of each face (not quad count)
    public static Vector3[] GenerateCubeVertices(int resolution, float halfSize)
    {
        Vector3[] vertices = new Vector3[resolution * resolution * 6];
        int vi = 0;

        foreach (Vector3 localUp in faceUpDirections)
        {
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnCube = localUp
                        + (percent.x - 0.5f) * 2f * axisA
                        + (percent.y - 0.5f) * 2f * axisB;
                    vertices[vi] = pointOnCube * halfSize;
                    vi++;
                }
            }
        }

        return vertices;
    }

    // Same face/x/y traversal order as GenerateCubeVertices, so the
    // indices line up -- each face gets the full 0..1 image on it.
    public static Vector2[] GenerateCubeUVs(int resolution)
    {
        Vector2[] uvs = new Vector2[resolution * resolution * 6];
        int vi = 0;

        for (int face = 0; face < 6; face++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    uvs[vi] = new Vector2((float)x / (resolution - 1), (float)y / (resolution - 1));
                    vi++;
                }
            }
        }

        return uvs;
    }

    public static Vector3[] GenerateSphereFromCube(Vector3[] cubeVertices, float radius)
    {
        Vector3[] result = new Vector3[cubeVertices.Length];
        for (int i = 0; i < cubeVertices.Length; i++)
        {
            result[i] = cubeVertices[i].normalized * radius;
        }
        return result;
    }

    public static int[] GenerateCubeTriangles(int resolution)
    {
        int trianglesPerFace = (resolution - 1) * (resolution - 1) * 6;
        int[] triangles = new int[trianglesPerFace * 6];
        int ti = 0;

        for (int face = 0; face < 6; face++)
        {
            int faceOffset = face * resolution * resolution;

            for (int y = 0; y < resolution - 1; y++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i = faceOffset + x + y * resolution;

                    triangles[ti] = i;
                    triangles[ti + 1] = i + resolution + 1;
                    triangles[ti + 2] = i + resolution;

                    triangles[ti + 3] = i;
                    triangles[ti + 4] = i + 1;
                    triangles[ti + 5] = i + resolution + 1;

                    ti += 6;
                }
            }
        }

        return triangles;
    }
}