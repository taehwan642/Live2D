using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LiveVertex
{
    public LiveVertex(Vector3 pos) 
    { 
        position = pos;
        uv = Vector2.zero;
        weight = new BoneWeight();
    }
    public Vector3 position;
    public Vector2 uv;

    public BoneWeight weight;
}

public struct LiveBone
{
    public Matrix4x4 bindPosition;
    public Transform bonePosition;
}

public class Live2D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       

        
    }
}
