using UnityEngine;
using UnityEngine.Jobs;

public class KinematicGravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;
    private float playerHeight, playerWidth;
    private Vector3 raycastStartingPos;

    private void Start()
    {
        playerHeight = GetComponent<CapsuleCollider2D>().bounds.size.y;
        playerWidth = GetComponent<CapsuleCollider2D>().bounds.size.x;
        //Gizmos.color = Color.blue;
    }

    private void Update()
    {
        // Check if the object is grounded using a simple raycast.
        isGrounded = CheckGrounded();

        if (!isGrounded)
        {
            // Apply custom gravity if not grounded.
            ApplyGravity(Time.deltaTime);
        }
    }

    private void ApplyGravity(float dt)
    {
        // Calculate the custom gravity force direction.
        Vector2 gravityDirection = Vector2.down;

        // Apply the gravity force to the rigidbody.
        transform.position = new Vector3(transform.position.x, transform.position.y - gravityStrength * dt, transform.position.z);
    }

    private bool CheckGrounded()
    {
        raycastStartingPos = new Vector3(transform.position.x, transform.position.y - (playerHeight / 2), transform.position.z);

        // Define a small raycast distance to check if the object is grounded.
        float raycastDistance = 0.1f;

        // Cast a ray directly down to check if the object is touching the ground.
        RaycastHit2D hit = Physics2D.Raycast(raycastStartingPos, Vector2.down, raycastDistance, groundLayer);

        // If the ray hits something, the object is grounded.
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        if (raycastStartingPos == null) return;

        Vector3 fromPos = new Vector3(transform.position.x - playerWidth / 2, raycastStartingPos.y, transform.position.z);
        Vector3 toPos = new Vector3(transform.position.x + playerWidth / 2, raycastStartingPos.y, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(fromPos, toPos);
    }
}
