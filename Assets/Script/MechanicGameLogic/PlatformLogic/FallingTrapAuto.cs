using System.Collections;
using UnityEngine;

public class FallingTrapAuto : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeFall = 1f;      // Waktu tunggu sebelum jatuh pertama kali
    [SerializeField] private float fallGravity = 3f;          // Kecepatan jatuh
    [SerializeField] private float fallDistance = 20f;        // Jarak jatuh sebelum respawn
    [SerializeField] private float respawnDelay = 2f;         // Waktu tunggu sebelum respawn

    [Header("Collision")]
    [SerializeField] private LayerMask playerLayer;           // Layer untuk deteksi player

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    // State
    private Vector3 startPos;
    private bool isFalling = false;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Save starting position
        startPos = transform.position;

        // Setup awal: kinematic, no gravity
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        // Validation
        if (rb == null) Debug.LogError("Rigidbody2D tidak ditemukan pada " + gameObject.name);
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer tidak ditemukan pada " + gameObject.name);
        if (boxCollider == null) Debug.LogError("BoxCollider2D tidak ditemukan pada " + gameObject.name);

        // Mulai cycle pertama
        StartCoroutine(FallCycle());
    }

    IEnumerator FallCycle()
    {
        while (true) // Loop forever
        {
            // PHASE 1: TUNGGU SEBELUM JATUH
            yield return new WaitForSeconds(delayBeforeFall);

            // PHASE 2: JATUH
            yield return StartCoroutine(Fall());

            // PHASE 3: TUNGGU SEBELUM RESPAWN
            yield return new WaitForSeconds(respawnDelay);

            // PHASE 4: RESPAWN
            Respawn();
        }
    }

    IEnumerator Fall()
    {
        isFalling = true;

        // Aktifkan physics untuk jatuh
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;

        // Tunggu sampai jatuh cukup jauh
        float targetY = startPos.y - fallDistance;
        while (transform.position.y > targetY)
        {
            yield return null; // Wait next frame
        }

        // Hide trap setelah jatuh
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (boxCollider != null)
            boxCollider.enabled = false;

        // Stop physics
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        isFalling = false;
    }

    void Respawn()
    {
        // Reset position
        transform.position = startPos;

        // Show trap
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (boxCollider != null)
            boxCollider.enabled = true;

    }

    // Collision detection untuk insta-kill
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hanya damage saat sedang jatuh
        if (!isFalling) return;

        // Check apakah yang kena adalah player
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} hit player - INSTA KILL!");

            // Dapatkan CheckpointManager untuk respawn player
            CheckpointManager checkpoint = collision.GetComponent<CheckpointManager>();
            if (checkpoint != null)
            {
                // Trigger respawn ke checkpoint terakhir
                // Menggunakan sistem yang sudah ada di CheckpointManager
                collision.transform.position = checkpoint.GetComponent<CheckpointManager>().transform.position;
            }
            else
            {
                Debug.LogWarning("Player tidak memiliki CheckpointManager!");
            }

            // TODO: Nanti bisa ditambahkan:
            // - PlayerHealth system untuk instant death
            // - Death animation
            // - Sound effect
        }
    }

    // Visualisasi di editor
    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? startPos : transform.position;

        // Visualisasi fall distance
        Gizmos.color = Color.red;
        Vector3 fallEnd = origin + Vector3.down * fallDistance;
        Gizmos.DrawLine(origin, fallEnd);

        // Visualisasi trap area
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        if (GetComponent<BoxCollider2D>() != null)
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Gizmos.DrawCube(origin + (Vector3)col.offset, col.size);
        }
    }
}