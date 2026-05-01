using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControls : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool facingRight = true;
    private bool jumpHeld = false;
    int flipMask;
    int groundMask;
    private float jumpCooldownCounter = 0f;
    private float jumpBufferCounter = 0f;
    private float coyoteTimeCounter = 0f;
    private float ledgeBoostCooldown = 0f;
    private bool isLedgeBoosting = false;
    private InputAction moveAction;
    private float moveInput = 0f;
    private float wallJumpCooldownCounter = 0f;

    [Header("Debug Raycasts")]
    [SerializeField] private bool showLedgeDebug = false;
    [SerializeField] private bool showWallDebug = false;

    [Header("Movement")]
    public bool autoMoveEnabled = false;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public LayerMask wallLayer;
    public LayerMask platformLayer;
    public LayerMask oneWayPlatformLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Jump Feel")]
    public float jumpCutMultiplier = 0.5f; //hur mycket det bromsar vid tidig release
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Apex modifiers")]
    public float apexThreshold = 2f;            //inom vilken Y-hastighet är vi vid apex?
    public float apexGravityMultiplier = 0.5f;  //Hur mycket lägre gravitation vid apex
    public float normalGravityScale = 3f;       // Normal gravitation
    public float fallGravityScale = 5f;         //Extra tyngd vid fall

    [Header("Jump Buffering")]
    [SerializeField] private float jumpBufferTime = 0.15f; // antal sekunder som vi "minns" ett hopp

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;

    [Header("Fall Speed")]
    public float maxFallSpeed = 20f;

    [Header("Ledge Detection")]
    public float ledgeCheckDistance = 0.3f;
    public float ledgeHeightOffset = 18f;      //hur högt vi boostar karaktären
    public float ledgeCheckXOffset = 0.5f;      //avstånd från mitten till framkanten
    public float ledgeCheckLowY = 0f;           //höjd för låg cirkel relativt spelarens mittpunkt
    public float ledgeCheckHighY = 0.6f;        //höjd för hög cirkel relativt spelarens mittpunkt
    [SerializeField] private float ledgeBoostCooldownTime = 0.15f;

    [Header("Wall Detection")]
    public Vector2 boxSize = new Vector2(0.5f, 1f);
    public float wallCheckDistance = 0.5f;

    [Header("Wall Jump")]
    public bool wallJumpEnabled = false;
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpCooldownTime = 0.8f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        flipMask = wallLayer | platformLayer;
        groundMask = groundLayer | platformLayer | oneWayPlatformLayer;

        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;
        moveAction = inputActions.FindAction("Move");
    }

    private void FixedUpdate()
    {
        AutoMove();
        HandleJump();
        bool ledgeBoosted = HandleLedgeDetection();
        if (!ledgeBoosted && ledgeBoostCooldown <=0 && autoMoveEnabled)
        {
            WallCheckAndFlip();
        }
        HandleVariableJump();
        HandleGravity();
    }

    private void Update()
    {
        bool grounded = IsGrounded();  // en physics check per frame

        coyoteTimeCounter = grounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpCooldownCounter > 0)
        {
            jumpCooldownCounter -= Time.deltaTime;
        }
        if (ledgeBoostCooldown > 0)
        {
            ledgeBoostCooldown -= Time.deltaTime;
        }
        if (wallJumpCooldownCounter > 0)
        {
            wallJumpCooldownCounter -= Time.deltaTime;
        }
    }

    public void MoveAction(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().x;
        
    }
    private void AutoMove()
    {
        if (autoMoveEnabled)
        {
            float direction = facingRight ? 1f : -1f;

            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        } else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            if (moveInput > 0 && !facingRight) Flip();
            if (moveInput < 0 && facingRight) Flip();
        }
        
    }

    #region Jump Mechanics
    public void JumpAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime; //Starta nedräkning
            jumpHeld = true;
        }
        if (context.canceled)
        {
            jumpHeld = false;
        }
        if (context.started)
        {
            jumpHeld = true;
        }
    }
    private void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;         //Resetta bufferten
            coyoteTimeCounter = 0f;         //Resetta coyoteTimeCountern
            jumpCooldownCounter = jumpCooldown; //Starta cooldown för hopp
            Debug.Log("Jump triggered");
        } else if (wallJumpEnabled && jumpBufferCounter > 0 && IsTouchingWall() && !IsGrounded())
        {
            float wallJumpDirection = facingRight ? -1 : 1f;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpHorizontalForce, jumpForce);
            jumpBufferCounter = 0f;         //Resetta bufferten
            wallJumpCooldownCounter = wallJumpCooldownTime;
            Debug.Log("Walljump triggered");
        }
    }

    private void HandleVariableJump()
    {
        if (ledgeBoostCooldown > 0 || wallJumpCooldownCounter > 0)
        {
            return;
        }

        //Om spelaren rör sig uppåt men inte håller inne spacebar
        if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y * jumpCutMultiplier, 0f));
        }
    }

    private void HandleGravity()
    {
        bool atApex = Mathf.Abs(rb.linearVelocity.y) < apexThreshold;
        bool falling = rb.linearVelocity.y < 0;

        if (falling)
        {
            rb.gravityScale = fallGravityScale;     //faller snabbt
        } else if (atApex && !IsGrounded())
        {
            rb.gravityScale = apexGravityMultiplier;       // svävar lite vid toppen
        }
        else
        {
            rb.gravityScale = normalGravityScale;
        }

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    private bool HandleLedgeDetection()
    {
        float XOffset = facingRight ? ledgeCheckXOffset : -ledgeCheckXOffset;
        Vector2 lowOrigin = (Vector2)transform.position + new Vector2(XOffset, ledgeCheckLowY);
        Vector2 highOrigin = (Vector2)transform.position + new Vector2(XOffset, ledgeCheckHighY);

        //kolla bara vid väggar, inte på marken
        if (IsGrounded())
        {
            return false;
        }

        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        float radius = 0.2f;


        RaycastHit2D lowHit = Physics2D.CircleCast(lowOrigin, radius, dir, ledgeCheckDistance, flipMask);
        RaycastHit2D highHit = Physics2D.CircleCast(highOrigin, radius, dir, ledgeCheckDistance, flipMask);

        if (showLedgeDebug)
        {
            DrawCircleCast(lowOrigin, radius, dir, ledgeCheckDistance, lowHit.collider != null);
            DrawCircleCast(highOrigin, radius, dir, ledgeCheckDistance, highHit.collider != null);

            if (lowHit.collider != null)
            {
                Debug.DrawLine(lowHit.point, lowHit.point + Vector2.up * 0.2f, Color.yellow);
                Debug.DrawLine(lowHit.point, lowHit.point + Vector2.right * 0.2f, Color.yellow);
            }

            if (Time.frameCount % 10 == 0)
            {
                Debug.Log($"[Ledge] Low hit: {(lowHit.collider != null ? lowHit.collider.name : "none")} | " +
                          $"High hit: {(highHit.collider != null ? highHit.collider.name : "none")} | " +
                          $"Y-velocity: {rb.linearVelocity.y:F2} | " +
                          $"Boost triggered: {(lowHit.collider != null && highHit.collider == null)}");
            }
        }

        if (lowHit.collider != null && highHit.collider == null)
        {
            if(!isLedgeBoosting)
            {
                //putta spelaren uppåt
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, ledgeHeightOffset);
                ledgeBoostCooldown = ledgeBoostCooldownTime;
                isLedgeBoosting = true;
                Debug.Log("Ledge boost triggered");
            }
            return true;
        } else
        {
            isLedgeBoosting = false;
        }
            return false;
    }

    private void DrawCircleCast(Vector2 origin, float radius, Vector2 dir, float distance, bool hit)
    {
        Color color = hit ? Color.red : Color.green;
        Vector2 endPoint = origin + dir * distance;

        // Rita linje längs casten
        Debug.DrawLine(origin, endPoint, color);

        // Rita cirklar vid start och slut med Debug.DrawLine
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angleA = (i / (float)segments) * Mathf.PI * 2f;
            float angleB = ((i + 1) / (float)segments) * Mathf.PI * 2f;

            Vector2 a = origin + new Vector2(Mathf.Cos(angleA), Mathf.Sin(angleA)) * radius;
            Vector2 b = origin + new Vector2(Mathf.Cos(angleB), Mathf.Sin(angleB)) * radius;
            Debug.DrawLine(a, b, color);

            Vector2 a2 = endPoint + new Vector2(Mathf.Cos(angleA), Mathf.Sin(angleA)) * radius;
            Vector2 b2 = endPoint + new Vector2(Mathf.Cos(angleB), Mathf.Sin(angleB)) * radius;
            Debug.DrawLine(a2, b2, color);
        }
    }

    #endregion Jump


    private bool IsGrounded()
    {
        if (jumpCooldownCounter > 0)
        {
            return false; //ignorera marken direkt efter hopp
        }
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    private bool IsTouchingWall()
    {
        if (wallJumpCooldownCounter > 0) return false;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, direction, wallCheckDistance, flipMask);
        if (hit.collider != null)
        {
            float maxSlopeAngle = 40f;
            float slopeThreshold = Mathf.Sin(maxSlopeAngle * Mathf.Deg2Rad);

            if (hit.normal.y < slopeThreshold)
            {
                return true;
            }
        }

        return false;

    }

    private void WallCheckAndFlip()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, direction, wallCheckDistance, flipMask);

        if (showWallDebug)
        {
            DrawBoxCast(transform.position, boxSize, direction, wallCheckDistance, hit.collider != null);

            //träffpunkt
            if (hit.collider != null)
            {
                Debug.DrawLine(hit.point, hit.point + Vector2.up * 0.2f, Color.yellow);
                Debug.DrawLine(hit.point, hit.point + Vector2.right * 0.2f, Color.yellow);
            }

            if (Time.frameCount % 10 == 0)
            {
                Debug.Log($"[Wall] Hit: {(hit.collider != null ? hit.collider.name : "none")} | " +
                          $"Direction: {(facingRight ? "right" : "left")} | " +
                          $"Flip triggered: {hit.collider != null}");
            }
        }

        if (hit.collider != null)
        {
            float maxSlopeAngle = 40f;
            float slopeThreshold = Mathf.Sin(maxSlopeAngle * Mathf.Deg2Rad);

            if (hit.normal.y < slopeThreshold)
            {
                Flip();
                if (facingRight)
                {
                    Debug.Log("Facing Right");
                }
                else
                {
                    Debug.Log("Facing Left");
                }
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void DrawBoxCast(Vector2 origin, Vector2 size, Vector2 dir, float distance, bool hit)
    {
        Color color = hit ? Color.red : Color.green;
        Vector2 endPoint = origin + dir * distance;

        // Rita ursprungsboxen
        DrawBox(origin, size, color);
        // Rita slutboxen
        DrawBox(endPoint, size, color);

        // Rita linjerna längs sidorna som förbinder start och slut
        Vector2 halfSize = size * 0.5f;
        Debug.DrawLine(origin + new Vector2(0, halfSize.y), endPoint + new Vector2(0, halfSize.y), color);
        Debug.DrawLine(origin - new Vector2(0, halfSize.y), endPoint - new Vector2(0, halfSize.y), color);
    }

    private void DrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 halfSize = size * 0.5f;

        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
}

