using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KeyItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer keyRenderer;
    [SerializeField] private Animator keyAnimator;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Save System")]
    [SerializeField] private string uniqueID;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Vector3 startPosition;
    private bool isCollected = false;
    private BoxCollider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;

        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"Key_{gameObject.scene.name}_{transform.position.x}_{transform.position.y}";
        }

        if (keyRenderer == null)
            keyRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        startPosition = transform.position;
        CheckIfAlreadyCollected();
    }

    private void Update()
    {
        if (!isCollected)
        {
            BobAnimation();
        }
    }

    private void BobAnimation()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag(playerTag))
        {
            CollectKey();
        }
    }

    private void CollectKey()
    {
        isCollected = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddKey();
        }

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        MarkAsCollected();

        if (keyAnimator != null)
            keyAnimator.enabled = false;

        if (keyRenderer != null)
            keyRenderer.enabled = false;

        triggerCollider.enabled = false;

        if (showDebugLogs)
            Debug.Log($"[KeyItem] Key collected at {transform.position}");

        Destroy(gameObject, destroyDelay);
    }

    private void CheckIfAlreadyCollected()
    {
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        string saveKey = $"Stage{currentStage}_{uniqueID}_Collected";

        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            if (showDebugLogs)
                Debug.Log($"[KeyItem] {uniqueID} already collected, destroying");

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);

        UnityEditor.Handles.Label(transform.position + Vector3.up, $"KeyItem\nID: {uniqueID}");
    }
#endif
}