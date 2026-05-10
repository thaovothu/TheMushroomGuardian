using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowDamage = 10f;
    [SerializeField] float arrowSpeed = 50f;
    [SerializeField] float lifetime = 3f; // tự hủy sau 5 giây nếu không chạm enemy
    
    Rigidbody rb;
    List<GameObject> hasHit; // tránh damage cùng 1 enemy nhiều lần
    Coroutine lifeCoroutine;

    void Start()
    {
        Debug.Log("[Arrow] Start called");
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("[Arrow] Rigidbody not found on Arrow!");
    }

    void OnEnable()
    {
        Debug.Log("[Arrow] OnEnable called");
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            Debug.Log($"[Arrow] Getting Rigidbody in OnEnable. Found: {rb != null}");
        }
        
        if (hasHit == null)
            hasHit = new List<GameObject>();
        else
            hasHit.Clear();
        
        // Bắn mũi tên theo hướng forward của nó
        if (rb != null)
        {
            rb.velocity = transform.forward * arrowSpeed;
            Debug.Log($"[Arrow] Arrow fired! Velocity: {rb.velocity}, Forward: {transform.forward}, Speed: {arrowSpeed}");
        }
        else
        {
            Debug.LogError("[Arrow] Arrow không có Rigidbody trong OnEnable!");
        }
        
        // Tự hủy sau timeout
        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"[Arrow] OnTriggerEnter called. Hit: {collision.name}, tag: {collision.tag}");
        
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            Debug.Log($"[Arrow] Hit {collision.tag}: {collision.name}");
            
            // Tránh damage cùng enemy nhiều lần
            if (hasHit.Contains(collision.gameObject))
            {
                Debug.Log($"[Arrow] Already hit this {collision.tag}, skipping");
                return;
            }

            hasHit.Add(collision.gameObject);
            
            // Gây damage
            HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                Debug.Log($"[Arrow] Found HealthSystem, dealing {arrowDamage} damage");
                healthSystem.TakeDamage(arrowDamage);
            }
            else
            {
                Debug.LogError($"[Arrow] HealthSystem not found on {collision.name}!");
            }
            
            // Trả arrow về pool
            ReturnToPool();
            return;
        }
        
        // Nếu chạm ground hoặc obstacle, cũng trả về pool
        if (collision.CompareTag("Ground") || collision.CompareTag("Obstacle"))
        {
            Debug.Log($"[Arrow] Hit {collision.tag}, returning to pool");
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);
        ArrowPool.Instance.ReturnArrow(gameObject);
    }
}

