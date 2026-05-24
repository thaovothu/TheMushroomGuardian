using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadInfoStatus : MonoBehaviour
{
    public enum EntityType { Player, Boss }

    [SerializeField] private EntityType entityType = EntityType.Player;
    [SerializeField] private HealthSystem targetHealth;
    [SerializeField] private PlayerSkillController playerSkillController;

    [Header("Health UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthBarText;

    [Header("Mana UI Elements (Player Only)")]
    [SerializeField] private Slider manaBar;
    [SerializeField] private TextMeshProUGUI manaBarText;

    [Header("Info Text")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameCharacterText;

    private bool isSubscribed = false;
    private int findRetry = 0;
    private const int MAX_RETRY = 100;

    void Awake()
    {
        if (entityType == EntityType.Boss)
        {
            BossEventBus.OnBossSpawned += OnBossSpawned; // subscribe khi còn active
            gameObject.SetActive(false);                  // sau đó mới ẩn
            return;
        }

        InitPlayer();
    }

    void OnEnable()
    {
        if (entityType == EntityType.Boss) return; // Boss dùng Awake rồi

        if (targetHealth == null)
            targetHealth = FindHealthSystemByTag();
        if (playerSkillController == null)
            playerSkillController = FindPlayerSkillController();

        if (targetHealth != null)
            SubscribeToHealthSystem();
        else
            StartCoroutine(WaitForHealthSystem());

        SetManaVisible(true);
    }

    void OnDisable()
    {
        if (entityType == EntityType.Boss) return; // Boss dùng OnDestroy

        UnsubscribeFromHealthSystem();
    }

    void OnDestroy()
    {
        UnsubscribeFromHealthSystem();
        if (entityType == EntityType.Boss)
            BossEventBus.OnBossSpawned -= OnBossSpawned;
    }

    // ── Boss spawn ────────────────────────────────────────────────────────────

    private void OnPlayerSpawned(GameObject _)
    {
        // Scene đã ready, bắt đầu lắng nghe boss
        // (OnEnable đã subscribe BossEventBus.OnBossSpawned rồi)
    }

    private void OnBossSpawned(GameObject bossGO)
    {
        if (bossGO == null) return;
        Debug.Log($"[UIHeadInfoStatus] OnBossSpawned received: {bossGO?.name}");
        targetHealth = bossGO.GetComponent<HealthSystem>();
        if (targetHealth == null)
        {
            Debug.LogWarning("[UIHeadInfoStatus] Boss spawned nhưng không có HealthSystem!");
            return;
        }

        // Hiện UI và gắn vào boss
        gameObject.SetActive(true);
        SetManaVisible(false);
        SubscribeToHealthSystem();

        Debug.Log($"[UIHeadInfoStatus] Boss UI shown — gắn vào {bossGO.name}");
    }

    // ── Player init ───────────────────────────────────────────────────────────

    private void InitPlayer()
    {
        if (targetHealth == null)
            targetHealth = FindHealthSystemByTag();
        if (playerSkillController == null)
            playerSkillController = FindPlayerSkillController();

        SetManaVisible(true);

        if (targetHealth != null)
            SubscribeToHealthSystem();
        else
            StartCoroutine(WaitForHealthSystem());
    }

    // ── Health subscribe ──────────────────────────────────────────────────────

    private void SubscribeToHealthSystem()
    {
        if (isSubscribed || targetHealth == null) return;

        GameEvent.Combat.OnHealthChanged += UpdateHealthBar;
        GameEvent.Combat.OnDeath += HandleDeath;
        isSubscribed = true;

        UpdateHealthBar(targetHealth, targetHealth.CurrentHealth, targetHealth.MaxHealth);
    }

    private void UnsubscribeFromHealthSystem()
    {
        if (!isSubscribed) return;
        GameEvent.Combat.OnHealthChanged -= UpdateHealthBar;
        GameEvent.Combat.OnDeath -= HandleDeath;
        isSubscribed = false;
    }

    private void HandleDeath(HealthSystem deadEntity)
    {
        if (deadEntity != targetHealth) return;

        if (entityType == EntityType.Boss)
        {
            // Ẩn UI boss, reset để sẵn sàng cho boss tiếp theo
            gameObject.SetActive(false);
            targetHealth = null;
            UnsubscribeFromHealthSystem();
            Debug.Log("[UIHeadInfoStatus] Boss chết → ẩn Boss UI");
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (entityType == EntityType.Player && playerSkillController != null)
            UpdateManaDisplay();

        if (entityType == EntityType.Player && playerSkillController == null && findRetry < MAX_RETRY)
        {
            findRetry++;
            playerSkillController = FindPlayerSkillController();
        }
    }

    // ── UI update ─────────────────────────────────────────────────────────────

    void UpdateHealthBar(HealthSystem healthSystem, float current, float max)
    {
        if (healthSystem != targetHealth) return;
        if (healthBar != null) healthBar.value = current / max;
        if (healthBarText != null) healthBarText.text = $"{current} / {max}";
    }

    void UpdateManaDisplay()
    {
        if (manaBar == null || manaBarText == null || playerSkillController == null) return;

        float manaPercent = playerSkillController.GetManaPercentage();
        manaBar.value = manaPercent;
        manaBarText.text = $"{playerSkillController.GetCurrentMana()} / {playerSkillController.GetMaxMana()}";
    }

    private void SetManaVisible(bool visible)
    {
        if (manaBar != null) manaBar.gameObject.SetActive(visible);
        if (manaBarText != null) manaBarText.gameObject.SetActive(visible);
    }

    // ── Find helpers ──────────────────────────────────────────────────────────

    private HealthSystem FindHealthSystemByTag()
    {
        string tag = entityType == EntityType.Player ? "Player" : "Boss";
        return GameObject.FindGameObjectWithTag(tag)?.GetComponent<HealthSystem>();
    }

    private PlayerSkillController FindPlayerSkillController()
    {
        return GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerSkillController>();
    }

    private IEnumerator WaitForHealthSystem()
    {
        for (int i = 0; i < 60; i++)
        {
            yield return null;
            targetHealth = FindHealthSystemByTag();
            if (targetHealth != null)
            {
                SubscribeToHealthSystem();
                yield break;
            }
        }
    }
}