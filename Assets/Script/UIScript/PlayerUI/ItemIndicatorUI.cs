using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemIndicatorUI : MonoBehaviour
{
    public static ItemIndicatorUI Instance { get; private set; }

    [Header("Key Indicator")]
    [SerializeField] private GameObject keyIndicatorPanel;
    [SerializeField] private Image keyIcon;
    [SerializeField] private TextMeshProUGUI keyCountText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private CanvasGroup keyCanvasGroup;

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

        SetupCanvasGroup();
        HideKeyIndicator();
    }

    private void Start()
    {
        LoadExistingKeys();
    }

    private void SetupCanvasGroup()
    {
        if (keyIndicatorPanel != null)
        {
            keyCanvasGroup = keyIndicatorPanel.GetComponent<CanvasGroup>();
            if (keyCanvasGroup == null)
                keyCanvasGroup = keyIndicatorPanel.AddComponent<CanvasGroup>();
        }
    }

    private void LoadExistingKeys()
    {
        if (InventoryManager.Instance == null) return;

        int keyCount = InventoryManager.Instance.GetKeyCount();
        if (keyCount > 0)
        {
            ShowKeyIndicator(keyCount);
        }
    }

    public void ShowKeyIndicator(int count)
    {
        if (keyIndicatorPanel == null) return;

        keyIndicatorPanel.SetActive(true);

        if (keyCountText != null)
            keyCountText.text = $"x{count}";

        StartCoroutine(FadeInIndicator());

        if (showDebugLogs)
            Debug.Log($"[ItemIndicatorUI] Key indicator shown: x{count}");
    }

    public void UpdateKeyCount(int count)
    {
        if (count > 0)
        {
            if (keyCountText != null)
                keyCountText.text = $"x{count}";
        }
        else
        {
            HideKeyIndicator();
        }
    }

    public void HideKeyIndicator()
    {
        if (keyIndicatorPanel == null) return;

        StartCoroutine(FadeOutAndHide());

        if (showDebugLogs)
            Debug.Log("[ItemIndicatorUI] Key indicator hidden");
    }

    public void ShowSkillIndicator(string skillName)
    {
        if (showDebugLogs)
            Debug.Log($"[ItemIndicatorUI] Skill '{skillName}' unlocked (auto-equipped, no UI display)");
    }

    private IEnumerator FadeInIndicator()
    {
        if (keyCanvasGroup == null) yield break;

        keyCanvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            keyCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        keyCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutAndHide()
    {
        if (keyCanvasGroup == null || keyIndicatorPanel == null) yield break;

        float elapsed = 0f;
        float startAlpha = keyCanvasGroup.alpha;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            keyCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        keyCanvasGroup.alpha = 0f;
        keyIndicatorPanel.SetActive(false);
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Show Key x3")]
    private void TestShowKey()
    {
        ShowKeyIndicator(3);
    }

    [ContextMenu("Test: Hide Key")]
    private void TestHideKey()
    {
        HideKeyIndicator();
    }
#endif
}