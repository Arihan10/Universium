using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 
    
    public CelestialBody[] allBodies; 

    public List<GameObject> items = new List<GameObject>(); 

    public int planetsMade = 0, totalPlanets; 

    private void Awake() {
        // allBodies = FindObjectsOfType<CelestialBody>(); 
        instance = this; 
    }

    private void FixedUpdate() {
        allBodies = FindObjectsOfType<CelestialBody>(); 

        /* foreach (CelestialBody body in allBodies) {
            body.UpdateVelocity(Time.fixedDeltaTime); 
        }

        foreach (CelestialBody body in allBodies) {
            body.UpdatePosition(Time.fixedDeltaTime);
        } */
    }
}
