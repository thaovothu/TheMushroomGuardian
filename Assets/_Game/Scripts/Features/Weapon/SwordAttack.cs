using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    bool canDealDamage;
    List<GameObject> hasDealtDamage;

    [SerializeField] float weaponLength;
    [SerializeField] float weaponDamage;
    [SerializeField] ParticleSystem slashVFX;

    // Cache để tránh cấp phát mỗi frame
    int layerMask;
    readonly Collider[] hitBuffer = new Collider[16];

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = new List<GameObject>();
        layerMask = LayerMask.GetMask("Enemy", "Boss");
    }

    void Update()
    {
        if (!canDealDamage) return;

        float radius = 0.8f; // độ rộng của kiếm (tăng từ 0.5f để catch enemy gần hơn)

        // OverlapSphereNonAlloc dùng buffer có sẵn -> không sinh rác cho GC mỗi frame
        int count = Physics.OverlapSphereNonAlloc(
            transform.position + transform.forward * (weaponLength * 0.5f),
            radius,
            hitBuffer,
            layerMask
        );

        for (int i = 0; i < count; i++)
        {
            Collider collider = hitBuffer[i];

            // Check nếu đã hit enemy này trong lần chém này
            if (hasDealtDamage.Contains(collider.gameObject))
                continue;

            if (collider.CompareTag("Enemy") || collider.CompareTag("Boss"))
            {
                collider.GetComponent<HealthSystem>()?.TakeDamage(weaponDamage);
                hasDealtDamage.Add(collider.gameObject); // Ghi nhớ đã hit enemy này
            }
        }
    }
    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage.Clear();
        if (slashVFX != null) slashVFX.Play();
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
