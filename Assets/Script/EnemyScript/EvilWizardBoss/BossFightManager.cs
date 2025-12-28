using UnityEngine;
using System.Collections;

public class BossFightManager : MonoBehaviour
{
    public static BossFightManager Instance;

    [Header("Boss Fight Settings")]
    [SerializeField] private EvilWizardBoss bossObject;
    [SerializeField] private GameObject bossArena;
    [SerializeField] private BossHealthBar bossHealthBar;

    [Header("Arena Doors")]
    [SerializeField] private BossDoor entranceDoor;
    [SerializeField] private BossDoor exitDoor;
    [SerializeField] private bool closeDoorsOnStart = true;
    [SerializeField] private bool openDoorsOnDefeat = true;

    [Header("Music & Audio")]
    [SerializeField] private AudioClip bossMusicClip;
    [SerializeField] private AudioClip victoryMusicClip;

    private bool bossFightActive = false;
    private bool bossDefeated = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (bossHealthBar == null)
        {
            bossHealthBar = FindFirstObjectByType<BossHealthBar>();
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.HideHealthBar(immediate: true);
        }

        if (bossObject != null)
        {
            bossObject.gameObject.SetActive(false);
        }
    }

    public void StartBossFight()
    {
        if (bossFightActive) return;

        bossFightActive = true;

        if (bossObject != null)
        {
            bossObject.gameObject.SetActive(true);

            if (bossHealthBar != null)
            {
                int bossMaxHealth = GetBossMaxHealth();
                bossHealthBar.SetMaxHealth(bossMaxHealth);
                bossHealthBar.ShowHealthBar();
            }
        }

        if (closeDoorsOnStart)
        {
            if (entranceDoor != null)
                entranceDoor.CloseDoor();

            if (exitDoor != null)
                exitDoor.CloseDoor();
        }
    }

    int GetBossMaxHealth()
    {
        if (bossObject == null) return 500;

        var bossScript = bossObject.GetComponent<EvilWizardBoss>();
        if (bossScript != null)
        {
            var field = typeof(EvilWizardBoss).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (int)field.GetValue(bossScript);
            }
        }

        return 500;
    }

    public void OnBossDefeated()
    {
        if (bossDefeated) return;

        bossDefeated = true;
        bossFightActive = false;

        if (bossHealthBar != null)
        {
            bossHealthBar.HideHealthBar();
        }

        if (openDoorsOnDefeat)
        {
            if (entranceDoor != null)
                entranceDoor.OpenDoor();

            if (exitDoor != null)
                exitDoor.OpenDoor();
        }
    }

    void OnValidate()
    {
        if (bossObject != null && bossHealthBar == null)
        {
            bossHealthBar = FindFirstObjectByType<BossHealthBar>();
        }
    }
}