using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Shoot : MonoBehaviour
{
    GameObject camera;

    [SerializeField] LayerMask mask;

    PhotonView PV;

    [SerializeField] float damage = 25f, fireRate = 4f, throwForce = 10f;
    float lastFireTime; 

    [SerializeField] bool automatic = false, RPG = false; 

    [SerializeField] int bulletItemIndex = 0, bulletPrefabIndex = 0; 

    [SerializeField] GameObject bulletExitPoint; 

    PlayerController player; 
    
    // Start is called before the first frame update
    void Start()
    {
        camera = transform.parent.parent.gameObject;
        PV = GetComponentInParent<PhotonView>();
        lastFireTime = Time.time;

        player = GetComponentInParent<PlayerController>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine || player.UI.craftItemsUI.activeSelf) return; 

        if (((!automatic && Input.GetKeyDown(KeyCode.Mouse0)) || (automatic && Input.GetKey(KeyCode.Mouse0))) && Time.time - lastFireTime > 1f/fireRate) {
            if (player.inventory[bulletItemIndex] > 0) {
                --player.inventory[bulletItemIndex]; 
                GetComponent<ProceduralRecoil>().Recoil(); 

                if (!RPG) {
                    RaycastHit hit;
                    if (Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask)) {
                        Debug.Log(hit.collider.name);

                        if (hit.collider.tag == "Enemy") {
                            // PV.RPC("HitEnemyAI_RPC", RpcTarget.All, hit.collider.GetComponent<PhotonView>().ViewID); 
                            hit.collider.GetComponentInParent<EnemyAI>().TakeDamage(damage);
                        } else if (hit.collider.tag == "Player") {
                            hit.collider.GetComponent<PlayerController>().TakeDamage(damage);
                        }

                        player.ShootBullet(bulletExitPoint.transform.position, hit.point, bulletPrefabIndex, 1f);
                        AudioManager.instance.Bullet(); 
                    }
                } else {
                    player.ShootGrenade(bulletExitPoint.transform.position, camera.transform.forward, 16, throwForce); 
                }

                lastFireTime = Time.time; 
            } else {
                // player.UI.DisplayMessage(player.UI.errorMsgHolder, 0.01f, 0.3f, 1f, "Out of "+player.UI.itemSprites[bulletItemIndex].name); 
                player.ErrorMsg("Out of " + player.UI.itemSprites[bulletItemIndex].name); 
            }
        }
    }
}
