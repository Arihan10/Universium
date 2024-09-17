using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAICollisions : MonoBehaviour
{
    public int planetCollisions = 0; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Chunk") {
            ++planetCollisions; 
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Chunk") {
            --planetCollisions; 
        }
    }
}
