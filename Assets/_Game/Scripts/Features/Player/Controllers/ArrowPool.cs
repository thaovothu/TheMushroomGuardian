using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPool : BaseSingleton<ArrowPool>
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] int poolSize = 5;
    
    Queue<GameObject> availableArrows;
    List<GameObject> allArrows;
    
    static ArrowPool instance;

    void Start()
    {
        instance = this;
        availableArrows = new Queue<GameObject>(poolSize);
        allArrows = new List<GameObject>();
        
        // Pre-spawn tất cả arrow trong pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform);
            arrow.SetActive(false);
            availableArrows.Enqueue(arrow);
            allArrows.Add(arrow);
        }
    }

    // Lấy arrow từ pool
    public static GameObject GetArrow(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[ArrowPool] GetArrow called. Pool available: {instance.availableArrows.Count}");
        
        if (instance.availableArrows.Count == 0)
        {
            Debug.LogWarning("Arrow pool hết! Tạo thêm arrow mới.");
            GameObject newArrow = Instantiate(instance.arrowPrefab, position, rotation, instance.transform);
            return newArrow;
        }

        GameObject arrow = instance.availableArrows.Dequeue();
        Debug.Log($"[ArrowPool] Got arrow from pool. Arrow name: {arrow.name}, has Arrow script: {arrow.GetComponent<Arrow>() != null}");
        
        arrow.transform.position = position;
        arrow.transform.rotation = rotation;
        
        // Reset Rigidbody
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false; // đảm bảo physics hoạt động
            Debug.Log($"[ArrowPool] Rigidbody found and reset");
        }
        else
        {
            Debug.LogError($"[ArrowPool] Arrow prefab không có Rigidbody!");
        }
        
        arrow.SetActive(true);
        Debug.Log($"[ArrowPool] Arrow activated");

        return arrow;
    }

    // Trả arrow về pool
    public static void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        instance.availableArrows.Enqueue(arrow);
    }
}
