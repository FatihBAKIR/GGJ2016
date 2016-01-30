using UnityEngine;
using System.Collections;

public class CubeGen : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Mesh m = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;

        Vector2[] uvs = new Vector2[24];

        for (int i = 0; i < m.vertexCount; i++)
        {
            uvs[i] = m.uv[i];
        }

        uvs[0] = uvs[6] = uvs[12] = uvs[16] = uvs[20] = new Vector2(0.5f, 0);
        uvs[1] = uvs[7] = uvs[13] = uvs[17] = uvs[21] = new Vector2(1, 0);
        uvs[2] = uvs[10] = uvs[14] = uvs[18] = uvs[22] = new Vector2(0.5f, 1);
        uvs[3] = uvs[11] = uvs[15] = uvs[19] = uvs[23] = new Vector2(1, 1);

        uvs[9] = new Vector2(0, 0);
        uvs[8] = new Vector2(0.5f, 0);
        uvs[5] = new Vector2(0, 1f);
        uvs[4] = new Vector2(0.5f, 1f);

        m.uv = uvs;
        m.UploadMeshData(false);

        for (int i = 0; i < m.vertexCount; i++)
        {
            Debug.Log(i + " - " + m.normals[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
