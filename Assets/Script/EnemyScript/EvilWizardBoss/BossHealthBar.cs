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
    [SerializeField] private string bossName = "Cynus - The Evil Wizard";
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float blendTreeMaxValue = 11f;

    [Header("Intro Animation")]
    [SerializeField] private bool playIntroAnimation = true;
    [SerializeField] private float introAnimationDuration = 1f;

    private int maxHealth;
    private int currentHealth;
    private float targetHealthValue;

    void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (healthBarAnimator == null)
        {
            Transform healthBarFill = transform.Find("HealthBarFill");
            if (healthBarFill != null)
            {
                healthBarAnimator = healthBarFill.GetComponent<Animator>();
            }
            else
            {
                healthBarAnimator = GetComponentInChildren<Animator>();
            }
        }

        HideHealthBar(immediate: true);
    }

    void Update()
    {
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
        targetHealthValue = blendTreeMaxValue;

        if (healthBarAnimator != null)
            healthBarAnimator.SetFloat("Health", blendTreeMaxValue);
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        float healthPercent = (float)currentHealth / maxHealth;
        targetHealthValue = healthPercent * blendTreeMaxValue;

        if (health < maxHealth)
        {
            StartCoroutine(ShakeHealthBar());
        }
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
        gameObject.SetActive(true);

        if (playIntroAnimation)
        {
            StartCoroutine(IntroSequence());
        }
        else
        {
            StartCoroutine(FadeHealthBar(1f, 0.5f));
        }
    }

    IEnumerator IntroSequence()
    {
        if (healthBarAnimator != null)
        {
            healthBarAnimator.SetFloat("Health", 0f);
        }

        yield return StartCoroutine(FadeHealthBar(1f, 0.3f));

        float elapsed = 0f;
        float startValue = 0f;
        float endValue = blendTreeMaxValue;

        while (elapsed < introAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introAnimationDuration;
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            float currentValue = Mathf.Lerp(startValue, endValue, smoothT);

            if (healthBarAnimator != null)
            {
                healthBarAnimator.SetFloat("Health", currentValue);
            }

            yield return null;
        }

        if (healthBarAnimator != null)
        {
            healthBarAnimator.SetFloat("Health", blendTreeMaxValue);
        }

        targetHealthValue = blendTreeMaxValue;
    }

    public void HideHealthBar(bool immediate = false)
    {
        if (immediate)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(FadeHealthBar(0f, 0.5f));
        }
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

        if (targetAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnValidate()
    {
        if (bossNameText != null && !string.IsNullOrEmpty(bossName))
        {
            bossNameText.text = bossName;
        }
    }
}