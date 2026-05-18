using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float baseDamage = 10f;  // damage của entity này khi tấn công

    [SerializeField] float _currentHealth;
    bool _isHit;
    bool _isDead;

    // Temporary defense buff
    private float _defenseBuffAmount = 0f;
    private float _defenseBuffEndTime = 0f;
    private Coroutine _defenseBuffCoroutine;

    public bool IsHit => _isHit;
    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float DefenseBuffAmount => _defenseBuffAmount;

    // Events — bất kỳ ai quan tâm đều subscribe, HealthSystem không biết UI tồn tại
    public static Action<HealthSystem, float, float> OnHealthChanged;
    public static Action<HealthSystem> OnDeath;

    void Start()
    {
        if (_currentHealth <= 0f) Init(maxHealth);
    }

    public void Init(float hp)
    {
        maxHealth = hp;
        _currentHealth = hp;
        _isHit = false;
        _isDead = false;

        OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);
    }

    // Overload không element — dùng cho damage không tính nguyên tố
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, ElementType.None);
    }

    // Overload chính — tính elemental multiplier và defense buff
    public void TakeDamage(float amount, ElementType incomingElement)
    {
        if (_isDead) return;

        float multiplier = ElementalSystem.GetMultiplier(incomingElement, GetElement());
        float finalDamage = amount * multiplier;
        
        // Áp dụng defense buff (giảm damage)
        if (_defenseBuffAmount > 0f && Time.time < _defenseBuffEndTime)
        {
            float reducedDamage = finalDamage * (1f - (_defenseBuffAmount / 100f));
            finalDamage = Mathf.Max(1f, reducedDamage); // Tối thiểu 1 damage
            Debug.Log($"[HealthSystem] ✓ Defense buff active: {_defenseBuffAmount:F1}% damage reduction applied. Original: {amount}, Final: {finalDamage:F2}");
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);
        _isHit = true;
        
        // Hiển thị damage text — detect ai nhận damage dựa vào tag
        bool isPlayerTakingDamage = gameObject.CompareTag("Player");
        if (UIDameGenerator.current != null)
        {
            UIDameGenerator.current.ShowDamageText(transform.position, finalDamage, isPlayerTakingDamage);
            Debug.Log($"[HealthSystem] ✓ Damage text shown: {finalDamage} (Player taking: {isPlayerTakingDamage})");
        }
        else
        {
            // Nếu current null, tìm lại
            UIDameGenerator damageGen = FindObjectOfType<UIDameGenerator>();
            if (damageGen != null)
            {
                damageGen.ShowDamageText(transform.position, finalDamage, isPlayerTakingDamage);
                Debug.Log($"[HealthSystem] ✓ Damage text shown (found): {finalDamage} (Player taking: {isPlayerTakingDamage})");
            }
        }
        
        OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);

        if (_currentHealth <= 0f)
        {
            _isDead = true;
            OnDeath?.Invoke(this);
        }
    }

    public void ClearHit() => _isHit = false;

    // Các getters mà Boss nodes cần
    public float GetHPPercent() => maxHealth > 0f ? _currentHealth / maxHealth : 0f;
    public float GetDamage() => baseDamage;
    public void Recover(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);
    }


    // Entity này thuộc nguyên tố nào — override ở subclass hoặc set qua Inspector
    public virtual ElementType GetElement() => ElementType.None;

    /// <summary>
    /// Thêm temporary defense buff — giảm damage đến từ enemy
    /// </summary>
    public void AddDefenseBuff(float defenseAmount, float duration)
    {
        // Nếu đã có buff, hủy cái cũ
        if (_defenseBuffCoroutine != null)
        {
            StopCoroutine(_defenseBuffCoroutine);
        }

        _defenseBuffAmount = defenseAmount;
        _defenseBuffEndTime = Time.time + duration;
        _defenseBuffCoroutine = StartCoroutine(DefenseBuffCoroutine(duration));
        
        Debug.Log($"[HealthSystem] ✓ Defense buff added: +{defenseAmount:F1}% for {duration}s");
    }

    private System.Collections.IEnumerator DefenseBuffCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _defenseBuffAmount = 0f;
        _defenseBuffEndTime = 0f;
        Debug.Log($"[HealthSystem] ✓ Defense buff expired");
    }
    
}


