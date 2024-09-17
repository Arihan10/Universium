using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Grenade : MonoBehaviour
{
    public GameObject player; 

    [SerializeField] float explosionRadius = 10f, damage = 100f; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision) {
        if (player.GetComponent<PhotonView>().IsMine) {
            Explode(); 
        }
    }

    void Explode() {
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius); 

        foreach (Collider _col in cols) {
            float _damage = Mathf.Clamp(damage / Vector3.Distance(transform.position, _col.transform.position), 0f, 100f); 

            if (_col.tag == "Enemy") {
                _col.GetComponentInParent<EnemyAI>().TakeDamage(_damage); 
            } else if (_col.tag == "Player") {
                _col.GetComponentInParent<PlayerController>().TakeDamage(_damage); 
            }
        }

        player.GetComponent<PlayerController>().DestroyItem(GetComponent<ItemInfo>().ID); 
    }
}
