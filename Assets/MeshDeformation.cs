using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    public int width = 5;
    public int height = 4;

    Vector3[] vertices;
    Mesh m;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m = new Mesh();
        vertices = new Vector3[width * height];
        for (int i=0; i < width * height; i++)
        {
            float x  = (i%width)*1.0f/(width-1);
            float y = i/width*1.0f/(height-1);
            vertices[i] = new Vector3(x, y,0);
            Debug.Log(x + "," + y);
        }

        m.vertices = vertices;

        // # boxes = (width-1)*(height-1)
        // # triangles = (width-1)*(height-1)*2
        // # triangle indices = above*3
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        for (int j=0; j<height-1; j++)
        {
            for (int i=0;i<width-1;i++)
            {
                //iterating through our boxes
                //box i,j
                //A: i,j
                //B: i, j+1
                //C: i+1, j+1
                //D: i+1,j
                //two triangles: ABC, ACD
                int t_index = (j * (width - 1) + i) * 6;
                //FIXED from previous: t_index=j*i*6;
                triangles[t_index] = j * width + i;//A
                triangles[t_index + 1] = (j + 1) * width + i; //B
                triangles[t_index + 2] = (j + 1) * width + i + 1;//C
                triangles[t_index + 3] = j * width + i;//A
                triangles[t_index + 4] = (j + 1) * width + i + 1;//C
                triangles[t_index + 5] = (j) * width + i + 1;//D
            }
        }
        m.triangles = triangles;
        m.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = m;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y, Mathf.Sin(Time.time));
        }
        m.vertices = vertices;
        m.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = m;

    }
}
