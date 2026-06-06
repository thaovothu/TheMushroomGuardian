using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý spawn enemy theo quest step.
/// - Lắng nghe OnStepChanged / OnQuestChanged từ QuestProgressManager
/// - Khi step mới được unlock → tìm QuestSpawnConfig theo spawnGroupId trong QuestData
/// - Spawn đúng nhóm enemy đó qua SimpleObjectPool
/// 
/// Setup:
///   1. Tạo QuestSpawnConfig asset cho mỗi step cần spawn (Right-click > Create > Quest > Quest Spawn Config)
///   2. Điền spawnGroupId khớp với cột SpawnGroupID trong quest.tsv
///   3. Kéo tất cả QuestSpawnConfig assets vào danh sách questSpawnConfigs
///   4. (Tuỳ chọn) Gán defaultSpawnPoints nếu config không tự có spawnPoints riêng
/// </summary>
public class QuestSpawnManager : BaseSingleton<QuestSpawnManager>
{
    private struct ActiveSpawnInfo
    {
        public int questId;
        public int stepId;
        public string groupId;
        public GameObject prefab; // pool key so we can release on respawn
    }

    [Header("Spawn Configs - kéo tất cả QuestSpawnConfig assets vào đây")]
    [SerializeField] private List<QuestSpawnConfig> questSpawnConfigs = new List<QuestSpawnConfig>();

    [Header("Fallback spawn points dùng khi config không có spawnPoints riêng")]
    [SerializeField] private Transform[] defaultSpawnPoints;

    [Header("Parent chứa các enemy đã spawn (giống DataEnemy trong PoolSpawnManager)")]
    [SerializeField] private GameObject enemyContainer;

    [Header("Kích thước khởi tạo pool cho mỗi prefab")]
    [SerializeField] private int initialPoolSize = 5;

    // Lookup nhanh: spawnGroupId → config
    private Dictionary<string, QuestSpawnConfig> configMap = new Dictionary<string, QuestSpawnConfig>();

    // Pool riêng của manager này: prefab → pool
    private Dictionary<GameObject, SimpleObjectPool> poolMap = new Dictionary<GameObject, SimpleObjectPool>();

    // Track enemy/boss đã spawn thuộc quest/step nào để complete step khi tất cả chết
    private Dictionary<GameObject, ActiveSpawnInfo> activeSpawnMap = new Dictionary<GameObject, ActiveSpawnInfo>();
    private Dictionary<string, int> aliveCountByQuestStep = new Dictionary<string, int>();

    // Danh sách spawnPoints fallback theo index
    private List<Transform> spawnPointList = new List<Transform>();

    private Dictionary<string, Transform[]> sceneSpawnPoints = new Dictionary<string, Transform[]>();

    private int linearIndex = 0;

    // ── Unity ──────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        GameEvent.Auth.OnLoginSuccess += OnLoginReady;
    }

    private void OnLoginReady(string _)
    {
        GameEvent.Auth.OnLoginSuccess -= OnLoginReady;
        BuildConfigMap();
        if (defaultSpawnPoints != null)
            spawnPointList.AddRange(defaultSpawnPoints);
    }

    private void OnEnable()
    {
        GameEvent.Quest.OnStepChanged += OnStepChanged;
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
    }

    // ── Event Handlers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi khi step mới được mở trong cùng một quest (step 1 → step 2, …)
    /// </summary>
    private void OnStepChanged(int questId, int stepId)
    {
        Debug.Log($"[QuestSpawnManager] OnStepChanged: Quest {questId} → Step {stepId}");
        TrySpawnForStep(questId, stepId);
    }

    /// <summary>
    /// Gọi khi quest mới được mở (quest 1 → quest 2, …)
    /// → Tự động spawn step 1 của quest mới nếu cần
    /// </summary>
    private void OnQuestChanged(int newQuestId)
    {
        Debug.Log($"[QuestSpawnManager] OnQuestChanged: Quest {newQuestId} unlocked, spawning step 1 if needed");
        TrySpawnForStep(newQuestId, 1);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Có thể gọi thủ công để spawn lại một step cụ thể (debug / retry)
    /// </summary>
    public void ForceSpawnForStep(int questId, int stepId)
    {
        TrySpawnForStep(questId, stepId);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private void TrySpawnForStep(int questId, int stepId)
    {
        // Lấy quest data để đọc SpawnGroupID
        var questData = QuestDataManager.Instance?.GetQuestStep(questId, stepId);
        if (questData == null)
        {
            Debug.LogWarning($"[QuestSpawnManager] QuestData not found for Quest {questId} Step {stepId}");
            return;
        }

        string groupId = questData.spawnGroupId;

        // Step này không cần spawn (waypoint, dialog, collect, …)
        if (string.IsNullOrEmpty(groupId))
        {
            Debug.Log($"[QuestSpawnManager] Quest {questId} Step {stepId}: no SpawnGroupID → skip spawn");
            return;
        }

        if (!configMap.TryGetValue(groupId, out var config))
        {
            Debug.LogWarning($"[QuestSpawnManager] SpawnGroupID '{groupId}' not found in questSpawnConfigs list!");
            return;
        }

        Debug.Log($"[QuestSpawnManager] Spawning group '{groupId}' for Quest {questId} Step {stepId}");
        SpawnGroup(questId, stepId, config);
    }

    private void SpawnGroup(int questId, int stepId, QuestSpawnConfig config)
    {
        // Ưu tiên: scene points (dynamic) → config points (asset) → default fallback (Inspector)
        var points = new List<Transform>();

        if (sceneSpawnPoints.TryGetValue(config.spawnGroupId, out var scenePts))
        {
            points.AddRange(scenePts);
            Debug.Log($"[QuestSpawnManager] Using {scenePts.Length} SCENE points for group '{config.spawnGroupId}'");
        }
        // else if (config.spawnPoints != null && config.spawnPoints.Length > 0)
        // {
        //     points.AddRange(config.spawnPoints);
        //     Debug.Log($"[QuestSpawnManager] Using {config.spawnPoints.Length} CONFIG points for group '{config.spawnGroupId}'");
        // }
        // else
        // {
        //     points.AddRange(spawnPointList);
        //     Debug.Log($"[QuestSpawnManager] Using {spawnPointList.Count} DEFAULT points for group '{config.spawnGroupId}'");
        // }

        if (points.Count == 0)
        {
            Debug.LogError($"[QuestSpawnManager] No spawn points for group '{config.spawnGroupId}'!");
            return;
        }
        
        WarmUpPools(config);

        int pointIndex = 0;
        int spawned = 0;

        foreach (var cfg in config.enemyConfigs)
        {
            if (!ValidateConfig(cfg)) continue;

            for (int i = 0; i < cfg.count; i++)
            {
                int level = (cfg.level != null && cfg.level.Length > 0)
                    ? cfg.level[i % cfg.level.Length]
                    : 1;

                Transform spawnPt = points[pointIndex % points.Count];
                pointIndex++;

                if (SpawnOne(cfg.baseEnemySO, level, spawnPt, questId, stepId, config.spawnGroupId))
                    spawned++;
            }
        }

        Debug.Log($"[QuestSpawnManager] Group '{config.spawnGroupId}': spawned {spawned} enemies");
    }

    private void WarmUpPools(QuestSpawnConfig config)
    {
        Transform parent = enemyContainer != null ? enemyContainer.transform : transform;

        foreach (var cfg in config.enemyConfigs)
        {
            if (!ValidateConfig(cfg)) continue;
            var variant = cfg.baseEnemySO.enemyVariants;
            if (variant?.enemyPrefab != null && !poolMap.ContainsKey(variant.enemyPrefab))
            {
                poolMap[variant.enemyPrefab] = new SimpleObjectPool(variant.enemyPrefab, initialPoolSize, parent);
                Debug.Log($"[QuestSpawnManager] Pool created: {variant.enemyPrefab.name} x{initialPoolSize}");
            }
        }
    }

    public void RegisterSceneSpawnPoints(string groupId, Transform[] points)
    {
        sceneSpawnPoints[groupId] = points;
        // [DEBUG SPAWN POS] Xác nhận đã nhận đúng vị trí từ SceneRegistry
        Debug.Log($"[QuestSpawnManager] Registered {points?.Length ?? 0} scene points for group '{groupId}':");
        if (points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                var pt = points[i];
                Debug.Log($"  └─ [{i}] {(pt == null ? "NULL" : $"'{pt.name}' world={pt.position}")}");
            }
        }
    }

    private bool SpawnOne(BaseEnemySO enemySOData, int level, Transform spawnPoint, int questId, int stepId, string groupId)
    {
        var variant = enemySOData.GetRandomEnemyVariant();
        if (variant?.enemyPrefab == null)
        {
            Debug.LogWarning($"[QuestSpawnManager] No valid prefab in {enemySOData.name}");
            return false;
        }

        if (!poolMap.TryGetValue(variant.enemyPrefab, out var pool))
        {
            Debug.LogWarning($"[QuestSpawnManager] Pool not found for {variant.enemyPrefab.name}");
            return false;
        }

        // [DEBUG SPAWN POS] vị trí spawnPoint truyền vào
        Debug.Log($"[QuestSpawnManager.SpawnOne] group='{groupId}' spawnPoint='{spawnPoint.name}' world={spawnPoint.position}  parent='{(spawnPoint.parent != null ? spawnPoint.parent.name : "<root>")}'");

        var go = pool.Get(spawnPoint);

        // [DEBUG SPAWN POS] sau pool.Get (đã set position bên trong pool)
        Debug.Log($"  └─ after pool.Get: {go.name} at {go.transform.position}  parent='{(go.transform.parent != null ? go.transform.parent.name : "<root>")}'");

        // Force set position & rotation để chắc chắn spawn đúng vị trí
        go.transform.position = spawnPoint.position;
        go.transform.rotation = spawnPoint.rotation;

        // [DEBUG SPAWN POS] sau khi force set
        Debug.Log($"  └─ after force-set: {go.name} at {go.transform.position}  (expected={spawnPoint.position})  match={(go.transform.position == spawnPoint.position)}");
        
        var data = enemySOData.GetEnemyData(level);
        RegisterSpawnedEnemy(go, questId, stepId, groupId, variant.enemyPrefab);

        if (go.TryGetComponent<EnemyController>(out var ec))
        {
            ec.OnSpawn(data);
            // Debug.Log($"[QuestSpawnManager] After OnSpawn (EnemyController): {go.name} at {go.transform.position}");
            return true;
        }

        if (go.TryGetComponent<BossBlackboard>(out var bb))
        {
            bb.OnSpawn(data);
            Debug.Log($"[QuestSpawnManager] After OnSpawn (BossBlackboard): {go.name} at {go.transform.position}");
            return true;
        }

        Debug.LogWarning($"[QuestSpawnManager] {go.name} missing EnemyController or BossBlackboard!");
        return false;
    }

    /// <summary>
    /// Được gọi khi enemy/boss chuẩn bị chết và được trả về pool.
    /// </summary>
    public void NotifySpawnedEnemyDied(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return;
        }

        if (!activeSpawnMap.TryGetValue(enemyObject, out var spawnInfo))
        {
            return;
        }

        activeSpawnMap.Remove(enemyObject);

        string stepKey = GetStepKey(spawnInfo.questId, spawnInfo.stepId);
        if (!aliveCountByQuestStep.TryGetValue(stepKey, out var aliveCount))
        {
            return;
        }

        aliveCount = Mathf.Max(0, aliveCount - 1);
        aliveCountByQuestStep[stepKey] = aliveCount;

        Debug.Log($"[QuestSpawnManager] Enemy died for Quest {spawnInfo.questId} Step {spawnInfo.stepId} ({spawnInfo.groupId}). Remaining alive: {aliveCount}");

        if (aliveCount != 0 || QuestProgressManager.Instance == null || QuestDataManager.Instance == null)
        {
            return;
        }

        if (!QuestProgressManager.Instance.IsQuestActive(spawnInfo.questId) ||
            !QuestProgressManager.Instance.IsStepActive(spawnInfo.questId, spawnInfo.stepId))
        {
            return;
        }

        var questSteps = QuestDataManager.Instance.GetQuestSteps(spawnInfo.questId);
        int maxStepId = questSteps.Count;

        Debug.Log($"[QuestSpawnManager] All enemies for Quest {spawnInfo.questId} Step {spawnInfo.stepId} defeated -> complete step");
        QuestProgressManager.Instance.CompleteCurrentStep(spawnInfo.questId, spawnInfo.stepId, maxStepId);
    }

    private void RegisterSpawnedEnemy(GameObject enemyObject, int questId, int stepId, string groupId, GameObject prefab)
    {
        if (enemyObject == null)
        {
            return;
        }

        activeSpawnMap[enemyObject] = new ActiveSpawnInfo
        {
            questId = questId,
            stepId = stepId,
            groupId = groupId,
            prefab = prefab
        };

        string stepKey = GetStepKey(questId, stepId);
        if (!aliveCountByQuestStep.ContainsKey(stepKey))
        {
            aliveCountByQuestStep[stepKey] = 0;
        }

        aliveCountByQuestStep[stepKey]++;
        Debug.Log($"[QuestSpawnManager] Registered {enemyObject.name} for Quest {questId} Step {stepId}. Alive count: {aliveCountByQuestStep[stepKey]}");
    }

    /// <summary>
    /// Despawn tất cả enemy còn sống của step hiện tại và spawn lại từ đầu.
    /// Gọi khi player respawn để reset lại toàn bộ encounter.
    /// </summary>
    public void ResetStepForRespawn(int questId, int stepId)
    {
        string stepKey = GetStepKey(questId, stepId);
        var toRemove = new List<GameObject>();

        foreach (var kvp in activeSpawnMap)
        {
            var info = kvp.Value;
            if (info.questId != questId || info.stepId != stepId) continue;

            toRemove.Add(kvp.Key);
            var go = kvp.Key;
            if (go == null) continue;

            if (info.prefab != null && poolMap.TryGetValue(info.prefab, out var pool))
                pool.Release(go);
            else
                go.SetActive(false);
        }

        foreach (var go in toRemove)
            activeSpawnMap.Remove(go);

        aliveCountByQuestStep.Remove(stepKey);

        Debug.Log($"[QuestSpawnManager] Respawn reset: despawned {toRemove.Count} enemies for Quest {questId} Step {stepId}, re-spawning...");
        TrySpawnForStep(questId, stepId);
    }

    private static string GetStepKey(int questId, int stepId)
    {
        return $"{questId}:{stepId}";
    }

    private void BuildConfigMap()
    {
        configMap.Clear();
        foreach (var cfg in questSpawnConfigs)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.spawnGroupId)) continue;
            if (configMap.ContainsKey(cfg.spawnGroupId))
            {
                Debug.LogWarning($"[QuestSpawnManager] Duplicate spawnGroupId: '{cfg.spawnGroupId}' — skipping");
                continue;
            }
            configMap[cfg.spawnGroupId] = cfg;
        }
        Debug.Log($"[QuestSpawnManager] Built configMap with {configMap.Count} spawn groups");
    }

    private static bool ValidateConfig(MapEnemyConfig cfg)
    {
        if (cfg == null || cfg.baseEnemySO == null || cfg.count <= 0) return false;
        return true;
    }
}
