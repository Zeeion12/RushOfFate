using System.Collections;
using UnityEngine;
public class FallingPlatform : MonoBehaviour
{
    [Header("Falling Settings")]
    [SerializeField] private float shakeTime = 0.5f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float respawnTime = 3f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeFrequency = 30f;

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionHeight = 0.5f;
    [SerializeField] private Vector2 detectionSize = new Vector2(1f, 0.2f); // Ukuran area deteksi

    [Header("Visual Effects (Optional)")]
    [SerializeField] private ParticleSystem warningParticle;
    [SerializeField] private ParticleSystem fallParticle;

    // Components
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    // State
    private Vector3 originalPosition;
    private Color originalColor;
    private bool isFalling = false;

    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Save original
        originalPosition = transform.position;
        originalColor = spriteRenderer.color;

        // Validation
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer tidak ditemukan!");
        if (boxCollider == null) Debug.LogError("BoxCollider2D tidak ditemukan!");
        if (rb == null) Debug.LogError("Rigidbody2D tidak ditemukan!");

        // Stop particles
        if (warningParticle != null) warningParticle.Stop();
        if (fallParticle != null) fallParticle.Stop();
    }

    void Update()
    {
        if (isFalling) return;
        CheckPlayerOnPlatform();
    }

    void CheckPlayerOnPlatform()
    {
        // Posisi deteksi: tepat di ATAS collider platform ini
        Vector2 detectionPosition = new Vector2(
            transform.position.x,
            transform.position.y + (boxCollider.size.y / 2f) + (detectionHeight / 2f)
        );

        // BoxCast untuk deteksi player
        RaycastHit2D hit = Physics2D.BoxCast(
            detectionPosition,
            detectionSize,
            0f,
            Vector2.up,
            0.01f,
            playerLayer
        );

        // Jika player terdeteksi, mulai fall sequence
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            StartCoroutine(FallSequence());
        }
    }

    IEnumerator FallSequence()
    {
        isFalling = true;

        // PHASE 1: SHAKE/WARNING
        if (warningParticle != null)
        {
            warningParticle.transform.position = transform.position;
            warningParticle.Play();
        }

        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < shakeTime)
        {
            // Shake effect
            float offsetX = Mathf.Sin(Time.time * shakeFrequency) * shakeIntensity;
            float offsetY = Mathf.Sin(Time.time * shakeFrequency * 1.3f) * shakeIntensity * 0.5f;
            transform.position = startPos + new Vector3(offsetX, offsetY, 0f);

            // Color blink
            float t = Mathf.PingPong(Time.time * 10f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset position & color
        transform.position = startPos;
        spriteRenderer.color = originalColor;

        if (warningParticle != null) warningParticle.Stop();

        // PHASE 2: FALL
        if (fallParticle != null)
        {
            fallParticle.transform.position = transform.position;
            fallParticle.Play();
        }

        // Disable collider
        boxCollider.enabled = false;

        // Jatuh
        while (transform.position.y > originalPosition.y - 20f)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            if (fallParticle != null)
                fallParticle.transform.position = transform.position;

            yield return null;
        }

        // PHASE 3: HIDE
        spriteRenderer.enabled = false;
        if (fallParticle != null) fallParticle.Stop();

        yield return new WaitForSeconds(respawnTime);

        // PHASE 4: RESPAWN
        Respawn();
    }

    void Respawn()
    {
        // Reset semua
        transform.position = originalPosition;
        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
        boxCollider.enabled = true;
        isFalling = false;

        Debug.Log($"{gameObject.name} respawned!");
    }

    // Visualisasi detection area di editor
    void OnDrawGizmosSelected()
    {
        // Get collider bounds
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Posisi detection box
        Vector3 detectionPos = new Vector3(
            transform.position.x,
            transform.position.y + (col.size.y / 2f) + (detectionHeight / 2f),
            0f
        );

        // Draw detection area
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(detectionPos, new Vector3(detectionSize.x, detectionSize.y, 0.1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(detectionPos, new Vector3(detectionSize.x, detectionSize.y, 0.1f));
    }
}