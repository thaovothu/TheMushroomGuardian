using UnityEngine;

/// <summary>
/// Component cho item pickup (worldspace).
/// Khi player nhặt → fire GameEvent.Item.OnItemPickedUp để QuestCollectTracker đếm.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private int itemId;
    [SerializeField] private int amount = 1;

    private float pickupDelay = 0.5f;
    private float pickupTimer;
    private Rigidbody rb;
    private bool isPickedUp = false;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetupPhysics();
    }

    private void Start()
    {
        pickupTimer = pickupDelay;
    }

    private void Update()
    {
        if (pickupTimer > 0)
            pickupTimer -= Time.deltaTime;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Initialize(int id, int amt)
    {
        itemId = id;
        amount = amt;
        pickupTimer = pickupDelay;
        isPickedUp = false;
    }

    // ── Physics ───────────────────────────────────────────────────────────────

    private void SetupPhysics()
    {
        // Rigidbody
        if (!TryGetComponent(out rb))
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Collider vật lý chính — KHÔNG trigger để item đứng trên mặt đất
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
            // Ignore collision với player để không bị đẩy
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var playerCol = playerObj.GetComponent<Collider>();
                if (playerCol != null)
                    Physics.IgnoreCollision(col, playerCol);
            }
        }

        // Pickup trigger — chỉ tạo 1 lần
        if (transform.Find("PickupTrigger") != null) return;

        var triggerGO = new GameObject("PickupTrigger");
        triggerGO.transform.SetParent(transform, false);
        triggerGO.transform.localPosition = Vector3.zero;
        triggerGO.layer = gameObject.layer;

        var sphere = triggerGO.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 0.75f;

        var handler = triggerGO.AddComponent<PickupTriggerHandler>();
        handler.Setup(this);
    }

    // ── Pickup ────────────────────────────────────────────────────────────────

    public void OnTriggerPickup(Collider other)
    {
        if (isPickedUp) return;
        if (pickupTimer > 0) return;
        if (!other.CompareTag("Player")) return;

        isPickedUp = true;
        PickUp();
    }

    private void PickUp()
    {
        // Thêm vào inventory
        InventorySystem.Instance?.AddItem(itemId, amount);

        // Cập nhật UI coin/exp
        if (itemId == 8) UIMoney.AddCoin(amount);
        else if (itemId == 7) UIMoney.AddExp(amount);

        // Fire event để QuestCollectTracker đếm  ← thiếu dòng này
        GameEvent.Item.OnItemPickedUp?.Invoke(itemId, amount);

        Debug.Log($"[ItemPickup] Picked up itemId={itemId} x{amount}");
        Destroy(gameObject);
    }
}