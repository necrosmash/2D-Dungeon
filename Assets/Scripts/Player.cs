using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction moveAction;

    [SerializeField] private float gravityStrength;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpSpeed;

    private float playerHeight, playerWidth, jumpStartingY, jumpLerp = 1;

    public float InputMoveAmount { get; private set; }
    public float JumpEndingY { get; private set; }
    public bool IsGainingHeight { get; private set; }

    private Vector3 raycastStartingPos;

    private void Awake()
    {
        jumpAction.performed += ctx => { OnJumpPerformed(ctx); };
        //jumpAction.canceled += ctx => { OnJumpEnded(ctx); };
    }

    private void Start()
    {
        IsGainingHeight = false;
        playerHeight = GetComponent<CapsuleCollider2D>().bounds.size.y;
        playerWidth = GetComponent<CapsuleCollider2D>().bounds.size.x;
    }

    private void Update()
    {
        Debug.Log(Time.deltaTime);
        InputMoveAmount = moveAction.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x + InputMoveAmount * movementSpeed, transform.position.y, transform.position.z);

        bool g = CheckGrounded();
        if (!g && !IsGainingHeight)
            ApplyGravity(Time.deltaTime);
        else if (IsGainingHeight)
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(jumpStartingY, JumpEndingY, jumpLerp += jumpSpeed * Time.deltaTime), transform.position.z);

        if (transform.position.y == JumpEndingY)
            IsGainingHeight = false;
    }

    private void ApplyGravity(float dt)
    {
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

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (!CheckGrounded()) return;

        Debug.Log("jump performed");
        jumpLerp = 0;
        jumpStartingY = transform.position.y;
        JumpEndingY = jumpStartingY + jumpHeight;
        IsGainingHeight = true;
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
