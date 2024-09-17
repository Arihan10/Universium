using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityItem : MonoBehaviour
{
	public Vector3 throwForce = Vector3.zero; 
	[SerializeField] float gravity = 15f; 
	
	public Vector3 planetCentre;

	public bool throwing = false; 

	Rigidbody rigidBody; 
	
	// Start is called before the first frame update
    void Awake()
    {
		rigidBody = GetComponent<Rigidbody>(); 
	}

    public void Shoot(Vector3 _force) {
		// rigidBody.AddForce(transform.up * 100f, ForceMode.VelocityChange); 
		throwForce = _force;
		throwing = true; 
		// if (rigidBody) rigidBody.AddForce(_force, ForceMode.VelocityChange); 
	}

    void FixedUpdate() {
		// Vector3 planetCentre = new Vector3(105,105,105);
		Vector3 gravityUp = (rigidBody.position - planetCentre).normalized;

		// Align body's up axis with the centre of planet
		Vector3 localUp = MathUtility.LocalToWorldVector(rigidBody.rotation, Vector3.up);
		rigidBody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidBody.rotation;

		rigidBody.velocity = CalculateNewVelocity(localUp);
		// rigidBody.velocity += planet.GetComponent<CelestialBody>().currentVelocity; 

		/*if (throwing && planetCentre != Vector3.zero) {
			rigidBody.AddForce(throwForce, ForceMode.VelocityChange);
		}*/
	}

	Vector3 CalculateNewVelocity(Vector3 localUp) {
		// Apply movement and gravity to rigidbody
		float deltaTime = Time.fixedDeltaTime;
		Vector3 currentLocalVelocity = MathUtility.WorldToLocalVector(rigidBody.rotation, rigidBody.velocity);

		float localYVelocity = currentLocalVelocity.y + (-gravity) * deltaTime;

		Vector3 desiredGlobalVelocity = MathUtility.LocalToWorldVector(rigidBody.rotation, new Vector3(0,0,0));
		desiredGlobalVelocity += localUp * localYVelocity;
		return desiredGlobalVelocity;
	}
}
