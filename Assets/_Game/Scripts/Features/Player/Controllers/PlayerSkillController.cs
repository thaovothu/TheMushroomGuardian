using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller cho Player Skill System
/// Quản lý: Chọn nguyên tố, Cast skill, Cooldown, Mana
/// </summary>
public class PlayerSkillController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [Tooltip("Tâm vòng tròn AoE (Đất/Khí/Lửa) — nên đặt sát chân player. Trống → dùng vị trí player.")]
    [SerializeField] private Transform aoeCenterPoint;
    private PlayerController playerController;
    private Camera cam;

    [Header("Skill Settings")]
    [SerializeField] private float skillCooldown = 1.5f;
    [SerializeField] private float maxMana = 100f;

    [Header("AoE — Đất / Khí / Lửa")]
    [Tooltip("Bán kính vòng tròn quanh player (Đất/Khí/Lửa). Nên khớp kích thước VFX vòng tròn.")]
    [SerializeField] private float aoeRadius = 5f;
    [Tooltip("Lệch Y điểm rơi quả cầu lửa so với chân enemy (0 = đúng chân; prefab tự lo phần rơi từ trên xuống).")]
    [SerializeField] private float fireImpactYOffset = 0f;
    [Tooltip("Lửa: thời gian chờ quả cầu rơi xuống TRƯỚC khi gây damage (giây). Khớp với độ dài VFX rơi.")]
    [SerializeField] private float fireImpactDelay = 0.6f;

    [Header("Nước — AoE phía trước")]
    [Tooltip("Tâm vùng băng cách player bao xa về phía trước.")]
    [SerializeField] private float waterForwardDistance = 3f;
    [Tooltip("Bán kính vùng băng phía trước (vùng gây damage).")]
    [SerializeField] private float waterRadius = 3f;
    [Tooltip("Xoay bù hướng VFX băng (độ, quanh Y) nếu prefab chĩa lệch trục. Thử 90 / -90 / 180 cho thẳng.")]
    [SerializeField] private float waterVfxYawOffset = 0f;

    private readonly Collider[] _aoeBuffer = new Collider[32]; // buffer cho OverlapSphereNonAlloc
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
        cam = Camera.main;

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
        Debug.Log("[PlayerSkillController] TriggerAttackCast() called");

        var attackSkill = SkillSystem.Instance.GetElementAttack(currentElement);
        if (attackSkill == null)
        {
            Debug.LogWarning($"[PlayerSkillController] No attack skill for {currentElement}");
            return;
        }

        Debug.Log($"[PlayerSkillController] Got skill: {attackSkill.skillName}");
        Debug.Log($"[PlayerSkillController] DEBUG - Skill range: {attackSkill.range}");
        Debug.Log($"[PlayerSkillController] DEBUG - Checking if can cast...");

        if (!CanCastSkill(attackSkill))
        {
            Debug.Log($"[PlayerSkillController] Cannot cast attack - cooldown or mana");
            return;
        }

        Debug.Log($"[PlayerSkillController] DEBUG - Can cast, consuming mana...");

        // Consume mana
        currentMana -= attackSkill.manaCost;
        SkillSystem.Instance.RecordSkillCast(attackSkill.skillId);

        Debug.Log($"[PlayerSkillController] DEBUG - Mana consumed. Calling ApplyDamageEffect...");

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
        // Phân loại theo hệ:
        //   Nước    → AoE chụm PHÍA TRƯỚC player (hướng theo camera, như kiếm)
        //   Lửa     → vòng tròn quanh player: spawn VFX trên đầu TỪNG enemy + damage
        //   Đất/Khí → vòng tròn quanh player: 1 VFX ở tâm + damage mọi enemy trong vòng
        switch (skill.element)
        {
            case ElementType.Water:
                ApplyForwardAoE(skill);
                break;
            case ElementType.Fire:
                ApplyCircleAoE(skill, vfxOnEachTarget: true);
                break;
            default: // Earth, Wind
                ApplyCircleAoE(skill, vfxOnEachTarget: false);
                break;
        }

        lastSkillCastTime = Time.time;
    }

    /// <summary>
    /// Skill vòng tròn quanh player (Đất/Khí/Lửa): damage mọi enemy trong bán kính aoeRadius.
    /// vfxOnEachTarget = false (Đất/Khí): 1 VFX ở tâm vòng.
    /// vfxOnEachTarget = true  (Lửa): spawn VFX trên đầu từng enemy trúng.
    /// </summary>
    private void ApplyCircleAoE(SkillData skill, bool vfxOnEachTarget)
    {
        Vector3 center = aoeCenterPoint != null ? aoeCenterPoint.position : transform.position;

        // VFX vòng tròn ở tâm (Đất/Khí). Lửa: bỏ qua, spawn trên đầu từng enemy ở dưới.
        if (!vfxOnEachTarget && SkillVFXManager.Instance != null)
            SkillVFXManager.Instance.SpawnSkillVFX(skill.skillId, center, Quaternion.identity);

        // Bán kính = riêng từng skill (skill.areaRadius) nếu > 0, không thì dùng aoeRadius chung.
        float r = skill.areaRadius > 0f ? skill.areaRadius : aoeRadius;
        int count = Physics.OverlapSphereNonAlloc(center, r, _aoeBuffer);

        var targets = new List<HealthSystem>();
        for (int i = 0; i < count; i++)
        {
            var col = _aoeBuffer[i];
            if (col.gameObject == gameObject) continue;

            // AncientPump (puzzle ấn) — nhận đòn theo hệ. GetComponentInParent gồm cả self.
            var pump = col.GetComponentInParent<AncientPump>();
            if (pump != null) { pump.ReceiveElementHit(skill.element); continue; }

            var hs = col.GetComponentInParent<HealthSystem>();
            if (hs == null || hs.IsDead || hs == healthSystem) continue;
            if (targets.Contains(hs)) continue; // mỗi enemy chỉ tính 1 lần (1 entity nhiều collider)
            targets.Add(hs);

            // Lửa: spawn quả cầu rơi trên enemy NGAY (damage hoãn lại tới khi rơi chạm đất).
            if (vfxOnEachTarget && SkillVFXManager.Instance != null)
                SkillVFXManager.Instance.SpawnSkillVFX(skill.skillId,
                    hs.transform.position + Vector3.up * fireImpactYOffset, Quaternion.identity);
        }

        // Đất/Khí: damage ngay. Lửa: chờ fireImpactDelay (quả cầu rơi) rồi mới gây damage.
        if (vfxOnEachTarget && fireImpactDelay > 0f)
            StartCoroutine(DealAoEDamageDelayed(targets, skill, fireImpactDelay));
        else
            foreach (var hs in targets) DealAoEDamage(hs, skill);

        Debug.Log($"[PlayerSkillController] AoE {skill.skillName}: {targets.Count} enemy trong bán kính {r}m");
    }

    private void DealAoEDamage(HealthSystem hs, SkillData skill)
    {
        if (hs == null || hs.IsDead) return;
        float multiplier = SkillSystem.Instance.GetElementalMultiplier(skill.element, hs.GetElement());
        hs.TakeDamage(skill.damage * multiplier, skill.element);
    }

    // Lửa: chờ quả cầu rơi xuống rồi mới gây damage (đồng bộ với VFX).
    private IEnumerator DealAoEDamageDelayed(List<HealthSystem> targets, SkillData skill, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var hs in targets) DealAoEDamage(hs, skill);
    }

    /// <summary>
    /// Skill Nước: AoE chụm PHÍA TRƯỚC theo HƯỚNG CAMERA — xoay player về hướng cam rồi
    /// đặt vùng băng + VFX theo cam.forward (KHÔNG lấy rotation player đang lệch).
    /// </summary>
    private void ApplyForwardAoE(SkillData skill)
    {
        // Hướng = TRƯỚC MẶT CAMERA (phẳng), KHÔNG lấy rotation hiện tại của player (có thể đang lệch).
        Vector3 aim = cam != null ? cam.transform.forward : transform.forward;
        aim.y = 0f;
        aim = aim.sqrMagnitude > 0.0001f ? aim.normalized : transform.forward;

        // Xoay player về lại hướng camera (snap mặt player về trước cam).
        if (playerController != null) playerController.RotateToward(aim);

        // Bán kính = riêng từng skill (skill.areaRadius) nếu > 0, không thì dùng waterRadius chung.
        float r = skill.areaRadius > 0f ? skill.areaRadius : waterRadius;

        // Tâm vùng băng + VFX đều theo HƯỚNG CAMERA (aim), không phụ thuộc rotation player.
        Vector3 center = transform.position + aim * waterForwardDistance;

        // Debug: đường cyan = hướng + tầm damage (xem trong Scene view khi Play).
        Debug.DrawRay(transform.position, aim * (waterForwardDistance + r), Color.cyan, 1f);

        if (SkillVFXManager.Instance != null)
        {
            // Xoay VFX theo hướng bắn + bù lệch trục prefab (waterVfxYawOffset).
            Quaternion vfxRot = Quaternion.LookRotation(aim) * Quaternion.Euler(0f, waterVfxYawOffset, 0f);
            SkillVFXManager.Instance.SpawnSkillVFX(skill.skillId, center, vfxRot);
        }

        // Damage mọi enemy chạm vùng phía trước (giống Đất/Khí nhưng tâm ở trước mặt).
        int count = Physics.OverlapSphereNonAlloc(center, r, _aoeBuffer);
        var targets = new List<HealthSystem>();
        for (int i = 0; i < count; i++)
        {
            var col = _aoeBuffer[i];
            if (col.gameObject == gameObject) continue;

            var pump = col.GetComponentInParent<AncientPump>();
            if (pump != null) { pump.ReceiveElementHit(skill.element); continue; }

            var hs = col.GetComponentInParent<HealthSystem>();
            if (hs == null || hs.IsDead || hs == healthSystem) continue;
            if (targets.Contains(hs)) continue;
            targets.Add(hs);
            DealAoEDamage(hs, skill);
        }

        Debug.Log($"[PlayerSkillController] Nước {skill.skillName}: {targets.Count} enemy trong vùng phía trước (r={r})");
    }

    /// <summary>
    /// Áp dụng effect phòng thủ (Defender/Shield)
    /// Tăng Defense tạm thời - giảm damage từ enemy/boss dựa trên skill.defense
    /// </summary>
    private void ApplyDefenderEffect(SkillData skill)
    {
        if (healthSystem != null)
        {
            // Áp dụng defense buff tạm thời - skill.defense là % giảm damage
            float buffDuration = 5f; // 5 giây
            healthSystem.AddDefenseBuff(skill.defense, buffDuration);
            Debug.Log($"[PlayerSkillController] ✓ Applied shield defense: +{skill.defense}% damage reduction for {buffDuration}s");
        }

        // Spawn defend VFX tại player position
        if (SkillVFXManager.Instance != null)
        {
            SkillVFXManager.Instance.SpawnSkillVFX(skill.skillId, transform.position, transform.rotation);
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

    public bool HasMana(float amount) => currentMana >= amount;
    public void ConsumeMana(float amount) => currentMana = Mathf.Max(0f, currentMana - amount);
    public void RecoverMana(float amount) => currentMana = Mathf.Min(currentMana + amount, maxMana);

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
