using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PuzzleDoor : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float moveDownDistance = 2f; // Door turun ke bawah sambil fade

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioSource audioSource;

    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;
    private bool isOpening = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Auto-find AudioSource jika tidak di-assign
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ValidateSetup();
    }

    /// <summary>
    /// Dipanggil oleh PuzzleInteractable saat puzzle solved
    /// </summary>
    public void OpenDoor()
    {
        if (isOpening) return; // Prevent double call

        Debug.Log($"Opening door: {gameObject.name}");

        // Play sound (optional)
        PlayOpenSound();

        // Start fade out + move down animation
        StartCoroutine(FadeOutAndDisable());
    }

    System.Collections.IEnumerator FadeOutAndDisable()
    {
        isOpening = true;

        // Disable collider immediately agar player bisa lewat
        if (doorCollider != null)
            doorCollider.enabled = false;

        // Simpan posisi awal
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * moveDownDistance;

        // Simpan alpha awal
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;

        // Fade out + move down simultaneously
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Smooth fade out
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            // Smooth move down
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // Ensure final values
        spriteRenderer.color = endColor;
        transform.position = endPosition;

        // Disable GameObject setelah animasi selesai
        gameObject.SetActive(false);

        Debug.Log($"Door fully opened: {gameObject.name}");
    }

    void PlayOpenSound()
    {
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
    }

    void ValidateSetup()
    {
        if (spriteRenderer == null)
            Debug.LogError($"PuzzleDoor on {gameObject.name}: SpriteRenderer not found!");

        if (doorCollider == null)
            Debug.LogError($"PuzzleDoor on {gameObject.name}: Collider2D not found!");

        // Warning untuk audio (optional)
        if (doorOpenSound != null && audioSource == null)
            Debug.LogWarning($"PuzzleDoor on {gameObject.name}: Door Open Sound assigned but no AudioSource found!");
    }

    // Gizmo untuk visualisasi di editor
    void OnDrawGizmosSelected()
    {
        // Visualisasi area move down
        Gizmos.color = Color.cyan;
        Vector3 endPos = transform.position + Vector3.down * moveDownDistance;
        Gizmos.DrawLine(transform.position, endPos);

        // Draw arrow
        Gizmos.DrawSphere(endPos, 0.2f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            "PUZZLE DOOR",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.cyan },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            }
        );
#endif
    }
}