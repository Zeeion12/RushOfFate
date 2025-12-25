using System.Collections;
using UnityEngine;

public class MovingSpikeTrap : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 3f; // Jarak naik-turun (dalam Unity units)
    [SerializeField] private float moveSpeed = 2f; // Kecepatan pergerakan
    [SerializeField] private float pauseDuration = 0.5f; // Jeda di posisi atas/bawah (0 = tanpa jeda)

    [Header("Movement Direction")]
    [SerializeField] private bool moveVertical = true; // true = naik-turun, false = kiri-kanan
    [SerializeField] private bool startMovingUp = true; // Arah awal: true = ke atas, false = ke bawah

    [Header("Optional Settings")]
    [SerializeField] private bool useSmoothing = true; // Gunakan smooth movement (SmoothStep)

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private float journeyProgress = 0f;
    private bool isPaused = false;

    void Start()
    {
        // Simpan posisi awal
        startPos = transform.position;

        // Hitung posisi target berdasarkan arah
        if (moveVertical)
        {
            // Naik-turun (vertikal)
            float direction = startMovingUp ? 1f : -1f;
            targetPos = startPos + new Vector3(0, moveDistance * direction, 0);
        }
        else
        {
            // Kiri-kanan (horizontal)
            float direction = startMovingUp ? 1f : -1f; // startMovingUp jadi startMovingRight
            targetPos = startPos + new Vector3(moveDistance * direction, 0, 0);
        }
    }

    void Update()
    {
        if (isPaused) return;

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

    void OnDrawGizmosSelected()
    {
        // Visualisasi range pergerakan di editor
        Vector3 start = Application.isPlaying ? startPos : transform.position;
        Vector3 target;

        if (moveVertical)
        {
            float direction = startMovingUp ? 1f : -1f;
            target = start + new Vector3(0, moveDistance * direction, 0);
        }
        else
        {
            float direction = startMovingUp ? 1f : -1f;
            target = start + new Vector3(moveDistance * direction, 0, 0);
        }

        // Gambar garis dari start ke target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, target);

        // Gambar cube di posisi start dan target
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(start, Vector3.one * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(target, Vector3.one * 0.3f);
    }
}
