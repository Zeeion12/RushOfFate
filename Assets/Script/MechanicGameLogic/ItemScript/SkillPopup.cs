using UnityEngine;
using TMPro;
using System.Collections;

public class SkillPopup : MonoBehaviour
{
    public static SkillPopup Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Text Format")]
    [SerializeField] private string popupFormat = "New Skill Unlocked!\n{0}";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (canvasGroup == null && popupPanel != null)
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
    }

    public void ShowSkillUnlocked(string skillName)
    {
        if (popupPanel == null || skillNameText == null)
        {
            Debug.LogError("[SkillPopup] UI references not assigned!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisplayPopup(skillName));

        if (showDebugLogs)
            Debug.Log($"[SkillPopup] Showing popup for: {skillName}");
    }

    private IEnumerator DisplayPopup(string skillName)
    {
        skillNameText.text = string.Format(popupFormat, skillName);

        popupPanel.SetActive(true);

        yield return StartCoroutine(FadeIn());

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeOut());

        popupPanel.SetActive(false);
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

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

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Show Popup")]
    private void TestShowPopup()
    {
        ShowSkillUnlocked("Sword Stab");
    }
#endif
}