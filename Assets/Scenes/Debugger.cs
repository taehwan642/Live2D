using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct LiveVertex
{
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
    int previousBoneIndex = 0;
    public Dropdown vertexDropdown;
    int currentVertexIndex = 0;
    public Slider weightSlider;
    public InputField boneIndexInput;

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
            if (i == currentVertexIndex)
                visualizer.GetComponent<SpriteRenderer>().color = Color.green;
            else
                visualizer.GetComponent<SpriteRenderer>().color = Color.red;
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

    void InitVertexWeight(ref LiveVertex vertex)
    {
        vertex.weight.boneIndex0 = 0;
        vertex.weight.boneIndex1 = -1;
        vertex.weight.boneIndex2 = -1;
        vertex.weight.boneIndex3 = -1;

        vertex.weight.weight0 = 1;
        vertex.weight.weight1 = 0;
        vertex.weight.weight2 = 0;
        vertex.weight.weight3 = 0;
    }

    void SetWeightAccumulateIn1(ref LiveVertex vertex)
    {
        float weightAcc = vertex.weight.weight0 + vertex.weight.weight1 + vertex.weight.weight2 + vertex.weight.weight3;
        if (weightAcc > 1.0f)
        {
            BoneWeight w = vertex.weight;
            float length = Mathf.Sqrt((w.weight0 * w.weight0) + (w.weight1 * w.weight1) + (w.weight2 * w.weight2) + (w.weight3 * w.weight3));
            vertex.weight.weight0 = w.weight0 / length;
            vertex.weight.weight1 = w.weight1 / length;
            vertex.weight.weight2 = w.weight2 / length;
            vertex.weight.weight3 = w.weight3 / length;
        }
    }

    public void AddBone()
    {
        LiveBone bone = new LiveBone();
        GameObject boneGameObject = Instantiate(bonePrefab, Vector3.zero, Quaternion.identity);
        boneGameObject.transform.parent = boneParent.transform.GetChild(0).transform;

        boneGameObject.GetComponent<SpriteRenderer>().color = Color.red;

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
        int input = int.Parse(boneIndexInput.transform.GetChild(2).GetComponent<Text>().text);

        LiveVertex vertex = liveVertices[vertexIndex];

        if (input == 0)
        {
            vertex.weight.boneIndex0 = boneIndex;
            vertex.weight.weight0 = weight;
        }
        else if (input == 1)
        {
            vertex.weight.boneIndex1 = boneIndex;
            vertex.weight.weight1 = weight;
        }
        else if (input == 2)
        {
            vertex.weight.boneIndex2 = boneIndex;
            vertex.weight.weight2 = weight;
        }
        else if (input == 3)
        {
            vertex.weight.boneIndex3 = boneIndex;
            vertex.weight.weight3 = weight;
        }
        SetWeightAccumulateIn1(ref vertex);
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

    public void ChangeVertexColor()
    {
        currentVertexIndex = vertexDropdown.value;
    }

    public void ChangeBoneColor()
    {
        liveBones[previousBoneIndex].bonePosition.gameObject.GetComponent<SpriteRenderer>().color = Color.red;

        liveBones[boneDropdown.value].bonePosition.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        previousBoneIndex = boneDropdown.value;
    }

    public void AddVertex(Vector3 worldPosition)
    {
        // 월드에서 로컬로 변환한 뒤, live2DObject mesh에 추가
        if (IsPointLiesInMesh(worldPosition))
        {
            Debug.Log("INSIDE!!!");

            Mesh mesh = live2DObject.GetComponent<MeshFilter>().mesh;
            int verticesLength = mesh.vertexCount + 1;

            int[] triangles = new int[verticesLength * 3];
            mesh.triangles.CopyTo(triangles, 0);
            triangles[mesh.triangles.Length + 0] = 0;
            triangles[mesh.triangles.Length + 1] = 2;
            triangles[mesh.triangles.Length + 2] = 4;

            Vector3[] vertices = new Vector3[verticesLength];
            mesh.vertices.CopyTo(vertices, 0);

            Vector4 worldPos;
            worldPos.x = worldPosition.x;
            worldPos.y = worldPosition.y;
            worldPos.z = 0;
            worldPos.w = 1;
            Vector3 P = live2DObject.transform.worldToLocalMatrix * worldPos;
            vertices[mesh.vertices.Length] = P;

            Vector2[] uv = new Vector2[verticesLength];
            mesh.uv.CopyTo(uv, 0);
            uv[mesh.uv.Length] = new Vector2(0.5f, 0.5f);

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            live2DObject.GetComponent<MeshFilter>().mesh = mesh;

            LiveVertex vtx = new LiveVertex();
            vtx.position = P;
            vtx.uv = new Vector2(0.5f, 0.5f);
            InitVertexWeight(ref vtx);
            liveVertices.Add(vtx);

            UpdateDropdowns();
        }
        else
        {
            Debug.Log("NOTINSIDE");
        }
    }

    bool IsPointLiesInMesh(Vector3 worldPosition)
    {
        // live2DObject의 Triangle을 다 돌면서 검사
        Mesh mesh = live2DObject.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;

        Vector4 worldPos;
        worldPos.x = worldPosition.x;
        worldPos.y = worldPosition.y;
        worldPos.z = 0;
        worldPos.w = 1;
        Vector3 P = live2DObject.transform.worldToLocalMatrix * worldPos;

        for (int i = 0; i < triangles.Length / 3; ++i)
        {
            Vector3 A = mesh.vertices[triangles[(i * 3)]];
            Vector3 B = mesh.vertices[triangles[(i * 3) + 1]];
            Vector3 C = mesh.vertices[triangles[(i * 3) + 2]];

            // 삼각형의 넓이 ABC == PAB + PBC + PAC라면 P는 ABC 안에 있다.
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            float ABCarea = 0.5f * (Vector3.Cross(AB,AC)).magnitude;

            Vector3 PA = A - P;
            Vector3 PB = B - P;
            Vector3 PC = C - P;
            float PABarea = 0.5f * (Vector3.Cross(PA, PB)).magnitude;
            float PBCarea = 0.5f * (Vector3.Cross(PB, PC)).magnitude;
            float PACarea = 0.5f * (Vector3.Cross(PA, PC)).magnitude;
            
            // 오차 범위가 존재하기 때문에, 반올림
            float result = Mathf.Round((PABarea + PBCarea + PACarea) * 10.0f) * 0.1f;
            if (ABCarea >= result)
                return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        liveVertices = new List<LiveVertex>();
        liveBones = new List<LiveBone>();

        // 쿼드 세팅
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.SetActive(false);

        Mesh mesh = Instantiate<Mesh>(quad.GetComponent<MeshFilter>().mesh);
        live2DObject.GetComponent<MeshFilter>().mesh = mesh;

        // Live 정보들 세팅
        Vector3[] vertices = mesh.vertices;

        LiveVertex v1 = new LiveVertex();
        v1.position = vertices[0];
        v1.uv = mesh.uv[0];
        liveVertices.Add(v1);

        LiveVertex v2 = new LiveVertex();
        v2.position = vertices[1];
        v2.uv = mesh.uv[1];
        liveVertices.Add(v2);

        LiveVertex v3 = new LiveVertex();
        v3.position = vertices[2];
        v3.uv = mesh.uv[2];
        liveVertices.Add(v3);

        LiveVertex v4 = new LiveVertex();
        v4.position = vertices[3];
        v4.uv = mesh.uv[3];
        liveVertices.Add(v4);

        for (int i = 0; i < 4; ++i)
        {
            LiveVertex vtx = liveVertices[i];
            InitVertexWeight(ref vtx);
            liveVertices[i] = vtx;
        }

        LiveBone bone = new LiveBone();
        bone.bonePosition = boneParent.transform.GetChild(0).transform;
        bone.bindPosition = bone.bonePosition.localToWorldMatrix;
        liveBones.Add(bone);

        UpdateDropdowns();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Mesh mesh = Instantiate<Mesh>(live2DObject.GetComponent<MeshFilter>().mesh);
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
