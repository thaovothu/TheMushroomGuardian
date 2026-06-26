using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI Skill System - Hiển thị nguyên tố, skill, mana, cooldown
/// Lắng nghe GameEvent.Player.OnSpawned thay vì retry trong Update.
/// </summary>
public class UISkillPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSkillController skillController;

    [Header("Element Display")]
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private Image elementIcon;

    [Header("Skill Buttons")]
    [SerializeField] private Button shieldButton;
    [SerializeField] private Image shieldIcon;
    [SerializeField] private TextMeshProUGUI shieldNameText;
    [SerializeField] private TextMeshProUGUI shieldStatsText;

    [SerializeField] private Button attackButton;
    [SerializeField] private Image attackIcon;
    [SerializeField] private TextMeshProUGUI attackNameText;
    [SerializeField] private TextMeshProUGUI attackStatsText;

    [Header("Skill Icons")]
    [SerializeField] private SkillIconSO skillIconSO;


    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
    }

    private void OnDisable()
    {
        GameEvent.Player.OnSpawned -= OnPlayerSpawned;
    }

    private void Start()
    {
        // Thử tìm ngay nếu player đã spawn trước đó (scene reload, DontDestroyOnLoad)
        if (skillController == null)
            TryFindPlayerSkillController();

        if (skillController != null)
        {
            HookButtonEvents();
            UpdateUI();
        }
    }

    private void Update()
    {
        if (skillController == null) return;
        UpdateUI();
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnPlayerSpawned(GameObject player)
    {
        if (player == null) return;

        skillController = player.GetComponent<PlayerSkillController>();

        if (skillController != null)
        {
            Debug.Log("[UISkillPanel] ✓ PlayerSkillController found via OnSpawned!");
            HookButtonEvents();
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("[UISkillPanel] Player spawned nhưng không có PlayerSkillController!");
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TryFindPlayerSkillController()
    {
        skillController = GetComponent<PlayerSkillController>();
        if (skillController != null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            skillController = player.GetComponent<PlayerSkillController>();
    }

    private bool _buttonsHooked;

    private void HookButtonEvents()
    {
        // Start() và OnPlayerSpawned() đều gọi hàm này → chặn đăng ký trùng,
        // nếu không 1 nút sẽ có 2 listener → bấm 1 lần cast 2 lần (VFX lặp).
        if (_buttonsHooked) return;
        _buttonsHooked = true;

        if (shieldButton != null)
            shieldButton.onClick.AddListener(OnShieldButtonClicked);
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);
    }

    // Hàm có tên (đọc skillController hiện tại) → vẫn đúng khi player respawn.
    private void OnShieldButtonClicked() => skillController?.CastShield();
    private void OnAttackButtonClicked() => skillController?.CastAttack();

    private void UpdateUI()
    {
        UpdateElementDisplay();
        UpdateSkillDisplay();
    }

    private void UpdateElementDisplay()
    {
        if (elementText == null || elementIcon == null) return;

        var element = skillController.GetCurrentElement();
        elementText.text = element.ToString().ToUpper();
    }

    private void UpdateSkillDisplay()
    {
        if (shieldButton == null || attackButton == null) return;
        if (skillController == null) return;

        var shieldSkill = skillController.GetCurrentShield();
        var attackSkill = skillController.GetCurrentAttack();

        if (shieldSkill != null && shieldNameText != null && shieldStatsText != null)
        {
            shieldNameText.text = shieldSkill.skillName;
            shieldStatsText.text = $"Def: {shieldSkill.defense} | Mana: {shieldSkill.manaCost}";
            shieldButton.interactable = skillController.GetCurrentMana() >= shieldSkill.manaCost;
            if (shieldIcon != null && skillIconSO != null)
                shieldIcon.sprite = skillIconSO.GetIcon(shieldSkill.skillId);
        }
        else if (shieldNameText != null && shieldStatsText != null)
        {
            shieldNameText.text = "N/A";
            shieldStatsText.text = "No Shield Skill";
            shieldButton.interactable = false;
        }

        if (attackSkill != null && attackNameText != null && attackStatsText != null)
        {
            attackNameText.text = attackSkill.skillName;
            attackStatsText.text = $"Dmg: {attackSkill.damage} | Mana: {attackSkill.manaCost}";
            attackButton.interactable = skillController.GetCurrentMana() >= attackSkill.manaCost;
            if (attackIcon != null && skillIconSO != null)
                attackIcon.sprite = skillIconSO.GetIcon(attackSkill.skillId);
        }
        else if (attackNameText != null && attackStatsText != null)
        {
            attackNameText.text = "N/A";
            attackStatsText.text = "No Attack Skill";
            attackButton.interactable = false;
        }
    }

}