using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkID;

    // x, z, y
    public Vector3Int startPos;

    public GameObject planet, waterSphere; 

    /* private void Update() {
        GetComponent<MeshRenderer>().material.SetVector("_center", waterSphere.transform.position); 
    } */
}
