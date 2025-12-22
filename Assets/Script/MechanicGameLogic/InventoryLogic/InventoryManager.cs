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
        for (int stage = 1; stage <= 5; stage++)
        {
            string stageKey = $"Stage{stage}_";
            PlayerPrefs.DeleteKey(stageKey + "UnlockedSkills");
        }

        PlayerPrefs.Save();
        unlockedSkills.Clear();

        if (showDebugLogs)
            Debug.Log("[Inventory] All skills reset");
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
#endif
}