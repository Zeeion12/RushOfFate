using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance { get; private set; }

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float attackDelay = 0.1f;

    [Header("Combo Settings")]
    [SerializeField] private int swordStabDamage = 2;
    [SerializeField] private float comboWindow = 0.5f;

    [Header("Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.8f, 0);
    [SerializeField] private LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference attackAction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalAttackSound;
    [SerializeField] private AudioClip swordStabSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showAttackGizmos = true;

    private float lastAttackTime = -999f;
    private float lastComboTime = -999f;
    private int comboCount = 0;
    private bool canAttack = true;
    private bool hasSwordStab = false;

    private const string ATTACK_TRIGGER = "Attack";
    private const string SWORD_STAB_TRIGGER = "SwordStab";

    private void Awake()
    {
        Instance = this;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerTransform == null)
            playerTransform = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CheckUnlockedSkills();
    }

    private void OnEnable()
    {
        if (attackAction != null)
            attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction != null)
            attackAction.action.Disable();
    }

    private void Update()
    {
        HandleAttackInput();
        UpdateComboTimer();
    }

    private void CheckUnlockedSkills()
    {
        if (InventoryManager.Instance != null)
        {
            hasSwordStab = InventoryManager.Instance.HasSkill("Sword Stab");

            if (showDebugLogs)
                Debug.Log($"[PlayerAttack] Sword Stab unlocked: {hasSwordStab}");
        }
    }

    private void HandleAttackInput()
    {
        if (attackAction == null) return;

        if (attackAction.action.triggered && canAttack)
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;

            if (timeSinceLastAttack >= attackCooldown)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        float timeSinceLastCombo = Time.time - lastComboTime;

        if (hasSwordStab && comboCount == 1 && timeSinceLastCombo <= comboWindow)
        {
            PerformSwordStab();
        }
        else
        {
            PerformNormalAttack();
        }

        lastAttackTime = Time.time;
    }

    private void PerformNormalAttack()
    {
        if (animator != null)
            animator.SetTrigger(ATTACK_TRIGGER);

        PlaySound(normalAttackSound);

        comboCount = 1;
        lastComboTime = Time.time;

        Invoke(nameof(DealNormalDamage), attackDelay);

        if (showDebugLogs)
            Debug.Log("[PlayerAttack] Normal Attack executed");
    }

    private void PerformSwordStab()
    {
        if (animator != null)
            animator.SetTrigger(SWORD_STAB_TRIGGER);

        PlaySound(swordStabSound);

        comboCount = 0;
        lastComboTime = -999f;

        Invoke(nameof(DealSwordStabDamage), attackDelay);

        if (showDebugLogs)
            Debug.Log("[PlayerAttack] Sword Stab executed!");
    }

    private void DealNormalDamage()
    {
        DealDamageInBox(attackDamage);
    }

    private void DealSwordStabDamage()
    {
        DealDamageInBox(swordStabDamage);
    }

    private void DealDamageInBox(int damage)
    {
        Vector2 attackPosition = GetAttackPosition();

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, (Vector2)playerTransform.position);

                if (showDebugLogs)
                    Debug.Log($"[PlayerAttack] Hit {enemy.name} for {damage} damage");
            }
        }

        if (showDebugLogs && hitEnemies.Length == 0)
            Debug.Log("[PlayerAttack] Attack missed, no enemies hit");
    }

    private Vector2 GetAttackPosition()
    {
        Vector2 offset = attackBoxOffset;

        if (playerTransform.localScale.x < 0)
        {
            offset.x = -offset.x;
        }

        return (Vector2)playerTransform.position + offset;
    }

    private void UpdateComboTimer()
    {
        if (comboCount > 0)
        {
            float timeSinceLastCombo = Time.time - lastComboTime;

            if (timeSinceLastCombo > comboWindow)
            {
                ResetCombo();
            }
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        lastComboTime = -999f;

        if (showDebugLogs)
            Debug.Log("[PlayerAttack] Combo reset");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void RefreshSkills()
    {
        CheckUnlockedSkills();
    }

    private void OnDrawGizmos()
    {
        if (!showAttackGizmos) return;

        Vector2 attackPos = GetAttackPosition();

        Gizmos.color = comboCount == 1 ? Color.yellow : Color.red;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);

        if (comboCount > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Unlock Sword Stab")]
    private void DebugUnlockSwordStab()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UnlockSkill("Sword Stab");
            CheckUnlockedSkills();
        }
    }

    [ContextMenu("Debug: Force Normal Attack")]
    private void DebugNormalAttack()
    {
        PerformNormalAttack();
    }

    [ContextMenu("Debug: Force Sword Stab")]
    private void DebugSwordStab()
    {
        PerformSwordStab();
    }
#endif
}