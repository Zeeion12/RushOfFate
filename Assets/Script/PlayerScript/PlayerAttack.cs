using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance { get; private set; }

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private int swordStabDamage = 2;
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float attackDelay = 0.1f;
    [SerializeField] private float comboWindow = 0.5f;

    [Header("Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.8f, 0);
    [SerializeField] private LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private InputActionReference attackAction;
    private PlayerMovement playerMovement;

    [Header("Audio")]
    [SerializeField] private AudioSource attackAudioSource;  // Dedicated for attack sounds
    [SerializeField] private AudioClip normalAttackSound;
    [SerializeField] private AudioClip swordStabSound;
    [SerializeField][Range(0f, 1f)] private float normalAttackVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float swordStabVolume = 1f;

    private float lastAttackTime = -999f;
    private float lastComboTime = -999f;
    private int comboCount = 0;
    private bool hasSwordStab = false;

    private void Awake()
    {
        Instance = this;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (playerTransform == null) playerTransform = transform;

        // Get or create dedicated attack audio source
        if (attackAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            // Use the third AudioSource if available (first two are for movement)
            if (audioSources.Length >= 3)
            {
                attackAudioSource = audioSources[2];
            }
            else
            {
                attackAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
            hasSwordStab = InventoryManager.Instance.HasSkill("Sword Stab");
    }

    private void OnEnable()
    {
        if (attackAction != null) attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction != null) attackAction.action.Disable();
    }

    private void Update()
    {


        if (attackAction != null && attackAction.action.triggered)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
            }
        }

        if (comboCount > 0 && Time.time - lastComboTime > comboWindow)
        {
            comboCount = 0;
        }
    }

    private void PerformAttack()
    {
        float timeSinceCombo = Time.time - lastComboTime;

        if (hasSwordStab && comboCount == 1 && timeSinceCombo <= comboWindow)
        {
            if (animator != null) animator.SetTrigger("SwordStab");
            if (attackAudioSource != null && swordStabSound != null) attackAudioSource.PlayOneShot(swordStabSound, swordStabVolume);
            Invoke(nameof(DealSwordStabDamage), attackDelay);
            comboCount = 0;
        }
        else
        {
            if (animator != null) animator.SetTrigger("Attack");
            if (attackAudioSource != null && normalAttackSound != null) attackAudioSource.PlayOneShot(normalAttackSound, normalAttackVolume);
            Invoke(nameof(DealNormalDamage), attackDelay);
            comboCount = 1;
            lastComboTime = Time.time;
        }

        lastAttackTime = Time.time;
    }

    private void DealNormalDamage()
    {
        DealDamage(attackDamage);
    }

    private void DealSwordStabDamage()
    {
        DealDamage(swordStabDamage);
    }

    private void DealDamage(int damage)
    {
        Vector2 attackPos = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, attackBoxSize, 0f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            CanineHealth canine = hit.GetComponent<CanineHealth>();
            if (canine != null) { canine.TakeDamage(damage, transform.position); continue; }

            BanditHealth bandit = hit.GetComponent<BanditHealth>();
            if (bandit != null) { bandit.TakeDamage(damage, transform.position); continue; }

            BanditArcherHealth archer = hit.GetComponent<BanditArcherHealth>();
            if (archer != null) { archer.TakeDamage(damage, transform.position); continue; }

            GolemHealth golem = hit.GetComponent<GolemHealth>();
            if (golem != null) { golem.TakeDamage(damage, transform.position); continue; }

            HarpyHealth harpy = hit.GetComponent<HarpyHealth>();
            if (harpy != null) { harpy.TakeDamage(damage, transform.position); continue; }

            EvilWizardBoss wizard = hit.GetComponent<EvilWizardBoss>();
            if (wizard != null) { wizard.TakeDamage(damage); }
        }
    }

    private Vector2 GetAttackPosition()
    {
        // Gunakan IsFacingRight() dari PlayerMovement untuk deteksi arah yang akurat
        float direction = 1f;

        if (playerMovement != null)
        {
            direction = playerMovement.IsFacingRight() ? 1f : -1f;
        }

        Vector2 offset = new Vector2(attackBoxOffset.x * direction, attackBoxOffset.y);
        return (Vector2)playerTransform.position + offset;
    }

    public void RefreshSkills()
    {
        if (InventoryManager.Instance != null)
            hasSwordStab = InventoryManager.Instance.HasSkill("Sword Stab");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackPosition(), attackBoxSize);
    }
}