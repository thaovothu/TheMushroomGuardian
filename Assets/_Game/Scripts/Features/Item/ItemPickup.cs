using UnityEngine;

/// <summary>
/// Component cho item pickup (worldspace)
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private int itemId;
    [SerializeField] private int amount = 1;
    
    private float _pickupDelay = 0.5f;  // Delay trước khi có thể pick up
    private float _pickupTimer;
    private Collider _physicsCollider;
    private Rigidbody _rb;

    void Awake()
    {
        SetupPhysics();
    }

    void Start()
    {
        _pickupTimer = _pickupDelay;
    }

    void Update()
    {
        if (_pickupTimer > 0)
        {
            _pickupTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Initialize item with ID và amount
    /// </summary>
    public void Initialize(int id, int amt)
    {
        itemId = id;
        amount = amt;
        _pickupTimer = _pickupDelay;
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        if (_rb == null && !TryGetComponent(out _rb))
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }

        _rb.useGravity = true;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (_physicsCollider == null)
        {
            _physicsCollider = GetComponent<Collider>();
        }

        if (_physicsCollider != null)
        {
            _physicsCollider.isTrigger = false;
        }

        var trigger = transform.Find("PickupTrigger");
        if (trigger == null)
        {
            var triggerObject = new GameObject("PickupTrigger");
            triggerObject.transform.SetParent(transform, false);
            triggerObject.transform.localPosition = Vector3.zero;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            var sphere = triggerObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 0.75f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_pickupTimer > 0) return;
        
        // Check nếu collider là player
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // TODO: Add to player inventory
        // Tạm thời chỉ log
        Debug.Log($"[ItemPickup] Picked up item ID: {itemId}, Amount: {amount}");
        
        // Destroy item
        Destroy(gameObject);
    }
}
