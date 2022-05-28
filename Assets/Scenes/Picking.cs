using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picking : MonoBehaviour
{
    public Debugger debugger;
    public Vector3 screenPosition;
    public Vector3 worldPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            screenPosition = Input.mousePosition;
            screenPosition.z = 1;

            worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            debugger.AddVertex(worldPosition);
        }
    }
}
