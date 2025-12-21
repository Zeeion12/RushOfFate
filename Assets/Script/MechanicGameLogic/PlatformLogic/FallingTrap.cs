using System.Collections;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionHeight = 0.5f;
    [SerializeField] private Vector2 detectionSize = new Vector2(1f, 0.3f);

    [Header("Falling")]
    [SerializeField] private float shakeTime = 0.5f;
    [SerializeField] private float fallGravity = 3f;
    [SerializeField] private float fallDistance = 20f;
    [SerializeField] private float respawnDelay = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Vector3 startPos;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        startPos = transform.position;

        // Setup awal: diam
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
    }

    void Update()
    {
        if (isFalling) return;

        // Cek player di atas
        Vector2 checkPos = (Vector2)transform.position + Vector2.up * detectionHeight;
        if (Physics2D.OverlapBox(checkPos, detectionSize, 0f, playerLayer))
        {
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        isFalling = true;

        // === PHASE 1: SHAKE ===
        float timer = 0;
        while (timer < shakeTime)
        {
            transform.position = startPos + new Vector3(Random.Range(-0.1f, 0.1f), 0, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;

        // === PHASE 2: FALL ===
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;

        // ✅ TUNGGU SAMPAI JATUH CUKUP JAUH (bukan tunggu waktu!)
        float targetY = startPos.y - fallDistance;
        while (transform.position.y > targetY)
        {
            yield return null; // Wait next frame
        }

        // === PHASE 3: HIDE & RESPAWN ===
        // Hide visual
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (boxCollider != null)
            boxCollider.enabled = false;

        // Stop physics
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        // Tunggu sebelum respawn
        yield return new WaitForSeconds(respawnDelay);

        // Reset ke posisi awal
        transform.position = startPos;

        // Show visual
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (boxCollider != null)
            boxCollider.enabled = true;

        isFalling = false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 checkPos = transform.position + Vector3.up * detectionHeight;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(checkPos, detectionSize);

        // ✅ Visualisasi fall distance
        Gizmos.color = Color.cyan;
        Vector3 fallEnd = transform.position + Vector3.down * fallDistance;
        Gizmos.DrawLine(transform.position, fallEnd);
    }
}