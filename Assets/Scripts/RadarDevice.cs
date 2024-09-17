using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime; 

public class RadarDevice : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI planetName;

    [SerializeField] Image img; 
    
    public void Refresh() {
        PlayerController[] players = FindObjectsOfType<PlayerController>(); 
        foreach (PlayerController player in players) {
            if (!player.GetComponent<PhotonView>().IsMine) {
                planetName.text = player.currPlanet; 
                img.color = GameObject.Find(player.currPlanet).transform.Find("WATER(Clone)").GetComponent<MeshRenderer>().material.GetColor("_groundCol"); 
                break; 
            }
        }
    }
}
