using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour
{
    public Vector3 targetPos;

    public float speed = 1f; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed);

        if (Vector3.Distance(transform.position, targetPos) <= 2f) Destroy(gameObject); 
    }
}
