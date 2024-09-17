using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public float mass, radius;
    [SerializeField] Vector3 initialVelocity;
    public Vector3 currentVelocity, lastFrameVelocity;

    [SerializeField] GameManager sim;

    const float G = 0.000000000066742f;

    // Rigidbody rb; 
    
    // Start is called before the first frame update
    void Start()
    {
        currentVelocity = initialVelocity;
        // rb = GetComponent<Rigidbody>();

        // sim = GameObject.Find("GravitySimulator").GetComponent<GravitySimulation>(); 
        sim = GameManager.instance; 
    }

    public void UpdateVelocity(float timestep) {
        lastFrameVelocity = currentVelocity; 
        foreach (CelestialBody body in sim.allBodies) {
            if (body != this) {
                /* float sqrDist = (body.transform.position - transform.position).sqrMagnitude; 
                Vector3 forceDir = (body.transform.position - transform.position).normalized; 
                Vector3 force = (G * mass * body.mass * forceDir) / sqrDist; 
                Vector3 acceleration = force / mass; 
                currentVelocity += acceleration * timestep; */

                float sqrDst = (body.GetComponent<Rigidbody>().position - GetComponent<Rigidbody>().position).sqrMagnitude;
                Vector3 forceDir = (body.GetComponent<Rigidbody>().position - GetComponent<Rigidbody>().position).normalized;

                Vector3 acceleration = forceDir * G * body.mass / sqrDst;
                currentVelocity += acceleration * timestep;
            }
        }
    }

    public void UpdatePosition(float timestep) {
        // Debug.Log(GetComponent<Rigidbody>()); 
        
        // GetComponent<Rigidbody>().position += currentVelocity * timestep; 
    }
}
