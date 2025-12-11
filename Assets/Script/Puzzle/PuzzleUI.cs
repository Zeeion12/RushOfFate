using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel; // Panel utama yang di-fade
    [SerializeField] private CanvasGroup canvasGroup; // Untuk fade animation
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons; // Array 4 button (A, B, C, D)
    [SerializeField] private TextMeshProUGUI[] answerTexts; // Text di dalam button

    [Header("Feedback UI (Optional)")]
    [SerializeField] private GameObject wrongAnswerIndicator; // Optional: icon X merah

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    // Event untuk notify saat puzzle solved
    public event Action OnPuzzleSolved;

    // Current puzzle data
    private string currentQuestion;
    private string[] currentAnswers;
    private int correctIndex;
    private bool isShowing = false;

    void Start()
    {
        // Hide panel di awal
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (wrongAnswerIndicator != null)
            wrongAnswerIndicator.SetActive(false);

        // Setup button listeners
        SetupButtons();

        // Validasi setup
        ValidateSetup();
    }

    void SetupButtons()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture index untuk closure
            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
        }
    }

    public void ShowPuzzle(string question, string[] answers, int correctAnswerIndex)
    {
        if (isShowing) return; // Prevent double open

        // Store data
        currentQuestion = question;
        currentAnswers = answers;
        correctIndex = correctAnswerIndex;

        // Update UI
        if (questionText != null)
            questionText.text = question;

        // Update button texts
        for (int i = 0; i < answerButtons.Length && i < answers.Length; i++)
        {
            if (answerTexts[i] != null)
            {
                // Format: "A) Jawaban"
                char letter = (char)('A' + i);
                answerTexts[i].text = $"{letter}) {answers[i]}";
            }

            // Enable/disable button based on available answers
            if (answerButtons[i] != null)
                answerButtons[i].interactable = (i < answers.Length);
        }

        // Show panel dengan fade in
        StartCoroutine(FadeIn());

        // Freeze player movement (opsional, tergantung kebutuhan)
        FreezePlayer(true);

        Debug.Log($"Puzzle UI opened: {question}");
    }

    void OnAnswerSelected(int selectedIndex)
    {
        Debug.Log($"Player selected answer {selectedIndex} (Correct: {correctIndex})");

        if (selectedIndex == correctIndex)
        {
            // ✅ JAWABAN BENAR
            OnCorrectAnswer();
        }
        else
        {
            // ❌ JAWABAN SALAH
            OnWrongAnswer();
        }
    }

    void OnCorrectAnswer()
    {
        Debug.Log("✅ Correct answer!");

        // Trigger solved event
        OnPuzzleSolved?.Invoke();

        // Close UI
        ClosePuzzle();
    }

    void OnWrongAnswer()
    {
        Debug.Log("❌ Wrong answer! Try again.");

        // Show wrong indicator (optional)
        if (wrongAnswerIndicator != null)
        {
            wrongAnswerIndicator.SetActive(true);
            StartCoroutine(HideWrongIndicatorAfterDelay(1f));
        }

        // TIDAK menutup popup, player bisa coba lagi langsung
        // UI tetap terbuka untuk retry
    }

    System.Collections.IEnumerator HideWrongIndicatorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (wrongAnswerIndicator != null)
            wrongAnswerIndicator.SetActive(false);
    }

    void ClosePuzzle()
    {
        StartCoroutine(FadeOut());
    }

    System.Collections.IEnumerator FadeIn()
    {
        isShowing = true;

        // Aktifkan panel
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        // Fade in animation
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }
    }

    System.Collections.IEnumerator FadeOut()
    {
        // Fade out animation
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 1f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        // Nonaktifkan panel
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        isShowing = false;

        // Unfreeze player
        FreezePlayer(false);

        Debug.Log("Puzzle UI closed");
    }

    void FreezePlayer(bool freeze)
    {
        // Find player dan freeze movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetCanMove(!freeze);
            }

            // Freeze Rigidbody velocity
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null && freeze)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public bool IsActive()
    {
        return isShowing;
    }

    void ValidateSetup()
    {
        if (puzzlePanel == null)
            Debug.LogError("PuzzleUI: Puzzle Panel belum di-assign!");

        if (canvasGroup == null)
            Debug.LogWarning("PuzzleUI: Canvas Group belum di-assign! Fade animation tidak akan bekerja.");

        if (questionText == null)
            Debug.LogError("PuzzleUI: Question Text belum di-assign!");

        if (answerButtons == null || answerButtons.Length == 0)
            Debug.LogError("PuzzleUI: Answer Buttons belum di-assign!");

        if (answerTexts == null || answerTexts.Length == 0)
            Debug.LogError("PuzzleUI: Answer Texts belum di-assign!");

        // Validate button count
        if (answerButtons != null && answerTexts != null && answerButtons.Length != answerTexts.Length)
            Debug.LogError("PuzzleUI: Jumlah Answer Buttons dan Answer Texts harus sama!");
    }
}