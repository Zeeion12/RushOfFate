using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemSkill : MonoBehaviour
{
    [Header("Skill Data")]
    [SerializeField] private string skillName = "Sword Stab";

    [Header("References")]
    [SerializeField] private SpriteRenderer skillRenderer;

    [Header("Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private float destroyDelay = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;

    [Header("Save System")]
    [SerializeField] private string uniqueID;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private BoxCollider2D triggerCollider;
    private bool isCollected = false;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;

        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"Skill_{skillName}_{gameObject.scene.name}_{transform.position.x}_{transform.position.y}";
        }

        if (skillRenderer == null)
            skillRenderer = GetComponentInChildren<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CheckIfAlreadyCollected();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
            CollectSkill();
        }
    }

    private void CollectSkill()
    {
        isCollected = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UnlockSkill(skillName);
        }

        if (SkillPopup.Instance != null)
        {
            SkillPopup.Instance.ShowSkillUnlocked(skillName);
        }

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        MarkAsCollected();

        if (skillRenderer != null)
            skillRenderer.enabled = false;

        triggerCollider.enabled = false;

        if (showDebugLogs)
            Debug.Log($"[ItemSkill] Skill collected: {skillName}");

        Destroy(gameObject, destroyDelay);
    }

    private void CheckIfAlreadyCollected()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";

        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            if (showDebugLogs)
                Debug.Log($"[ItemSkill] {uniqueID} already collected, destroying");

            Destroy(gameObject);
        }
    }

    private void MarkAsCollected()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"ItemSkill: {skillName}\nID: {uniqueID}");
    }

    [ContextMenu("Debug: Check Collection Status")]
    private void DebugCheckStatus()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";
        int savedValue = PlayerPrefs.GetInt(saveKey, 0);

        Debug.Log($"[ItemSkill] DEBUG Status:\n" +
                  $"Skill Name: {skillName}\n" +
                  $"Unique ID: {uniqueID}\n" +
                  $"Current Stage: {currentStage}\n" +
                  $"Save Key: {saveKey}\n" +
                  $"Collected: {(savedValue == 1 ? "YES" : "NO")}");
    }

    [ContextMenu("Debug: Reset Collection Flag")]
    private void DebugResetFlag()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        Debug.Log($"[ItemSkill] Collection flag reset for: {skillName}");
        Debug.Log("Reload scene to see the ItemSkill appear again!");
    }

    [ContextMenu("Debug: Force Recollect (Play Mode)")]
    private void DebugForceCollect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ItemSkill] This only works in Play Mode!");
            return;
        }

        CollectSkill();
    }
#endif
}