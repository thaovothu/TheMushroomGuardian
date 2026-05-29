using UnityEngine;

public class AncientSeal : MonoBehaviour
{
    [SerializeField] private int sealIndex;
    [SerializeField] private Vector3 requiredDashDir;
    public bool PlayerInRange { get; private set; } = false;
    public int SealIndex => sealIndex;
    public bool IsBroken { get; private set; } = false;

    // Thêm vào AncientSeal — detect tất cả collider vào, không chỉ Player
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[AncientSeal {sealIndex}] OnTriggerEnter: '{other.gameObject.name}' | Tag: '{other.tag}' | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        if (other.CompareTag("Player"))
        {
            PlayerInRange = true;
            Debug.Log($"[AncientSeal {sealIndex}] ✓ Player IN RANGE");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[AncientSeal {sealIndex}] OnTriggerExit: {other.gameObject.name} | Tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            PlayerInRange = false;
            Debug.Log($"[AncientSeal {sealIndex}] Player OUT OF RANGE");
        }
    }

    public void SetActive(bool active)
    {
        if (IsBroken) return;
        Debug.Log($"[AncientSeal {sealIndex}] SetActive: {active}");
    }

    public bool CheckDashDirection(Vector3 dashDir)
    {
        float dot = Vector3.Dot(dashDir.normalized, requiredDashDir.normalized);
        Debug.Log($"[AncientSeal {sealIndex}] CheckDash — dashDir: {dashDir:F2} | required: {requiredDashDir:F2} | dot: {dot:F2} | pass: {dot >= 0.6f}");
        return dot >= 0.6f;
    }

    public void Break()
    {
        Debug.Log($"[AncientSeal {sealIndex}] 💥 BROKEN!");
        IsBroken = true;

        var col = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
    }
    public void Reset()
    {
        Debug.Log($"[AncientSeal {sealIndex}] Reset");
        IsBroken = false;

        var col = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;
    }
}