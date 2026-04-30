// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;

// using UnityEngine.Events;

// public class HeathSystem : MonoBehaviour
// {
//     public UnityEvent<HeathSystem, float, float> OnUpdateHealthBar;
//     [SerializeField] float maxHealth;
//     private bool isHit = false;
//     private bool isDie = false;
//     [SerializeField]  float currentHealth;

//     void Start()
//     {
//         currentHealth = maxHealth;
//         OnUpdateHealthBar?.Invoke(this,currentHealth, maxHealth);
//         UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, currentHealth, maxHealth);
//     }



//     public void TakeDamage(float damageAmount)
//     {
//         currentHealth -= damageAmount;
//         OnUpdateHealthBar?.Invoke(this,currentHealth, maxHealth);
//         UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, currentHealth, maxHealth);
//         isHit = true;

//         if (currentHealth <= 0)
//         {
//             Die();
//         }
//     }

//     public bool IsHitPlayer()
//     {
//         return isHit;
//     }

//     public void ClearHit()
//     {
//         isHit = false;
//     }

//     public bool IsDiePlayer()
//     {
//         return isDie;
//     }

//     public void Die()
//     {
//         isDie = true;
//     }
// }

// Systems/Health/HealthSystem.cs
// using UnityEngine;

// public class HealthSystem : MonoBehaviour
// {
//     [SerializeField] float maxHealth = 100f;

//     float _currentHealth;
//     bool _isHit;
//     bool _isDead;

//     public bool IsHit => _isHit;
//     public bool IsDead => _isDead;
//     public float CurrentHealth => _currentHealth;
//     public float MaxHealth => maxHealth;

//     void Start()
//     {
//         // fallback nếu không được gọi qua Init() (e.g. để trực tiếp trong scene)
//         if (_currentHealth <= 0f) Init(maxHealth);
//     }

//     public void Init(float hp)
//     {
//         maxHealth = hp;
//         _currentHealth = hp;
//         _isHit = false;
//         _isDead = false;

//         UIHeadInfoEnemy.OnUpdateHealthBar?.Invoke(this, _currentHealth, maxHealth);
//         UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, _currentHealth, maxHealth);
//     }

//     public void TakeDamage(float amount)
//     {
//         if (_isDead) return;

//         _currentHealth = Mathf.Max(0f, _currentHealth - amount);
//         _isHit = true;

//         UIHeadInfoEnemy.OnUpdateHealthBar?.Invoke(this, _currentHealth, maxHealth);
//         UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, _currentHealth, maxHealth);

//         if (_currentHealth <= 0f) _isDead = true;
//     }

//     public void ClearHit() => _isHit = false;
// }


using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float baseDamage = 10f;  // damage của entity này khi tấn công

    [SerializeField] float _currentHealth;
    bool _isHit;
    bool _isDead;

    public bool IsHit => _isHit;
    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;

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

    // Overload chính — tính elemental multiplier
    public void TakeDamage(float amount, ElementType incomingElement)
    {
        if (_isDead) return;

        float multiplier = ElementalSystem.GetMultiplier(incomingElement, GetElement());
        float finalDamage = amount * multiplier;

        _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);
        _isHit = true;

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
    
}


