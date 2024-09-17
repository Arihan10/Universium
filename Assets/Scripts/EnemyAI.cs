using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using Photon.Pun;
using Photon.Realtime;

public class EnemyAI : MonoBehaviour {

	public enum MoveState {
		Idle,
		Walk,
		Run,
		Swim
	}

	// public vars
	public float gravity = 15;
	public float grav;
	public float buoyancy = 6;
	public float walkSpeed = 6;
	public float runSpeed = 12;
	public float swimSpeed = 10;
	public float jumpForce = 220;
	public float waterDrag = 0.5f;
	public float jumpStateRayLength = 2f, wanderTime = 5f, achieveDist = 8f, health = 100f; 
	public LayerMask terrainMask, playerMask;

	// System vars
	public float groundedRaySizeFactor = 0.7f;
	public float groundedRayLength = 0.1f;
	public bool grounded;
	Vector3 desiredLocalVelocity, smoothMoveVelocity; 
	float verticalLookRotation; 
	Rigidbody rigidBody;
	CapsuleCollider capsuleCollider;

	[SerializeField] GameObject eyeLevel; 
	public GameObject water, planet;
	public Vector3 planetCentre; 

	public MoveState currentMoveState { get; private set; }
	float waterRadius, lastTime, startTime, lastTimeMove;
	bool underwater, point = false; 

	Vector3 targetAIPos, lastPos, moveDir; 

	[SerializeField] bool offline = false; 

	PhotonView PV;

	public GameObject headCollider;

	[SerializeField] GameObject[] mobDrops; 
	GameObject[] players; 

	[SerializeField] GameObject _player; 

	void Awake() {
		if (!planet) planet = GameObject.Find("HomePlanet"); 

		grav = gravity;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.useGravity = false;
		rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		capsuleCollider = GetComponent<CapsuleCollider>();
		Time.fixedDeltaTime = 1f / 60f;

		PV = GetComponent<PhotonView>();

		if (!PV.IsMine) {
			Destroy(GetComponent<Rigidbody>());
			return;
		}

		targetAIPos = transform.position;

		moveDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

		// players = PhotonNetwork.PlayerList; 
		// players = GameObject.FindGameObjectsWithTag("Player");

		Vector3 col = new Vector3(Random.Range(0f, 0.75f), Random.Range(0f, 0.75f), Random.Range(0f, 0.75f));
		GetComponent<MeshRenderer>().materials[0].color = new Color(col.x, col.y, col.z);
		GetComponent<MeshRenderer>().materials[1].color = new Color(col.x + 0.15f, col.y + 0.15f, col.z + 0.15f); 
	}

	private void Start() {
		if (!PV.IsMine) return;

		lastTime = Time.time;
		startTime = Time.time;
		lastTimeMove = Time.time;

		NewTargetPos(); 
	}

	void Update() {
		if (!PV.IsMine) return; 

		if (!planet) planet = GameObject.Find("HomePlanet");

		if (planet.GetComponent<PlanetGeneration>().waterSphere) {
			water = planet.GetComponent<PlanetGeneration>().waterSphere;
			waterRadius = water.transform.localScale.x / 2f;
			planetCentre = water.transform.position;
		} else {
			waterRadius = planet.transform.localScale.x / 3;
			planetCentre = Vector3.zero;
		}

		/*if (!underwater && (transform.position - planetCentre).magnitude < waterRadius - 0.7f) {
			rigidBody.velocity /= 7f;
		}
		underwater = (transform.position - planetCentre).magnitude < waterRadius - 0.7f;
		// Debug.Log(underwater); 

		if (underwater) {
			currentMoveState = MoveState.Swim;
			gravity = 2f;
		} else {
			gravity = grav;
			currentMoveState = MoveState.Walk;
		}*/

		currentMoveState = MoveState.Walk; 

		float desiredMoveSpeed = 0;

		if (currentMoveState == MoveState.Walk) {
			desiredMoveSpeed = walkSpeed;
		} else if (currentMoveState == MoveState.Run) {
			desiredMoveSpeed = runSpeed;
		} else if (currentMoveState == MoveState.Swim) {
			desiredMoveSpeed = swimSpeed;
		}

		// /* PLAYER STUFF
		// GameObject _player = null;
		players = GameObject.FindGameObjectsWithTag("Player"); 
		_player = null; 

		for (int i = 0; i < players.Length; ++i) {
			RaycastHit playerHit;
			// Debug.Log(PhotonView.Find(players[i].ActorNumber).gameObject.name + " IS NAME"); 
			if (Physics.Raycast(eyeLevel.transform.position, (players[i].transform.position - eyeLevel.transform.position), out playerHit, Mathf.Infinity, playerMask)) {
				if (playerHit.collider.tag == "Player") {
					_player = playerHit.collider.gameObject;
					break;
				}
			}
		}

		/*
		// if (_player != null) {
		if (_player != null) {
			moveDir = (_player.transform.position - transform.position).normalized;
		} else {
			// Vector3 moveDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized; 
			if (Time.time - lastTime > wanderTime) {
				moveDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
				lastTime = Time.time;
			}
		}
		// */

		if (_player != null) {
			targetAIPos = _player.transform.position;
			transform.Translate(moveDir * runSpeed * Time.deltaTime); 
		} else {
			if (Vector3.Distance(targetAIPos, transform.position) < 10f) {
				NewTargetPos();
			}
		}

		moveDir = (targetAIPos - transform.position).normalized; 
		moveDir = new Vector3(moveDir.x, 0, moveDir.z); 
		// transform.localRotation = Quaternion.LookRotation(moveDir); 
		Vector3 targetMoveVelocity = moveDir * desiredMoveSpeed;
		desiredLocalVelocity = Vector3.SmoothDamp(desiredLocalVelocity, targetMoveVelocity, ref smoothMoveVelocity, .15f);

		// Jump
		if (underwater) {
			// rigidBody.AddForce(transform.up * jumpForce * Time.deltaTime * 2f, ForceMode.VelocityChange);
		} /*else if (Input.GetButtonDown("Jump")) {
			if (grounded) {
				rigidBody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
			}
		} IN CASE OF JUMPING*/

		// Grounded check
		Ray ray = new Ray(transform.position, -transform.up);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 2f, terrainMask)) {
			grounded = true;
		} else {
			grounded = false;
		}

		/*RaycastHit _hit;
		if (Physics.Raycast(eyeLevel.transform.position, transform.forward, out _hit, jumpStateRayLength)) {
			if (grounded) {
				rigidBody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
				Debug.Log("JUMPING"); 
			}
		}*/


		/*if (Vector3.Distance(lastPos, transform.position) < 0.1f * Time.deltaTime) {
			Debug.Log("MOVED LESS");
			NewTargetPos(); 
        }*/

		if (Time.time - lastTimeMove > 3f) {
			if (Vector3.Distance(lastPos, transform.position) < 2f) {
				// Debug.Log("MOVED LESS");
				NewTargetPos();
			}

			lastPos = transform.position;
			lastTimeMove = Time.time; 
		}

		if (headCollider.GetComponent<EnemyAICollisions>().planetCollisions > 0 && !_player) {
			// if (grounded) {
				rigidBody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
				// Debug.Log("JUMPING");
			// }
		}

		// grounded = IsGrounded();

		// if (Time.time - startTime > 2f && Time.time - startTime > 0.01f) {
		if (Vector3.Magnitude(rigidBody.velocity) > 50f) {
			rigidBody.velocity /= 10f; 
        }

		// lastPos = transform.position; 
	}

	void NewTargetPos() {
		List<GameObject> _meshes = planet.GetComponent<PlanetGeneration>().meshes;

		GameObject _mesh;
		do {
			_mesh = _meshes[Random.Range(0, _meshes.Count)];
		} while (!_mesh || !_mesh.GetComponent<MeshFilter>() || _mesh.GetComponent<MeshFilter>().mesh.vertices.Length < 1);

		targetAIPos = _mesh.GetComponent<MeshFilter>().mesh.vertices[Random.Range(0, _mesh.GetComponent<MeshFilter>().mesh.vertices.Length)] + planetCentre; 

		// Debug.Log("CHOOSING NEW POINT:");

		point = true; 
	}

	void FixedUpdate() {
		if (!PV.IsMine) return; 

		// Vector3 planetCentre = new Vector3(105,105,105);
		Vector3 gravityUp = (rigidBody.position - planetCentre).normalized;

		// Align body's up axis with the centre of planet
		Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
		rigidBody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidBody.rotation;

		rigidBody.velocity = CalculateNewVelocity(localUp);
		// rigidBody.velocity += planet.GetComponent<CelestialBody>().currentVelocity; 
	}

	Vector3 CalculateNewVelocity(Vector3 localUp) {
		// Apply movement and gravity to rigidbody
		float deltaTime = Time.fixedDeltaTime;
		Vector3 currentLocalVelocity = MathUtility.WorldToLocalVector(rigidBody.rotation, rigidBody.velocity);

		float localYVelocity = currentLocalVelocity.y + (-gravity) * deltaTime;

		Vector3 desiredGlobalVelocity = MathUtility.LocalToWorldVector(rigidBody.rotation, desiredLocalVelocity);
		desiredGlobalVelocity += localUp * localYVelocity;
		return desiredGlobalVelocity;
	}

	bool IsGrounded() {
		Vector3 centre = rigidBody.position;
		Vector3 upDir = transform.up;

		// Vector3 castOrigin = centre + upDir * (-capsuleCollider.height / 10f + capsuleCollider.radius/200f); 
		Vector3 castOrigin = centre + upDir * -0.4f;
		float groundedRayRadius = capsuleCollider.radius * groundedRaySizeFactor;

		float groundedRayDst = capsuleCollider.radius - groundedRayRadius + groundedRayLength;
		RaycastHit hitInfo;

		if (Physics.SphereCast(castOrigin, groundedRayRadius, -upDir, out hitInfo, groundedRayDst)) {
			return true;
		}

		return false;
	}

	public void TakeDamage(float damage) {
		int mobDropIndex = Random.Range(0, mobDrops.Length); 
		PV.RPC("TakeDamage_RPC", RpcTarget.All, damage, mobDropIndex); 
    }

	[PunRPC]
	void TakeDamage_RPC(float damage, int mobDropIndex) {
		health -= damage;

		if (health <= 0f) {
			// int mobDropIndex = Random.Range(0, mobDrops.Length); 
			GameObject mobDrop = Instantiate(mobDrops[mobDropIndex], transform.position, Quaternion.identity);
			mobDrop.GetComponent<GravityItem>().planetCentre = planetCentre;
			mobDrop.name = mobDrops[mobDropIndex].name; 
			mobDrop.GetComponent<ItemInfo>().ID = GameManager.instance.items.Count;
			GameManager.instance.items.Add(mobDrop); 

			if (PV.IsMine) PhotonNetwork.Destroy(gameObject);
		}
	}

	public void SetPlanetCentre(Vector3 _planetCentre) {
		planetCentre = _planetCentre; 
    }

	private void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "Chunk") {
			planet = collision.transform.parent.gameObject; 
		} else if (collision.collider.tag == "Player" && collision.collider.GetComponent<PhotonView>().IsMine) {
			collision.collider.GetComponent<PlayerController>().TakeDamage(5f); 
		}
	}

    void OnDrawGizmos() {
		if (!PV.IsMine || !PhotonNetwork.IsMasterClient) return; 

		if (Application.isPlaying) {
			bool grounded = IsGrounded();
			// Debug.Log(grounded); 

			Vector3 centre = rigidBody.position;
			Vector3 upDir = transform.up;
			// Vector3 castOrigin = centre + upDir * (-capsuleCollider.height / 10f + capsuleCollider.radius/200f);
			Vector3 castOrigin = centre + upDir * -0.5f;
			float groundedRayRadius = capsuleCollider.radius * groundedRaySizeFactor;

			float groundedRayDst = capsuleCollider.radius - groundedRayRadius + groundedRayLength;
			Gizmos.color = (grounded) ? Color.green : Color.red;
			Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
			Gizmos.DrawSphere(castOrigin, groundedRayRadius);


			Vector3 collisionSphereTip = castOrigin - upDir * (groundedRayRadius + groundedRayDst);
			Gizmos.DrawSphere(collisionSphereTip + upDir * groundedRayRadius, groundedRayRadius);
			Gizmos.color = Color.red;
			Gizmos.DrawRay(castOrigin - upDir * groundedRayRadius, -upDir * groundedRayDst);
		}
	}
}