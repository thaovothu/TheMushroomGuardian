using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
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
    
        RaycastHit hit;
        float radius = 0.5f; // độ rộng của kiếm
        int layerMask = LayerMask.GetMask("Enemy"); // dùng tên layer

        if (Physics.SphereCast(transform.position, radius, transform.forward, out hit, weaponLength, layerMask))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                //Debug.Log($"Hit enemy {hit.collider.name}");
                hit.collider.GetComponent<HealthSystem>()?.TakeDamage(weaponDamage);

                canDealDamage = false; 
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
