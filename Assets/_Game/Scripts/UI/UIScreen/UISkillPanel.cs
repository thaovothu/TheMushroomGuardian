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
    [SerializeField] private TextMeshProUGUI shieldNameText;
    [SerializeField] private TextMeshProUGUI shieldStatsText;

    [SerializeField] private Button attackButton;
    [SerializeField] private TextMeshProUGUI attackNameText;
    [SerializeField] private TextMeshProUGUI attackStatsText;

    [Header("Element Colors")]
    [SerializeField] private Color earthColor = new Color(0.6f, 0.4f, 0.2f);
    [SerializeField] private Color windColor = new Color(0.2f, 0.8f, 0.9f);
    [SerializeField] private Color waterColor = new Color(0.2f, 0.4f, 0.9f);
    [SerializeField] private Color fireColor = new Color(0.9f, 0.4f, 0.2f);

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

    private void HookButtonEvents()
    {
        if (shieldButton != null)
            shieldButton.onClick.AddListener(() => skillController?.CastShield());

        if (attackButton != null)
            attackButton.onClick.AddListener(() => skillController?.CastAttack());
    }

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
        elementIcon.color = GetElementColor(element);
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
        }
        else if (attackNameText != null && attackStatsText != null)
        {
            attackNameText.text = "N/A";
            attackStatsText.text = "No Attack Skill";
            attackButton.interactable = false;
        }
    }

    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Earth => earthColor,
            ElementType.Wind => windColor,
            ElementType.Water => waterColor,
            ElementType.Fire => fireColor,
            _ => Color.white
        };
    }
}