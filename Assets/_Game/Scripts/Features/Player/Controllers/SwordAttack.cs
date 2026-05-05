using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    bool canDealDamage;
    List<GameObject> hasDealtDamage;
    
    [SerializeField] float weaponLength;
    [SerializeField] float weaponDamage;
    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = new List<GameObject>();
    }

    void Update()
    {
        if (!canDealDamage) return;
    
        float radius = 0.8f; // độ rộng của kiếm (tăng từ 0.5f để catch enemy gần hơn)
        int layerMask = LayerMask.GetMask("Enemy", "Boss");

        // Dùng OverlapSphere để detect mọi enemy trong phạm vi, kể cả quá gần
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position + transform.forward * (weaponLength * 0.5f), 
            radius, 
            layerMask
        );

        foreach (Collider collider in hitColliders)
        {
            // Check nếu đã hit enemy này trong lần chém này
            if (hasDealtDamage.Contains(collider.gameObject))
                continue;

            if (collider.CompareTag("Enemy") || collider.CompareTag("Boss"))
            {
                //Debug.Log($"Hit enemy {collider.name}");
                collider.GetComponent<HealthSystem>()?.TakeDamage(weaponDamage);
                hasDealtDamage.Add(collider.gameObject); // Ghi nhớ đã hit enemy này
            }
        }
    }
    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage.Clear();
    }
    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
