using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float baseDamage = 10f;

    [SerializeField] float _currentHealth;
    bool _isHit;
    bool _isDead;

    private float _defenseBuffAmount = 0f;
    private float _defenseBuffEndTime = 0f;
    private Coroutine _defenseBuffCoroutine;

    public bool IsHit => _isHit;
    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float DefenseBuffAmount => _defenseBuffAmount;

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

        GameEvent.Combat.OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, ElementType.None);
    }

    public void TakeDamage(float amount, ElementType incomingElement)
    {
        if (_isDead) return;

        // ── Elemental multiplier ──
        float multiplier = ElementalSystem.GetMultiplier(incomingElement, GetElement());
        float finalDamage = amount * multiplier;

        // ── Boss Lửa Phase 1: giáp dung nham giảm 70% damage cho non-Water ──
        var bb = GetComponent<BossBlackboard>();
        if (bb != null
            && bb.bossBaseElement == ElementType.Fire
            && GetHPPercent() > 0.6f
            && incomingElement != ElementType.Water)
        {
            finalDamage *= 0.3f;
            Debug.Log($"[LavaGiant] Armor reduces damage to {finalDamage}");
        }

        // ── Defense buff (player skill) ──
        if (_defenseBuffAmount > 0f && Time.time < _defenseBuffEndTime)
        {
            float reducedDamage = finalDamage * (1f - (_defenseBuffAmount / 100f));
            finalDamage = Mathf.Max(1f, reducedDamage);
            Debug.Log($"[HealthSystem] ✓ Defense buff active: {_defenseBuffAmount:F1}% damage reduction. Final: {finalDamage:F2}");
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);
        _isHit = true;

        // ── Hiển thị damage text ──
        bool isPlayerTakingDamage = gameObject.CompareTag("Player");
        if (UIDameGenerator.current != null)
        {
            UIDameGenerator.current.ShowDamageText(transform.position, finalDamage, isPlayerTakingDamage);
        }
        else
        {
            UIDameGenerator damageGen = FindObjectOfType<UIDameGenerator>();
            if (damageGen != null)
                damageGen.ShowDamageText(transform.position, finalDamage, isPlayerTakingDamage);
        }

        GameEvent.Combat.OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);

        if (_currentHealth <= 0f)
        {
            _isDead = true;
            GameEvent.Combat.OnDeath?.Invoke(this);
        }
    }

    public void ClearHit() => _isHit = false;

    public float GetHPPercent() => maxHealth > 0f ? _currentHealth / maxHealth : 0f;
    public float GetDamage() => baseDamage;

    public void Recover(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        GameEvent.Combat.OnHealthChanged?.Invoke(this, _currentHealth, maxHealth);
    }

    public virtual ElementType GetElement() => ElementType.None;

    public void AddDefenseBuff(float defenseAmount, float duration)
    {
        if (_defenseBuffCoroutine != null)
            StopCoroutine(_defenseBuffCoroutine);

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