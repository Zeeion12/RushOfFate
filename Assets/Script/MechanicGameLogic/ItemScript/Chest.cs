using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Chest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private GameObject itemSkillPrefab;
    [SerializeField] private Transform itemSpawnPoint;

    [Header("Prompt Settings")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Prompt Messages")]
    [SerializeField] private string openPromptText = "Press E to Interact";
    [SerializeField] private string lockedPromptText = "You need a key!";
    [SerializeField] private float lockedMessageDuration = 2f;

    [Header("Animation Parameters")]
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string chestStateParam = "ChestState";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chestOpenSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Save System")]
    [SerializeField] private string uniqueID;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private BoxCollider2D interactTrigger;
    private bool isPlayerNear = false;
    private bool isOpened = false;
    private bool isShowingLockedMessage = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        interactTrigger = GetComponent<BoxCollider2D>();
        interactTrigger.isTrigger = true;

        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"Chest_{gameObject.scene.name}_{transform.position.x}_{transform.position.y}";
        }

        if (chestAnimator == null)
            chestAnimator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void Start()
    {
        CheckIfAlreadyOpened();
    }

    private void Update()
    {
        if (isPlayerNear && !isOpened && !isShowingLockedMessage)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryOpenChest();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isOpened)
        {
            isPlayerNear = true;
            ShowPrompt(openPromptText);

            if (showDebugLogs)
                Debug.Log("[Chest] Player entered chest trigger zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            HidePrompt();

            if (showDebugLogs)
                Debug.Log("[Chest] Player exited chest trigger zone");
        }
    }

    private void TryOpenChest()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[Chest] InventoryManager not found!");
            return;
        }

        if (InventoryManager.Instance.HasKey())
        {
            OpenChest();
        }
        else
        {
            ShowLockedMessage();
        }
    }

    private void OpenChest()
    {
        if (InventoryManager.Instance.UseKey())
        {
            isOpened = true;
            HidePrompt();

            if (chestAnimator != null)
            {
                chestAnimator.SetTrigger(openTrigger);
            }

            PlaySound(chestOpenSound);
            SpawnItemSkill();
            MarkAsOpened();

            if (showDebugLogs)
                Debug.Log($"[Chest] Chest opened: {uniqueID}");
        }
    }

    private void ShowLockedMessage()
    {
        PlaySound(lockedSound);
        StartCoroutine(DisplayLockedMessage());

        if (showDebugLogs)
            Debug.Log("[Chest] Chest is locked, need a key!");
    }

    private IEnumerator DisplayLockedMessage()
    {
        isShowingLockedMessage = true;

        if (promptText != null)
            promptText.text = lockedPromptText;

        yield return new WaitForSeconds(lockedMessageDuration);

        isShowingLockedMessage = false;

        if (isPlayerNear)
        {
            if (promptText != null)
                promptText.text = openPromptText;
        }
        else
        {
            HidePrompt();
        }
    }

    private void SpawnItemSkill()
    {
        if (itemSkillPrefab != null)
        {
            Vector3 spawnPosition = itemSpawnPoint != null ? itemSpawnPoint.position : transform.position + Vector3.up * 2f;
            GameObject spawnedItem = Instantiate(itemSkillPrefab, spawnPosition, Quaternion.identity);

            if (showDebugLogs)
                Debug.Log($"[Chest] Item skill spawned at {spawnPosition}");
        }
    }

    private void ShowPrompt(string message)
    {
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(true);

            if (promptText != null)
                promptText.text = message;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadePrompt(1f));
        }
    }

    private void HidePrompt()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadePrompt(0f));
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptCanvasGroup == null) yield break;

        float startAlpha = promptCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        promptCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f && promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void CheckIfAlreadyOpened()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Opened";

        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            isOpened = true;

            if (chestAnimator != null)
            {
                chestAnimator.SetInteger(chestStateParam, 2);
            }

            if (showDebugLogs)
                Debug.Log($"[Chest] {uniqueID} already opened, setting to open state");
        }
    }

    private void MarkAsOpened()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Opened";
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (interactTrigger != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)interactTrigger.offset, interactTrigger.size);
        }

        Gizmos.color = Color.green;
        Vector3 spawnPos = itemSpawnPoint != null ? itemSpawnPoint.position : transform.position + Vector3.up * 2f;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"Chest\nID: {uniqueID}\nOpened: {isOpened}");
    }

    [ContextMenu("Force Open Chest")]
    private void DebugForceOpen()
    {
        isOpened = true;
        if (chestAnimator != null)
            chestAnimator.SetTrigger(openTrigger);
        SpawnItemSkill();
    }
#endif
}