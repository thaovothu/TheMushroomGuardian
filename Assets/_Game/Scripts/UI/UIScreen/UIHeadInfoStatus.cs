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

    // Cache giá trị mana đã hiển thị để tránh tạo string mới mỗi frame
    private float lastManaPercent = -1f;
    private int lastCurrentMana = -1;
    private int lastMaxMana = -1;

    void Awake()
    {
        if (entityType == EntityType.Boss)
        {
            BossEventBus.OnBossSpawned += OnBossSpawned;
            gameObject.SetActive(false);
            return;
        }

        GameEvent.Player.OnRespawn += HandleRespawn;
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
    }

    void OnEnable()
    {
        if (entityType == EntityType.Boss) return;

        // Player đã sẵn (re-enable sau respawn) → subscribe lại luôn
        if (targetHealth != null)
            SubscribeToHealthSystem();

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
        else
        {
            GameEvent.Player.OnRespawn -= HandleRespawn;
            GameEvent.Player.OnSpawned -= OnPlayerSpawned;
        }
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

    private void OnPlayerSpawned(GameObject player)
    {
        if (player == null) return;
        targetHealth = player.GetComponent<HealthSystem>();
        playerSkillController = player.GetComponent<PlayerSkillController>();
        SetManaVisible(true);
        if (targetHealth != null)
            SubscribeToHealthSystem();
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
            // HandleRespawn (subscribed in Awake) will re-enable this GO when OnRespawn fires
        }
    }

    private void HandleRespawn()
    {
        gameObject.SetActive(true);
        // OnEnable fires automatically and re-subscribes to health/mana events
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (entityType == EntityType.Player && playerSkillController != null)
            UpdateManaDisplay();
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
        if (!Mathf.Approximately(manaPercent, lastManaPercent))
        {
            manaBar.value = manaPercent;
            lastManaPercent = manaPercent;
        }

        // Chỉ build lại string (và rebuild mesh TMP) khi số mana thực sự đổi
        int current = playerSkillController.GetCurrentMana();
        int max = playerSkillController.GetMaxMana();
        if (current != lastCurrentMana || max != lastMaxMana)
        {
            manaBarText.text = $"{current} / {max}";
            lastCurrentMana = current;
            lastMaxMana = max;
        }
    }

    private void SetManaVisible(bool visible)
    {
        if (manaBar != null) manaBar.gameObject.SetActive(visible);
        if (manaBarText != null) manaBarText.gameObject.SetActive(visible);
    }

}