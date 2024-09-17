using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime; 

public class FirstPersonController : MonoBehaviour {

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
	public float mouseSensitivityX = 1;
	public float mouseSensitivityY = 1;
	public float walkSpeed = 6;
	public float runSpeed = 12;
	public float swimSpeed = 10;
	public float jumpForce = 220;
	public float waterDrag = 0.5f, waterDamage = 5f; 
	public Vector2 lookAngleMinMax = new Vector2(-75, 80);
	public LayerMask terrainMask;

	// System vars
	public float groundedRaySizeFactor = 0.7f;
	public float groundedRayLength = 0.1f;
	public bool grounded, underwater;
	Vector3 desiredLocalVelocity;
	Vector3 smoothMoveVelocity;
	float verticalLookRotation;
	public Transform cameraTransform;
	Rigidbody rigidBody;
	CapsuleCollider capsuleCollider;

	public GameObject water, planet;
	public Vector3 planetCentre; 

	public MoveState currentMoveState { get; private set; }
	float waterRadius; 

	PhotonView PV; 

	bool debug_stopMovement;

	[SerializeField] bool offline = false; 

	void Awake() {
		PV = GetComponent<PhotonView>();

		if (!PV.IsMine) return; 

		if (!planet) planet = GameObject.Find("HomePlanet");

		grav = gravity;
		Cursor.lockState = CursorLockMode.Locked; 
		Cursor.visible = false; 
		// cameraTransform = Camera.main.transform;
		rigidBody = GetComponent<Rigidbody>();
		rigidBody.useGravity = false;
		rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		capsuleCollider = GetComponent<CapsuleCollider>();
		Time.fixedDeltaTime = 1f / 60f;
	}

    private void Start() {
		// transform.parent = GameObject.Find("HomePlanet").transform; 
	}

    void Update() {
		// return; 
		if (!offline && !PV.IsMine) return;
		if (GetComponentInChildren<PlayerUI>().craftItemsUI.activeSelf) return; 

		if (!planet) planet = GameObject.Find("Home Planet"); 
		if (!planet) return; 

		if (planet.GetComponent<PlanetGeneration>().waterSphere) {
			water = planet.GetComponent<PlanetGeneration>().waterSphere;
			waterRadius = water.transform.localScale.x / 2f;
			planetCentre = water.transform.position;

			Color waterCol = water.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_BaseCol");
			RenderSettings.fogColor = new Color(waterCol.r, waterCol.g, waterCol.b, 1); 
		} else {
			waterRadius = planet.transform.localScale.x/3;
			planetCentre = Vector3.zero; 
        }

		if (!underwater && (cameraTransform.position - planetCentre).magnitude < waterRadius - 0.7f) {
			rigidBody.velocity /= 7f; 
        }
		underwater = (cameraTransform.position - planetCentre).magnitude < waterRadius - 0.7f;
		// Debug.Log(underwater); 

		// Look rotation:
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
		verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, lookAngleMinMax.x, lookAngleMinMax.y);
		cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

		// Calculate movement:
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");

		if (underwater) {
			currentMoveState = MoveState.Swim;
			gravity = 2f;
			RenderSettings.fog = true; 
		} else {
			RenderSettings.fog = false; 
			gravity = grav; 
			currentMoveState = MoveState.Idle;
			if (inputX != 0 || inputY != 0) {
				currentMoveState = Input.GetKey(KeyCode.LeftShift) ? MoveState.Run : MoveState.Walk;
			}
		}

		currentMoveState = MoveState.Idle;
		if (inputX != 0 || inputY != 0) {
			currentMoveState = Input.GetKey(KeyCode.LeftShift) ? MoveState.Run : MoveState.Walk;
		}

		float desiredMoveSpeed = 0;

		if (currentMoveState == MoveState.Walk) {
			desiredMoveSpeed = walkSpeed;
		} else if (currentMoveState == MoveState.Run) {
			desiredMoveSpeed = runSpeed;
		} else if (currentMoveState == MoveState.Swim) {
			desiredMoveSpeed = swimSpeed;
		}


		Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveVelocity = moveDir * desiredMoveSpeed;
		desiredLocalVelocity = Vector3.SmoothDamp(desiredLocalVelocity, targetMoveVelocity, ref smoothMoveVelocity, .15f);

		// Jump
		if (underwater && Input.GetButton("Jump")) {
			rigidBody.AddForce(transform.up * jumpForce * Time.deltaTime * 2f, ForceMode.VelocityChange); 
		} else if (Input.GetButtonDown("Jump")) {
			if (grounded) {
				rigidBody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
			}
		}

		// Grounded check
		Ray ray = new Ray(transform.position, -transform.up);
		RaycastHit hit;


		if (Physics.Raycast(ray, out hit, 1 + .1f, terrainMask)) {
			grounded = true;
		} else {
			grounded = false;
		}

		grounded = IsGrounded();
	}

	void FixedUpdate() {
		// return; 
		if (!PV.IsMine) return; 

		// Vector3 planetCentre = new Vector3(105,105,105);
		Vector3 gravityUp = (rigidBody.position - planetCentre).normalized;

		// Align body's up axis with the centre of planet
		Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
		rigidBody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidBody.rotation;

		rigidBody.velocity = CalculateNewVelocity(localUp);
		// rigidBody.velocity += planet.GetComponent<CelestialBody>().currentVelocity; 
	}

	void LateUpdate() {
		// return; 
		if (!offline && !PV.IsMine) return; 

		if (terraUpdate) {
			Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
			//Debug.Log("Update");
			TerraTest(localUp);
			terraUpdate = false;
			//Debug.Break();
		}
	}

	void TerraTest(Vector3 localUp) {
		float heightOffset = 2.5f;
		Vector3 a = transform.position - localUp * (capsuleCollider.height / 2 + capsuleCollider.radius - heightOffset);
		Vector3 b = transform.position + localUp * (capsuleCollider.height / 2 + capsuleCollider.radius + heightOffset);
		RaycastHit hitInfo;

		if (Physics.CapsuleCast(a, b, capsuleCollider.radius, -localUp, out hitInfo, heightOffset, terrainMask)) {
			hp = hitInfo.point;
			Vector3 newPos = (hp + transform.up * 1);
			float deltaY = Vector3.Dot(transform.up, (newPos - transform.position));
			if (deltaY > 0.05f) {
				transform.position = newPos;
				grounded = true;
			}
		}

	}

	public void NotifyTerrainChanged(Vector3 point, float radius) {
		float dstFromCam = (point - cameraTransform.position).magnitude;
		if (dstFromCam < radius + 3) {
			terraUpdate = true;
		}
	}

	bool terraUpdate;
	Vector3 hp;


	Vector3 CalculateNewVelocitySwim(Vector3 localUp) {
		float deltaTime = Time.fixedDeltaTime;
		Vector3 currentVelocity = rigidBody.velocity;

		Vector3 newVelocity = currentVelocity + localUp * (buoyancy - gravity) * deltaTime;
		Vector3 drag = -newVelocity * waterDrag;
		newVelocity += drag * deltaTime;

		Vector3 swimForce = MathUtility.LocalToWorldVector(cameraTransform.rotation, desiredLocalVelocity);
		Vector3 swimDeltaV = swimForce * deltaTime * 5;

		if (newVelocity.x * Mathf.Sign(swimForce.x) < Mathf.Abs(swimForce.x)) {
			newVelocity.x += swimDeltaV.x;
		}
		if (newVelocity.y * Mathf.Sign(swimForce.y) < Mathf.Abs(swimForce.y)) {
			newVelocity.y += swimDeltaV.y;
		}
		if (newVelocity.z * Mathf.Sign(swimForce.z) < Mathf.Abs(swimForce.z)) {
			newVelocity.z += swimDeltaV.z;
		}


		return newVelocity;
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

		Vector3 castOrigin = centre + upDir * (-capsuleCollider.height / 2f + capsuleCollider.radius);
		float groundedRayRadius = capsuleCollider.radius * groundedRaySizeFactor;

		float groundedRayDst = capsuleCollider.radius - groundedRayRadius + groundedRayLength;
		RaycastHit hitInfo;

		if (Physics.SphereCast(castOrigin, groundedRayRadius, -upDir, out hitInfo, groundedRayDst)) {
			return true;
		}

		return false;
	}


	void OnDrawGizmos() {
		// return; 
		// if (PV == null || !offline && !PV.IsMine) return; 

		if (PV == null || !PV.IsMine) return; 

		if (Application.isPlaying) {
			bool grounded = IsGrounded();
			// Debug.Log(grounded); 

			Vector3 centre = rigidBody.position;
			Vector3 upDir = transform.up;
			Vector3 castOrigin = centre + upDir * (-capsuleCollider.height / 2f + capsuleCollider.radius);
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