using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Transform bonePosition;
}

public class Debugger : MonoBehaviour
{
    public GameObject live2DObject;
    public GameObject vertexVisualizer;
    public GameObject bonePrefab;
    public GameObject boneParent;
    private GameObject quad;

    public Dropdown boneDropdown;
    public Dropdown vertexDropdown;
    public Slider weightSlider;

    List<LiveVertex> liveVertices;
    List<LiveBone> liveBones;

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

    void InitLiveQuadVertex(ref LiveVertex vertex)
    {
        vertex.weight.boneIndex0 = -1;
        vertex.weight.boneIndex1 = -1;
        vertex.weight.boneIndex2 = -1;
        vertex.weight.boneIndex3 = -1;

        vertex.weight.weight0 = 0;
        vertex.weight.weight1 = 0;
        vertex.weight.weight2 = 0;
        vertex.weight.weight3 = 0;
    }

    public void AddBone()
    {
        LiveBone bone = new LiveBone();
        GameObject boneGameObject = Instantiate(bonePrefab, Vector3.zero, Quaternion.identity);
        boneGameObject.transform.parent = boneParent.transform;
        bone.bonePosition = boneGameObject.transform;
        // 업데이트 필요
        bone.bindPosition = Matrix4x4.identity;
        liveBones.Add(bone);

        UpdateDropdowns();
    }

    public void AttachVertexToBone()
    {
        int boneIndex = boneDropdown.value;
        int vertexIndex = vertexDropdown.value;
        float weight = weightSlider.value;

        LiveVertex vertex = liveVertices[vertexIndex];

        if (vertex.weight.boneIndex0 == -1)
        {
            vertex.weight.boneIndex0 = boneIndex;
            vertex.weight.weight0 = weight;
        }
        else if (vertex.weight.boneIndex1 == -1)
        {
            vertex.weight.boneIndex1 = boneIndex;
            vertex.weight.weight1 = weight;
        }
        else if (vertex.weight.boneIndex2 == -1)
        {
            vertex.weight.boneIndex2 = boneIndex;
            vertex.weight.weight2 = weight;
        }
        else if (vertex.weight.boneIndex3 == -1)
        {
            vertex.weight.boneIndex3 = boneIndex;
            vertex.weight.weight3 = weight;
        }

        liveVertices[vertexIndex] = vertex;
    }

    void UpdateDropdowns()
    {
        vertexDropdown.ClearOptions();
        for (int i = 0; i < liveVertices.Count; ++i)
        {
            Dropdown.OptionData data = new Dropdown.OptionData(string.Format("Vertex {0}", i));
            vertexDropdown.options.Add(data);
        }
        vertexDropdown.transform.GetChild(0).GetComponent<Text>().text = vertexDropdown.options[0].text;

        boneDropdown.ClearOptions();
        for (int i = 0; i < liveBones.Count; ++i)
        {
            Dropdown.OptionData data = new Dropdown.OptionData(string.Format("Bone {0}", i));
            boneDropdown.options.Add(data);
        }

        if (boneDropdown.options.Count != 0)
            boneDropdown.transform.GetChild(0).GetComponent<Text>().text = boneDropdown.options[0].text;
    }

    public void Play()
    {
        for (int i = 0; i < liveBones.Count; ++i)
        {
            LiveBone bone = liveBones[i];
            bone.bindPosition = bone.bonePosition.localToWorldMatrix;
            liveBones[i] = bone;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        liveVertices = new List<LiveVertex>();
        liveBones = new List<LiveBone>();

        // 쿼드 세팅
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.SetActive(false);

        // Live 정보들 세팅
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        LiveVertex v1 = new LiveVertex();
        v1.vertexIndex = 0;
        v1.position = vertices[0];
        v1.uv = mesh.uv[0];
        liveVertices.Add(v1);

        LiveVertex v2 = new LiveVertex();
        v2.vertexIndex = 1;
        v2.position = vertices[1];
        v2.uv = mesh.uv[1];
        liveVertices.Add(v2);

        LiveVertex v3 = new LiveVertex();
        v3.vertexIndex = 2;
        v3.position = vertices[2];
        v3.uv = mesh.uv[2];
        liveVertices.Add(v3);

        LiveVertex v4 = new LiveVertex();
        v4.vertexIndex = 3;
        v4.position = vertices[3];
        v4.uv = mesh.uv[3];
        liveVertices.Add(v4);

        for (int i = 0; i < 4; ++i)
        {
            LiveVertex vtx = liveVertices[i];
            InitLiveQuadVertex(ref vtx);
            liveVertices[i] = vtx;
        }

        UpdateDropdowns();
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
                    if (w.boneIndex0 == -1)
                        break; 
                    boneIndex = w.boneIndex0;
                    boneWeight = w.weight0;
                }
                else if (j == 1)
                {
                    if (w.boneIndex1 == -1)
                        break;
                    boneIndex = w.boneIndex1;
                    boneWeight = w.weight1;
                }
                else if (j == 2)
                {
                    if (w.boneIndex2 == -1)
                        break;
                    boneIndex = w.boneIndex2;
                    boneWeight = w.weight2;
                }
                else if (j == 3)
                {
                    if (w.boneIndex3 == -1)
                        break;
                    boneIndex = w.boneIndex3;
                    boneWeight = w.weight3;
                }

                if (boneIndex >= liveBones.Count)
                    continue;

                Matrix4x4 boneMatrix = liveBones[boneIndex].bonePosition.localToWorldMatrix; // 월드
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
