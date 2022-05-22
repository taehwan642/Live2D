using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct LiveVertex
{
    public int vertexIndex;

    public Vector3 position;
    public Vector2 uv;

    public BoneWeight weight;
}

struct LiveBone
{
    public Matrix4x4 bindPosition;
}

public class Debugger : MonoBehaviour
{
    public GameObject live2DObject;
    public GameObject vertexVisualizer;
    public GameObject[] bones;
    private GameObject quad;

    LiveVertex[] liveVertices;
    LiveBone[] liveBones;

    void DrawVertexAndLine(Vector3[] vertices)
    {
        Vector3[] worldVertices = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; ++i)
        {
            worldVertices[i] = live2DObject.transform.TransformPoint(vertices[i]);
            GameObject visualizer = Instantiate(vertexVisualizer, worldVertices[i], Quaternion.identity);
            visualizer.transform.SetParent(transform);
        }

        Mesh mesh = live2DObject.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length / 3; ++i)
        {
            GameObject lineRendererObject = new GameObject();
            lineRendererObject.transform.SetParent(transform);
            lineRendererObject.name = "LineRender";
            LineRenderer renderer = lineRendererObject.AddComponent<LineRenderer>();
            renderer.startColor = Color.black;
            renderer.endColor = Color.black;
            renderer.startWidth = 0.05f;
            renderer.endWidth = 0.05f;
            renderer.material = new Material(Shader.Find("Diffuse"));
            renderer.positionCount = 4;

            for (int j = 0; j < 4; ++j)
            {
                if (j != 3)
                    renderer.SetPosition(j, worldVertices[triangles[(i * 3) + j]]);
                else
                    renderer.SetPosition(j, worldVertices[triangles[(i * 3)]]);
            }
        }
    }
    void SetWeightAndIndex(BoneWeight weight)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        // 쿼드 세팅
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.SetActive(false);

        // Live 정보들 세팅
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        liveVertices = new LiveVertex[vertices.Length];


        // 본
        liveVertices[0].vertexIndex = 0;
        liveVertices[0].position = vertices[0];
        liveVertices[0].uv = mesh.uv[0];
        liveVertices[0].weight.boneIndex0 = 0;
        liveVertices[0].weight.boneIndex1 = 1;
        liveVertices[0].weight.weight0 = 1;
        liveVertices[0].weight.weight1 = 0;
        // 본
        liveVertices[1].vertexIndex = 1;
        liveVertices[1].position = vertices[1];
        liveVertices[1].uv = mesh.uv[1];
        liveVertices[1].weight.boneIndex0 = 0;
        liveVertices[1].weight.boneIndex1 = 1;
        liveVertices[1].weight.weight0 = 0.5f;
        liveVertices[1].weight.weight1 = 0.5f;
        // 본
        liveVertices[2].vertexIndex = 2;
        liveVertices[2].position = vertices[2];
        liveVertices[2].uv = mesh.uv[2];
        liveVertices[2].weight.boneIndex0 = 0;
        liveVertices[2].weight.boneIndex1 = 1;
        liveVertices[2].weight.weight0 = 0.5f;
        liveVertices[2].weight.weight1 = 0.5f;
        // 본
        liveVertices[3].vertexIndex = 3;
        liveVertices[3].position = vertices[3];
        liveVertices[3].uv = mesh.uv[3];
        liveVertices[3].weight.boneIndex0 = 0;
        liveVertices[3].weight.boneIndex1 = 1;
        liveVertices[3].weight.weight0 = 0;
        liveVertices[3].weight.weight1 = 1;

        liveBones = new LiveBone[bones.Length];
        for (int i = 0; i < bones.Length; ++i)
            liveBones[i].bindPosition = bones[i].transform.localToWorldMatrix;

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Mesh mesh = Instantiate<Mesh>(quad.GetComponent<MeshFilter>().mesh);
        Vector3[] vertices = mesh.vertices;

        // 본을 통한 움직임
        for (int i = 0; i < vertices.Length; ++i)
        {
            // 위치에 대해 스키닝 연산 수행
            Vector3 totalPosition = Vector4.zero;
            BoneWeight w = liveVertices[i].weight;
            for (int j = 0; j < 4; ++j)
            {
                int boneIndex = 0;
                float boneWeight = 0;
                if (j == 0)
                {
                    boneIndex = w.boneIndex0;
                    boneWeight = w.weight0;
                }
                else if (j == 1)
                {
                    boneIndex = w.boneIndex1;
                    boneWeight = w.weight1;
                }
                else if (j == 2)
                {
                    boneIndex = w.boneIndex2;
                    boneWeight = w.weight2;
                }
                else if (j == 3)
                {
                    boneIndex = w.boneIndex3;
                    boneWeight = w.weight3;
                }

                Matrix4x4 boneMatrix = bones[boneIndex].transform.localToWorldMatrix; // 월드
                Matrix4x4 boneBindMatrix = liveBones[boneIndex].bindPosition; // 월드

                // 로컬
                Matrix4x4 boneLocal = boneBindMatrix.inverse * boneMatrix;

                Vector4 currentVertex = vertices[i];
                currentVertex.w = 1;
                // 로컬 정점 위치
                Vector4 localPosition = boneBindMatrix.inverse * currentVertex;

                Vector4 skinnedLocalPosition = boneLocal * localPosition;

                Vector3 skinnedWorldPosition = boneBindMatrix * skinnedLocalPosition;

                totalPosition += skinnedWorldPosition * boneWeight;
            }

            vertices[i] = totalPosition;
        }
        mesh.vertices = vertices;
        live2DObject.GetComponent<MeshFilter>().mesh = mesh;

        DrawVertexAndLine(mesh.vertices);
    }
}
