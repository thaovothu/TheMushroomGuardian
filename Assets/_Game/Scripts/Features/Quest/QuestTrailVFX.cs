using System.Collections;
using UnityEngine;

/// <summary>
/// Spawn dải VFX particle dẫn đường từ player đến objective.
/// - Số điểm tự động tính theo khoảng cách thực tế.
/// - Khi player tiến lại gần, các điểm phía sau lưng tắt dần
///   (điểm gần player nhất tắt trước, điểm gần objective tắt sau).
/// </summary>
public class QuestTrailVFX : MonoBehaviour
{
    [Header("Particle")]
    [Tooltip("Prefab ParticleSystem nhỏ — 1 emitter cho mỗi điểm trên trail")]
    [SerializeField] private GameObject trailPointPrefab;

    [Header("Spacing / Count")]
    [Tooltip("Khoảng cách world-unit giữa mỗi điểm trên trail")]
    [SerializeField] private float spacingPerPoint = 3f;
    [Tooltip("Số điểm tối thiểu (dù đứng rất gần objective)")]
    [SerializeField] private int minPoints = 3;
    [Tooltip("Số điểm tối đa (dù đứng rất xa objective)")]
    [SerializeField] private int maxPoints = 20;
    [Tooltip("Độ cao các điểm so với mặt đất")]
    [SerializeField] private float pointHeight = 0.5f;

    [Header("Animation")]
    [Tooltip("Các điểm xuất hiện lần lượt, delay giữa mỗi điểm (giây)")]
    [SerializeField] private float spawnDelay = 0.08f;
    [Tooltip("Tần suất update vị trí trail theo player (giây)")]
    [SerializeField] private float updateInterval = 0.15f;

    [Header("Wave")]
    [Tooltip("Các điểm nhấp nháy theo sóng để tạo cảm giác dẫn đường")]
    [SerializeField] private bool enableWave = true;
    [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private float waveAmplitude = 0.15f;

    [Header("Proximity Fade")]
    [Tooltip(
        "Khi player vào trong vùng này, các điểm gần player sẽ bắt đầu tắt dần.\n" +
        "Nên đặt bằng khoảng (pointCount * spacingPerPoint * 0.5f) hoặc điều chỉnh tay.")]
    [SerializeField] private float fadeStartDistance = 12f;

    // ── Runtime ──────────────────────────────────────────────────────────────

    private GameObject[] points;
    private ParticleSystem[] particles;
    private Vector3 targetPosition;
    private Transform playerTransform;
    private bool isActive = false;
    private float lastUpdateTime = 0f;
    private Coroutine spawnCoroutine;

    // Objective được set trước khi Player kịp spawn → buffer lại, spawn khi Player xuất hiện.
    private QuestObjectiveManager.ObjectiveLocation? pendingObjective = null;

    /// <summary>Số điểm đang dùng trong pool hiện tại.</summary>
    private int pointCount = 0;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (trailPointPrefab == null)
            Debug.LogWarning("[QuestTrailVFX] Chưa gán trailPointPrefab!");
    }

    private void OnEnable()
    {
        GameEvent.Quest.OnObjectiveSet += OnObjectiveSet;
        GameEvent.Quest.OnObjectiveReached += OnObjectiveReached;
        GameEvent.Player.OnSpawned += OnPlayerSpawned;

        // Re-enable fix: nếu đã có objective đang active
        var existing = QuestObjectiveManager.Instance?.GetCurrentObjective();
        if (existing.HasValue)
            OnObjectiveSet(existing.Value);
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnObjectiveSet -= OnObjectiveSet;
        GameEvent.Quest.OnObjectiveReached -= OnObjectiveReached;
        GameEvent.Player.OnSpawned -= OnPlayerSpawned;
        HideTrail();
    }

    private void OnPlayerSpawned(GameObject player)
    {
        if (player != null)
            playerTransform = player.transform;

        // Có objective đang chờ Player → spawn trail ngay bây giờ
        if (pendingObjective.HasValue && playerTransform != null)
        {
            var objective = pendingObjective.Value;
            pendingObjective = null;
            OnObjectiveSet(objective);
        }
    }

    private void Update()
    {
        if (!isActive || playerTransform == null) return;

        // 1. Wave effect — mỗi điểm bob lên xuống lệch pha nhau
        if (enableWave && points != null)
        {
            for (int i = 0; i < pointCount; i++)
            {
                if (points[i] == null || !points[i].activeSelf) continue;
                Vector3 pos = points[i].transform.position;
                float phase = i * (Mathf.PI / pointCount);
                pos.y = GetBaseY(i) + Mathf.Sin(Time.time * waveSpeed + phase) * waveAmplitude;
                points[i].transform.position = pos;
            }
        }

        // 2. Update vị trí trail + proximity fade theo interval
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateTrailPositions();
            UpdateProximityFade();
        }
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnObjectiveSet(QuestObjectiveManager.ObjectiveLocation objective)
    {
        targetPosition = objective.position;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (playerTransform == null)
        {
            // Player chưa spawn — buffer lại, OnPlayerSpawned sẽ spawn trail sau.
            pendingObjective = objective;
            return;
        }

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnTrailSequential());
    }

    private void OnObjectiveReached(QuestObjectiveManager.ObjectiveLocation objective)
    {
        HideTrail();
    }

    // ── Trail Logic ───────────────────────────────────────────────────────────

    /// <summary>
    /// Tính số điểm cần thiết dựa trên khoảng cách hiện tại giữa player và objective.
    /// </summary>
    private int CalculatePointCount()
    {
        if (playerTransform == null) return minPoints;
        float dist = Vector3.Distance(playerTransform.position, targetPosition);
        int count = Mathf.RoundToInt(dist / Mathf.Max(spacingPerPoint, 0.1f));
        return Mathf.Clamp(count, minPoints, maxPoints);
    }

    /// <summary>
    /// Spawn từng điểm lần lượt (stagger). Tái tính pointCount ngay trước khi spawn.
    /// </summary>
    private IEnumerator SpawnTrailSequential()
    {
        // Tính lại số điểm theo khoảng cách hiện tại
        int needed = CalculatePointCount();
        EnsurePoolReady(needed);
        isActive = true;

        for (int i = 0; i < pointCount; i++)
        {
            if (points[i] == null) continue;

            points[i].transform.position = GetTrailPoint(i);
            points[i].SetActive(true);

            if (particles[i] != null)
                particles[i].Play();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Tắt dần các điểm gần player khi khoảng cách nhỏ hơn fadeStartDistance.
    /// Điểm có index nhỏ (gần player) tắt trước; index lớn (gần objective) tắt sau.
    /// Ngưỡng tắt mỗi điểm tỉ lệ tuyến tính với vị trí của nó trên trail.
    /// </summary>
    private void UpdateProximityFade()
    {
        if (points == null || playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, targetPosition);

        for (int i = 0; i < pointCount; i++)
        {
            if (points[i] == null) continue;

            // Điểm i tắt khi dist < fadeStartDistance * (i+1)/pointCount
            // → điểm 0 tắt sớm nhất (dist < fadeStartDistance / pointCount)
            // → điểm cuối tắt muộn nhất (dist < fadeStartDistance)
            float thresholdForPoint = fadeStartDistance * ((float)(i + 1) / pointCount);
            bool shouldBeVisible = dist >= thresholdForPoint;

            if (points[i].activeSelf != shouldBeVisible)
            {
                if (!shouldBeVisible)
                {
                    // Tắt particle trước khi deactivate
                    var ps = particles[i];
                    if (ps != null)
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    points[i].SetActive(false);
                }
                else
                {
                    // Bật lại nếu player lùi xa ra (ví dụ player quay đầu)
                    points[i].SetActive(true);
                    if (particles[i] != null && !particles[i].isPlaying)
                        particles[i].Play();
                }
            }
        }
    }

    /// <summary>
    /// Tính lại vị trí tất cả điểm trail đang visible theo vị trí player hiện tại.
    /// </summary>
    private void UpdateTrailPositions()
    {
        if (points == null) return;

        for (int i = 0; i < pointCount; i++)
        {
            if (points[i] == null || !points[i].activeSelf) continue;
            points[i].transform.position = GetTrailPoint(i);
        }
    }

    /// <summary>
    /// Tính vị trí điểm thứ i trên đường thẳng từ player đến objective.
    /// Điểm 0 gần player nhất, điểm (pointCount-1) gần objective nhất.
    /// </summary>
    private Vector3 GetTrailPoint(int index)
    {
        if (playerTransform == null) return targetPosition;

        // t đi từ ~0.15 đến ~0.9 — không spawn sát player hoặc sát objective
        float t = Mathf.Lerp(0.15f, 0.9f, (float)index / Mathf.Max(pointCount - 1, 1));
        Vector3 pos = Vector3.Lerp(playerTransform.position, targetPosition, t);
        pos.y += pointHeight;
        return pos;
    }

    /// <summary>
    /// Base Y (không wave) tại điểm i — dùng để tính wave offset.
    /// </summary>
    private float GetBaseY(int index)
    {
        if (playerTransform == null) return targetPosition.y + pointHeight;
        float t = Mathf.Lerp(0.15f, 0.9f, (float)index / Mathf.Max(pointCount - 1, 1));
        return Mathf.Lerp(playerTransform.position.y, targetPosition.y, t) + pointHeight;
    }

    /// <summary>
    /// Tạo hoặc recycle pool. Chỉ rebuild nếu size thay đổi.
    /// </summary>
    private void EnsurePoolReady(int needed)
    {
        if (points != null && points.Length >= needed && pointCount == needed) return;

        // Dọn pool cũ nếu cần mở rộng
        if (points != null && points.Length < needed)
        {
            foreach (var p in points)
                if (p != null) Destroy(p);
            points = null;
            particles = null;
        }

        if (points == null)
        {
            points = new GameObject[needed];
            particles = new ParticleSystem[needed];

            for (int i = 0; i < needed; i++)
            {
                points[i] = Instantiate(trailPointPrefab, transform);
                points[i].SetActive(false);
                particles[i] = points[i].GetComponentInChildren<ParticleSystem>();
            }
        }
        else
        {
            // Pool đủ lớn nhưng pointCount thay đổi — tắt những điểm thừa
            for (int i = needed; i < points.Length; i++)
            {
                if (points[i] == null) continue;
                var ps = points[i].GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                points[i].SetActive(false);
            }
        }

        pointCount = needed;
    }

    private void HideTrail()
    {
        isActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (points == null) return;

        foreach (var p in points)
        {
            if (p == null) continue;
            var ps = p.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.SetActive(false);
        }
    }
}