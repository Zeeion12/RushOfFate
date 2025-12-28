using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Animator healthBarAnimator;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private string bossName = "Evil Wizard";
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float blendTreeMaxValue = 11f; // Max value dari Blend Tree (0 = empty, 11 = full)

    private int maxHealth;
    private int currentHealth;
    private float targetHealthValue;

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (bossNameText != null)
            bossNameText.text = bossName;

        // Get Animator from HealthBarFill if not assigned
        if (healthBarAnimator == null)
            healthBarAnimator = GetComponentInChildren<Animator>();

        // Start hidden
        HideHealthBar();
    }

    void Update()
    {
        // Smooth health bar animation using Animator parameter
        if (healthBarAnimator != null)
        {
            float currentHealthValue = healthBarAnimator.GetFloat("Health");
            float smoothedHealth = Mathf.Lerp(currentHealthValue, targetHealthValue, Time.deltaTime * animationSpeed);
            healthBarAnimator.SetFloat("Health", smoothedHealth);
        }
    }

    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        targetHealthValue = blendTreeMaxValue; // Full health = max blend tree value (11)

        if (healthBarAnimator != null)
            healthBarAnimator.SetFloat("Health", blendTreeMaxValue);
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        // Convert health percentage to blend tree range (0 to blendTreeMaxValue)
        float healthPercent = (float)currentHealth / maxHealth;
        targetHealthValue = healthPercent * blendTreeMaxValue;

        // Shake effect saat kena damage
        StartCoroutine(ShakeHealthBar());
    }

    IEnumerator ShakeHealthBar()
    {
        Vector3 originalPosition = transform.localPosition;
        float shakeDuration = 0.2f;
        float shakeMagnitude = 5f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    public void ShowHealthBar()
    {
        StartCoroutine(FadeHealthBar(1f, 0.5f));
    }

    public void HideHealthBar()
    {
        StartCoroutine(FadeHealthBar(0f, 0.5f));
    }

    IEnumerator FadeHealthBar(float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
