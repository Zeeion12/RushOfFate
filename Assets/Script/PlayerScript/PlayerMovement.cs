using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Roll Settings")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    public float invincibilityDuration = 0.5f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference rollAction;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool canMove = true; // Untuk disable movement saat attack

    // Roll state
    private bool isRolling = false;
    private bool isInvincible = false;
    private float lastRollTime = -999f;
    private float rollTimer = 0f;
    private Vector2 rollDirection;

    private PlayerMana manaScript;

    [Header("Mana Cost")]
    [SerializeField] private int rollManaCost = 2; // Cost untuk roll

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        manaScript = GetComponent<PlayerMana>();
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
        // Cek grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle roll timer
        if (isRolling)
        {
            rollTimer += Time.deltaTime;

            // Roll selesai setelah durasi habis
            if (rollTimer >= rollDuration)
            {
                EndRoll();
            }

            // Lakukan roll movement
            PerformRollMovement();
            return; // Skip normal movement saat rolling
        }

        // Gerakan horizontal (hanya kalau bisa gerak dan tidak rolling)
        if (canMove && !isRolling)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            // Flip sprite berdasarkan arah gerak
            if (moveInput.x != 0)
                spriteRenderer.flipX = moveInput.x < 0;
        }

        // Update parameter animasi (hanya jika tidak rolling)
        if (!isRolling)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Tidak bisa jump saat rolling
        if (isGrounded && canMove && !isRolling)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        // Check apakah bisa roll
        if (!CanRoll()) return;

        // Tentukan arah roll berdasarkan input movement
        Vector2 inputDirection = moveAction.action.ReadValue<Vector2>();

        // Jika tidak ada input, roll ke arah facing
        if (inputDirection.magnitude < 0.1f)
        {
            // Roll ke arah sprite menghadap
            rollDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        else
        {
            // Roll ke arah input (normalized agar konsisten)
            rollDirection = new Vector2(inputDirection.x, 0f).normalized;

            // Update facing direction sebelum roll
            if (rollDirection.x != 0)
                spriteRenderer.flipX = rollDirection.x < 0;
        }

        StartRoll();
    }

    private bool CanRoll()
    {
        // Check mana (PRIORITY) sebelum cooldown
        if (manaScript != null && !manaScript.HasEnoughMana(rollManaCost))
        {
            Debug.Log("Not enough mana to roll!");
            return false;
        }

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
                return; // Gagal consume, cancel roll
            }
        }

        isRolling = true;
        isInvincible = true;
        rollTimer = 0f;
        lastRollTime = Time.time;

        // Trigger animation
        animator.SetTrigger("Roll");

        // Stop vertical velocity untuk roll yang smooth
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Start invincibility coroutine
        StartCoroutine(InvincibilityCoroutine());

        Debug.Log("Roll started! Mana consumed.");
    }

    private void PerformRollMovement()
    {
        // Apply roll velocity
        rb.linearVelocity = new Vector2(rollDirection.x * rollSpeed, rb.linearVelocity.y);
    }

    private void EndRoll()
    {
        isRolling = false;
        rollTimer = 0f;

        Debug.Log("Roll ended!");
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        Debug.Log("Invincibility ended!");
    }

    // Public method untuk disable/enable movement (dipanggil dari PlayerAttack)
    public void SetCanMove(bool value)
    {
        canMove = value;

        // Stop movement kalau di-disable
        if (!canMove && !isRolling) // Jangan stop movement kalau sedang roll
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // Public getter untuk facing direction (dipakai PlayerAttack)
    public bool IsFacingRight()
    {
        return !spriteRenderer.flipX;
    }

    // Public getter untuk invincibility state (bisa dipakai CanineAttack atau damage system)
    public bool IsInvincible()
    {
        return isInvincible;
    }

    // Public getter untuk rolling state (bisa dipakai PlayerAttack untuk restrict attack saat roll)
    public bool IsRolling()
    {
        return isRolling;
    }

    // Visualisasi untuk debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualisasi roll direction saat rolling
        if (isRolling && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, rollDirection * 2f);
        }
    }
}