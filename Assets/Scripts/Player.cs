using UnityEngine;
using UnityEngine.InputSystem;

public class KinematicGravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float movementSpeed = 0.03f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction moveAction;

    private float inputMoveAmount;
    private bool isGrounded;
    private float playerHeight, playerWidth;
    private Vector3 raycastStartingPos;

    private void Awake()
    {
        jumpAction.performed += ctx => { OnJump(ctx); };
    }

    private void Start()
    {
        playerHeight = GetComponent<CapsuleCollider2D>().bounds.size.y;
        playerWidth = GetComponent<CapsuleCollider2D>().bounds.size.x;
    }

    private void Update()
    {
        isGrounded = CheckGrounded();

        inputMoveAmount = moveAction.ReadValue<float>();
        transform.position = new Vector3(transform.position.x + inputMoveAmount * movementSpeed, transform.position.y, transform.position.z);

        // Check if the object is grounded using a simple raycast.
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

    private void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("jump");
    }

    private void OnDrawGizmos()
    {
        if (raycastStartingPos == null) return;

        Vector3 fromPos = new Vector3(transform.position.x - playerWidth / 2, raycastStartingPos.y, transform.position.z);
        Vector3 toPos = new Vector3(transform.position.x + playerWidth / 2, raycastStartingPos.y, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(fromPos, toPos);
    }

    public void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    public void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }
}
