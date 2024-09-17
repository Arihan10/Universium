using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings : MonoBehaviour
{
    public int octaves = 1, noiseScale = 25; 
    public float amplitude = 25f, lacunarity = 2f, persistence = 0.9f; 
}
