using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    // ❌ REMOVED: public LayerMask groundLayer;

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

    [Header("Mana Cost")]
    [SerializeField] private int rollManaCost = 2;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference rollAction;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    private AudioManager audioManager;
    private bool isFootstepPlaying = false;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool canMove = true;

    // Roll state
    private bool isRolling = false;
    private bool isInvincible = false;
    private float lastRollTime = -999f;
    private float rollTimer = 0f;
    private Vector2 rollDirection;

    private PlayerMana manaScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        manaScript = GetComponent<PlayerMana>();
        audioManager = AudioManager.Instance;
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
        // ✅ NEW: Tag-based ground check
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
            return;
        }

        // Gerakan horizontal
        if (canMove && !isRolling)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            if (moveInput.x != 0)
                spriteRenderer.flipX = moveInput.x < 0;
        }

        // Update animasi
        if (!isRolling)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", isGrounded);
        }

        HandleFootstepSound();
    }

    // ✅ NEW: Ground check pakai Tag "Ground"
    private bool CheckGroundWithTag()
    {
        // Cek semua collider dalam radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);

        // Loop semua collider yang terdeteksi
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Cek apakah punya tag "Ground"
            if (hitCollider.CompareTag("Ground"))
            {
                return true; // Ground detected!
            }
        }

        return false; // Tidak ada ground
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isRolling || !canMove) return;

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
        if (manaScript != null && !manaScript.HasEnoughMana(rollManaCost))
        {
            return false;
        }

        return !isRolling
            && Time.time >= lastRollTime + rollCooldown
            && canMove
            && isGrounded;
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (isRolling && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, rollDirection * 2f);
        }
    }

    private void HandleFootstepSound()
    {
        if (audioManager == null) return;

        bool shouldPlayFootstep =
            Mathf.Abs(rb.linearVelocity.x) > 0.1f &&
            isGrounded &&
            canMove &&
            !isRolling;

        if (shouldPlayFootstep)
        {
            if (!isFootstepPlaying)
            {
                audioManager.PlayFootstep();
                isFootstepPlaying = true;
            }
        }
        else
        {
            if (isFootstepPlaying)
            {
                audioManager.StopFootstep();
                isFootstepPlaying = false;
            }
        }
    }


}