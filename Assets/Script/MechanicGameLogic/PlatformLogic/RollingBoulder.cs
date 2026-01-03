using System.Collections;
using UnityEngine;

public class RollingBoulder : MonoBehaviour
{
    // Enum untuk arah boulder
    public enum RollDirection
    {
        Right,  // Menggelinding ke kanan
        Left    // Menggelinding ke kiri
    }

    [Header("Movement Settings")]
    [SerializeField] private float rollSpeed = 5f;              // Kecepatan menggelinding
    [SerializeField] private float rotationSpeed = 360f;        // Kecepatan rotasi sprite (derajat per detik)
    [SerializeField] private RollDirection rollDirection = RollDirection.Right;  // Arah menggelinding

    [Header("Spawn Settings")]
    [SerializeField] private float delayBeforeRoll = 1f;        // Waktu tunggu sebelum menggelinding pertama kali
    [SerializeField] private float respawnDelay = 3f;           // Waktu tunggu sebelum respawn setelah destroy
    [SerializeField] private float maxLifetime = 10f;           // Waktu maksimal boulder hidup sebelum auto-destroy

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;             // Point untuk cek ground
    [SerializeField] private float groundCheckDistance = 0.5f;  // Jarak raycast ke bawah
    [SerializeField] private LayerMask groundLayer;             // Layer untuk ground/platform

    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 2;              // Damage yang diberikan ke player
    [SerializeField] private bool destroyOnHitPlayer = true;    // Hancur setelah kena player

    [Header("Physics Settings")]
    [SerializeField] private float fallGravityScale = 3f;       // Gravity saat jatuh

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource boulderAudioSource;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField][Range(0f, 1f)] private float rollVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float hitVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showDebugLogs = false;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D boulderCollider;

    // State
    private Vector3 startPos;
    private Quaternion startRotation;
    private bool isRolling = false;
    private bool isGrounded = false;
    private bool hasHitPlayer = false;
    private float lifetimeTimer = 0f;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boulderCollider = GetComponent<Collider2D>();

        // Auto-create ground check if not assigned
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = checkObj.transform;
        }

        // Get or create audio source
        if (boulderAudioSource == null)
        {
            boulderAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Save starting position and rotation
        startPos = transform.position;
        startRotation = transform.rotation;

        // Setup physics: dynamic dengan gravity
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Freeze rotation fisik, kita rotate manual

        // Validation
        if (rb == null) Debug.LogError("[RollingBoulder] Rigidbody2D tidak ditemukan pada " + gameObject.name);
        if (spriteRenderer == null) Debug.LogError("[RollingBoulder] SpriteRenderer tidak ditemukan pada " + gameObject.name);
        if (boulderCollider == null) Debug.LogError("[RollingBoulder] Collider2D tidak ditemukan pada " + gameObject.name);

        // Mulai roll cycle pertama
        StartCoroutine(RollCycle());
    }

    void Update()
    {
        if (isRolling)
        {
            // Check ground
            isGrounded = CheckGround();

            // Rotate sprite untuk efek menggelinding
            RotateSprite();

            // Track lifetime
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= maxLifetime)
            {
                if (showDebugLogs)
                    Debug.Log($"[RollingBoulder] {gameObject.name} reached max lifetime, destroying...");

                DestroyBoulder();
            }
        }
    }

    void FixedUpdate()
    {
        if (isRolling)
        {
            // Gerakkan boulder secara horizontal
            float direction = (rollDirection == RollDirection.Right) ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
        }
    }

    IEnumerator RollCycle()
    {
        while (true) // Loop forever untuk auto respawn
        {
            // PHASE 1: TUNGGU SEBELUM ROLL
            yield return new WaitForSeconds(delayBeforeRoll);

            // PHASE 2: MULAI ROLL
            StartRolling();

            // Play roll sound
            if (boulderAudioSource != null && rollSound != null)
            {
                boulderAudioSource.clip = rollSound;
                boulderAudioSource.volume = rollVolume;
                boulderAudioSource.loop = true;
                boulderAudioSource.Play();
            }

            // Tunggu sampai boulder di-destroy (by lifetime, fall, atau hit player)
            while (isRolling)
            {
                yield return null;
            }

            // Stop roll sound
            if (boulderAudioSource != null && boulderAudioSource.isPlaying)
            {
                boulderAudioSource.Stop();
            }

            // PHASE 3: TUNGGU SEBELUM RESPAWN
            yield return new WaitForSeconds(respawnDelay);

            // PHASE 4: RESPAWN
            Respawn();
        }
    }

    void StartRolling()
    {
        isRolling = true;
        lifetimeTimer = 0f;
        hasHitPlayer = false;

        if (showDebugLogs)
            Debug.Log($"[RollingBoulder] {gameObject.name} started rolling!");
    }

    void RotateSprite()
    {
        // Rotate sprite untuk visual effect menggelinding
        // Negatif jika roll ke kanan (clockwise), positif jika ke kiri (counter-clockwise)
        float rotationDir = (rollDirection == RollDirection.Right) ? -1f : 1f;
        float rotationAmount = rotationDir * rotationSpeed * Time.deltaTime;

        transform.Rotate(0, 0, rotationAmount);
    }

    bool CheckGround()
    {
        if (groundCheck == null) return false;

        // Raycast ke bawah untuk deteksi ground
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Jika tidak ada ground, berarti boulder jatuh
        return hit.collider != null;
    }

    void DestroyBoulder()
    {
        isRolling = false;

        // Hide boulder
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (boulderCollider != null)
            boulderCollider.enabled = false;

        // Stop physics
        rb.linearVelocity = Vector2.zero;

        if (showDebugLogs)
            Debug.Log($"[RollingBoulder] {gameObject.name} destroyed!");
    }

    void Respawn()
    {
        // Reset position dan rotation
        transform.position = startPos;
        transform.rotation = startRotation;

        // Reset velocity
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Show boulder
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (boulderCollider != null)
            boulderCollider.enabled = true;

        if (showDebugLogs)
            Debug.Log($"[RollingBoulder] {gameObject.name} respawned!");
    }

    // Collision detection untuk damage player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hanya damage saat sedang rolling dan belum hit player
        if (!isRolling || hasHitPlayer) return;

        // Check apakah yang kena adalah player
        if (collision.CompareTag("Player"))
        {
            hasHitPlayer = true;

            if (showDebugLogs)
                Debug.Log($"[RollingBoulder] {gameObject.name} hit player - Dealing {damageAmount} damage!");

            // Dapatkan PlayerHealth untuk damage
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Check apakah player invincible (dari roll atau respawn)
                PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
                bool playerInvincible = false;

                if (playerMovement != null && playerMovement.IsInvincible())
                {
                    playerInvincible = true;
                }

                if (!playerInvincible && !playerHealth.IsInvincible)
                {
                    playerHealth.TakeDamage(damageAmount);

                    // Play hit sound
                    if (boulderAudioSource != null && hitSound != null)
                    {
                        boulderAudioSource.PlayOneShot(hitSound, hitVolume);
                    }
                }
                else
                {
                    if (showDebugLogs)
                        Debug.Log($"[RollingBoulder] Player is invincible, no damage dealt!");
                }
            }
            else
            {
                Debug.LogWarning("[RollingBoulder] Player tidak memiliki PlayerHealth component!");
            }

            // Hancurkan boulder jika setting enabled
            if (destroyOnHitPlayer)
            {
                DestroyBoulder();
            }
        }
    }

    // Visualisasi di editor
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Vector3 origin = Application.isPlaying ? startPos : transform.position;

        // Visualisasi ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance);
            Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * groundCheckDistance, 0.1f);
        }

        // Visualisasi arah roll
        Gizmos.color = Color.cyan;
        float arrowLength = 2f;
        Vector3 direction = (rollDirection == RollDirection.Right) ? Vector3.right : Vector3.left;
        Vector3 arrowEnd = origin + direction * arrowLength;
        Gizmos.DrawLine(origin, arrowEnd);

        // Arrow head
        Vector3 arrowHead1 = arrowEnd + Quaternion.Euler(0, 0, 135) * direction * 0.5f;
        Vector3 arrowHead2 = arrowEnd + Quaternion.Euler(0, 0, -135) * direction * 0.5f;
        Gizmos.DrawLine(arrowEnd, arrowHead1);
        Gizmos.DrawLine(arrowEnd, arrowHead2);

        // Visualisasi boulder area
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        if (GetComponent<CircleCollider2D>() != null)
        {
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            Gizmos.DrawWireSphere(origin + (Vector3)col.offset, col.radius);
        }
        else if (GetComponent<BoxCollider2D>() != null)
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Gizmos.DrawCube(origin + (Vector3)col.offset, col.size);
        }
    }
}
