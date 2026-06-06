using System.Collections.Generic;
using UnityEngine;

public class ArrowPool : BaseSingleton<ArrowPool>
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] int poolSize = 5;

    [Header("Model Rotation Fix")]
    [Tooltip("Bù rotation nếu arrow model lệch trục so với +Z. " +
             "Mũi nhọn chỉ +X → Y=-90. Mũi nhọn chỉ -Z → Y=180. Mũi nhọn chỉ +Z → Y=0 (không cần bù).")]
    [SerializeField] Vector3 modelRotationOffset = new Vector3(0f, -90f, 0f);

    Queue<GameObject> availableArrows;
    List<GameObject> allArrows;

    protected override void Awake()
    {
        base.Awake();
        GameEvent.Auth.OnLoginSuccess += OnLoginReady;
    }

    private void OnLoginReady(string _)
    {
        GameEvent.Auth.OnLoginSuccess -= OnLoginReady;
        availableArrows = new Queue<GameObject>(poolSize);
        allArrows = new List<GameObject>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform);
            arrow.SetActive(false);
            availableArrows.Enqueue(arrow);
            allArrows.Add(arrow);
        }

        Debug.Log($"[ArrowPool] Initialized {poolSize} arrows. ModelOffset={modelRotationOffset}");
    }

    /// <summary>
    /// Lấy arrow từ pool, set đúng vị trí + hướng bay.
    /// rotation = hướng arrow CẦN BAY (Quaternion.LookRotation(dir)).
    /// modelRotationOffset bù thêm nếu model lệch trục so với +Z.
    /// </summary>
    public GameObject GetArrow(Vector3 position, Quaternion rotation)
    {
        if (availableArrows == null) return null;
        GameObject arrow;

        if (availableArrows.Count == 0)
        {
            Debug.LogWarning("[ArrowPool] Pool hết — tạo thêm arrow mới.");
            arrow = Instantiate(arrowPrefab, transform);
        }
        else
        {
            arrow = availableArrows.Dequeue();
        }

        arrow.transform.position = position;
        // Ghép: hướng bay thật * offset bù model lệch trục.
        arrow.transform.rotation = rotation * Quaternion.Euler(modelRotationOffset);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        else
        {
            Debug.LogError("[ArrowPool] Arrow prefab thiếu Rigidbody!");
        }

        arrow.SetActive(true); // → Arrow.OnEnable sẽ set velocity theo transform.forward
        return arrow;
    }

    public void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        availableArrows.Enqueue(arrow);
    }
}