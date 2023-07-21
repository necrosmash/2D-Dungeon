using UnityEngine;
using UnityEngine.Jobs;

public class KinematicGravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private LayerMask groundLayer;

    private Transform t;
    private bool isGrounded;
    private float playerHeight;

    private Transform debugFloorSprite;

    private void Start()
    {
        t = transform;
        playerHeight = GetComponent<CapsuleCollider2D>().bounds.size.y;
        debugFloorSprite = t.Find("debugGo").transform;
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
        t.position = new Vector3(t.position.x, t.position.y - gravityStrength * dt, t.position.z);
    }

    private bool CheckGrounded()
    {
        Vector3 raycastStartingPos = new Vector3(t.position.x, t.position.y - (playerHeight / 2), t.position.z);

        // Define a small raycast distance to check if the object is grounded.
        float raycastDistance = 0.1f;

        debugFloorSprite.position = raycastStartingPos;
        Debug.Log("ds [" + debugFloorSprite.transform.position + "] | rcs [" + raycastStartingPos + "]");
        // Cast a ray directly down to check if the object is touching the ground.
        RaycastHit2D hit = Physics2D.Raycast(raycastStartingPos, Vector2.down, raycastDistance, groundLayer);

        // If the ray hits something, the object is grounded.
        return hit.collider != null;
    }
}
