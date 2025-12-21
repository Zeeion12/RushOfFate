using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    [SerializeField] private int keyCount = 0;
    [SerializeField] private List<string> unlockedSkills = new List<string>();

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip keyPickupSound;
    [SerializeField] private AudioClip skillUnlockSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKey()
    {
        keyCount++;
        PlaySound(keyPickupSound);
        SaveInventory();

        if (showDebugLogs)
            Debug.Log($"[Inventory] Key collected! Total keys: {keyCount}");

        if (ItemIndicatorUI.Instance != null)
            ItemIndicatorUI.Instance.ShowKeyIndicator(keyCount);
    }

    public bool HasKey()
    {
        return keyCount > 0;
    }

    public bool UseKey()
    {
        if (keyCount > 0)
        {
            keyCount--;
            SaveInventory();

            if (showDebugLogs)
                Debug.Log($"[Inventory] Key used! Remaining keys: {keyCount}");

            if (ItemIndicatorUI.Instance != null)
                ItemIndicatorUI.Instance.UpdateKeyCount(keyCount);

            return true;
        }
        return false;
    }

    public void UnlockSkill(string skillName)
    {
        if (!unlockedSkills.Contains(skillName))
        {
            unlockedSkills.Add(skillName);
            PlaySound(skillUnlockSound);
            SaveInventory();

            if (showDebugLogs)
                Debug.Log($"[Inventory] Skill unlocked: {skillName}");

            if (ItemIndicatorUI.Instance != null)
                ItemIndicatorUI.Instance.ShowSkillIndicator(skillName);
        }
    }

    public bool HasSkill(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }

    public int GetKeyCount()
    {
        return keyCount;
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

    private void SaveInventory()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string stageKey = $"Stage{currentStage}_";

        PlayerPrefs.SetInt(stageKey + "KeyCount", keyCount);

        string skillsData = string.Join(",", unlockedSkills);
        PlayerPrefs.SetString(stageKey + "UnlockedSkills", skillsData);

        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string stageKey = $"Stage{currentStage}_";

        keyCount = PlayerPrefs.GetInt(stageKey + "KeyCount", 0);

        string skillsData = PlayerPrefs.GetString(stageKey + "UnlockedSkills", "");
        unlockedSkills.Clear();

        if (!string.IsNullOrEmpty(skillsData))
        {
            string[] skills = skillsData.Split(',');
            unlockedSkills.AddRange(skills);
        }

        if (showDebugLogs)
            Debug.Log($"[Inventory] Loaded - Keys: {keyCount}, Skills: {unlockedSkills.Count}");
    }

    public void ResetInventory()
    {
        keyCount = 0;
        unlockedSkills.Clear();
        SaveInventory();

        if (showDebugLogs)
            Debug.Log("[Inventory] Reset complete");
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Add Key")]
    private void DebugAddKey()
    {
        AddKey();
    }

    [ContextMenu("Debug: Reset Inventory")]
    private void DebugResetInventory()
    {
        ResetInventory();
    }
#endif
}