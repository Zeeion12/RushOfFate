using UnityEngine;
using System.Collections;

public class BossDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isEntranceDoor = true; // true = pintu masuk, false = pintu keluar
    [SerializeField] private bool startClosed = false; // Apakah pintu mulai dalam keadaan tertutup?

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    [Header("Collider Settings")]
    [SerializeField] private BoxCollider2D doorCollider;
    [SerializeField] private bool enableColliderWhenClosed = true; // Collider aktif saat pintu tertutup

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorCloseSound;
    [SerializeField] private AudioClip doorOpenSound;

    private bool isClosed = false;

    void Start()
    {
        // Auto-get components jika tidak di-assign
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Set initial state
        if (startClosed)
        {
            CloseDoor(immediate: true);
        }
        else
        {
            OpenDoor(immediate: true);
        }
    }

    /// <summary>
    /// Menutup pintu dengan animasi
    /// </summary>
    public void CloseDoor(bool immediate = false)
    {
        if (isClosed) return;

        isClosed = true;

        if (doorAnimator != null)
        {
            if (immediate)
            {
                // Set langsung ke state closed tanpa animasi
                doorAnimator.Play("DoorClosed", 0, 1f);
            }
            else
            {
                // Play animasi close
                doorAnimator.SetTrigger(closeTrigger);
            }
        }

        // Enable collider untuk block player
        if (doorCollider != null && enableColliderWhenClosed)
        {
            doorCollider.enabled = true;
        }

        // Play sound effect
        if (!immediate && audioSource != null && doorCloseSound != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }

        Debug.Log($"Door '{gameObject.name}' closed.");
    }

    /// <summary>
    /// Membuka pintu dengan animasi
    /// </summary>
    public void OpenDoor(bool immediate = false)
    {
        if (!isClosed && !immediate) return;

        isClosed = false;

        if (doorAnimator != null)
        {
            if (immediate)
            {
                // Set langsung ke state open tanpa animasi
                doorAnimator.Play("DoorOpen", 0, 1f);
            }
            else
            {
                // Play animasi open
                doorAnimator.SetTrigger(openTrigger);
            }
        }

        // Disable collider agar player bisa lewat
        if (doorCollider != null)
        {
            if (immediate)
            {
                doorCollider.enabled = false;
            }
            else
            {
                // Delay sedikit agar collider disable setelah animasi mulai
                StartCoroutine(DisableColliderDelayed(0.2f));
            }
        }

        // Play sound effect
        if (!immediate && audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }

        Debug.Log($"Door '{gameObject.name}' opened.");
    }

    IEnumerator DisableColliderDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (doorCollider != null)
            doorCollider.enabled = false;
    }

    /// <summary>
    /// Toggle pintu (buka jika tertutup, tutup jika terbuka)
    /// </summary>
    public void ToggleDoor()
    {
        if (isClosed)
            OpenDoor();
        else
            CloseDoor();
    }

    public bool IsClosed()
    {
        return isClosed;
    }

    public bool IsEntranceDoor()
    {
        return isEntranceDoor;
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = isClosed ? Color.red : Color.green;

        BoxCollider2D col = doorCollider != null ? doorCollider : GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}
