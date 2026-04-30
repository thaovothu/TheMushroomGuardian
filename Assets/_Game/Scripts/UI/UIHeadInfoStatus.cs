using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadInfoStatus : MonoBehaviour
{
    [SerializeField] private HealthSystem targetHealth;
    [SerializeField] private Slider healthBar;
    [SerializeField] private float currentHealthBar;
    [SerializeField] private float maxHealthBar;
    public static Action<HealthSystem, float, float> OnUpdateHealthBar;

    public void Awake()
    {
        HealthSystem.OnHealthChanged += UpdateHealthBar;

        if (targetHealth == null)
        {
            targetHealth = GetComponentInParent<HealthSystem>();
        }
    }

    void OnDestroy()
    {
        HealthSystem.OnHealthChanged -= UpdateHealthBar;
    }
    // void UpdateHealthBar(HealthSystem healthSystem, float currentHealth, float maxHealth)
    // {
    //     Debug.Log($"[UIHeadInfo] UpdateHealthBar called with currentHealth: {currentHealth}, maxHealth: {maxHealth}");
    //     if (healthSystem == targetHealth)
    //     {
    //         currentHealthBar = currentHealth;
    //         maxHealthBar = maxHealth;
    //         healthBar.value = currentHealthBar / maxHealthBar;
    //     }
    // }

    void UpdateHealthBar(HealthSystem healthSystem, float current, float max)
    {
        if (healthSystem != targetHealth) return;
        healthBar.value = current / max;
    }


}
