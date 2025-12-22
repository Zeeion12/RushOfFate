using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Unlocked Skills")]
    [SerializeField] private List<string> unlockedSkills = new List<string>();

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip skillUnlockSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSkills();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UnlockSkill(string skillName)
    {
        if (!unlockedSkills.Contains(skillName))
        {
            unlockedSkills.Add(skillName);
            PlaySound(skillUnlockSound);
            SaveSkills();

            if (showDebugLogs)
                Debug.Log($"[Inventory] Skill unlocked: {skillName}");

            if (PlayerAttack.Instance != null)
            {
                PlayerAttack.Instance.RefreshSkills();
            }
        }
    }

    public bool HasSkill(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }

    public List<string> GetUnlockedSkills()
    {
        return new List<string>(unlockedSkills);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SaveSkills()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string stageKey = $"Stage{currentStage}_";

        string skillsData = string.Join(",", unlockedSkills);
        PlayerPrefs.SetString(stageKey + "UnlockedSkills", skillsData);

        PlayerPrefs.Save();
    }

    private void LoadSkills()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string stageKey = $"Stage{currentStage}_";

        string skillsData = PlayerPrefs.GetString(stageKey + "UnlockedSkills", "");
        unlockedSkills.Clear();

        if (!string.IsNullOrEmpty(skillsData))
        {
            string[] skills = skillsData.Split(',');
            unlockedSkills.AddRange(skills);
        }

        if (showDebugLogs)
            Debug.Log($"[Inventory] Loaded {unlockedSkills.Count} skills");
    }

    public void ResetAllSkills()
    {
        // Reset unlocked skills data
        for (int stage = 1; stage <= 5; stage++)
        {
            string stageKey = $"Stage{stage}_";
            PlayerPrefs.DeleteKey(stageKey + "UnlockedSkills");
        }

        // AGGRESSIVE: Delete ALL PlayerPrefs keys that contain "Skill_" and "_Collected"
        // This is more reliable than trying to guess key names
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        int deletedCount = 0;

        // Try to delete common patterns across all stages
        for (int stage = 1; stage <= 5; stage++)
        {
            // Pattern 1: Stage-based keys
            string prefix = $"Stage{stage}_Skill_";

            // Try deleting keys with various patterns
            // We'll try common coordinates and skill names
            for (float x = -10f; x <= 10f; x += 0.01f)
            {
                for (float y = -10f; y <= 10f; y += 0.01f)
                {
                    string testKey1 = $"{prefix}Sword Stab_{stage}_{x:F2}_{y:F2}_Collected";
                    string testKey2 = $"{prefix}Dash Attack_{stage}_{x:F2}_{y:F2}_Collected";

                    if (PlayerPrefs.HasKey(testKey1))
                    {
                        PlayerPrefs.DeleteKey(testKey1);
                        deletedCount++;
                    }
                    if (PlayerPrefs.HasKey(testKey2))
                    {
                        PlayerPrefs.DeleteKey(testKey2);
                        deletedCount++;
                    }
                }
            }
        }

        PlayerPrefs.Save();
        unlockedSkills.Clear();

        if (showDebugLogs)
            Debug.Log($"[Inventory] All skills reset - Cleared {deletedCount} collection flags");
    }

    public void NuclearResetAllData()
    {
        // NUCLEAR OPTION: Delete EVERYTHING
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        unlockedSkills.Clear();

        if (showDebugLogs)
            Debug.Log("[Inventory] NUCLEAR RESET: All PlayerPrefs deleted!");
    }

    public void ManualDeleteItemSkillKey(string uniqueID)
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";

        if (PlayerPrefs.HasKey(saveKey))
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();

            if (showDebugLogs)
                Debug.Log($"[Inventory] Manually deleted key: {saveKey}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"[Inventory] Key not found: {saveKey}");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Reset All Skills")]
    private void DebugResetSkills()
    {
        ResetAllSkills();
    }

    [ContextMenu("Debug: Show Unlocked Skills")]
    private void DebugShowSkills()
    {
        Debug.Log($"[Inventory] Unlocked Skills ({unlockedSkills.Count}):\n" + string.Join("\n", unlockedSkills));
    }

    [ContextMenu("Debug: NUCLEAR RESET (Delete ALL PlayerPrefs)")]
    private void DebugNuclearReset()
    {
        NuclearResetAllData();
        Debug.LogWarning("[Inventory] NUCLEAR RESET executed! Reload scene to see effect.");
    }

    [ContextMenu("Debug: Show All PlayerPrefs Keys")]
    private void DebugShowAllKeys()
    {
        // Try to list common keys
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        Debug.Log($"[Inventory] Current Stage: {currentStage}");
        Debug.Log($"[Inventory] Checking for ItemSkill keys...");

        // Check if specific key exists (from error log)
        string testKey = $"Stage{currentStage}_Skill_Sword Stab_{currentStage}_3.84_-0.76_Collected";
        bool exists = PlayerPrefs.HasKey(testKey);
        Debug.Log($"Key exists: {testKey} = {exists}");
    }
#endif
}