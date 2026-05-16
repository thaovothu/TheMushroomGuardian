using UnityEngine;

/// <summary>
/// Controller cho Player Skill System
/// Quản lý: Chọn nguyên tố, Cast skill, Cooldown, Mana
/// </summary>
public class PlayerSkillController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform attackPoint; // Vị trí phát chiêu
    private PlayerController playerController;

    [Header("Skill Settings")]
    [SerializeField] private float skillCooldown = 1.5f;
    [SerializeField] private float maxMana = 100f;
    private float currentMana;
    private ElementType currentElement = ElementType.Earth;
    private float lastSkillCastTime = -999f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode shieldKey = KeyCode.E;        // Cast Shield/Defender skill
    [SerializeField] private KeyCode attackKey = KeyCode.Q;        // Cast Attack skill
    [SerializeField] private KeyCode switchElementKey = KeyCode.R; // Switch element

    void Start()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (attackPoint == null) attackPoint = transform;

        currentMana = maxMana;
        Debug.Log($"[PlayerSkillController] ✓ Initialized - Element: {currentElement}, Mana: {currentMana}/{maxMana}");
    }

    void Update()
    {
        // Regen mana over time
        RegenerateMana();

        // Input handling
        HandleSkillInput();
    }

    /// <summary>
    /// Xử lý input từ keyboard
    /// </summary>
    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(shieldKey))
        {
            if (playerController != null)
                playerController.SkillDefend();
        }

        if (Input.GetKeyDown(attackKey))
        {
            if (playerController != null)
                playerController.SkillAttack();
        }

        if (Input.GetKeyDown(switchElementKey))
        {
            SwitchElement();
        }
    }

    /// <summary>
    /// Trigger shield cast từ PlayerController state machine
    /// </summary>
    public void TriggerShieldCast()
    {
        var shieldSkill = SkillSystem.Instance.GetElementShield(currentElement);
        if (shieldSkill == null)
        {
            Debug.LogWarning($"[PlayerSkillController] No shield skill for {currentElement}");
            return;
        }

        if (!CanCastSkill(shieldSkill))
        {
            Debug.Log($"[PlayerSkillController] Cannot cast shield - cooldown or mana");
            return;
        }

        // Consume mana
        currentMana -= shieldSkill.manaCost;
        SkillSystem.Instance.RecordSkillCast(shieldSkill.skillId);

        // Apply defender effect
        ApplyDefenderEffect(shieldSkill);

        Debug.Log($"[PlayerSkillController] ✓ Cast Shield: {shieldSkill.skillName}, Defense: +{shieldSkill.defense}, Mana: {currentMana}");
    }

    /// <summary>
    /// Trigger attack cast từ PlayerController state machine
    /// </summary>
    public void TriggerAttackCast()
    {
        var attackSkill = SkillSystem.Instance.GetElementAttack(currentElement);
        if (attackSkill == null)
        {
            Debug.LogWarning($"[PlayerSkillController] No attack skill for {currentElement}");
            return;
        }

        if (!CanCastSkill(attackSkill))
        {
            Debug.Log($"[PlayerSkillController] Cannot cast attack - cooldown or mana");
            return;
        }

        // Consume mana
        currentMana -= attackSkill.manaCost;
        SkillSystem.Instance.RecordSkillCast(attackSkill.skillId);

        // Apply damage effect
        ApplyDamageEffect(attackSkill);

        Debug.Log($"[PlayerSkillController] ✓ Cast Attack: {attackSkill.skillName}, Damage: {attackSkill.damage}, Mana: {currentMana}");
    }

    /// <summary>
    /// Legacy method - redirects to TriggerShieldCast
    /// </summary>
    public void CastShield()
    {
        TriggerShieldCast();
    }

    /// <summary>
    /// Legacy method - redirects to TriggerAttackCast
    /// </summary>
    public void CastAttack()
    {
        TriggerAttackCast();
    }

    /// <summary>
    /// Chuyển đổi nguyên tố
    /// </summary>
    public void SwitchElement()
    {
        int nextElement = ((int)currentElement + 1) % 5;
        currentElement = (ElementType)nextElement;

        // Bỏ qua None
        if (currentElement == ElementType.None)
        {
            currentElement = ElementType.Earth;
        }

        Debug.Log($"[PlayerSkillController] ✓ Switched to element: {currentElement}");
    }

    /// <summary>
    /// Kiểm tra có thể cast skill không
    /// </summary>
    private bool CanCastSkill(SkillData skill)
    {
        // Kiểm tra mana
        if (currentMana < skill.manaCost)
        {
            Debug.Log($"[PlayerSkillController] Not enough mana! Need: {skill.manaCost}, Current: {currentMana}");
            return false;
        }

        // Kiểm tra cooldown
        if (Time.time - lastSkillCastTime < skillCooldown)
        {
            Debug.Log($"[PlayerSkillController] Skill on cooldown! {skillCooldown - (Time.time - lastSkillCastTime):F1}s remaining");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Áp dụng effect tấn công (Damage)
    /// </summary>
    private void ApplyDamageEffect(SkillData skill)
    {
        // Raycast để tìm enemy trong range
        RaycastHit[] hits = Physics.SphereCastAll(attackPoint.position, 3f, attackPoint.forward, 10f);

        foreach (RaycastHit hit in hits)
        {
            // Skip nếu hit vào player
            if (hit.collider.gameObject == gameObject)
                continue;

            HealthSystem enemyHealth = hit.collider.GetComponent<HealthSystem>();
            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                // Tính damage với elemental multiplier
                float multiplier = SkillSystem.Instance.GetElementalMultiplier(skill.element, enemyHealth.GetElement());
                float finalDamage = skill.damage * multiplier;

                enemyHealth.TakeDamage(finalDamage, skill.element);
                Debug.Log($"[PlayerSkillController] ✓ Hit {hit.collider.name} with {skill.skillName}: {finalDamage} damage (multiplier: {multiplier:F2})");
            }
        }

        lastSkillCastTime = Time.time;
    }

    /// <summary>
    /// Áp dụng effect phòng thủ (Defender/Shield)
    /// Tăng Defense tạm thời
    /// </summary>
    private void ApplyDefenderEffect(SkillData skill)
    {
        if (healthSystem != null)
        {
            // TODO: Implement temporary defense buff
            // healthSystem.AddDefenseBuff(skill.defense, 5f); // +defense cho 5 giây
            Debug.Log($"[PlayerSkillController] ✓ Applied shield defense: +{skill.defense}");
        }

        lastSkillCastTime = Time.time;
    }

    /// <summary>
    /// Hồi mana over time
    /// </summary>
    private void RegenerateMana()
    {
        float manaRegenRate = 5f; // 5 mana/giây
        currentMana = Mathf.Min(currentMana + manaRegenRate * Time.deltaTime, maxMana);
    }

    // ────── Getters cho UI ──────

    public int GetCurrentMana() => Mathf.RoundToInt(currentMana);
    public int GetMaxMana() => Mathf.RoundToInt(maxMana);
    public ElementType GetCurrentElement() => currentElement;
    public float GetManaPercentage() => currentMana / maxMana;

    /// <summary>
    /// Lấy skill hiện tại (để hiển thị UI)
    /// </summary>
    public SkillData GetCurrentShield()
    {
        if (SkillSystem.Instance == null)
        {
            Debug.LogError("[PlayerSkillController] SkillSystem.Instance is NULL!");
            return null;
        }
        var shield = SkillSystem.Instance.GetElementShield(currentElement);
        if (shield == null)
            Debug.LogWarning($"[PlayerSkillController] No shield skill for {currentElement}");
        return shield;
    }

    public SkillData GetCurrentAttack()
    {
        if (SkillSystem.Instance == null)
        {
            Debug.LogError("[PlayerSkillController] SkillSystem.Instance is NULL!");
            return null;
        }
        var attack = SkillSystem.Instance.GetElementAttack(currentElement);
        if (attack == null)
            Debug.LogWarning($"[PlayerSkillController] No attack skill for {currentElement}");
        return attack;
    }
}
