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
    [SerializeField] private float wallClingDuration = 3f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1f, 1.5f);

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

    private bool isRolling = false;
    private bool isInvincible = false;
    private float lastRollTime = -999f;
    private float rollTimer = 0f;
    private Vector2 rollDirection;

    private bool isWallClinging = false;
    private bool hasWallClingSkill = false;
    private float wallClingTimer = 0f;
    private bool isTouchingWall = false;
    private int wallDirection = 0;

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
        wasGrounded = isGrounded;
        isGrounded = CheckGroundWithTag();

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }

        if (hasWallClingSkill && !isRolling && !isGrounded)
        {
            HandleWallClingLogic();
        }
        else if (isWallClinging)
        {
            ExitWallCling();
        }

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

        if (canMove && !isRolling && !isWallClinging)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        else if (isWallClinging)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (!isRolling && !isWallClinging && canMove && moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }

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

    public void RefreshSkills()
    {
        CheckUnlockedSkills();
    }

    private void HandleWallClingLogic()
    {
        isTouchingWall = CheckWallCollision(out wallDirection);

        if (isTouchingWall && !isWallClinging)
        {
            EnterWallCling();
        }
        else if (isWallClinging)
        {
            wallClingTimer += Time.deltaTime;

            if (wallClingTimer >= wallClingDuration)
            {
                ExitWallCling();
                return;
            }

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

        animator.SetBool("isWallClinging", true);

        if (wallDirection > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (wallDirection < 0)
        {
            spriteRenderer.flipX = true;
        }

        if (showWallClingDebugLogs)
            Debug.Log($"[WallCling] Entered wall cling on {(wallDirection < 0 ? "left" : "right")} wall");
    }

    private void ExitWallCling()
    {
        isWallClinging = false;
        wallClingTimer = 0f;

        animator.SetBool("isWallClinging", false);

        if (showWallClingDebugLogs)
            Debug.Log("[WallCling] Exited wall cling");
    }

    private bool CheckWallCollision(out int direction)
    {
        direction = 0;

        Vector2 rightCheckPos = (Vector2)transform.position + new Vector2(wallCheckOffset.x, wallCheckOffset.y);
        RaycastHit2D rightHit = Physics2D.Raycast(rightCheckPos, Vector2.right, wallCheckDistance);

        if (rightHit.collider != null && rightHit.collider.CompareTag("Ground"))
        {
            direction = 1;
            return true;
        }

        Vector2 leftCheckPos = (Vector2)transform.position + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
        RaycastHit2D leftHit = Physics2D.Raycast(leftCheckPos, Vector2.left, wallCheckDistance);

        if (leftHit.collider != null && leftHit.collider.CompareTag("Ground"))
        {
            direction = -1;
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

        if (isWallClinging)
        {
            PerformWallJump();
            return;
        }

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
        ExitWallCling();

        float jumpDirectionX = -wallDirection;
        Vector2 jumpVelocity = new Vector2(
            jumpDirectionX * wallJumpDirection.x * moveSpeed,
            firstJumpForce
        );

        rb.linearVelocity = jumpVelocity;

        if (jumpDirectionX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (jumpDirectionX < 0)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetTrigger("Jump");

        jumpCount = 1;

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
        if (isWallClinging) return false;
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

    private void OnDrawGizmosSelected()
    {
        if (!showWallClingGizmos) return;

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (Application.isPlaying)
        {
            Vector2 rightCheckPos = (Vector2)transform.position + new Vector2(wallCheckOffset.x, wallCheckOffset.y);
            Gizmos.color = isTouchingWall && wallDirection == 1 ? Color.red : Color.blue;
            Gizmos.DrawRay(rightCheckPos, Vector2.right * wallCheckDistance);

            Vector2 leftCheckPos = (Vector2)transform.position + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
            Gizmos.color = isTouchingWall && wallDirection == -1 ? Color.red : Color.blue;
            Gizmos.DrawRay(leftCheckPos, Vector2.left * wallCheckDistance);

            if (isWallClinging)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}