using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviourPunCallbacks {
    PhotonView PV;

    SimplexNoiseGenerator noise;

    [SerializeField] GameObject planetGenerator;
    public GameObject player;

    [SerializeField] float homePlanetRadius = 50f;
    float startTime;

    [SerializeField] int homePlanetRange = 120, totalPlanets = 0;

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }

    IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);

        if (PV.IsMine && PhotonNetwork.IsMasterClient) {
            // GeneratePlanet(Vector3.zero, true);
            yield return PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "HomePlanet"), Vector3.zero, Quaternion.identity).GetComponent<PlanetGeneration>().SetValues(); 

            Debug.Log("first planet at " + Time.time);

            // ++GravitySimulation.instance.planetsMade; 

            for (int i = 0; i < totalPlanets; ++i) {
                // PV.RPC("SendPacket_RPC", RpcTarget.All); 
                yield return new WaitForSeconds(time);
                // GeneratePlanet(new Vector3(Random.Range(-3000, 3000), Random.Range(-3000, 3000), Random.Range(-3000, 3000))); 
                yield return PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "HomePlanet"), Vector3.zero, Quaternion.identity).GetComponent<PlanetGeneration>().SetValues(); 
                Debug.Log("new planet at " + Time.time);

                // ++GravitySimulation.instance.planetsMade;
            }

            Debug.Log("end at " + Time.time);

            PV.RPC("SpawnShips", RpcTarget.All);
        }
    }

    [PunRPC]
    void SendPacket_RPC() {
        Debug.Log("Packet sent");
    }

    [PunRPC]
    void SpawnShips() {
        Vector3 homePlanetPos = new Vector3(homePlanetRange, homePlanetRange, homePlanetRange);
        Vector3 shipPosition = Random.onUnitSphere * homePlanetRadius * 4 + homePlanetPos;

        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Spaceship"), shipPosition, Quaternion.identity);
    }

    // Start is called before the first frame update
    void Start() {
        GameManager.instance.totalPlanets = totalPlanets + 1; 

        if (PV.IsMine) CreateController(); 

        // startTime = Time.time; 
        Debug.Log("start at " + Time.time);
        StartCoroutine(Delay(0.05f)); 
    }

    void CreateController() {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), new Vector3(140, 140, 140), Quaternion.identity, 0, new object[] { PV.ViewID });
    }

    /* void GeneratePlanet(Vector3 _position, bool initial = false) {
        noise = new SimplexNoiseGenerator(); 
        Vector3 ground = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
        Vector3 cliff = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
        Vector3 ground2 = new Vector3(Mathf.Abs(ground.x - 0.15f), Mathf.Abs(ground.y - 0.15f), Mathf.Abs(ground.z - 0.15f)); 
        Vector3 waterCol = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)); 
        Vector3 atmo = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
        if (!initial) PV.RPC("GeneratePlanet_RPC", RpcTarget.All, noise.GetSeed(), ground, cliff, ground2, waterCol, Random.Range(1,12), atmo, _position); 
        else PV.RPC("GeneratePlanetInitial_RPC", RpcTarget.All, noise.GetSeed(), ground, cliff, ground2, waterCol, Random.Range(1, 12), atmo, _position); 
    } */

    [PunRPC]
    public void GeneratePlanet_RPC(string seed, Vector3 _ground, Vector3 _cliff, Vector3 _ground2, Vector3 waterCol, int waterOffset, Vector3 atmo, Vector3 _position) {
        GameObject _planet = Instantiate(planetGenerator, Vector3.zero, Quaternion.identity);
        _planet.GetComponent<PlanetGeneration>().GeneratePlanet(seed, _ground, _cliff, _ground2, waterCol, waterOffset, atmo, _position);
    }

    [PunRPC]
    public void GeneratePlanetInitial_RPC(string seed, Vector3 _ground, Vector3 _cliff, Vector3 _ground2, Vector3 waterCol, int waterOffset, Vector3 atmo, Vector3 _position) {
        GameObject.Find("HomePlanet").GetComponent<PlanetGeneration>().GeneratePlanet(seed, _ground, _cliff, _ground2, waterCol, waterOffset, atmo, _position);
    }

    private void OnDestroy() {
        Debug.Log("the frick?");
    }
}