using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class ShipMovement : MonoBehaviour
{
    const float G = 0.000000000066742f;

    [SerializeField] float rotSpeed = 1f, rotSmoothSpeed = 15, mass = 1000f, thrustStrength = 10f, rollSpeed = 25f; 

    public Vector3 thrusterInput, currentVelocity;
    Quaternion targetRot, smoothedRot;

    int collisions = 0; 

    Rigidbody rb;

    Vector3 collisionUp;

    public bool piloting = false; 

    public GameObject planet; 
    [SerializeField] GameObject camera;

    public PhotonView player; 
    PhotonView PV; 
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // rb.centerOfMass = Vector3.zero; 

        smoothedRot = transform.rotation;
        targetRot = transform.rotation;

        PV = GetComponent<PhotonView>(); 
    }

    private void Update() {
        if (!player || (player && !player.IsMine)) return; 

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 

        HandleMovement(); 
    }

    private void FixedUpdate() {
        if ((collisions > 0 && !player) || (player && !player.IsMine)) {
            rb.isKinematic = true; 
            return;
        }

        rb.isKinematic = false; 

        Debug.Log("controlling ship locally");

        CelestialBody[] bodies = GameManager.instance.allBodies; 

        foreach (CelestialBody body in bodies) {
            Vector3 bodyCenter = body.transform.position; 
            if (body.GetComponent<PlanetGeneration>()) {
                bodyCenter = body.GetComponent<PlanetGeneration>().waterSphere.transform.position; 
            }
            float sqrDist = (bodyCenter - transform.position).sqrMagnitude;
            Vector3 forceDir = (bodyCenter - transform.position).normalized;
            Vector3 force = (G * mass * body.mass * forceDir) / sqrDist;
            Vector3 acceleration = force / mass; 
            rb.AddForce(acceleration, ForceMode.Acceleration); 
        }

        Vector3 thrustDir = transform.TransformVector(thrusterInput);
        rb.AddForce(thrustDir * thrustStrength, ForceMode.Acceleration);
        // if (collisions == 0) rb.MoveRotation(smoothedRot); 
        if (true) rb.MoveRotation(smoothedRot); 
    }

    public void HandleMovement() {
        // thrust on the x, y, and z
        thrusterInput = new Vector3(GetInputAxis(KeyCode.A, KeyCode.D), GetInputAxis(KeyCode.LeftControl, KeyCode.LeftShift), GetInputAxis(KeyCode.S, KeyCode.W));

        // FYI COLLISIONS DONT WORK
        // if (collisions == 0) {
        if (true) {
            var yaw = Quaternion.AngleAxis(Input.GetAxisRaw("Mouse X") * rotSpeed, transform.up);
            var pitch = Quaternion.AngleAxis(-Input.GetAxisRaw("Mouse Y") * rotSpeed, transform.right);
            var roll = Quaternion.AngleAxis(-GetInputAxis(KeyCode.Q, KeyCode.E) * rollSpeed * Time.deltaTime, transform.forward);

            targetRot = yaw * pitch * roll *  targetRot; 
            smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed); 
        } else {
            //targetRot = transform.rotation;
            //smoothedRot = transform.rotation; 
            targetRot = Quaternion.FromToRotation(transform.up, collisionUp) * transform.rotation; 
            smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed);
        }
    }

    int GetInputAxis(KeyCode negativeAxis, KeyCode positiveAxis) {
        int axis = 0;
        if (Input.GetKey(positiveAxis)) {
            axis++;
        }
        if (Input.GetKey(negativeAxis)) {
            axis--;
        }
        return axis;
    }

    public void StartPiloting(PhotonView _player) {
        camera.SetActive(true); 
        PV.RPC("StartPiloting_RPC", RpcTarget.All, _player.ViewID); 
    }

    [PunRPC]
    void StartPiloting_RPC(int _viewID) {
        currentVelocity = Vector3.zero; 
        piloting = true;
        player = PhotonView.Find(_viewID); 
    }

    public void StopPiloting() {
        camera.SetActive(false);
        player.GetComponent<FirstPersonController>().planet = planet; 
        PV.RPC("StopPiloting_RPC", RpcTarget.All); 
    }

    [PunRPC]
    void StopPiloting_RPC() {
        currentVelocity = Vector3.zero; 
        piloting = false; 
        player = null; 
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "Chunk") {
            ++collisions;
            collisionUp = collision.contacts[0].normal;
            planet = collision.transform.parent.gameObject; 
        }
    }

    private void OnCollisionExit(Collision collision) {
        // if (collision.transform.tag == "Chunk") --collisions; 
    }
}
