using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeathSystem : MonoBehaviour
{
    [SerializeField] float maxHealth;
    private bool isHit = false;
    private bool isDie = false;
    [SerializeField]  float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIHeadInfoEnemy.OnUpdateHealthBar?.Invoke(this,currentHealth, maxHealth);
        UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, currentHealth, maxHealth);
    }
    
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        UIHeadInfoEnemy.OnUpdateHealthBar?.Invoke(this,currentHealth, maxHealth);
        UIHeadInfoStatus.OnUpdateHealthBar?.Invoke(this, currentHealth, maxHealth);
        isHit = true;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsHitPlayer()
    {
        return isHit;
    }

    public void ClearHit()
    {
        isHit = false;
    }

    public bool IsDiePlayer()
    {
        return isDie;
    }

    public void Die()
    {
        isDie = true;
    }
}
