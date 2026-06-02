using UnityEngine;

/// <summary>
/// Vùng nước của BigBoss phase Nước. Player đứng trong vùng → mất máu liên tục (DoT).
/// Gắn lên prefab có Collider (isTrigger = true). Tự huỷ sau lifetime.
///
/// BigBossAbilities sẽ spawn liên tiếp các vùng này tại vị trí player → player phải né ra.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BossWaterZone : MonoBehaviour
{
    [Tooltip("Damage mỗi tick.")]
    public float damage = 8f;
    [Tooltip("Khoảng cách giữa 2 lần gây damage (giây).")]
    public float tickInterval = 0.5f;
    [Tooltip("Vùng tồn tại bao lâu rồi tự huỷ.")]
    public float lifetime = 4f;
    [Tooltip("Thời gian báo hiệu (telegraph) trước khi vùng bắt đầu gây damage.")]
    public float warmup = 0.5f;
    public ElementType element = ElementType.Water;

    private float _activeTime;   // thời điểm vùng bắt đầu gây damage
    private float _nextTick;

    /// <summary>Gọi ngay sau Instantiate để set thông số.</summary>
    public void Init(float dmg, float life, float tick = 0.5f, float warmupTime = 0.5f)
    {
        damage = dmg;
        lifetime = life;
        tickInterval = tick;
        warmup = warmupTime;
        _activeTime = Time.time + warmup;
        _nextTick = _activeTime;
        Destroy(gameObject, lifetime);
    }

    private void Start()
    {
        // Nếu spawn bằng Instantiate mà không gọi Init → vẫn tự huỷ + chạy được.
        if (_activeTime <= 0f)
        {
            _activeTime = Time.time + warmup;
            _nextTick = _activeTime;
            Destroy(gameObject, lifetime);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < _activeTime || Time.time < _nextTick) return;

        _nextTick = Time.time + tickInterval;
        var hs = other.GetComponentInParent<HealthSystem>();
        if (hs != null) hs.TakeDamage(damage, element);
    }
}
