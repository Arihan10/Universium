using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

public class WaterScroll : MonoBehaviour
{
    [SerializeField] Material mat; 

    [SerializeField] Vector2 scrollSpeed;

    Vector2 offset;

    Color colour; 
    
    // Start is called before the first frame update
    void Start()
    {
        // colour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
        // GetComponent<MeshRenderer>().material.color = colour;
        // RenderSettings.fogColor = colour;

        Volume volume = GetComponent<Volume>();
        Vignette vignette; 
        if (volume.profile.TryGet<Vignette>(out vignette)) {
            vignette.color.Override(colour);  
        }

        mat = GetComponent<MeshRenderer>().material; 
    }

    // Update is called once per frame
    void Update()
    {
        offset.x += scrollSpeed.x * Time.deltaTime;
        offset.y += scrollSpeed.y * Time.deltaTime;
        mat.mainTextureOffset = offset; 
    }
}
