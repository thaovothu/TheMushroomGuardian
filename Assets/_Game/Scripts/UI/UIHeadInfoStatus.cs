using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadInfoStatus : MonoBehaviour
{
    [SerializeField] private HeathSystem targetHealth;
    [SerializeField] private Slider healthBar;
    [SerializeField] private float currentHealthBar;
    [SerializeField] private float maxHealthBar;
    public static Action<HeathSystem, float, float> OnUpdateHealthBar;

    public void Awake()
    {
        OnUpdateHealthBar -= UpdateHealthBar;
        OnUpdateHealthBar += UpdateHealthBar;

        if(targetHealth == null)
        {
            targetHealth = GetComponentInParent<HeathSystem>();
        }
    }
    void UpdateHealthBar(HeathSystem healthSystem, float currentHealth, float maxHealth)
    {
        Debug.Log($"[UIHeadInfo] UpdateHealthBar called with currentHealth: {currentHealth}, maxHealth: {maxHealth}");
        if (healthSystem == targetHealth)
        {
            currentHealthBar = currentHealth;
            maxHealthBar = maxHealth;
            healthBar.value = currentHealthBar / maxHealthBar;
        }
    }
        
}
