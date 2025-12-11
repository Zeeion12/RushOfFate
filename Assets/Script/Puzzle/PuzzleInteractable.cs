using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PuzzleInteractable : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField][TextArea(2, 4)] private string puzzleQuestion = "Berapa hasil dari 2 + 2?";
    [SerializeField] private string[] answerOptions = new string[] { "3", "4", "5", "6" };
    [SerializeField] private int correctAnswerIndex = 1; // Index 0-based (0 = A, 1 = B, dst)

    [Header("Connected Objects")]
    [SerializeField] private PuzzleDoor connectedDoor;
    [Tooltip("Canvas UI yang akan muncul saat puzzle dibuka")]
    [SerializeField] private PuzzleUI puzzleUI;

    [Header("Visual Indicator")]
    [SerializeField] private GameObject interactPrompt; // GameObject "Press E to Interact"

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    // State
    private bool playerInRange = false;
    private bool isSolved = false;

    void Start()
    {
        // Validasi setup
        ValidateSetup();

        // Hide prompt di awal
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Subscribe ke PuzzleUI events
        if (puzzleUI != null)
        {
            puzzleUI.OnPuzzleSolved += HandlePuzzleSolved;
        }
    }

    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
    }

    void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe untuk prevent memory leak
        if (puzzleUI != null)
        {
            puzzleUI.OnPuzzleSolved -= HandlePuzzleSolved;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isSolved) return; // Jangan trigger jika sudah solved

        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            // Tampilkan prompt "Press E"
            if (interactPrompt != null)
                interactPrompt.SetActive(true);

            Debug.Log($"Player entered puzzle range: {gameObject.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            // Hide prompt
            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            Debug.Log($"Player exited puzzle range: {gameObject.name}");
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        // Hanya bisa interact kalau:
        // 1. Player dalam range
        // 2. Puzzle belum solved
        // 3. UI belum aktif (prevent double open)
        if (playerInRange && !isSolved && puzzleUI != null && !puzzleUI.IsActive())
        {
            OpenPuzzle();
        }
    }

    void OpenPuzzle()
    {
        Debug.Log($"Opening puzzle: {puzzleQuestion}");

        // Hide prompt saat puzzle terbuka
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Kirim data puzzle ke UI
        puzzleUI.ShowPuzzle(puzzleQuestion, answerOptions, correctAnswerIndex);
    }

    void HandlePuzzleSolved()
    {
        Debug.Log($"✅ Puzzle solved: {gameObject.name}");

        isSolved = true;

        // Buka pintu yang terhubung
        if (connectedDoor != null)
        {
            connectedDoor.OpenDoor();
        }

        // Disable collider agar tidak bisa di-trigger lagi
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Optional: Hide puzzle object
        // gameObject.SetActive(false);
    }

    void ValidateSetup()
    {
        // Auto-detect collider dan set as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PuzzleInteractable on {gameObject.name}: Collider2D harus trigger! Auto-fixing...");
            col.isTrigger = true;
        }

        // Warning jika belum setup
        if (connectedDoor == null)
            Debug.LogWarning($"PuzzleInteractable on {gameObject.name}: Connected Door belum di-assign!");

        if (puzzleUI == null)
            Debug.LogWarning($"PuzzleInteractable on {gameObject.name}: Puzzle UI belum di-assign!");

        if (interactAction == null)
            Debug.LogWarning($"PuzzleInteractable on {gameObject.name}: Interact Action belum di-assign!");

        if (answerOptions.Length == 0)
            Debug.LogError($"PuzzleInteractable on {gameObject.name}: Answer Options kosong!");

        if (correctAnswerIndex < 0 || correctAnswerIndex >= answerOptions.Length)
            Debug.LogError($"PuzzleInteractable on {gameObject.name}: Correct Answer Index out of range!");
    }

    // Gizmo untuk visualisasi area trigger di editor
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = isSolved ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.bounds.size);

            // Draw label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.5f,
                isSolved ? "SOLVED" : "PUZZLE",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = isSolved ? Color.green : Color.yellow },
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                }
            );
#endif
        }
    }
}