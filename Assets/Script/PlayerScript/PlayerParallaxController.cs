using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player Movement Controller untuk Mountain Parallax Scene
/// Mendukung auto-move dan manual control
/// Disesuaikan dengan PlayerMovement.cs yang sudah ada
/// </summary>
public class PlayerParallaxMovement : MonoBehaviour
{
    [Header("Parallax Movement Settings")]
    [Tooltip("Enable auto-move untuk parallax scene")]
    public bool autoMove = true;
    [Tooltip("Auto-move ke arah kanan (false = kiri)")]
    public bool autoMoveRight = true;
    [Tooltip("Speed saat auto-move")]
    public float autoMoveSpeed = 5f;
    
    [Header("Manual Movement Settings")]
    [Tooltip("Speed saat manual control")]
    public float manualMoveSpeed = 6f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;

    [Header("Jump Settings (Optional)")]
    [Tooltip("Enable jump untuk parallax scene")]
    public bool allowJump = false;
    public float firstJumpForce = 12f;
    public float secondJumpForce = 10f;
    private int jumpCount = 0;
    private const int maxJumps = 2;
    private bool wasGrounded;

    [Header("Roll Settings (Optional)")]
    [Tooltip("Enable roll untuk parallax scene")]
    public bool allowRoll = false;
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    public float invincibilityDuration = 0.5f;

    [Header("Mana Cost")]
    [SerializeField] private int rollManaCost = 2;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference rollAction;

    [Header("Animation")]
    [Tooltip("Update animator parameters")]
    public bool updateAnimator = true;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private PlayerMana manaScript;

    // Movement state
    private Vector2 moveInput;
    private bool isGrounded;
    private bool canMove = true;
    private bool isMoving = false;

    // Roll state
    private bool isRolling = false;
    private bool isInvincible = false;
    private float lastRollTime = -999f;
    private float rollTimer = 0f;
    private Vector2 rollDirection;

    // Current move speed
    private float currentMoveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        manaScript = GetComponent<PlayerMana>();

        // Set initial speed
        currentMoveSpeed = autoMove ? autoMoveSpeed : manualMoveSpeed;

        // Start auto-move if enabled
        if (autoMove)
        {
            StartAutoMovement();
        }
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (rollAction != null) rollAction.action.Enable();

        if (allowJump && jumpAction != null)
            jumpAction.action.performed += OnJump;

        if (allowRoll && rollAction != null)
            rollAction.action.performed += OnRoll;
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (rollAction != null) rollAction.action.Disable();

        if (allowJump && jumpAction != null)
            jumpAction.action.performed -= OnJump;

        if (allowRoll && rollAction != null)
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

        // Handle roll timer
        if (isRolling)
        {
            rollTimer += Time.deltaTime;

            if (rollTimer >= rollDuration)
            {
                EndRoll();
            }

            PerformRollMovement();
            return; // Skip movement saat rolling
        }

        // Movement handling
        if (canMove && !isRolling)
        {
            if (autoMove)
            {
                // Auto-move mode
                HandleAutoMovement();
            }
            else
            {
                // Manual control mode
                HandleManualMovement();
            }
        }

        // Update animations
        if (updateAnimator && animator != null && !isRolling)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    #region Movement Methods

    /// <summary>
    /// Auto-movement untuk parallax scene
    /// </summary>
    private void HandleAutoMovement()
    {
        if (isMoving)
        {
            float direction = autoMoveRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(currentMoveSpeed * direction, rb.linearVelocity.y);

            // Update sprite direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !autoMoveRight;
            }
        }
        else
        {
            // Stop horizontal movement
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Manual movement control
    /// </summary>
    private void HandleManualMovement()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, rb.linearVelocity.y);

            // Flip sprite based on input
            if (moveInput.x != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = moveInput.x < 0;
            }

            isMoving = moveInput.x != 0;
        }
    }

    #endregion

    #region Ground Check

    /// <summary>
    /// Ground check menggunakan Tag "Ground"
    /// </summary>
    private bool CheckGroundWithTag()
    {
        if (groundCheck == null) return false;

        // Cek semua collider dalam radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);

        // Loop semua collider yang terdeteksi
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Skip self collider
            if (hitCollider == playerCollider) continue;

            // Cek apakah punya tag "Ground"
            if (hitCollider.CompareTag("Ground"))
            {
                return true; // Ground detected!
            }
        }

        return false; // Tidak ada ground
    }

    #endregion

    #region Jump

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!allowJump || isRolling || !canMove) return;

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

        if (animator != null && updateAnimator)
        {
            animator.SetTrigger("Jump");
        }
    }

    #endregion

    #region Roll

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (!allowRoll || !CanRoll()) return;

        // Determine roll direction
        if (autoMove)
        {
            // Auto-move: roll in auto-move direction
            rollDirection = autoMoveRight ? Vector2.right : Vector2.left;
        }
        else
        {
            // Manual: roll based on input or facing direction
            Vector2 inputDirection = moveAction.action.ReadValue<Vector2>();

            if (inputDirection.magnitude < 0.1f)
            {
                // No input: roll in facing direction
                rollDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }
            else
            {
                // Has input: roll in input direction
                rollDirection = new Vector2(inputDirection.x, 0f).normalized;

                if (rollDirection.x != 0 && spriteRenderer != null)
                {
                    spriteRenderer.flipX = rollDirection.x < 0;
                }
            }
        }

        StartRoll();
    }

    private bool CanRoll()
    {
        // Check mana
        if (manaScript != null && !manaScript.HasEnoughMana(rollManaCost))
        {
            return false;
        }

        // Check conditions
        return !isRolling
            && Time.time >= lastRollTime + rollCooldown
            && canMove
            && isGrounded;
    }

    private void StartRoll()
    {
        // Consume mana
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

        if (animator != null && updateAnimator)
        {
            animator.SetTrigger("Roll");
        }

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

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Start auto-movement
    /// </summary>
    public void StartAutoMovement()
    {
        isMoving = true;
        autoMove = true;
        currentMoveSpeed = autoMoveSpeed;
    }

    /// <summary>
    /// Stop auto-movement
    /// </summary>
    public void StopAutoMovement()
    {
        isMoving = false;
        if (autoMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Switch to manual control
    /// </summary>
    public void EnableManualControl()
    {
        autoMove = false;
        currentMoveSpeed = manualMoveSpeed;
        isMoving = false;
    }

    /// <summary>
    /// Switch to auto-move
    /// </summary>
    public void EnableAutoMove()
    {
        autoMove = true;
        currentMoveSpeed = autoMoveSpeed;
        StartAutoMovement();
    }

    /// <summary>
    /// Set auto-move direction
    /// </summary>
    public void SetAutoMoveDirection(bool moveRight)
    {
        autoMoveRight = moveRight;
    }

    /// <summary>
    /// Set movement speed
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (autoMove)
        {
            autoMoveSpeed = speed;
            currentMoveSpeed = speed;
        }
        else
        {
            manualMoveSpeed = speed;
            currentMoveSpeed = speed;
        }
    }

    /// <summary>
    /// Enable/disable movement
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove && !isRolling)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            isMoving = false;
        }
    }

    /// <summary>
    /// Stop all movement immediately
    /// </summary>
    public void StopMovement()
    {
        isMoving = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    #endregion

    #region Getters

    public bool IsFacingRight()
    {
        return spriteRenderer != null ? !spriteRenderer.flipX : true;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    public bool IsAutoMoving()
    {
        return autoMove;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsGroundedCheck()
    {
        return isGrounded;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        // Ground check visualization
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Roll direction visualization
        if (isRolling && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, rollDirection * 2f);
        }

        // Auto-move direction visualization
        if (autoMove && Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = autoMoveRight ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(transform.position, direction * 1.5f);
        }
    }

    #endregion
}