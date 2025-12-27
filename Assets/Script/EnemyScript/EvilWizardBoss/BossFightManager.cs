using UnityEngine;
using System.Collections;

public class BossFightManager : MonoBehaviour
{
    public static BossFightManager Instance;

    [Header("Boss Fight Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bossArena;
    [SerializeField] private GameObject arenaBarriers;

    [Header("Arena Doors")]
    [SerializeField] private BossDoor entranceDoor; // Pintu masuk
    [SerializeField] private BossDoor exitDoor; // Pintu keluar
    [SerializeField] private bool closeDoorsOnStart = true; // Tutup pintu saat boss fight dimulai
    [SerializeField] private bool openDoorsOnDefeat = true; // Buka pintu saat boss kalah

    [Header("Music & Audio")]
    [SerializeField] private AudioClip bossMusicClip;
    [SerializeField] private AudioClip victoryMusicClip;

    private GameObject currentBoss;
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
        if (arenaBarriers != null)
            arenaBarriers.SetActive(false);
    }

    public void StartBossFight()
    {
        if (bossFightActive) return;

        bossFightActive = true;

        // Spawn boss
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        }

        // Close arena doors
        if (closeDoorsOnStart)
        {
            if (entranceDoor != null)
                entranceDoor.CloseDoor();

            if (exitDoor != null)
                exitDoor.CloseDoor();
        }

        // Activate arena barriers (optional)
        if (arenaBarriers != null)
        {
            arenaBarriers.SetActive(true);
        }

        // Start boss music (TODO: Implement AudioManager jika diperlukan)
        // if (bossMusicClip != null)
        //     AudioManager.Instance?.PlayMusic(bossMusicClip);

        Debug.Log("Boss fight started! Doors closed.");
    }

    public void OnBossDefeated()
    {
        if (bossDefeated) return;

        bossDefeated = true;
        bossFightActive = false;

        // Open arena doors
        if (openDoorsOnDefeat)
        {
            if (entranceDoor != null)
                entranceDoor.OpenDoor();

            if (exitDoor != null)
                exitDoor.OpenDoor();
        }

        // Play victory music (TODO: Implement AudioManager jika diperlukan)
        // if (victoryMusicClip != null)
        //     AudioManager.Instance?.PlayMusic(victoryMusicClip);

        // Deactivate barriers after delay
        StartCoroutine(DeactivateBarriersDelayed(3f));

        // Award time bonus (TODO: Implement TimeManager jika diperlukan)
        // if (TimeManager.Instance != null)
        //     TimeManager.Instance.AddTimeBonus(120); // 2 minutes bonus

        Debug.Log("Boss defeated! Doors opened.");
    }

    IEnumerator DeactivateBarriersDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (arenaBarriers != null)
            arenaBarriers.SetActive(false);
    }
}
