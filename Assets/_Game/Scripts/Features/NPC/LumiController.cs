using UnityEngine;

/// <summary>
/// Lumi là GameObject độc lập (DontDestroyOnLoad), tự tìm và bay theo Player.
///
/// Trạng thái:
///   Hidden  — ẩn hoàn toàn (trước quest 1 step 4)
///   Follow  — float theo player (sau khi unlock)
///
/// Natural motion:
///   - SmoothDamp với velocity persistence → quán tính, không cứng
///   - Distance-based smoothTime → lazy gần, bắt kịp nhanh khi xa
///   - 2-layer sine float → chuyển động không đều, tự nhiên
///   - Velocity tilt → nghiêng nhẹ theo chiều bay như tinh linh
///
/// Setup Editor:
///   Đặt LumiNPC trên DataGame prefab (hoặc scene riêng), SetActive = false.
///   Không cần gán playerTransform — tự tìm qua tag "Player" lúc unlock.
/// </summary>
public class LumiController : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("Offset trong world-space phía sau bên phải player")]
    [SerializeField] private Vector3 followOffset = new Vector3(1f, 0.6f, -1f);

    [Header("SmoothDamp Settings")]
    [Tooltip("Smooth bình thường khi gần player — cao hơn = lazy hơn")]
    [SerializeField] private float normalSmoothTime = 0.35f;
    [Tooltip("Smooth khi xa player quá snapDistance — bắt kịp nhanh hơn")]
    [SerializeField] private float fastSmoothTime = 0.08f;
    [Tooltip("Khoảng cách để bắt đầu tăng tốc bắt kịp player")]
    [SerializeField] private float snapDistance = 6f;
    [Tooltip("Tốc độ tối đa (SmoothDamp clamp)")]
    [SerializeField] private float maxFollowSpeed = 8f;

    [Header("Float Animation")]
    [Tooltip("Biên độ lơ lửng lên xuống (layer chính)")]
    [SerializeField] private float floatAmplitude = 0.18f;
    [Tooltip("Tốc độ lơ lửng")]
    [SerializeField] private float floatSpeed = 1.8f;

    [Header("Tilt")]
    [Tooltip("Độ nghiêng tối đa theo hướng di chuyển (degree-ish scale)")]
    [SerializeField] private float tiltStrength = 14f;
    [Tooltip("Tốc độ quay mặt về phía player")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Animator")]
    [SerializeField] private string animIsMoving = "IsMoving";

    // ── Runtime ───────────────────────────────────────────────────────────────

    private enum State { Hidden, Follow }
    private State _state = State.Hidden;

    private Transform _playerTransform;
    private Animator _animator;

    private Vector3 _smoothVelocity = Vector3.zero;
    private float _floatTimer = 0f;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
        GameEvent.Player.OnRespawn += OnPlayerRespawn;
    }

    private void OnDisable()
    {
        GameEvent.Player.OnSpawned -= OnPlayerSpawned;
        GameEvent.Player.OnRespawn -= OnPlayerRespawn;
    }

    private void Update()
    {
        if (_state != State.Follow || _playerTransform == null) return;

        _floatTimer += Time.deltaTime;

        // ── Float offset (2-layer sine) ───────────────────────────────────────
        float yFloat = Mathf.Sin(_floatTimer * floatSpeed) * floatAmplitude
                     + Mathf.Sin(_floatTimer * floatSpeed * 1.37f) * floatAmplitude * 0.35f;

        // ── Target position (world-space offset phía sau bên phải player) ─────
        Vector3 right   = _playerTransform.right;
        Vector3 forward = _playerTransform.forward;
        Vector3 targetPos = _playerTransform.position
                          + right   * followOffset.x
                          - forward * Mathf.Abs(followOffset.z)
                          + Vector3.up * (followOffset.y + yFloat);

        // ── SmoothDamp với distance-based smoothTime ──────────────────────────
        float dist = Vector3.Distance(transform.position, targetPos);
        float t = Mathf.InverseLerp(snapDistance, 0f, dist); // 0 khi xa, 1 khi gần
        float smoothTime = Mathf.Lerp(fastSmoothTime, normalSmoothTime, t);

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos,
            ref _smoothVelocity, smoothTime, maxFollowSpeed);

        // ── Rotation: mặt về phía player + nghiêng theo velocity ─────────────
        Vector3 toPlayer = _playerTransform.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(toPlayer.normalized);

            // Tilt theo hướng di chuyển (cross product velocity × up)
            Vector3 vel = _smoothVelocity;
            vel.y = 0f;
            float tiltAngle = Mathf.Clamp(vel.magnitude * tiltStrength * 0.1f, -30f, 30f);
            Vector3 tiltAxis = Vector3.Cross(vel.normalized, Vector3.up);
            Quaternion tiltRot = tiltAxis.sqrMagnitude > 0.01f
                ? Quaternion.AngleAxis(-tiltAngle, tiltAxis)
                : Quaternion.identity;

            transform.rotation = Quaternion.Slerp(
                transform.rotation, lookRot * tiltRot, rotationSpeed * Time.deltaTime);
        }

        SetAnim(_smoothVelocity.magnitude > 0.3f);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi khi quest 1 step 4 hoàn thành — Lumi unlock và bắt đầu bay theo player.
    /// </summary>
    public void UnlockAndFollow()
    {
        if (_state == State.Follow) return;

        // Tìm player nếu chưa có reference
        if (_playerTransform == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogWarning("[LumiController] Player not found!");
                return;
            }
            _playerTransform = playerGO.transform;
        }

        _state = State.Follow;
        gameObject.SetActive(true);

        // Snap ngay sát player để tránh bay từ vị trí cũ
        SnapToPlayer();

        Debug.Log("[LumiController] Lumi unlocked — bắt đầu follow player");
    }

    /// <summary>Dừng follow tạm thời (dialog, cutscene).</summary>
    public void StopFollow()
    {
        _state = State.Hidden;
        SetAnim(false);
    }

    /// <summary>Tiếp tục follow sau StopFollow.</summary>
    public void ResumeFollow()
    {
        if (_playerTransform == null) return;
        _state = State.Follow;
        SetAnim(true);
    }

    public bool IsFollowing => _state == State.Follow;

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnPlayerSpawned(GameObject playerGO)
    {
        if (playerGO != null)
            _playerTransform = playerGO.transform;
    }

    private void OnPlayerRespawn()
    {
        if (_state != State.Follow || _playerTransform == null) return;
        // Snap về gần player sau khi respawn (tránh Lumi bay từ điểm chết về điểm spawn)
        SnapToPlayer();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SnapToPlayer()
    {
        if (_playerTransform == null) return;
        transform.position = _playerTransform.position
                           + _playerTransform.right * followOffset.x
                           + Vector3.up * followOffset.y;
        _smoothVelocity = Vector3.zero;
    }

    private void SetAnim(bool isMoving)
    {
        if (_animator == null) return;
        _animator.SetBool(animIsMoving, isMoving);
    }
}
