using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlanetGeneration : MonoBehaviour {
    [HideInInspector] public float[,,] matrix;
    [SerializeField] int xRange, yRange, zRange, chunkSize = 10, homePlanetRange = 120;
    // [SerializeField] int octaves = 1; 
    // resolution 150 = noise scale 1
    public float threshold;
    [SerializeField] float radius = 1f, noiseScale = 1.3333f, homePlanetRadius = 50f;
    // [SerializeField] float noiseScale = 0.9f, amplitude = 0.5f, lacunarity = 2f, persistence = 0.9f, radius = 20f; 
    float noiseSeed;

    Mesh mesh;

    List<Vector3> verts, totalVerts = new List<Vector3>(); 
    List<int> triangles; 

    SimplexNoiseGenerator noise; 

    Dictionary<Vector3, int> vertsDict = new Dictionary<Vector3, int>();

    [SerializeField] GameObject pointPrefab, meshPrefab, water, atmospherePrefab, enemyPrefab, magneticCore, moltenCore; 
    public GameObject waterSphere, atmosphere; 
    [SerializeField] GameObject[] itemPrefabs; 
    public List<GameObject> meshes = new List<GameObject>(); 
    public GameObject[,,] chunks;

    [SerializeField] NoiseSettings[] noiseSettings;

    [SerializeField] Color electricWaterCol, electricWaterRippleCol; 
    Color ground, cliff, ground2;

    Vector3 prevPos;

    PhotonView PV;

    string seedRPC;
    Vector3 groundRPC, cliffRPC, ground2RPC, waterColRPC, atmoRPC, positionRPC;
    int waterOffsetRPC, numAliens = 0;

    public bool electric = false; 
    bool RPCDoneOnline = false, RPCDoneLocal = false, atmosphereSpawn = true; 

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start() {
        // PV = GetComponent<PhotonView>(); 

        // name = "Planet"; 

        // Strategy: Generate noise object, get seed, pass into RPC with all this generation/initialization code BOTH
    }

    void SendPacket() {
        PV.RPC("SendPacket_RPC", RpcTarget.All);
    }

    [PunRPC]
    void SendPacket_RPC() {
        Debug.Log("sent"); 
    }

    public bool SetValues() {
        // if (PhotonNetwork.IsMasterClient) {
            noise = new SimplexNoiseGenerator();
            Vector3 _ground = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Vector3 _cliff = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Vector3 _ground2 = new Vector3(Mathf.Abs(_ground.x - 0.15f), Mathf.Abs(_ground.y - 0.15f), Mathf.Abs(_ground.z - 0.15f));
            Vector3 _waterCol = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f));
            Vector3 _atmo = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            Vector3 _position = new Vector3(Random.Range(-6000, 6000), Random.Range(-6000, 6000), Random.Range(-6000, 6000));
            float _radius = radius;
            int _range = xRange; 
            string _planetName = PlanetNameGenerator.planetNames[Random.Range(0, PlanetNameGenerator.planetNames.Length)]; 
            if (GameManager.instance.planetsMade == 0) {
                _position = Vector3.zero;
                _radius = homePlanetRadius; // RESTORE WHEN PLAYING
                _range = homePlanetRange; // RESTORE WHEN PLAYING
                _planetName = "Home Planet"; 
            }
            bool _electric = false; 
            if (Random.Range(0, 2) == 1) _electric = true;
            bool _atmosphere = true;
            if (Random.Range(0, 4) == 1) _atmosphere = false; 

            // _radius = homePlanetRadius; // COMMENT WHEN PLAYING
            // _range = homePlanetRange; // COMMENT WHEN PLAYING
            // _electric = true; // COMMENT WHEN PLAYING
            
            PV.RPC("SetValues_RPC", RpcTarget.All, noise.GetSeed(), _ground, _cliff, _ground2, _waterCol, Random.Range(1, 12), _atmo, _position, _radius, _range, _planetName, _electric, _atmosphere);

        return true; 
        // }
    }

    [PunRPC]
    void SetValues_RPC(string seed, Vector3 _ground, Vector3 _cliff, Vector3 _ground2, Vector3 _water, int waterOffset, Vector3 _atmo, Vector3 _position, float _radius, int _range, string _planetName, bool _electric, bool _atmosphere) {
        seedRPC = seed;
        groundRPC = _ground;
        cliffRPC = _cliff;
        ground2RPC = _ground2;
        waterColRPC = _water;
        waterOffsetRPC = waterOffset;
        atmoRPC = _atmo;

        positionRPC = _position;
        radius = _radius;
        xRange = _range;
        zRange = _range;
        yRange = _range;
        name = _planetName;

        electric = _electric;
        atmosphereSpawn = _atmosphere; 

        // YO MAKE A DEFINED ATMOSPHERE RADIUS LOL

        GetComponent<CelestialBody>().mass = 4f / 3f * Mathf.PI * radius * radius * radius * 278445738f; 

        RPCDoneOnline = true;
    }

    // [PunRPC]
    public void GeneratePlanet(string seed, Vector3 _ground, Vector3 _cliff, Vector3 _ground2, Vector3 _water, int waterOffset, Vector3 _atmo, Vector3 _position) {
        // Debug.Log("frick you unity"); 
        matrix = new float[xRange, zRange, yRange];
        chunks = new GameObject[(int)(xRange / chunkSize) + 1, (int)(zRange / chunkSize) + 1, (int)(yRange / chunkSize) + 1];
        noiseSeed = Random.Range(0, 1000);

        noise = new SimplexNoiseGenerator(seed); 

        // SendPacket(); 

        // Debug.Log("SEED-REAL " + noise.GetSeed() + " SEED-EXPECTED " + seed);

        /* ground = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        cliff = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        // ground2 = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
        ground2 = new Color(Mathf.Abs(ground.r - 0.15f), Mathf.Abs(ground.g - 0.15f), Mathf.Abs(ground.b - 0.15f)); */
        ground = new Vector4(_ground.x, _ground.y, _ground.z, 1);
        cliff = new Vector4(_cliff.x, _cliff.y, _cliff.z, 1);
        ground2 = new Vector4(_ground2.x, _ground2.y, _ground2.z);

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // waterSphere = Instantiate(water, new Vector3(xRange, yRange, zRange) / 2f, Quaternion.identity);
        waterSphere = Instantiate(water, transform);
        waterSphere.transform.position = new Vector3(xRange, yRange, zRange) / 2f;
        waterSphere.transform.localScale = new Vector3(1f, 1f, 1f) * (radius * 2f - waterOffset);
        // waterSphere.GetComponent<MeshRenderer>().material.color = new Vector4(_water.x, _water.y, _water.z, waterSphere.GetComponent<MeshRenderer>().material.color.a);
        float factor = Mathf.Pow(2, 1.416925f); 
        if (!electric) {
            waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseCol", new Vector4(_water.x, _water.y, _water.z, waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol").a));
            waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_RippleCol", new Vector4((_water.x + 0.2f) * factor, (_water.y + 0.2f) * factor, (_water.z + 0.2f) * factor, waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol").a));
        } else {
            waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseCol", new Vector4(electricWaterCol.r, electricWaterCol.g, electricWaterCol.b, waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol").a));
            // waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_RippleCol", new Vector4((electricWaterCol.r + 0.2f) * factor, (electricWaterCol.g + 0.2f) * factor, (electricWaterCol.b + 0.2f) * factor, waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol").a)); 
            waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_RippleCol", new Vector4((electricWaterRippleCol.r + 0.2f) * factor, (electricWaterRippleCol.g + 0.2f) * factor, (electricWaterRippleCol.b + 0.2f) * factor, waterSphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol").a)); 
        }

        if (atmosphereSpawn) {
            atmosphere = Instantiate(atmospherePrefab, waterSphere.transform);
            // atmosphere.transform.localScale = new Vector3(1f, 1f, 1f) * ((xRange + Random.Range(125, 500)) / waterSphere.transform.lossyScale.x);
            atmosphere.transform.localScale = new Vector3(1f, 1f, 1f) * Random.Range(2f, 5f);
            Color col = new Vector4(_atmo.x, _atmo.y, _atmo.z, atmosphere.GetComponent<MeshRenderer>().material.color.a);
            atmosphere.GetComponent<MeshRenderer>().material.SetColor("_Color", col);
            atmosphere.GetComponent<MeshRenderer>().material.SetColor("_Color2", col -= (col / 100f * 65f));
            atmosphere.GetComponent<MeshRenderer>().material.SetVector("_Position", waterSphere.transform.position);
            atmosphere.GetComponent<MeshRenderer>().material.SetFloat("_Radius", atmosphere.transform.localScale.x / 2f);
            col = new Vector4(_atmo.x, _atmo.y, _atmo.z, atmosphere.GetComponent<MeshRenderer>().material.color.a);
            atmosphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", col);
            atmosphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color2", col -= (col / 100f * 65f));
            atmosphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetVector("_Position", waterSphere.transform.position);
            atmosphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_Radius", atmosphere.transform.localScale.x / 2f);
        }

        SendPacket();

        CreateShape(_position); 
        // SendPacket(); 
        // MakePoints(); 
    }

    void CreateShape(Vector3 _position) {
        if (verts == null) {
            verts = new List<Vector3>();
            triangles = new List<int>();
        } else {
            verts.Clear();
            triangles.Clear();
        }

        ++GameManager.instance.planetsMade; 

        List<Vector3> ironPos = new List<Vector3>();
        List<Vector3> ironRots = new List<Vector3>(); 

        // mesh.vertices = verts.ToArray();
        // mesh.triangles = triangles.ToArray(); 

        // SendPacket(); 

        foreach (GameObject _mesh in meshes) Destroy(_mesh);

        // SendPacket(); 

        for (int i = 0; i < xRange; ++i) {
            for (int j = 0; j < zRange; ++j) {
                for (int k = 0; k < yRange; ++k) {
                    if (i == 0 || i == xRange - 1 || j == 0 || j == zRange - 1 || k == 0 || k == yRange - 1) {
                        matrix[i, j, k] = threshold - 1;
                    } else {
                        // matrix[i, j, k] = Perlin3D(i/(float)xRange * noiseScale + noiseSeed, j/(float)zRange * noiseScale + noiseSeed, k/(float)yRange * noiseScale + noiseSeed);
                        // matrix[i, j, k] = -(Vector3.Distance(new Vector3(xRange / 2f, yRange / 2f, zRange / 2f), new Vector3(i * interval, k * interval, j * interval)) - radius) + Perlin3D(i / (float)xRange * noiseScale + noiseSeed, j / (float)zRange * noiseScale + noiseSeed, k / (float)yRange * noiseScale + noiseSeed) * 5f + noise.coherentNoise(i * 0.9f, j * 0.9f, k * 0.9f, octaves, (int)noiseScale, amplitude, lacunarity, persistence) * 10f; 
                        // Debug.Log(matrix[i, j, k] + " " + i / (float)xRange + " " + j / (float)zRange + " " + k / (float)yRange);

                        matrix[i, j, k] = -(Vector3.Distance(new Vector3(xRange / 2f, yRange / 2f, zRange / 2f), new Vector3(i, k, j)) - radius);

                        float _amp = 50f;
                        float _freq = 1f;
                        for (int iter = 0; iter < 5; ++iter) {
                            matrix[i, j, k] += noise.coherentNoise((i * 0.9f * _freq) / noiseScale, (j * 0.9f * _freq) / noiseScale, (k * 0.9f * _freq) / noiseScale) * _amp * 3f;
                            _freq *= 2f;
                            _amp /= 2f;
                        }

                        /*foreach (NoiseSettings _noise in noiseSettings) {
                            matrix[i, j, k] += noise.coherentNoise((i * 0.9f) / noiseScale, (j * 0.9f) / noiseScale, (k * 0.9f) / noiseScale, _noise.octaves, _noise.noiseScale, _noise.amplitude, _noise.lacunarity, _noise.persistence); 
                        }*/
                    }
                }
            }
            SendPacket();
        }

        for (int x = 0; x < xRange - 1; x += chunkSize) {
            for (int z = 0; z < zRange - 1; z += chunkSize) {
                for (int y = 0; y < yRange - 1; y += chunkSize) {

                    List<Vector3> triVerts = new List<Vector3>();
                    for (int i = x; i < x + chunkSize && i < xRange - 1; ++i) {
                        for (int j = z; j < z + chunkSize && j < zRange - 1; ++j) {
                            for (int k = y; k < y + chunkSize && k < yRange - 1; ++k) {
                                // binary to decimal
                                int sum = 0;
                                for (int l = 0; l < 8; ++l) {
                                    Vector3Int v = MarchingCubesLookup.getVertexOffsets(l);

                                    if (matrix[i + v.x, j + v.z, k + v.y] > threshold) {
                                        sum += (int)Mathf.Pow(2, l);
                                    }

                                    // Debug.Log(matrix[i + v.x, j + v.y, k + v.z]); 
                                }

                                for (int l = 0; l < 4; ++l) {
                                    int edge = MarchingCubesLookup.TriangleConnectionTable[sum, l * 3];
                                    if (edge == -1) break;

                                    int[] tri = new int[3];
                                    for (int m = 0; m < 3; ++m) {
                                        edge = MarchingCubesLookup.TriangleConnectionTable[sum, l * 3 + m];

                                        int vertA = MarchingCubesLookup.edgeLookup[edge, 0];
                                        int vertB = MarchingCubesLookup.edgeLookup[edge, 1];

                                        Vector3 vertAOffset = MarchingCubesLookup.getVertexOffsets(vertA);
                                        Vector3 vertBOffset = MarchingCubesLookup.getVertexOffsets(vertB);

                                        float vertAVal = matrix[i + (int)vertAOffset.x, j + (int)vertAOffset.z, k + (int)vertAOffset.y];
                                        float vertBVal = matrix[i + (int)vertBOffset.x, j + (int)vertBOffset.z, k + (int)vertBOffset.y];

                                        Vector3 vertAPos = new Vector3(i + vertAOffset.x, k + vertAOffset.y, j + vertAOffset.z);
                                        Vector3 vertBPos = new Vector3(i + vertBOffset.x, k + vertBOffset.y, j + vertBOffset.z);

                                        Vector3 vert = linearInterpolate(threshold, vertAPos, vertBPos, vertAVal, vertBVal) + transform.position;

                                        if (!vertsDict.ContainsKey(vert)) {
                                            vertsDict[vert] = verts.Count;
                                            verts.Add(vert);
                                        }

                                        tri[m] = vertsDict[vert];
                                        triVerts.Add(vert);
                                    }

                                    int vertsSize = verts.Count;
                                    triangles.Add(tri[2]);
                                    triangles.Add(tri[1]);
                                    triangles.Add(tri[0]);
                                }
                            }
                        }
                    }

                    GameObject _mesh = Instantiate(meshPrefab, Vector3.zero, Quaternion.identity, transform);
                    _mesh.GetComponent<MeshFilter>().mesh.vertices = verts.ToArray();
                    _mesh.GetComponent<MeshFilter>().mesh.triangles = triangles.ToArray();
                    _mesh.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                    _mesh.GetComponent<Chunk>().chunkID = meshes.Count;
                    _mesh.GetComponent<Chunk>().startPos = new Vector3Int(x, z, y);
                    _mesh.GetComponent<Chunk>().planet = gameObject;
                    _mesh.GetComponent<Chunk>().waterSphere = waterSphere;
                    _mesh.GetComponent<MeshCollider>().sharedMesh = _mesh.GetComponent<MeshFilter>().mesh;
                    // if (verts.Count > 0) _mesh.GetComponent<MeshFilter>().mesh.uv = UvCalculator.CalculateUVs(verts.ToArray(), 10f); 
                    Vector2[] _uvs = new Vector2[verts.Count];
                    // Debug.Log("calculating uvs");
                    for (int i = 0; i < _uvs.Length; i++) {
                        _uvs[i] = new Vector2(verts[i].x, verts[i].z);
                    }
                    _mesh.GetComponent<MeshFilter>().mesh.uv = _uvs;
                    _mesh.GetComponent<MeshRenderer>().material.SetColor("_groundCol", ground);
                    _mesh.GetComponent<MeshRenderer>().material.SetColor("_cliffCol", cliff);
                    _mesh.GetComponent<MeshRenderer>().material.SetColor("_groundDarkCol", ground2);
                    meshes.Add(_mesh);
                    chunks[(int)(x / chunkSize), (int)(z / chunkSize), (int)(y / chunkSize)] = _mesh;

                    for (int i = 0; i < triangles.Count; i += 3) {
                        Vector3 _vert1 = verts[triangles[i]];
                        Vector3 _vert2 = verts[triangles[i + 1]];
                        Vector3 _vert3 = verts[triangles[i + 2]];

                        totalVerts.Add(_vert1); 
                        totalVerts.Add(_vert2); 
                        totalVerts.Add(_vert3); 
                    }

                    if (PhotonNetwork.IsMasterClient) {
                        Vector3[] _meshVerts = _mesh.GetComponent<MeshFilter>().mesh.vertices;
                        foreach (Vector3 _currVert in _meshVerts) {
                            if (Random.Range(0, (int)Mathf.Pow(Vector3.Distance(waterSphere.transform.position, _currVert) * 1f, 2.58f) / 71 - 1000) == 1) {
                            // if (Random.Range(0, 1000) == 1) {
                                Debug.Log(Vector3.Distance(_currVert, waterSphere.transform.position) < waterSphere.transform.localScale.x / 2f);
                                ironPos.Add(_currVert); 
                            }
                        }
                    }

                    verts.Clear();
                    triangles.Clear();
                    vertsDict.Clear();
                }
                SendPacket();
            }
            SendPacket();
        }

        // mesh.vertices = verts.ToArray();
        // mesh.triangles = triangles.ToArray();

        transform.position = _position;
        UpdatePlanetMaterialCentre();
        mesh.RecalculateNormals();

        if (PhotonNetwork.IsMasterClient) {
            if (GameManager.instance.planetsMade != 1) {
                for (int i = 0; i < 70; ++i) {
                    int _index = Random.Range(0, totalVerts.Count / 3);

                    Vector3 _vert1 = totalVerts[_index];
                    Vector3 _vert2 = totalVerts[_index + 1];
                    Vector3 _vert3 = totalVerts[_index + 2];

                    Plane _tri = new Plane(_vert3, _vert2, _vert1);

                    Vector3 _normal = _tri.normal;

                    // float steepness = Vector3.Dot(Vector3.Normalize(_vert1 - new Vector3(xRange, zRange, yRange)), _normal);

                    // if (steepness < 0f && Random.Range(0, 1000) == 1) Instantiate(enemyPrefab, _vert1, Quaternion.identity);
                    //if (steepness < 0f && Random.Range(0, 1000) == 1) {
                    /*GameObject _enemy = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Alien"), _vert1 + transform.position, Quaternion.identity);
                    _enemy.GetComponent<EnemyAI>().planet = gameObject; 
                    _enemy.GetComponent<EnemyAI>().planetCentre = waterSphere.transform.position;*/
                    //}

                    // Debug.Log(steepness); 

                    if (i % 5 == 0) SendPacket();
                }
            }

            List<Vector3> treePos = new List<Vector3>(); 
            List<Vector3> treeRots = new List<Vector3>();
            List<Vector3> energyPos = new List<Vector3>();
            List<Vector3> diamondPos = new List<Vector3>();
            List<Vector3> stalagcitePos = new List<Vector3>();
            List<Vector3> stalagciteRot = new List<Vector3>(); 
            for (int i = 0; i < totalVerts.Count; i += 3) {
                Vector3 _vert1 = totalVerts[i];
                Vector3 _vert2 = totalVerts[i + 1];
                Vector3 _vert3 = totalVerts[i + 2];
                Plane _tri = new Plane(_vert3, _vert2, _vert1);

                Vector3 _normal = _tri.normal;
                float steepness = Vector3.Dot((_vert1 - new Vector3(xRange, zRange, yRange) / 2f).normalized, _normal); 

                if (steepness < -0.85f && Random.Range(0, 1000) == 1) {
                    treePos.Add(_vert1);
                    treeRots.Add(_normal);
                    SendPacket();
                } else if (steepness > 0.935f) {
                    if (Random.Range(0, 12) == 1) diamondPos.Add(_vert1);
                    if (Random.Range(0, 17) == 1) {
                        stalagcitePos.Add(_vert1);
                        stalagciteRot.Add(_normal); 
                    }
                    SendPacket(); 
                    // Debug.Log("place found"); 
                }

                if (Vector3.Distance(_vert1, waterSphere.transform.localPosition) <= waterSphere.transform.localScale.x / 2f && Random.Range(0,1000) == 1) {
                    energyPos.Add(_vert1 - _normal * 10f); 
                }
            }

            PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)ironPos.ToArray(), (object)(new Vector3[ironPos.Count]), 0, true, false, false); 
            PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)treePos.ToArray(), (object)treeRots.ToArray(), 1, false, true, false);
            if (electric) {
                PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)energyPos.ToArray(), (object)(new Vector3[energyPos.Count]), 6, true, false, true);
                PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)stalagcitePos.ToArray(), (object)stalagciteRot.ToArray(), 9, false, false, false); 
            } else {
                PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)diamondPos.ToArray(), (object)(new Vector3[diamondPos.Count]), 8, true, false, false); 
            }

            List<Vector3> corePos = new List<Vector3>();
            corePos.Add(waterSphere.transform.localPosition);
            if (electric) {
                PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)corePos.ToArray(), (object)(new Vector3[1]), 12, true, false, false);
            } else {
                PV.RPC("SpawnItems_RPC", RpcTarget.All, (object)corePos.ToArray(), (object)(new Vector3[1]), 11, true, false, false);
            }
        }

        /*GameObject _core;
        if (electric) {
            _core = Instantiate(magneticCore, transform);
        } else {
            _core = Instantiate(moltenCore, transform);
        }
        _core.transform.localPosition = waterSphere.transform.localPosition;*/

        if (GameManager.instance.planetsMade == GameManager.instance.totalPlanets) {
            Debug.Log("ALL PLANETS DONE");

            GameObject[] _players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject _player in _players) {
                if (_player.GetComponentInChildren<PlayerUI>() && _player.GetComponent<PlayerController>().landed) {
                    _player.GetComponentInChildren<PlayerUI>().loadingPanel.SetActive(false);
                }
            }
        }
    }

    [PunRPC]
    void SpawnItems_RPC(Vector3[] _spawnPos, Vector3[] spawnRots, int itemIndex, bool randomRot, bool treeMat, bool gravityItem) {
        for (int i = 0; i < _spawnPos.Length; ++i) {
            GameObject _item = Instantiate(itemPrefabs[itemIndex], transform); 
            _item.transform.localPosition = _spawnPos[i];
            if (!randomRot) _item.transform.rotation = Quaternion.FromToRotation(-_item.transform.up, spawnRots[i]); 
            else _item.transform.rotation = Random.rotation; 
            _item.name = itemPrefabs[itemIndex].name;
            _item.GetComponent<ItemInfo>().ID = GameManager.instance.items.Count;
            if (treeMat) {
                _item.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = cliff; 
                _item.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1].color = ground; 
            }
            if (gravityItem) {
                _item.GetComponent<GravityItem>().planetCentre = waterSphere.transform.position; 
            }
            GameManager.instance.items.Add(_item); 

            if (i % 5 == 0) Debug.Log("sent packet"); 
        }
    }

    [PunRPC]
    public void RedrawChunkNew_RPC(int _chunkID, Vector3 _startPos, Vector3 hitPointVec, Vector3 hitColPos, int brushSize, float brushSpeed, bool left) {
        // point minus transform
        float hitXF = hitPointVec.x - hitColPos.x, hitZF = hitPointVec.z - hitColPos.z, hitYF = hitPointVec.y - hitColPos.y;
        int hitX = (int)hitXF, hitZ = (int)hitZF, hitY = (int)hitYF;
        Vector3 hitPoint = new Vector3(hitXF, hitYF, hitZF);

        List<Vector3> points = new List<Vector3>();
        for (int i = hitX - brushSize; i < hitX + brushSize; ++i) {
            for (int j = hitZ - brushSize; j < hitZ + brushSize; ++j) {
                for (int k = hitY - brushSize; k < hitY + brushSize; ++k) {
                    if (Vector3.Distance(new Vector3(i, k, j), hitPoint) <= brushSize) {
                        if (left) matrix[i, j, k] += brushSize * brushSpeed * Time.deltaTime;
                        else matrix[i, j, k] -= brushSize * brushSpeed * Time.deltaTime;
                        points.Add(new Vector3(i, j, k)); 
                    }
                }
            }
        }

        // RedrawChunk(_chunkID, _startPos, points);
    }

    public void RedrawChunk(int _chunkID, Vector3 startPos, Vector3 hitPointVec, Vector3 hitColPos, int brushSize, float brushSpeed, bool left, float deltaTime) {
        // PV.RPC("RedrawChunk_RPC", RpcTarget.All, _chunkID, startPos, _points.ToArray()); 
        PV.RPC("RedrawChunk_RPC", RpcTarget.All, _chunkID, startPos, hitPointVec, hitColPos, brushSize, brushSpeed, left, deltaTime);
    }

    [PunRPC]
    // public void RedrawChunk_RPC(int _chunkID, Vector3 _startPos, Vector3[] _pointsL) {
    public void RedrawChunk_RPC(int _chunkID, Vector3 _startPos, Vector3 hitPointVec, Vector3 hitColPos, int brushSize, float brushSpeed, bool left, float deltaTime) {
        Debug.Log("REGERATING");

        // point minus transform
        float hitXF = hitPointVec.x - hitColPos.x, hitZF = hitPointVec.z - hitColPos.z, hitYF = hitPointVec.y - hitColPos.y;
        int hitX = (int)hitXF, hitZ = (int)hitZF, hitY = (int)hitYF;
        Vector3 hitPoint = new Vector3(hitXF, hitYF, hitZF);

        List<Vector3> points = new List<Vector3>();
        for (int i = hitX - brushSize; i < hitX + brushSize; ++i) {
            for (int j = hitZ - brushSize; j < hitZ + brushSize; ++j) {
                for (int k = hitY - brushSize; k < hitY + brushSize; ++k) {
                    if (Vector3.Distance(new Vector3(i, k, j), hitPoint) <= brushSize) {
                        if (left) matrix[i, j, k] += brushSize * brushSpeed * deltaTime;
                        else matrix[i, j, k] -= brushSize * brushSpeed * deltaTime;
                        points.Add(new Vector3(i, j, k));
                    }
                }
            }
        }

        Vector3Int startPos = new Vector3Int((int)_startPos.x, (int)_startPos.y, (int)_startPos.z);
        List<Vector3Int> _points = new List<Vector3Int>();
        foreach (Vector3 vec in points) {
            _points.Add(new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z));
        }

        // Destroy(meshes[_chunkID]); 
        HashSet<Vector3Int> _chunkStarts = new HashSet<Vector3Int>();
        for (int i = 0; i < _points.Count; ++i) {
            Chunk _chunk = chunks[(int)(_points[i].x / chunkSize), (int)(_points[i].y / chunkSize), (int)(_points[i].z / chunkSize)].GetComponent<Chunk>();
            _chunkStarts.Add(_chunk.startPos);
            Destroy(_chunk.gameObject);
        }


        // Chunk _chunk = chunks[(int)(hitPos.x / chunkSize), (int)(hitPos.z / chunkSize), (int)(hitPos.y / chunkSize)].GetComponent<Chunk>();
        // startPos = _chunk.startPos; 
        // Destroy(chunks[(int)(hitPos.x / chunkSize), (int)(hitPos.z / chunkSize), (int)(hitPos.y / chunkSize)]); 

        foreach (Vector3Int _pos in _chunkStarts) {
            startPos = _pos;

            verts.Clear();
            triangles.Clear();
            vertsDict.Clear();

            int x = startPos.x, z = startPos.y, y = startPos.z;

            List<Vector3> triVerts = new List<Vector3>();

            for (int i = x; i < x + chunkSize && i < xRange - 1; ++i) {
                for (int j = z; j < z + chunkSize && j < zRange - 1; ++j) {
                    for (int k = y; k < y + chunkSize && k < yRange - 1; ++k) {
                        // binary to decimal
                        int sum = 0;
                        for (int l = 0; l < 8; ++l) {
                            Vector3Int v = MarchingCubesLookup.getVertexOffsets(l);

                            if (matrix[i + v.x, j + v.z, k + v.y] > threshold) {
                                sum += (int)Mathf.Pow(2, l);
                            }

                            // Debug.Log(matrix[i + v.x, j + v.y, k + v.z]); 
                        }

                        for (int l = 0; l < 4; ++l) {
                            int edge = MarchingCubesLookup.TriangleConnectionTable[sum, l * 3];
                            if (edge == -1) break;

                            int[] tri = new int[3];
                            for (int m = 0; m < 3; ++m) {
                                edge = MarchingCubesLookup.TriangleConnectionTable[sum, l * 3 + m];

                                int vertA = MarchingCubesLookup.edgeLookup[edge, 0];
                                int vertB = MarchingCubesLookup.edgeLookup[edge, 1];

                                Vector3 vertAOffset = MarchingCubesLookup.getVertexOffsets(vertA);
                                Vector3 vertBOffset = MarchingCubesLookup.getVertexOffsets(vertB);

                                float vertAVal = matrix[i + (int)vertAOffset.x, j + (int)vertAOffset.z, k + (int)vertAOffset.y];
                                float vertBVal = matrix[i + (int)vertBOffset.x, j + (int)vertBOffset.z, k + (int)vertBOffset.y];

                                Vector3 vertAPos = new Vector3(i + vertAOffset.x, k + vertAOffset.y, j + vertAOffset.z);
                                Vector3 vertBPos = new Vector3(i + vertBOffset.x, k + vertBOffset.y, j + vertBOffset.z);

                                Vector3 vert = linearInterpolate(threshold, vertAPos, vertBPos, vertAVal, vertBVal) + transform.position;

                                if (!vertsDict.ContainsKey(vert)) {
                                    vertsDict[vert] = verts.Count;
                                    verts.Add(vert);
                                }

                                tri[m] = vertsDict[vert];
                                triVerts.Add(vert);
                            }

                            triangles.Add(tri[2]);
                            triangles.Add(tri[1]);
                            triangles.Add(tri[0]);
                        }
                    }
                }
            }

            GameObject _mesh = Instantiate(meshPrefab, Vector3.zero, Quaternion.identity, transform);
            _mesh.GetComponent<MeshFilter>().mesh.vertices = verts.ToArray();
            _mesh.GetComponent<MeshFilter>().mesh.triangles = triangles.ToArray();
            _mesh.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            _mesh.GetComponent<Chunk>().chunkID = meshes.Count;
            _mesh.GetComponent<Chunk>().startPos = new Vector3Int(x, z, y);
            _mesh.GetComponent<Chunk>().planet = gameObject;
            _mesh.GetComponent<Chunk>().waterSphere = waterSphere;
            _mesh.GetComponent<MeshCollider>().sharedMesh = _mesh.GetComponent<MeshFilter>().mesh;
            // if (verts.Count > 0) _mesh.GetComponent<MeshFilter>().mesh.uv = UvCalculator.CalculateUVs(verts.ToArray(), 10f); 
            Vector2[] _uvs = new Vector2[verts.Count];
            Debug.Log("calculating uvs");
            for (int i = 0; i < _uvs.Length; i++) {
                _uvs[i] = new Vector2(verts[i].x, verts[i].z);
            }
            _mesh.GetComponent<MeshFilter>().mesh.uv = _uvs;
            _mesh.GetComponent<MeshRenderer>().material.SetColor("_groundCol", ground);
            _mesh.GetComponent<MeshRenderer>().material.SetColor("_cliffCol", cliff);
            _mesh.GetComponent<MeshRenderer>().material.SetColor("_groundDarkCol", ground2);
            _mesh.GetComponent<MeshRenderer>().material.SetVector("_centre", waterSphere.transform.position);
            meshes.Add(_mesh);
            chunks[(int)(startPos.x / chunkSize), (int)(startPos.y / chunkSize), (int)(startPos.z / chunkSize)] = _mesh;

            verts.Clear();
            triangles.Clear();
            vertsDict.Clear();
        }
    }

    public void UpdatePlanetMaterialCentre() {
        foreach (GameObject obj in meshes) obj.GetComponent<MeshRenderer>().material.SetVector("_centre", waterSphere.transform.position);
        // for (int i = 0; i < xRange; ++i) for (int j = 0; j < zRange; ++j) for (int k = 0; k < yRange; ++k) chunks[i, j, k].GetComponent<MeshRenderer>().material.SetVector("_centre", waterSphere.transform.position); 
        if (!atmosphereSpawn) return; 
        atmosphere.GetComponent<MeshRenderer>().material.SetVector("_Position", atmosphere.transform.position); 
        atmosphere.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetVector("_Position", atmosphere.transform.position); 
        // Debug.Log("updated planet material centre " + waterSphere.transform.position); 
    }

    private void Update() {
        if (RPCDoneOnline && !RPCDoneLocal) {
            GeneratePlanet(seedRPC, groundRPC, cliffRPC, ground2RPC, waterColRPC, waterOffsetRPC, atmoRPC, positionRPC);
            RPCDoneLocal = true;
        }

        if (Vector3.Distance(transform.position, prevPos) >= 1f) {
            UpdatePlanetMaterialCentre();
        }

        prevPos = transform.position;
    }

    Vector3 linearInterpolate(float isolevel, Vector3 v1, Vector3 v2, float val1, float val2) {
        if (Mathf.Abs(isolevel - val1) < 0.00001f) return v1;
        if (Mathf.Abs(isolevel - val2) < 0.00001f) return v2;
        if (Mathf.Abs(val1 - val2) < 0.00001f) return v1;

        float mu = (isolevel - val1) / (val2 - val1);
        Vector3 _vert = new Vector3(v1.x + mu * (v2.x - v1.x), v1.y + mu * (v2.y - v1.y), v1.z + mu * (v2.z - v1.z));
        return _vert;
    }

    void MakePoints() {
        for (int i = 0; i < xRange; ++i) {
            for (int j = 0; j < zRange; ++j) {
                for (int k = 0; k < yRange; ++k) {
                    GameObject _point = Instantiate(pointPrefab, new Vector3(i, k, j), Quaternion.identity);
                    // float _pointCol = (matrix[i, j, k] + 0.1f) * 100f;
                    // _pointCol = matrix[i, j, k]; 
                    // _point.GetComponent<MeshRenderer>().material.color = new Color(_pointCol, _pointCol, _pointCol); 

                    if (matrix[i, j, k] > threshold) _point.GetComponent<MeshRenderer>().material.color = Color.white;
                    else _point.GetComponent<MeshRenderer>().material.color = Color.black;
                    // else Destroy(_point);
                }
            }
        }
    }
}