using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance;

    [SerializeField] AudioSource BG, ambient, FX; 

    [SerializeField] AudioClip mine, hurt, bullet; 

    private void Awake() {
        if (instance != null) Destroy(gameObject);
        else instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Background() {
        BG.Play(); 
    }

    public void Mine() {
        FX.PlayOneShot(mine, 1f);
    }

    public void Hurt() {
        FX.PlayOneShot(hurt); 
    }

    public void Bullet() {
        FX.PlayOneShot(bullet); 
    }
}
