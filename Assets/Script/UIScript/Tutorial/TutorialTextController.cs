using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialTextController : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField][TextArea(2, 4)] private string tutorialText = "Tekan A/D untuk bergerak";
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string playerTag = "Player";

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textComponent;

    private bool hasTriggered = false;
    private Coroutine displayCoroutine;

    void Start()
    {
        // Auto-find references jika belum di-assign
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();

        // Set text dari inspector
        if (textComponent != null)
            textComponent.text = tutorialText;

        // Pastikan mulai transparan
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang masuk adalah player
        if (!other.CompareTag(playerTag)) return;

        // Cek apakah sudah pernah trigger (jika triggerOnce = true)
        if (triggerOnce && hasTriggered) return;

        // Tampilkan text
        ShowTutorialText();
        hasTriggered = true;
    }

    void ShowTutorialText()
    {
        // Stop coroutine sebelumnya jika ada
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        // Mulai coroutine baru
        displayCoroutine = StartCoroutine(DisplayTextCoroutine());
    }

    IEnumerator DisplayTextCoroutine()
    {
        // FASE 1: FADE IN
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // FASE 2: HOLD (tampil penuh)
        yield return new WaitForSeconds(displayDuration);

        // FASE 3: FADE OUT
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    // Untuk debugging di editor
    void OnDrawGizmosSelected()
    {
        // Visualisasi trigger area
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        if (trigger != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position + (Vector3)trigger.offset, trigger.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + (Vector3)trigger.offset, trigger.size);
        }
    }
}