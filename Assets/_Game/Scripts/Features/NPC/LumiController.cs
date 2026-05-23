using UnityEngine;

/// <summary>
/// Lumi là child của Player prefab — tồn tại xuyên scene.
///
/// Trạng thái:
///   Hidden  — ẩn hoàn toàn (trước step 4)
///   Caged   — hiện tại vị trí chuồng, đứng yên (step 4 đang diễn ra)
///   Follow  — float theo player (step 5+)
///
/// Setup:
///   1. Đặt LumiNPC làm child của Player prefab
///   2. Add LumiController vào LumiNPC
///   3. SetActive = false mặc định
/// </summary>
public class LumiController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Offset so với player khi follow (phía sau bên phải)")]
    [SerializeField] private Vector3 followOffset = new Vector3(0.8f, 0.5f, -0.8f);
    [Tooltip("Tốc độ Lumi di chuyển về vị trí follow")]
    [SerializeField] private float followSpeed = 5f;

    [Header("Float Animation")]
    [Tooltip("Lumi lơ lửng lên xuống nhẹ khi follow")]
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Animator Parameters")]
    [SerializeField] private string animIsMoving = "IsMoving";

    // ── Runtime ───────────────────────────────────────────────────────────────
    private enum State { Hidden, Caged, Follow }
    private State currentState = State.Hidden;

    private Transform playerTransform;
    private Animator animator;
    private float floatTimer = 0f;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        playerTransform = transform.parent;
        animator = GetComponent<Animator>();
        // gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvent.Quest.OnStepCompleted += OnStepCompleted;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepCompleted -= OnStepCompleted;
    }

    private void Update()
    {
        if (currentState != State.Follow || playerTransform == null) return;

        floatTimer += Time.deltaTime * floatSpeed;

        Vector3 targetPos = playerTransform.position +
                            playerTransform.TransformDirection(followOffset) +
                            Vector3.up * Mathf.Sin(floatTimer) * floatAmplitude;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        Vector3 lookDir = playerTransform.position - transform.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);

        SetAnim(true);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi từ QuestNPCSpawner khi step 4 bắt đầu.
    /// Lumi hiện tại vị trí chuồng, đứng yên.
    /// </summary>
    public void ShowAtCage(Vector3 position)
    {
        currentState = State.Caged;
        transform.SetParent(null); // Tách khỏi player để đứng yên tại chuồng
        transform.position = position;
        gameObject.SetActive(true);
        SetAnim(false);
        Debug.Log("[LumiController] Lumi hiện tại chuồng");
    }

    /// <summary>
    /// Gọi sau khi dialog step 5 kết thúc — Lumi bắt đầu float theo player.
    /// </summary>
    public void StartFollow()
    {
        currentState = State.Follow;
        transform.SetParent(playerTransform); // Gắn lại vào player
        Debug.Log("[LumiController] Lumi bắt đầu follow player");
    }

    /// <summary>
    /// Dừng follow tạm thời (cutscene, dialog).
    /// </summary>
    public void StopFollow()
    {
        currentState = State.Caged;
        SetAnim(false);
    }

    /// <summary>
    /// Tiếp tục follow sau StopFollow.
    /// </summary>
    public void ResumeFollow()
    {
        currentState = State.Follow;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnStepCompleted(int questId, int stepId)
    {
        if (questId == 1 && stepId == 4)
            Debug.Log("[LumiController] Step 4 complete — Lumi được giải thoát, chờ dialog");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetAnim(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool(animIsMoving, isMoving);
    }
}