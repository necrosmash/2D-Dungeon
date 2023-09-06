using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction dashAction;

    [SerializeField] private float gravityStrength;
    [SerializeField] private float runSpeedMax;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration; // in seconds
    [SerializeField] private float runAcceleration;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpDampener;
    [SerializeField] private float dashDistance;
    [SerializeField] [Range(0, 1)] private float minJumpLerp; // between 0 and 1

    private Rigidbody2D rb;

    private float halvedPlayerHeight, halvedPlayerWidth, jumpStartingY, jumpLerp = 1, runLerp = 0, currentDashDuration = 0;
    private bool isJumpButtonHeld = false;

    public float InputRunAmount { get; private set; }
    private float inputRunAmountPrevious;
    public float JumpEndingY { get; private set; }
    public bool IsGainingHeight { get; private set; } = false;

    private Vector3 raycastStartingPos;
    private GameObject ground;

    float xDestination, yDestination; // for controlling player position
    public Vector2 Speed { get; private set; } = new Vector2(0f, 0f); // for logging

    public enum EnumDashingState
    {
        DashingLeft,
        DashingRight,
        NotDashing
    }
    public EnumDashingState DashingState { get; private set; } = EnumDashingState.NotDashing;

    private void Awake()
    {
        jumpAction.performed += ctx => { OnJumpPerformed(ctx); };
        jumpAction.canceled += ctx => { OnJumpEnded(ctx); };
        dashAction.performed += ctx => { OnDashPerformed(ctx); };
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        halvedPlayerHeight = GetComponent<CapsuleCollider2D>().bounds.extents.y;
        halvedPlayerWidth = GetComponent<CapsuleCollider2D>().bounds.extents.x;
    }

    private void Update()
    {
        ground = GetGround();

        if (DashingState != EnumDashingState.NotDashing && ((currentDashDuration += Time.deltaTime) >= dashDuration))
            DashingState = EnumDashingState.NotDashing;
        
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
    }

    private void FixedUpdate()
    {
        ground = GetGround();

        switch (DashingState)
        {
            case EnumDashingState.DashingLeft:
            case EnumDashingState.DashingRight:
                xDestination = transform.position.x + inputRunAmountPrevious * dashSpeed;
                break;
            default:
            case EnumDashingState.NotDashing:
                xDestination = transform.position.x + inputRunAmountPrevious * runSpeedMax * runLerp;
                break;
        }

        yDestination = transform.position.y;
        if (DashingState == EnumDashingState.NotDashing)
        {
            if (IsGainingHeight)
            {
                float d = jumpDampener * jumpLerp;
                yDestination = Mathf.Lerp(
                    jumpStartingY,
                    JumpEndingY,
                    jumpLerp += jumpSpeed * Time.deltaTime * Mathf.Exp(d)
                );
            }
            else if (ground == null)
                yDestination = transform.position.y - gravityStrength * Time.deltaTime; // apply gravity
            else
            {
                // we're on the ground and not gaining height.
                // This is done to ensure consistent height:
                yDestination = ground.transform.position.y + ground.GetComponent<BoxCollider2D>().bounds.extents.y + halvedPlayerHeight;
            }
        }

        Speed = new Vector2(xDestination - transform.position.x, yDestination - transform.position.y); // for logging

        rb.MovePosition(new Vector3(
            xDestination,
            yDestination,
            transform.position.z
        ));

        if (transform.position.y == JumpEndingY || (jumpLerp >= minJumpLerp && !isJumpButtonHeld))
            IsGainingHeight = false;
    }

    private GameObject GetGround()
    {
        raycastStartingPos = new Vector3(transform.position.x, transform.position.y - halvedPlayerHeight, transform.position.z);

        // Define a small raycast distance to check if the object is grounded.
        float raycastDistance = 0.1f;

        // Cast a ray directly down to check if the object is touching the ground.
        RaycastHit2D hit = Physics2D.Raycast(raycastStartingPos, Vector2.down, raycastDistance, groundLayer);

        if (hit.collider == null) return null;
        
        return hit.collider.gameObject;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (GetGround() == null) return;

        jumpLerp = 0;
        jumpStartingY = transform.position.y;
        JumpEndingY = jumpStartingY + jumpHeight;
        isJumpButtonHeld = IsGainingHeight = true;
    }

    private void OnJumpEnded(InputAction.CallbackContext ctx)
    {
        isJumpButtonHeld = false;
    }

    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (DashingState != EnumDashingState.NotDashing) return;

        currentDashDuration = 0;
        IsGainingHeight = false;

        switch (inputRunAmountPrevious) {
            case -1:
                DashingState = EnumDashingState.DashingLeft;
                break;
            case 1:
                DashingState = EnumDashingState.DashingRight;
                break;
            case 0:
            default:
                DashingState = EnumDashingState.NotDashing;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (raycastStartingPos == null) return;

        Vector3 fromPos = new Vector3(transform.position.x - halvedPlayerWidth, raycastStartingPos.y, transform.position.z);
        Vector3 toPos = new Vector3(transform.position.x + halvedPlayerWidth, raycastStartingPos.y, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(fromPos, toPos);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, halvedPlayerHeight);
    }

    public void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
    }

    public void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
    }
}
