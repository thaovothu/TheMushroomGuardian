using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadInfoEnemy : MonoBehaviour
{
    [SerializeField] private HealthSystem targetHealth;
    [SerializeField] private Slider healthBar;
    [SerializeField] private float currentHealthBar;
    [SerializeField] private float maxHealthBar;
    public static Action<HealthSystem, float, float> OnUpdateHealthBar;

    public void Awake()
    {
        GameEvent.Combat.OnHealthChanged += UpdateHealthBar;

        if(targetHealth == null)
        {
            targetHealth = GetComponentInParent<HealthSystem>();
        }
    }

    void OnDestroy()
    {
        GameEvent.Combat.OnHealthChanged -= UpdateHealthBar;
    }

    void UpdateHealthBar(HealthSystem healthSystem, float current, float max)
    {
        if (healthSystem != targetHealth) return;
        healthBar.value = current / max;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}
