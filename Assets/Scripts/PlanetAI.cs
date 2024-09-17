using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAI : MonoBehaviour {
    public float movementSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = -9.81f;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        // Check if the AI is on the ground
        isGrounded = Physics.CheckSphere(transform.position, groundDistance, groundMask);

        // Apply gravity to the AI
        if (isGrounded && rb.velocity.y < 0) {
            rb.velocity = Vector3.zero;
        } else {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }

        // Move the AI
        Vector3 movement = GetMovementDirection();
        rb.MovePosition(transform.position + movement * movementSpeed * Time.deltaTime);

        // Jump if the AI is grounded and there's an obstacle ahead
        if (isGrounded && IsObstacleAhead()) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Orient the AI towards the planet's surface
        OrientTowardsPlanetSurface();
    }

    private Vector3 GetMovementDirection() {
        // Calculate the direction the AI should move towards
        Vector3 forwardDirection = transform.forward;
        Vector3 gravityDirection = -transform.position.normalized;
        Vector3 rightDirection = Vector3.Cross(forwardDirection, gravityDirection);
        Vector3 movementDirection = Vector3.Cross(gravityDirection, rightDirection);

        return movementDirection.normalized;
    }

    private bool IsObstacleAhead() {
        // Check if there's an obstacle ahead of the AI
        Vector3 forwardDirection = transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, forwardDirection, out hit, 5f)) {
            return true;
        }

        return false;
    }

    private void OrientTowardsPlanetSurface() {
        // Cast a ray downwards to find the point where the AI intersects with the planet's surface
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.position.normalized, out hit)) {
            // Orient the AI towards the planet's surface
            transform.up = hit.normal;
        }
    }
}
