using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;

    [Header("Jump Settings")]
    public float firstJumpForce = 12f;
    public float secondJumpForce = 10f;
    private int jumpCount = 0;
    private const int maxJumps = 2;
    private bool wasGrounded;

    [Header("Roll Settings")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    public float invincibilityDuration = 0.5f;

    [Header("Wall Cling Settings")]
    [SerializeField] private float wallClingDuration = 3f; // Max 3 detik
    [SerializeField] private float wallSlideSpeed = 1f; // Kecepatan turun pelan
    [SerializeField] private float wallCheckDistance = 0.6f; // Jarak check wall
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.5f, 0f); // Offset dari center
    [SerializeField] private LayerMask wallLayer; // Layer untuk wall
    [SerializeField] private float wallJumpForce = 10f; // Jump force dari wall
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1f, 1.5f); // Direction wall jump

    [Header("Mana Cost")]
    [SerializeField] private int rollManaCost = 2;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference rollAction;

    [Header("Debug Wall Cling")]
    [SerializeField] private bool showWallClingGizmos = true;
    [SerializeField] private bool showWallClingDebugLogs = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool canMove = true;

    // Roll state
    private bool isRolling = false;
    private bool isInvincible = false;
    private float lastRollTime = -999f;
    private float rollTimer = 0f;
    private Vector2 rollDirection;

    // Wall Cling state
    private bool isWallClinging = false;
    private bool hasWallClingSkill = false;
    private float wallClingTimer = 0f;
    private bool isTouchingWall = false;
    private int wallDirection = 0; // -1 = left wall, 1 = right wall

    private PlayerMana manaScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        manaScript = GetComponent<PlayerMana>();
    }

    private void Start()
    {
        CheckUnlockedSkills();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        rollAction.action.Enable();

        jumpAction.action.performed += OnJump;
        rollAction.action.performed += OnRoll;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        rollAction.action.Disable();

        jumpAction.action.performed -= OnJump;
        rollAction.action.performed -= OnRoll;
    }

    private void Update()
    {
        // Ground check
        wasGrounded = isGrounded;
        isGrounded = CheckGroundWithTag();

        // Reset jump count saat landing
        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }

        // Wall Cling Logic
        if (hasWallClingSkill && !isRolling && !isGrounded)
        {
            HandleWallClingLogic();
        }
        else if (isWallClinging)
        {
            // Force exit wall cling jika skill hilang atau grounded
            ExitWallCling();
        }

        // Handle roll timer
        if (isRolling)
        {
            rollTimer += Time.deltaTime;

            if (rollTimer >= rollDuration)
            {
                EndRoll();
            }

            PerformRollMovement();
            return;
        }

        // Gerakan horizontal (disable saat wall cling)
        if (canMove && !isRolling && !isWallClinging)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            if (moveInput.x != 0)
                spriteRenderer.flipX = moveInput.x < 0;
        }

        // Update animasi
        if (!isRolling && !isWallClinging)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void CheckUnlockedSkills()
    {
        if (InventoryManager.Instance != null)
        {
            hasWallClingSkill = InventoryManager.Instance.HasSkill("Wall Cling");

            if (showWallClingDebugLogs)
                Debug.Log($"[PlayerMovement] Wall Cling unlocked: {hasWallClingSkill}");
        }
    }

    // ✅ PUBLIC METHOD: untuk di-call dari PlayerAttack
    public void RefreshSkills()
    {
        CheckUnlockedSkills();
    }

    private void HandleWallClingLogic()
    {
        // Check wall collision
        isTouchingWall = CheckWallCollision(out wallDirection);

        if (isTouchingWall && !isWallClinging)
        {
            // Enter wall cling
            EnterWallCling();
        }
        else if (isWallClinging)
        {
            // Update wall cling state
            wallClingTimer += Time.deltaTime;

            // Auto-release setelah durasi habis
            if (wallClingTimer >= wallClingDuration)
            {
                ExitWallCling();
                return;
            }

            // Slow slide down
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);

            // Check jika lepas dari wall
            if (!isTouchingWall)
            {
                ExitWallCling();
            }
        }
    }

    private void EnterWallCling()
    {
        isWallClinging = true;
        wallClingTimer = 0f;

        // Set animator
        animator.SetBool("isWallClinging", true);

        // Face the wall (flip sprite)
        spriteRenderer.flipX = wallDirection < 0; // Face right jika wall di kiri, vice versa

        if (showWallClingDebugLogs)
            Debug.Log($"[WallCling] Entered wall cling on {(wallDirection < 0 ? "left" : "right")} wall");
    }

    private void ExitWallCling()
    {
        isWallClinging = false;
        wallClingTimer = 0f;

        // Reset animator
        animator.SetBool("isWallClinging", false);

        if (showWallClingDebugLogs)
            Debug.Log("[WallCling] Exited wall cling");
    }

    private bool CheckWallCollision(out int direction)
    {
        direction = 0;

        // Check right wall
        Vector2 rightCheckPos = (Vector2)transform.position + new Vector2(wallCheckOffset.x, wallCheckOffset.y);
        RaycastHit2D rightHit = Physics2D.Raycast(rightCheckPos, Vector2.right, wallCheckDistance, wallLayer);

        if (rightHit.collider != null)
        {
            direction = 1; // Wall on right
            return true;
        }

        // Check left wall
        Vector2 leftCheckPos = (Vector2)transform.position + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
        RaycastHit2D leftHit = Physics2D.Raycast(leftCheckPos, Vector2.left, wallCheckDistance, wallLayer);

        if (leftHit.collider != null)
        {
            direction = -1; // Wall on left
            return true;
        }

        return false;
    }

    private bool CheckGroundWithTag()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isRolling || !canMove) return;

        // Wall Jump
        if (isWallClinging)
        {
            PerformWallJump();
            return;
        }

        // Normal Jump
        if (jumpCount < maxJumps)
        {
            PerformJump();
        }
    }

    private void PerformJump()
    {
        float jumpForce = (jumpCount == 0) ? firstJumpForce : secondJumpForce;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount++;
        animator.SetTrigger("Jump");
    }

    private void PerformWallJump()
    {
        // Exit wall cling
        ExitWallCling();

        // Jump away from wall
        float jumpDirectionX = -wallDirection; // Opposite dari wall direction
        Vector2 jumpVelocity = new Vector2(
            jumpDirectionX * wallJumpDirection.x * moveSpeed,
            wallJumpDirection.y * firstJumpForce * 0.8f
        );

        rb.linearVelocity = jumpVelocity;

        // Flip sprite untuk face jump direction
        spriteRenderer.flipX = jumpDirectionX < 0;

        // Trigger jump animation
        animator.SetTrigger("Jump");

        // Reset jump count
        jumpCount = 1; // Wall jump count as first jump

        if (showWallClingDebugLogs)
            Debug.Log($"[WallCling] Wall jump! Direction: {jumpDirectionX}");
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (!CanRoll()) return;

        Vector2 inputDirection = moveAction.action.ReadValue<Vector2>();

        if (inputDirection.magnitude < 0.1f)
        {
            rollDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        else
        {
            rollDirection = new Vector2(inputDirection.x, 0f).normalized;

            if (rollDirection.x != 0)
                spriteRenderer.flipX = rollDirection.x < 0;
        }

        StartRoll();
    }

    private bool CanRoll()
    {
        if (isRolling) return false;
        if (isWallClinging) return false; // Cannot roll while wall clinging
        if (!isGrounded) return false;
        if (Time.time - lastRollTime < rollCooldown) return false;

        if (manaScript != null && !manaScript.HasEnoughMana(rollManaCost))
        {
            return false;
        }

        return true;
    }

    private void StartRoll()
    {
        if (manaScript != null)
        {
            if (!manaScript.ConsumeMana(rollManaCost))
            {
                return;
            }
        }

        isRolling = true;
        isInvincible = true;
        rollTimer = 0f;
        lastRollTime = Time.time;

        animator.SetTrigger("Roll");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        StartCoroutine(InvincibilityCoroutine());
    }

    private void PerformRollMovement()
    {
        rb.linearVelocity = new Vector2(rollDirection.x * rollSpeed, rb.linearVelocity.y);
    }

    private void EndRoll()
    {
        isRolling = false;
        rollTimer = 0f;
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove && !isRolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public bool IsFacingRight()
    {
        return !spriteRenderer.flipX;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    public bool IsWallClinging()
    {
        return isWallClinging;
    }

    // Gizmos untuk debugging
    private void OnDrawGizmosSelected()
    {
        if (!showWallClingGizmos) return;

        // Ground check visualization
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Wall check visualization
        if (Application.isPlaying)
        {
            // Right wall check
            Vector2 rightCheckPos = (Vector2)transform.position + new Vector2(wallCheckOffset.x, wallCheckOffset.y);
            Gizmos.color = isTouchingWall && wallDirection == 1 ? Color.red : Color.blue;
            Gizmos.DrawRay(rightCheckPos, Vector2.right * wallCheckDistance);

            // Left wall check
            Vector2 leftCheckPos = (Vector2)transform.position + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
            Gizmos.color = isTouchingWall && wallDirection == -1 ? Color.red : Color.blue;
            Gizmos.DrawRay(leftCheckPos, Vector2.left * wallCheckDistance);

            // Wall cling indicator
            if (isWallClinging)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}
