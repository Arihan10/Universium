using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro; 

public class PlayerController : MonoBehaviour
{
    [SerializeField] float[] itemMineTimes; 
    [SerializeField] float detectVehicleRadius = 10f, detectMetalDistance = 4f, waterDamage = 5f, maxHealth = 100f; 
    float pickupStartTime, miningSpeedFactor = 1f; 
    public float plasmaRemaining = 0f, health = 100f; 
    
    PhotonView PV; 

    public bool piloting = false, landed = false, radarUse = false; 

    public GameObject[] itemPrefabs, bulletTrails; 
    public GameObject ship; 
    [SerializeField] GameObject itemHolder, radar; 

    public List<GameObject> holdableItems = new List<GameObject>(); 

    public PlayerManager playerManager; 

    public PlayerUI UI; 

    Transform camTransform;

    [SerializeField] LayerMask metalRaycastMask; 

    int currItemIndex = 0; 

    public string currPlanet = ""; 

    // Iron - 0, // Rubber - 1, // Iron Gun - 2, // Rubber Armor - 3, Plasma - 4, Plasma Gun - 5, Energy Crystal - 6
    public int[] inventory, itemUnitAmounts; 

    [SerializeField] int totalItems = 1; 

    private KeyCode[] numKeyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         // KeyCode.Alpha6,
         // KeyCode.Alpha7,
         // KeyCode.Alpha8,
         // KeyCode.Alpha9,
     };

    private void Awake() {
        PV = GetComponent<PhotonView>();
        camTransform = GetComponent<FirstPersonController>().cameraTransform;

        // playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        // PV.RPC("SetPlayerReference_RPC", RpcTarget.All); 

        inventory = new int[totalItems];
        // abilities = new int[totalItems]; 
    }

    [PunRPC]
    void SetPlayerReference_RPC() {
        // playerManager.GetComponent<PlayerManager>().player = gameObject; 
    }

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>(); 

        if (!PV.IsMine) {
            Destroy(GetComponentInChildren<Camera>()); 
            Destroy(GetComponentInChildren<Terraform>()); 
            Destroy(GetComponentInChildren<AudioListener>()); 
            Destroy(GetComponent<Rigidbody>()); 
            Destroy(UI.gameObject);
            gameObject.layer = LayerMask.NameToLayer("RemotePlayer"); 
        }
    }

    // Update is called once per frame
    void Update() {
        if (PV == null || !PV.IsMine) return;

        UI.interactableText.gameObject.SetActive(false);

        if (!piloting) {
            if (Input.GetKeyDown(KeyCode.C)) {
                UI.ToggleCrafting(); 
            }

            if (UI.craftItemsUI.activeSelf) return; 

            bool _metal = false;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectVehicleRadius);
            foreach (var hitCollider in hitColliders) {
                if (hitCollider.tag == "Ship" && !hitCollider.transform.parent.GetComponent<ShipMovement>().player) {
                    InteractiveAlert("<b>F</b> to fly <b>SHIP</b>");
                    if (Input.GetKeyDown(KeyCode.F)) {
                        if (!hitCollider.transform.parent.GetComponent<PhotonView>().IsMine) hitCollider.transform.parent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                        PilotShip(hitCollider.gameObject);
                    }
                } else if (hitCollider.tag == "Metal") _metal = true; 
            }

            if (Input.GetKeyDown(KeyCode.T)) {
                if (PhotonNetwork.PlayerList.Length > 1 && Random.Range(0, 2) == 0) {
                    GameObject[] _players = GameObject.FindGameObjectsWithTag("Player");
                    for (int i = 0; i < _players.Length; ++i) {
                        if (!_players[i].GetComponent<PhotonView>().IsMine) {
                            transform.position = _players[i].transform.position + new Vector3(1, 1, 1);
                            Debug.Log("Teleported");
                            break;
                        }
                    }
                } else {
                    transform.position = GameObject.FindGameObjectWithTag("Ship").transform.position + new Vector3(1, 1, 1);
                }
            }

            RaycastHit _hit;
            GameObject _currItem = gameObject;
            if (_metal && Physics.Raycast(camTransform.position, camTransform.TransformDirection(Vector3.forward), out _hit, detectMetalDistance, metalRaycastMask)) {
                _currItem = _hit.collider.gameObject;
                if (_hit.collider.tag == "Metal") {
                    string _itemName = _hit.collider.name;
                    if (Input.GetKeyDown(KeyCode.F)) {
                        /*if (_itemName == "Iron") {
                            StartMining(0); // Iron
                        } else if (_itemName == "Tree") {
                            StartMining(1); // Rubber
                        } else if (_itemName == "Plasma") {
                            StartMining(4); 
                        }*/
                        StartMining(_hit.collider.GetComponentInParent<ItemInfo>().itemIndex); 
                    } else if (Input.GetKeyUp(KeyCode.F)) {
                        StopMining(); 
                    }

                    InteractiveAlert("HOLD <B>F</B> to mine <b>" + _itemName.ToUpper() + "</b>");
                } else {
                    StopMining();
                }
            } else StopMining(); 

            if (currItemIndex != -1) {
                if (Time.time - pickupStartTime < itemMineTimes[currItemIndex] * miningSpeedFactor) {
                    UI.progressBar.transform.localScale = new Vector3((Time.time - pickupStartTime) / (itemMineTimes[currItemIndex] * miningSpeedFactor), UI.progressBar.transform.localScale.y, UI.progressBar.transform.localScale.z); 
                } else {
                    inventory[currItemIndex] += itemUnitAmounts[currItemIndex]; 
                    // PV.RPC("DestroyItem_RPC", RpcTarget.All, _currItem.GetComponentInParent<ItemInfo>().ID); 
                    DestroyItem(_currItem.GetComponentInParent<ItemInfo>().ID); 
                    StopMining(); 
                    UI.UpdateInventory();
                    AudioManager.instance.Mine(); 
                }
            }

            for (int i = 0; i < itemHolder.transform.childCount; ++i) {
                if (Input.GetKeyDown(numKeyCodes[i])) {
                    // itemHolder.transform.GetChild(i).gameObject.SetActive(true); 
                    if (itemHolder.transform.GetChild(i).name == "Heavy Miner") miningSpeedFactor = 0.2f;
                    else miningSpeedFactor = 1f;
                    PV.RPC("EquipItem_RPC", RpcTarget.All, i); 
                    break; 
                }
            }

            // CHEATS
            if (Input.GetKeyDown("[0]")) {
                inventory[6] = 100; 
            } else if (Input.GetKeyDown("[1]")) {
                inventory[8] = 100; 
            } else if (Input.GetKeyDown("[2]")) {
                inventory[9] = 100; 
            } else if (Input.GetKeyDown("[3]")) {
                inventory[10] = 100; 
            } else if (Input.GetKeyDown("[4]")) {
                inventory[11] = 1; 
            } else if (Input.GetKeyDown("[5]")) {
                inventory[12] = 1; 
            } else if (Input.GetKeyDown("[6]")) {
                inventory[20] = 100; 
            }

            if (Input.GetKeyDown(KeyCode.H)) {
                UI.helpUI.SetActive(!UI.helpUI.activeSelf); 
            }

            if (Input.GetKeyDown(KeyCode.R) && radarUse) {
                radar.GetComponent<RadarDevice>().Refresh(); 
                radar.SetActive(!radar.activeSelf); 
            }

            if (GetComponent<FirstPersonController>().planet.GetComponent<PlanetGeneration>().electric && GetComponent<FirstPersonController>().underwater && inventory[3] < 1 && landed) {
                TakeDamage(waterDamage * Time.deltaTime); 
            }
        } else {
            if (Input.GetKeyDown(KeyCode.F)) {
                StopPilotingShip(); 
            }
        }

        Vector3 healthScale = UI.healthBar.transform.localScale; 
        UI.healthBar.transform.localScale = new Vector3(Mathf.Clamp(health / maxHealth, 0f, 1f), healthScale.y, healthScale.z);
    }

    [PunRPC]
    void EquipItem_RPC(int _index) {
        for (int i = 0; i < itemHolder.transform.childCount; ++i) {
            if (i == _index) itemHolder.transform.GetChild(i).gameObject.SetActive(true);
            else itemHolder.transform.GetChild(i).gameObject.SetActive(false); 
        }
    }

    public void DestroyItem(int _ID) {
        PV.RPC("DestroyItem_RPC", RpcTarget.All, _ID); 
    }

    [PunRPC]
    void DestroyItem_RPC(int _ID) {
        Destroy(GameManager.instance.items[_ID]);
        GameManager.instance.items[_ID] = null; 
    }

    void StartMining(int _index) {
        currItemIndex = _index; 
        pickupStartTime = Time.time; 
        UI.progressBar.transform.parent.gameObject.SetActive(true); 
    }

    void StopMining() {
        pickupStartTime = Time.time; 
        currItemIndex = -1;
        UI.progressBar.transform.parent.gameObject.SetActive(false); 
    }

    public void AddHoldableItem(CraftItem _item) {
        UI.AddHoldable(_item); 
        PV.RPC("AddHoldeableItem_RPC", RpcTarget.All, _item.itemIndex); 
    }

    [PunRPC]
    void AddHoldeableItem_RPC(int _itemIndex) {
        holdableItems.Add(itemPrefabs[_itemIndex]);
        GameObject _holdable = Instantiate(itemPrefabs[_itemIndex], itemHolder.transform);
        _holdable.name = itemPrefabs[_itemIndex].name; 
        _holdable.SetActive(false);
        ++inventory[_itemIndex]; 
    }

    public void AddAbility(CraftItem _item) {
        if (inventory[_item.itemIndex] < 1) UI.AddAbility(_item); 
        inventory[_item.itemIndex] += _item.amount; 
        if (_item.itemIndex == 19) radarUse = true; 
    }

    public void AddAmmo(CraftItem _item) {
        inventory[_item.itemIndex] += _item.amount;
        plasmaRemaining = inventory[10]; 
    }

    void InteractiveAlert(string _alertText) {
        UI.interactableText.gameObject.SetActive(true);
        UI.interactableText.text = _alertText; 
    }

    void PilotShip(GameObject _ship) {
        piloting = true;
        ship = _ship.transform.parent.gameObject; 
        GetComponent<FirstPersonController>().enabled = false; 
        GetComponent<FirstPersonController>().cameraTransform.gameObject.SetActive(false); 
        ship.GetComponent<ShipMovement>().StartPiloting(PV); 
        UI.characterUI.SetActive(false);
        UI.shipUI.SetActive(true); 
    }

    void StopPilotingShip() {
        piloting = false;
        GetComponent<FirstPersonController>().enabled = true;
        GetComponent<FirstPersonController>().cameraTransform.gameObject.SetActive(true);
        ship.GetComponent<ShipMovement>().StopPiloting();
        transform.position = ship.transform.position + ship.transform.up * 10f; 
        UI.characterUI.SetActive(true);
        UI.shipUI.SetActive(false); 
    }

    public void TakeDamage(float _damage) {
        AudioManager.instance.Hurt(); 
        PV.RPC("TakeDamage_RPC", RpcTarget.All, _damage); 
    }

    [PunRPC]
    void TakeDamage_RPC(float _damage) {
        health -= _damage; 

        if (health <= 0f) {
            PlayerController[] _players = FindObjectsOfType<PlayerController>(); 
            foreach (PlayerController _player in _players) {
                if (_player.PV.IsMine) {
                    _player.Die();
                    break; 
                }
            }
        }
    }

    public void Die() {
        UI.GameOver();
        GetComponent<FirstPersonController>().enabled = false; 
        GetComponent<PlayerController>().enabled = false; 
    }

    public void ShootGrenade(Vector3 _pos, Vector3 _dir, int _index, float _throwForce) {
        PV.RPC("ShootGrenade_RPC", RpcTarget.All, _pos, _dir, _index, _throwForce); 
    }

    [PunRPC]
    void ShootGrenade_RPC(Vector3 _pos, Vector3 _dir, int _index, float _throwForce) {
        GameObject _grenade = Instantiate(itemPrefabs[_index], _pos, Quaternion.identity); 

        _grenade.GetComponent<Rigidbody>().AddForce(_dir * _throwForce, ForceMode.Impulse);
        _grenade.GetComponent<Grenade>().player = gameObject; 

        _grenade.GetComponent<ItemInfo>().ID = GameManager.instance.items.Count; 
        GameManager.instance.items.Add(_grenade); 
    }

    public void ShootBullet(Vector3 _pos, Vector3 _target, int _bulletIndex, float _speed) {
        PV.RPC("ShootBullet_RPC", RpcTarget.All, _pos, _target, _bulletIndex, _speed); 
    }

    [PunRPC]
    void ShootBullet_RPC(Vector3 _pos, Vector3 _target, int _bulletIndex, float _speed) {
        GameObject _bullet = Instantiate(bulletTrails[_bulletIndex], _pos, Quaternion.identity); 
        _bullet.GetComponent<Tracer>().targetPos = _target; 
        // _bullet.GetComponent<Tracer>().speed = _speed; 
    }

    IEnumerator Fade(float increment, float durationSecs, float stayDurationSecs, string _text) {
        // UI.ToggleCrosshair(false); 
        UI.planetNameHolder.SetActive(true);
        UI.planetNameHolder.GetComponentInChildren<TextMeshProUGUI>().text = _text; 
        UI.characterUI.SetActive(false); 

        Color _col = UI.planetNameHolder.GetComponentInChildren<TextMeshProUGUI>().faceColor;
        Color _col2 = UI.planetNameHolder.GetComponentInChildren<Image>().color; 

        for (int i = 0; i < durationSecs/increment; ++i) {
            UI.planetNameHolder.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, increment / durationSecs * (i));
            UI.planetNameHolder.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, increment / durationSecs * (i));

            yield return new WaitForSeconds(increment);
        }

        UI.planetNameHolder.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, 1f); 
        UI.planetNameHolder.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, 1f); 

        yield return new WaitForSeconds(stayDurationSecs); 

        for (int i = 0; i < durationSecs / increment; ++i) {
            UI.planetNameHolder.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, 1f - increment / durationSecs * (i));
            UI.planetNameHolder.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, 1f - increment / durationSecs * (i));

            yield return new WaitForSeconds(increment);
        }

        UI.planetNameHolder.SetActive(false); 
        UI.characterUI.SetActive(true); 
    }

    private void OnCollisionEnter(Collision collision) {
        if (!PV.IsMine) {
            /*if (collision.collider.tag == "Chunk") {
                currPlanet = collision.transform.parent.name; 
            }*/
            return;
        }

        if (collision.collider.tag == "Chunk") {
            if (!landed) {
                landed = true;
            }

            if (GameManager.instance.planetsMade == GameManager.instance.totalPlanets) {
                UI.loadingPanel.SetActive(false);
            }

            if (collision.transform.parent.name != currPlanet) {
                // currPlanet = collision.transform.parent.name; 
                PV.RPC("SetPlanetName_RPC", RpcTarget.All, collision.transform.parent.name); 
                // StartCoroutine(Fade(0.01f, 1f, 3f, currPlanet)); 
                UI.DisplayMessage(UI.planetNameHolder, 0.01f, 1f, 3f, currPlanet);
            }
        }
    }

    [PunRPC]
    void SetPlanetName_RPC(string _name) {
        currPlanet = _name; 
    }

    public void ErrorMsg(string _message) {
        UI.DisplayMessage(UI.errorMsgHolder, 0.01f, 0.3f, 1f, _message); 
    }
}
