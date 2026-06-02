using UnityEngine;

/// <summary>
/// Chiêu quét sàn của BigBoss phase Lửa: một vòng lửa lan từ tâm boss ra ngoài.
/// Khi vòng quét tới vị trí player:
///   - Nếu player đang BAY (nhảy) hoặc đang DASH (né) → an toàn.
///   - Ngược lại → dính damage cao.
///
/// Visual: scale prefab theo bán kính (giả định mesh vòng bán kính 0.5 ở scale 1).
/// Gắn lên prefab vòng lửa; BigBossAbilities spawn định kỳ trong phase Lửa.
/// </summary>
public class BossFireSweep : MonoBehaviour
{
    [Tooltip("Damage khi player không né được.")]
    public float damage = 35f;
    [Tooltip("Tốc độ lan của vòng (m/giây).")]
    public float expandSpeed = 12f;
    [Tooltip("Bán kính tối đa rồi tự huỷ.")]
    public float maxRadius = 20f;
    [Tooltip("Có scale visual theo bán kính không (tắt nếu VFX tự lo phần phình to).")]
    public bool scaleVisual = true;
    public ElementType element = ElementType.Fire;

    private Transform _player;
    private PlayerController _pc;
    private float _radius;
    private float _prevRadius;
    private bool _hitPlayer;
    private float _baseScaleY = 1f;

    /// <summary>Gọi ngay sau Instantiate.</summary>
    public void Init(Transform player, float dmg, float expand, float max)
    {
        _player = player;
        if (_player != null) _pc = _player.GetComponentInParent<PlayerController>();
        damage = dmg;
        expandSpeed = expand;
        maxRadius = max;
        _radius = 0f;
        _prevRadius = 0f;
        _hitPlayer = false;
        _baseScaleY = transform.localScale.y;
    }

    private void Update()
    {
        _prevRadius = _radius;
        _radius += expandSpeed * Time.deltaTime;

        if (scaleVisual)
            transform.localScale = new Vector3(_radius * 2f, _baseScaleY, _radius * 2f);

        if (!_hitPlayer && _player != null)
        {
            // Khoảng cách phẳng (bỏ qua trục Y) từ tâm vòng tới player.
            Vector3 c = transform.position; c.y = 0f;
            Vector3 p = _player.position; p.y = 0f;
            float dist = Vector3.Distance(c, p);

            // Vòng vừa quét qua player trong frame này.
            if (_prevRadius < dist && dist <= _radius)
            {
                _hitPlayer = true;

                bool airborne = _pc != null && !_pc.IsGrounded;
                bool dodging  = _pc != null && _pc.IsDashing;

                if (!airborne && !dodging)
                {
                    var hs = _player.GetComponentInParent<HealthSystem>();
                    if (hs != null) hs.TakeDamage(damage, element);
                }
            }
        }

        if (_radius >= maxRadius)
            Destroy(gameObject);
    }
}
