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

    private Transform camTransform;

    public void Awake()
    {
        GameEvent.Combat.OnHealthChanged += UpdateHealthBar;

        if(targetHealth == null)
        {
            targetHealth = GetComponentInParent<HealthSystem>();
        }

        if (Camera.main != null)
            camTransform = Camera.main.transform;
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
        // Camera.main gọi mỗi frame = FindGameObjectWithTag, rất tốn. Cache lại 1 lần.
        if (camTransform == null)
        {
            if (Camera.main == null) return;
            camTransform = Camera.main.transform;
        }

        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward, camTransform.rotation * Vector3.up);
    }
}
