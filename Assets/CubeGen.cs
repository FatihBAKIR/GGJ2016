using UnityEngine;
using System.Collections;

public static class CubeGen
{
    private static Mesh _tileCube;
    public static Mesh TileCube
    {
        get
        {
            if(_tileCube == null)
            {
                _tileCube = GenerateCube();
            }
            return _tileCube;
        }
    }
    // Use this for initialization
    static Mesh GenerateCube()
    {
        Mesh m = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;

        Vector2[] uvs = new Vector2[24];

        for (int i = 0; i < m.vertexCount; i++)
        {
            uvs[i] = m.uv[i];
        }

        uvs[0] = uvs[7] = uvs[12] = uvs[16] = uvs[20] = new Vector2(0.5f, 0);
        uvs[1] = uvs[6] = uvs[15] = uvs[19] = uvs[23] = new Vector2(1, 0);
        uvs[2] = uvs[11] = uvs[13] = uvs[17] = uvs[21] = new Vector2(0.5f, 1);
        uvs[3] = uvs[10] = uvs[14] = uvs[18] = uvs[22] = new Vector2(1, 1);

        //Debug.DrawLine(m.vertices[20], m.vertices[20] + m.normals[20], Color.blue, 600);
        //Debug.DrawLine(m.vertices[21], m.vertices[21] + m.normals[21], Color.green, 600);
        //Debug.DrawLine(m.vertices[22], m.vertices[22] + m.normals[22], Color.red, 600);
        //Debug.DrawLine(m.vertices[23], m.vertices[23] + m.normals[23], Color.yellow, 600);

        uvs[9] = new Vector2(0, 0);
        uvs[8] = new Vector2(0.5f, 0);
        uvs[5] = new Vector2(0, 1f);
        uvs[4] = new Vector2(0.5f, 1f);

        m.uv = uvs;
        m.UploadMeshData(false);

        //for (int i = 0; i < m.vertexCount; i++)
        //{
        //    Debug.Log(i + " - " + m.normals[i]);
        //}
        return m;
    }
}
