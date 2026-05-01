using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadInfoStatus : MonoBehaviour
{
    public enum EntityType { Player, Boss }
    
    [SerializeField] private EntityType entityType = EntityType.Player;
    [SerializeField] private HealthSystem targetHealth;
    [Header("UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthBarText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameCharacterText;
    private bool isSubscribed = false;

    void OnEnable()
    {
        // Retry tìm HealthSystem mỗi lần UI active (không chỉ 1 lần ở Awake)
        if (targetHealth == null)
        {
            targetHealth = FindHealthSystemByTag();
        }
        
        if (targetHealth != null)
        {
            SubscribeToHealthSystem();
        }
        else
        {
            StartCoroutine(WaitForHealthSystem());
        }
    }

    public void Awake()
    {
        // Nếu không set trực tiếp, tìm theo tag
        if (targetHealth == null)
        {
            targetHealth = FindHealthSystemByTag();
        }
        
        // Nếu vẫn null, start coroutine để retry (vì entity có thể chưa spawn)
        if (targetHealth == null)
        {
            StartCoroutine(WaitForHealthSystem());
        }
        else
        {
            SubscribeToHealthSystem();
        }
    }

    private HealthSystem FindHealthSystemByTag()
    {
        // Tìm entity theo tag dựa vào EntityType
        string searchTag = entityType == EntityType.Player ? "Player" : "Boss";
        GameObject target = GameObject.FindGameObjectWithTag(searchTag);
        
        //Debug.Log($"[UIHeadInfoStatus] EntityType: {entityType}, SearchTag: {searchTag}, Found: {(target != null ? target.name : "NULL")}");
        
        if (target != null)
        {
            HealthSystem hs = target.GetComponent<HealthSystem>();
            //Debug.Log($"[UIHeadInfoStatus] HealthSystem found on {target.name}: {(hs != null ? "✓" : "✗ NULL")}");
            return hs;
        }
        
        return null;
    }

    private IEnumerator WaitForHealthSystem()
    {
        // Chờ lâu hơn (60 frame ~ 1 giây) vì Boss spawn từ pooling
        for (int i = 0; i < 60; i++)
        {
            yield return null;
            targetHealth = FindHealthSystemByTag();
            if (targetHealth != null)
            {
                SubscribeToHealthSystem();
                //Debug.Log($"[UIHeadInfoStatus] ✓ Found {entityType} HealthSystem after {i} frames");
                yield break;
            }
        }
        
        //Debug.LogWarning($"[UIHeadInfoStatus] ✗ Không tìm thấy HealthSystem cho {entityType} sau 60 frame");
    }

    private void SubscribeToHealthSystem()
    {
        if (!isSubscribed && targetHealth != null)
        {
            HealthSystem.OnHealthChanged += UpdateHealthBar;
            HealthSystem.OnDeath += HandleDeath;
            isSubscribed = true;
            
            // Update ngay hiện tại health khi subscribe (không cần chờ event)
            UpdateHealthBar(targetHealth, targetHealth.CurrentHealth, targetHealth.MaxHealth);
            //Debug.Log($"[UIHeadInfoStatus] ✓ Subscribed to {entityType} HealthSystem. Current HP: {targetHealth.CurrentHealth}/{targetHealth.MaxHealth}");
        }
    }

    private void HandleDeath(HealthSystem deadEntity)
    {
        if (deadEntity == targetHealth)
        {
            //Debug.Log($"[UIHeadInfoStatus] {entityType} died! Hiding UI...");
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (isSubscribed)
        {
            HealthSystem.OnHealthChanged -= UpdateHealthBar;
            HealthSystem.OnDeath -= HandleDeath;
        }
    }
    
    void UpdateHealthBar(HealthSystem healthSystem, float current, float max)
    {
        if (healthSystem != targetHealth) return;
        healthBar.value = current / max;

        healthBarText.text = $"{current} / {max}";
    }


}
