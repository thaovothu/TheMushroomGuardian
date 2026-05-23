using System.Collections;
using UnityEngine;
/// <summary>
/// Spawn dải VFX particle dẫn đường từ player đến objective.
/// Tự động bật/tắt theo GameEvent.Quest.OnObjectiveSet / OnObjectiveReached.
/// Không cần đặt tay trên scene — tính path động từ playerTransform.
/// </summary>
public class QuestTrailVFX : MonoBehaviour
{
    [Header("Particle")]
    [Tooltip("Prefab ParticleSystem nhỏ — 1 emitter cho mỗi điểm trên trail")]
    [SerializeField] private GameObject trailPointPrefab;
    [Tooltip("Số điểm particle chia đều giữa player và objective")]
    [SerializeField] private int pointCount = 6;
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

    // ── Runtime ──────────────────────────────────────────────────────────────

    private GameObject[] points;
    private ParticleSystem[] particles;
    private Vector3 targetPosition;
    private Transform playerTransform;
    private bool isActive = false;
    private float lastUpdateTime = 0f;
    private Coroutine spawnCoroutine;

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

        // Re-enable fix: nếu đã có objective đang active
        var existing = QuestObjectiveManager.Instance?.GetCurrentObjective();
        if (existing.HasValue)
            OnObjectiveSet(existing.Value);
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnObjectiveSet -= OnObjectiveSet;
        GameEvent.Quest.OnObjectiveReached -= OnObjectiveReached;

        HideTrail();
    }

    private void Update()
    {
        if (!isActive || playerTransform == null) return;

        // Wave effect — mỗi điểm bob lên xuống lệch pha nhau
        if (enableWave && points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null || !points[i].activeSelf) continue;
                Vector3 pos = points[i].transform.position;
                float phase = i * (Mathf.PI / pointCount);
                pos.y = GetBaseY(i) + Mathf.Sin(Time.time * waveSpeed + phase) * waveAmplitude;
                points[i].transform.position = pos;
            }
        }

        // Update trail positions theo player
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateTrailPositions();
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
            Debug.LogWarning("[QuestTrailVFX] Không tìm thấy Player — trail không thể spawn.");
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
    /// Spawn từng điểm lần lượt (stagger) để tạo hiệu ứng xuất hiện đẹp.
    /// </summary>
    private IEnumerator SpawnTrailSequential()
    {
        EnsurePoolReady();
        isActive = true;

        for (int i = 0; i < pointCount; i++)
        {
            if (points[i] == null) continue;

            Vector3 pos = GetTrailPoint(i);
            points[i].transform.position = pos;
            points[i].SetActive(true);

            if (particles[i] != null)
                particles[i].Play();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Tính lại vị trí tất cả điểm trail theo vị trí player hiện tại.
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
        float t = Mathf.Lerp(0.15f, 0.9f, (float)index / (pointCount - 1));
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
        float t = Mathf.Lerp(0.15f, 0.9f, (float)index / (pointCount - 1));
        return Mathf.Lerp(playerTransform.position.y, targetPosition.y, t) + pointHeight;
    }

    /// <summary>
    /// Tạo hoặc recycle pool điểm trail.
    /// </summary>
    private void EnsurePoolReady()
    {
        if (points != null && points.Length == pointCount) return;

        // Dọn pool cũ
        if (points != null)
        {
            foreach (var p in points)
                if (p != null) Destroy(p);
        }

        points = new GameObject[pointCount];
        particles = new ParticleSystem[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            points[i] = Instantiate(trailPointPrefab, transform);
            points[i].SetActive(false);
            particles[i] = points[i].GetComponentInChildren<ParticleSystem>();
        }
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