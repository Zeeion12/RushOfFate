using System.Collections;
using UnityEngine;

/// <summary>
/// Moving Wall yang membuat player "menempel" dan ikut bergerak
/// Berbeda dengan MovingPlatform yang mendeteksi dari atas,
/// MovingWall mendeteksi player yang menempel dari samping (kiri/kanan)
/// </summary>
public class MovingWall : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 5f;
    [Tooltip("Jarak pergerakan wall (dalam Unity units)")]

    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("Kecepatan pergerakan wall")]

    [SerializeField] private float pauseDuration = 0.5f;
    [Tooltip("Durasi jeda di posisi atas/bawah (0 = tanpa jeda)")]

    [Header("Movement Direction")]
    [SerializeField] private bool moveVertical = true;
    [Tooltip("true = naik-turun (vertical), false = kiri-kanan (horizontal)")]

    [SerializeField] private bool startMovingUp = true;
    [Tooltip("Arah awal gerakan: true = ke atas/kanan, false = ke bawah/kiri")]

    [Header("Optional Settings")]
    [SerializeField] private bool useSmoothing = true;
    [Tooltip("Gunakan smooth movement (SmoothStep) untuk gerakan lebih halus")]

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Tag untuk mendeteksi player (default: Player)")]

    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Layer untuk detection player (optional, bisa pakai tag saja)")]

    // Private variables
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private float journeyProgress = 0f;
    private bool isPaused = false;

    // Player tracking
    private Transform playerOnWall = null;
    private Vector3 lastWallPosition;

    void Start()
    {
        // Simpan posisi awal
        startPos = transform.position;
        lastWallPosition = transform.position;

        // Hitung posisi target berdasarkan arah
        CalculateTargetPosition();

        // Validasi
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"MovingWall on {gameObject.name}: Missing Collider2D! Player detection won't work.");
        }
    }

    void CalculateTargetPosition()
    {
        if (moveVertical)
        {
            // Naik-turun (vertikal)
            float direction = startMovingUp ? 1f : -1f;
            targetPos = startPos + new Vector3(0, moveDistance * direction, 0);
        }
        else
        {
            // Kiri-kanan (horizontal)
            float direction = startMovingUp ? 1f : -1f;
            targetPos = startPos + new Vector3(moveDistance * direction, 0, 0);
        }
    }

    void Update()
    {
        if (isPaused) return;

        // Simpan posisi sebelum bergerak
        Vector3 previousPosition = transform.position;

        // Update posisi wall
        MoveWall();

        // Hitung delta pergerakan
        Vector3 wallDelta = transform.position - previousPosition;

        // ✅ KEY FIX: Gerakkan player bersama wall
        if (playerOnWall != null)
        {
            playerOnWall.position += wallDelta;
        }

        // Update last position
        lastWallPosition = transform.position;
    }

    void MoveWall()
    {
        // Tentukan posisi awal dan tujuan
        Vector3 from = movingToTarget ? startPos : targetPos;
        Vector3 to = movingToTarget ? targetPos : startPos;

        // Update progress pergerakan
        journeyProgress += Time.deltaTime * moveSpeed / moveDistance;

        // Interpolasi posisi
        float t = Mathf.Clamp01(journeyProgress);
        if (useSmoothing)
        {
            // Smooth movement (ease in-out)
            t = Mathf.SmoothStep(0f, 1f, t);
        }

        transform.position = Vector3.Lerp(from, to, t);

        // Jika sudah sampai tujuan
        if (journeyProgress >= 1f)
        {
            journeyProgress = 0f;
            movingToTarget = !movingToTarget;

            // Pause jika ada durasi jeda
            if (pauseDuration > 0)
            {
                StartCoroutine(PauseAtPosition());
            }
        }
    }

    IEnumerator PauseAtPosition()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    // ✅ PLAYER DETECTION: Saat player menempel ke wall
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check apakah yang menempel adalah player
        if (IsPlayer(collision.gameObject))
        {
            // ✅ CRITICAL: Cek apakah player benar-benar menempel di SAMPING wall
            // Bukan menyentuh dari atas atau bawah
            if (IsPlayerOnSide(collision))
            {
                playerOnWall = collision.transform;
                Debug.Log($"Player menempel ke wall: {gameObject.name}");
            }
        }
    }

    // ✅ PLAYER DETECTION: Saat player lepas dari wall
    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            if (playerOnWall == collision.transform)
            {
                playerOnWall = null;
                Debug.Log($"Player lepas dari wall: {gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Check apakah GameObject adalah player
    /// </summary>
    bool IsPlayer(GameObject obj)
    {
        // Check by tag
        if (obj.CompareTag(playerTag))
            return true;

        // Optional: Check by layer (jika di-assign)
        if (playerLayer != 0)
        {
            if (((1 << obj.layer) & playerLayer) != 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// ✅ CRITICAL FIX: Check apakah player benar-benar di SAMPING wall
    /// Mencegah player "menempel" saat menyentuh dari atas/bawah
    /// </summary>
    bool IsPlayerOnSide(Collision2D collision)
    {
        // Cek contact point pertama
        if (collision.contactCount > 0)
        {
            ContactPoint2D contact = collision.GetContact(0);

            // Normal vector menunjuk ke arah "keluar" dari collider
            // Jika player di SAMPING, normal.x akan lebih besar dari normal.y
            // Threshold 0.5f untuk horizontal contact
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    // Visualisasi di editor
    void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPos : transform.position;
        Vector3 target;

        // Hitung target position untuk gizmo
        if (moveVertical)
        {
            float dir = startMovingUp ? 1f : -1f;
            target = start + new Vector3(0, moveDistance * dir, 0);
        }
        else
        {
            float dir = startMovingUp ? 1f : -1f;
            target = start + new Vector3(moveDistance * dir, 0, 0);
        }

        // Gambar garis dari start ke target
        Gizmos.color = Color.magenta; // Magenta untuk wall (beda dari cyan platform)
        Gizmos.DrawLine(start, target);

        // Gambar cube di posisi start dan target
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(start, Vector3.one * 0.3f);

        Gizmos.color = Color.red; // Red untuk target wall
        Gizmos.DrawWireCube(target, Vector3.one * 0.3f);

        // Draw arrow untuk visualisasi arah
        Vector3 arrowDirection = (target - start).normalized;
        Vector3 arrowTip = start + arrowDirection * moveDistance * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, arrowTip);

        // Draw arrowhead
        Vector3 right = Vector3.Cross(arrowDirection, Vector3.forward).normalized * 0.2f;
        Gizmos.DrawLine(arrowTip, arrowTip - arrowDirection * 0.3f + right);
        Gizmos.DrawLine(arrowTip, arrowTip - arrowDirection * 0.3f - right);
    }
}
