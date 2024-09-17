using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Terraform : MonoBehaviour {
    [SerializeField] int brushSize = 2, bulletItemIndex = 10; 
    [SerializeField] float brushSpeed = 50f, useRate = 5f; 

    [SerializeField] LayerMask mask;

    [SerializeField] GameObject muzzle; 
    GameObject player; 

    // Start is called before the first frame update
    void Start() {
        player = GetComponentInParent<PlayerController>().gameObject; 
    }

    // Update is called once per frame
    void Update() {
        if (!GetComponentInParent<PhotonView>().IsMine || player.GetComponentInChildren<PlayerUI>().craftItemsUI.activeSelf) return; 

        bool left = false, right = false; 
        if (Input.GetKey(KeyCode.Mouse0)) left = true;
        else if (Input.GetKey(KeyCode.Mouse1)) right = true;
        RaycastHit hit;
        if ((left || right) && Physics.Raycast(transform.parent.parent.position, transform.parent.parent.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask)) {
            if (player.GetComponent<PlayerController>().inventory[bulletItemIndex] > 0) {
                // Debug.Log("hit " + hit.collider.name); 
                if (hit.collider.tag == "Chunk") {
                    Chunk chunk = hit.collider.GetComponent<Chunk>();
                    PlanetGeneration planetGen = chunk.planet.GetComponent<PlanetGeneration>();

                    planetGen.RedrawChunk(chunk.chunkID, chunk.startPos, hit.point, hit.collider.transform.parent.position, brushSize, brushSpeed, left, Time.deltaTime);

                    player.GetComponent<PlayerController>().plasmaRemaining -= useRate * Time.deltaTime;
                    player.GetComponent<PlayerController>().inventory[bulletItemIndex] = (int)Mathf.Ceil(player.GetComponent<PlayerController>().plasmaRemaining);
                    player.GetComponent<PlayerController>().ShootBullet(muzzle.transform.position, hit.point, 1, 1f); 
                }
            } else {
                player.GetComponent<PlayerController>().ErrorMsg("Out of " + player.GetComponent<PlayerController>().UI.itemSprites[bulletItemIndex].name); 
            }
        }
    }
}