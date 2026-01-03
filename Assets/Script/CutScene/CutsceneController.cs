using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    [Header("Character")]
    public GameObject player;
    public float runSpeed = 5f;
    public float jumpForce = 12f;
    
    [Header("Rock Obstacle")]
    public Transform rockPosition;
    public float slowMotionScale = 0.3f;
    public float slowMotionDuration = 2f;
    public float detectionDistance = 3f;
    
    [Header("QTE (Quick Time Event)")]
    public GameObject qteUI;
    public Text qteTimerText;
    public float qteTimeLimit = 1.5f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    [Header("Camera")]
    public Camera mainCamera;
    public float cameraFollowSpeed = 3f;
    
    private Rigidbody2D rb;
    private bool isRunning = true;
    private bool qteActive = false;
    private bool cutsceneComplete = false;
    private bool hasJumped = false;
    private Vector3 cameraOffset;
    private Animator animator;
    private float originalAnimatorSpeed = 1f;

    [Header("Credits")]
    public CreditsController creditsController;

    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        animator = player.GetComponent<Animator>();
        
        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
        }
        
        cameraOffset = mainCamera.transform.position - player.transform.position;
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (qteUI != null)
            qteUI.SetActive(false);
        
        // TAMBAHAN: Continue footsteps dari loading scene
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ContinueFootstepsIfPlaying();
        }
        
        StartCoroutine(CutsceneSequence());
    }

    void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("=== DEBUG INFO ===");
            Debug.Log($"isRunning: {isRunning}");
            Debug.Log($"qteActive: {qteActive}");
            Debug.Log($"cutsceneComplete: {cutsceneComplete}");
            Debug.Log($"Velocity: {rb.linearVelocity}");
            Debug.Log($"Position: {player.transform.position}");
            Debug.Log($"IsGrounded: {IsGrounded()}");
            
            if (rockPosition != null)
            {
                float dist = Vector2.Distance(player.transform.position, rockPosition.position);
                Debug.Log($"Distance to Rock: {dist:F2} / {detectionDistance}");
            }
            else
            {
                Debug.LogError("ROCK POSITION IS NULL!");
            }
        }
        
        // VISUALISASI: Draw line dari player ke rock (hanya di editor)
        if (rockPosition != null && Application.isEditor)
        {
            Debug.DrawLine(player.transform.position, rockPosition.position, Color.cyan);
        }
        
        // Movement control
        if (isRunning && !qteActive && !cutsceneComplete)
        {
            rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);
            
            if (IsGrounded() && AudioManager.Instance != null)
            {
                AudioManager.Instance.StartFootsteps();
            }
        }
        else
        {
            if (!qteActive && !cutsceneComplete)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopFootsteps();
            }
        }
        
        // Camera follow
        if (!cutsceneComplete)
        {
            Vector3 targetPosition = player.transform.position + cameraOffset;
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetPosition, 
                cameraFollowSpeed * Time.deltaTime
            );
        }
        
        // QTE Input
        if (qteActive && Input.GetKeyDown(KeyCode.Space))
        {
            SuccessfulJump();
        }

         if (animator != null)
        {
            animator.SetBool("isGrounded", IsGrounded());
            
            // Update running animation speed based on actual velocity
            if (isRunning && !qteActive)
            {
                float speedMultiplier = rb.linearVelocity.x / runSpeed;
                animator.speed = originalAnimatorSpeed * Mathf.Clamp(speedMultiplier, 0.5f, 2f);
            }
        }
    }

    IEnumerator CutsceneSequence()
    {
        Debug.Log("=== CUTSCENE START ===");
        
        // Phase 1: Running menuju batu
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"Waiting for player to reach rock...");
        Debug.Log($"Rock Position: {rockPosition.position}");
        Debug.Log($"Detection Distance: {detectionDistance}");
        
        // Tunggu sampai mendekati batu
        float timeout = 30f; // Safety timeout 30 detik
        float elapsed = 0f;
        
        while (Vector2.Distance(player.transform.position, rockPosition.position) > detectionDistance)
        {
            elapsed += Time.deltaTime;
            
            // Debug setiap 1 detik
            if (elapsed % 1f < 0.1f)
            {
                float distance = Vector2.Distance(player.transform.position, rockPosition.position);
                Debug.Log($"Distance to rock: {distance:F2} (need < {detectionDistance})");
            }
            
            // Timeout safety
            if (elapsed > timeout)
            {
                Debug.LogError("TIMEOUT: Player tidak sampai ke batu dalam 30 detik!");
                yield break;
            }
            
            yield return null;
        }
        
        Debug.Log("Player reached rock! Starting QTE...");
        
        // STOP movement sebelum QTE
        isRunning = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // Phase 2: Slow motion QTE
        yield return StartCoroutine(RockJumpQTE());
        
        // Jika tidak berhasil jump, jangan lanjut
        if (!hasJumped)
        {
            Debug.Log("Jump failed, stopping cutscene");
            yield break;
        }
        
        Debug.Log("Jump successful! Continuing...");
        
        // Phase 3: Tunggu landing setelah jump
        yield return new WaitForSeconds(0.5f);
        
        while (!IsGrounded())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Phase 4: Continue running menuju cliff
        isRunning = true;
        yield return new WaitForSeconds(2f);
        
        
        // Phase 6: Landing dan parallax
        yield return new WaitForSeconds(2f);

        // TAMBAHKAN: Show Credits!
        Debug.Log("=== SHOWING END CREDITS ===");
        
        if (creditsController != null)
        {
            creditsController.ShowCredits();
            
            // Tunggu credits selesai (fade in + display + fade out)
            float totalCreditsTime = creditsController.fadeInDuration 
                                    + creditsController.displayDuration 
                                    + creditsController.fadeOutDuration;
            yield return new WaitForSeconds(totalCreditsTime);
        }

        // End cutscene
        cutsceneComplete = false;
        isRunning = true;
        Debug.Log("=== CUTSCENE COMPLETE ===");
    }

    IEnumerator RockJumpQTE()
    {
        Debug.Log("Starting QTE - Slow Motion!");
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopFootsteps();
        
        Time.timeScale = slowMotionScale;
        
        if (qteUI != null)
            qteUI.SetActive(true);
        
        qteActive = true;
        float qteTimer = qteTimeLimit;
        
        // Get text component untuk ubah warna
        Color originalColor = Color.white;
        if (qteTimerText != null)
        {
            originalColor = qteTimerText.color;
        }
        
        while (qteTimer > 0 && qteActive)
        {
            qteTimer -= Time.unscaledDeltaTime;
            
            if (qteTimerText != null)
            {
                qteTimerText.text = "PRESS SPACE!\n" + qteTimer.ToString("F2") + "s";
                
                // TAMBAHAN: Ubah warna jadi merah saat hampir habis
                if (qteTimer < 0.5f)
                {
                    // Flash merah cepat
                    qteTimerText.color = Color.Lerp(Color.red, Color.yellow, 
                        Mathf.PingPong(Time.unscaledTime * 5f, 1f));
                }
                else if (qteTimer < 1f)
                {
                    // Kuning warning
                    qteTimerText.color = Color.yellow;
                }
                else
                {
                    // Putih normal
                    qteTimerText.color = originalColor;
                }
            }
            
            yield return null;
        }
        
        // Reset color
        if (qteTimerText != null)
        {
            qteTimerText.color = originalColor;
        }
        
        if (qteActive)
        {
            FailedJump();
        }
    }

    void SuccessfulJump()
    {
        qteActive = false;
        hasJumped = true;
        
        if (qteUI != null)
            qteUI.SetActive(false);
        
        Time.timeScale = 1f;
        
        rb.gravityScale = 3;
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(runSpeed * 1.5f, jumpForce * 1.2f);
        
        // PERBAIKAN: Trigger jump animation
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
            animator.SetTrigger("DoJump"); // Trigger jump
            animator.SetBool("isGrounded", false); // Set not grounded
        }
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSFX);
        
        Debug.Log($"Jump! Triggered DoJump animator");
        
        StartCoroutine(MaintainJumpMomentum());
    }

    void FailedJump()
    {
        Debug.Log("QTE Failed! Restarting...");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reset animator speed
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
        }
        
        // Reset cutscene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    IEnumerator MaintainJumpMomentum()
    {
        float targetHorizontalSpeed = runSpeed * 1.5f;
        
        Debug.Log("=== JUMP PHASE START ===");
        
        // Phase 1: In Air (Jump animation playing)
        while (!IsGrounded())
        {
            // Maintain horizontal velocity
            if (rb.linearVelocity.x < targetHorizontalSpeed * 0.8f)
            {
                rb.linearVelocity = new Vector2(targetHorizontalSpeed, rb.linearVelocity.y);
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        Debug.Log("=== LANDING DETECTED ===");
        
        // Phase 2: Landing (trigger landing animation)
        if (animator != null)
        {
            animator.SetTrigger("OnLanding"); // Trigger landing animation
            animator.SetBool("isGrounded", true);
            
            Debug.Log("Triggered OnLanding animation");
        }
        
        // Play landing SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.landingSFX);
        }
        
        // Wait for landing animation to finish
        yield return new WaitForSeconds(0.3f); // Durasi landing animation
        
        Debug.Log("=== LANDING COMPLETE, RESUMING RUN ===");
        
        // Phase 3: Resume running
        isRunning = true;
    }
    
    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck is null!");
            return true;
        }
        
        // Check dengan OverlapCircle
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Loop dan ignore player
        foreach (Collider2D col in colliders)
        {
            // IGNORE player dan child-nya
            if (col.gameObject == player || col.transform.IsChildOf(player.transform))
            {
                continue; // Skip
            }
            
            // Ada ground!
            return true;
        }
        
        // Tidak ada ground
        return false;
    }
    
    // Debug Gizmos
    void OnDrawGizmos()
    {
        if (rockPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rockPosition.position, detectionDistance);
        }
        
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}