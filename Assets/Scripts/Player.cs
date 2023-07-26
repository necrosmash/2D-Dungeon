using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction moveAction;
    
    [SerializeField] private float gravityStrength;
    [SerializeField] private float runSpeedMax;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpDampener;
    [SerializeField] [Range(0, 1)] private float minJumpLerp; // between 0 and 1

    private float playerHeight, playerWidth, jumpStartingY, jumpLerp = 1, runLerp = 0;
    private bool isJumpButtonHeld = false;

    public float InputRunAmount { get; private set; }
    private float inputRunAmountPrevious;
    public float JumpEndingY { get; private set; }
    public bool IsGainingHeight { get; private set; } = false;

    private Vector3 raycastStartingPos;

    private void Awake()
    {
        jumpAction.performed += ctx => { OnJumpPerformed(ctx); };
        jumpAction.canceled += ctx => { OnJumpEnded(ctx); };
    }

    private void Start()
    {
        playerHeight = GetComponent<CapsuleCollider2D>().bounds.size.y;
        playerWidth = GetComponent<CapsuleCollider2D>().bounds.size.x;
    }

    private void Update()
    {
        InputRunAmount = moveAction.ReadValue<float>();
        
        if (InputRunAmount * inputRunAmountPrevious < 0.0f) // kill momentum upon direction change
            runLerp = 0;

        if (InputRunAmount != 0)
            inputRunAmountPrevious = InputRunAmount;

        if (InputRunAmount == 0)
            // could decelerate faster if needs be by switching out runAcceleration
            runLerp -= Time.deltaTime * runAcceleration;
        else
            runLerp += Time.deltaTime * runAcceleration;
        
        runLerp = Mathf.Clamp(runLerp, 0f, 1f);

        Debug.Log(runLerp);
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(
            transform.position.x + inputRunAmountPrevious * runSpeedMax * runLerp,
            transform.position.y,
            transform.position.z
        );

        bool g = CheckGrounded();
        if (!g && !IsGainingHeight)
            ApplyGravity(Time.deltaTime);
        else if (IsGainingHeight)
        {
            float d = jumpDampener * jumpLerp;
            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(
                    jumpStartingY,
                    JumpEndingY,
                    jumpLerp += jumpSpeed * Time.deltaTime * Mathf.Exp(d)
                ),
                transform.position.z
            );
        }

        if (transform.position.y == JumpEndingY || (jumpLerp >= minJumpLerp && !isJumpButtonHeld))
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

        jumpLerp = 0;
        jumpStartingY = transform.position.y;
        JumpEndingY = jumpStartingY + jumpHeight;
        isJumpButtonHeld = IsGainingHeight = true;
    }

    private void OnJumpEnded(InputAction.CallbackContext ctx)
    {
        isJumpButtonHeld = false;
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
