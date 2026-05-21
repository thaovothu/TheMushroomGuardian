using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI Skill System - Hiển thị nguyên tố, skill, mana, cooldown
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

    private int findRetry = 0;
    private const int MAX_RETRY = 100; // Thử 100 lần (~1.6s ở 60fps)

    void Start()
    {
        if (skillController == null)
        {
            TryFindPlayerSkillController();
        }

        if (skillController == null)
        {
            Debug.LogWarning("[UISkillPanel] ⚠ PlayerSkillController not found yet (will retry in Update)");
            return;
        }

        HookButtonEvents();
        UpdateUI();
    }

    void Update()
    {
        // Retry tìm PlayerSkillController nếu vẫn null
        if (skillController == null && findRetry < MAX_RETRY)
        {
            findRetry++;
            TryFindPlayerSkillController();
            
            if (skillController != null)
            {
                Debug.Log($"[UISkillPanel] ✓ PlayerSkillController found after {findRetry} retries!");
                HookButtonEvents();
                UpdateUI();
                return;
            }
        }

        if (skillController == null) 
        {
            if (findRetry == MAX_RETRY)
                Debug.LogError($"[UISkillPanel] ✗ Không tìm thấy PlayerSkillController sau {MAX_RETRY} retries");
            return;
        }
        UpdateUI();
    }

    /// <summary>
    /// Thử tìm PlayerSkillController
    /// </summary>
    private void TryFindPlayerSkillController()
    {
        // Thử GetComponent từ cùng GameObject
        skillController = GetComponent<PlayerSkillController>();
        if (skillController != null) return;

        // Thử tìm Player tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            skillController = player.GetComponent<PlayerSkillController>();
        }
    }

    /// <summary>
    /// Hook button events
    /// </summary>
    private void HookButtonEvents()
    {
        if (shieldButton != null)
            shieldButton.onClick.AddListener(() => skillController?.CastShield());

        if (attackButton != null)
            attackButton.onClick.AddListener(() => skillController?.CastAttack());
    }

    /// <summary>
    /// Cập nhật toàn bộ UI
    /// </summary>
    private void UpdateUI()
    {
        UpdateElementDisplay();
        UpdateSkillDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị nguyên tố
    /// </summary>
    private void UpdateElementDisplay()
    {
        if (elementText == null || elementIcon == null)
            return;

        var element = skillController.GetCurrentElement();
        elementText.text = element.ToString().ToUpper();

        // Đổi màu theo nguyên tố
        Color elementColor = GetElementColor(element);
        elementIcon.color = elementColor;
    }

    /// <summary>
    /// Cập nhật hiển thị skill
    /// </summary>
    private void UpdateSkillDisplay()
    {
        if (shieldButton == null || attackButton == null)
        {
            Debug.LogError("[UISkillPanel] ✗ Button references missing!");
            return;
        }

        if (skillController == null)
        {
            Debug.LogError("[UISkillPanel] ✗ skillController is NULL in UpdateSkillDisplay!");
            return;
        }

        var shieldSkill = skillController.GetCurrentShield();
        var attackSkill = skillController.GetCurrentAttack();
        
        // Debug.Log($"[UISkillPanel] shieldSkill: {(shieldSkill != null ? shieldSkill.skillName : "NULL")}, attackSkill: {(attackSkill != null ? attackSkill.skillName : "NULL")}");

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

        // Debug.Log("[UISkillPanel] UI updated with current skill info: " + shieldNameText.text + ", " + attackNameText.text);
    }

    /// <summary>
    /// Lấy màu theo nguyên tố
    /// </summary>
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
