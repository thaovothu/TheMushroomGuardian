using UnityEngine;

/// <summary>
/// Quản lý 3D prefab đánh dấu vị trí mục tiêu quest.
/// Tự động spawn khi quest/step mới được mở và có locationName trong infoQuest.
/// Prefab bob lên xuống + billboard về phía camera.
/// Ẩn khi player đến đủ gần (proximity check trong QuestObjectiveManager).
/// Buffer objective nếu được gọi trước khi scene load xong (UILoading.OnLoadingComplete).
/// </summary>
public class WaypointMarker : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("3D GameObject sẽ được spawn tại vị trí mục tiêu")]
    [SerializeField] private GameObject markerPrefab;
    [Tooltip("Độ cao marker so với ObjectiveLocation.position")]
    [SerializeField] private float markerHeight = 3f;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Proximity")]
    [Tooltip("Tần suất kiểm tra khoảng cách player với mục tiêu (giây)")]
    [SerializeField] private float checkInterval = 0.2f;

    private GameObject markerInstance;
    private Vector3 basePosition;
    private Transform playerTransform;
    private bool isActive = false;
    private float lastCheckTime = 0f;

    private bool isSceneLoaded = false;
    private QuestObjectiveManager.ObjectiveLocation? pendingObjective = null;

    private static WaypointMarker instance;

    static WaypointMarker()
    {
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
    }

    private static void OnPlayerSpawned(GameObject player)
    {
        if (instance != null && player != null)
            instance.playerTransform = player.transform;
    }

    private void Awake()
    {
        instance = this;

        if (markerPrefab == null)
            Debug.LogWarning("[WaypointMarker] Chưa gán markerPrefab trong Inspector!");
    }

    private void OnEnable()
    {
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        GameEvent.Quest.OnObjectiveSet += ShowMarker;
        GameEvent.Quest.OnObjectiveReached += HideMarker;
        UILoading.OnLoadingComplete += OnSceneLoadingComplete;

        // Race condition / re-enable fix: nếu objective đã set hoặc scene đã loaded rồi
        var existing = QuestObjectiveManager.Instance?.GetCurrentObjective();
        if (existing.HasValue)
            ShowMarker(existing.Value); // isSceneLoaded=true → SpawnMarker ngay; false → buffer
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnObjectiveSet -= ShowMarker;
        GameEvent.Quest.OnObjectiveReached -= HideMarker;
        UILoading.OnLoadingComplete -= OnSceneLoadingComplete;

        if (markerInstance != null)
            markerInstance.SetActive(false);

        isActive = false;
        // isSceneLoaded và pendingObjective KHÔNG reset ở đây:
        // OnDisable có thể fire do parent bị disable tạm thời trong scene hiện tại.
        // Nếu reset, marker sẽ buffer mãi khi OnEnable lại vì OnLoadingComplete đã fire rồi.
        // Hai field này chỉ reset trong Awake() khi object thực sự mới được tạo.
    }

    private void OnDestroy()
    {
        // Reset khi object thực sự bị destroy (scene unload hoặc Destroy())
        isSceneLoaded = false;
        pendingObjective = null;
    }

    private void OnSceneLoadingComplete()
    {
        isSceneLoaded = true;

        // Ưu tiên pending objective
        if (pendingObjective.HasValue)
        {
            Debug.Log($"[WaypointMarker] Scene loaded — showing buffered objective '{pendingObjective.Value.name}'");
            SpawnMarker(pendingObjective.Value);
            pendingObjective = null;
            return;
        }

        // Không có pending → check xem có objective hiện tại không
        // (trường hợp marker đã spawn nhưng bị destroy do scene transition)
        var existing = QuestObjectiveManager.Instance?.GetCurrentObjective();
        if (existing.HasValue)
        {
            Debug.Log($"[WaypointMarker] Scene loaded — respawning existing objective '{existing.Value.name}'");
            SpawnMarker(existing.Value);
        }
    }

    private void Update()
    {
        if (!isActive || markerInstance == null) return;

        if (Camera.main != null)
            markerInstance.transform.rotation = Camera.main.transform.rotation;

        if (playerTransform != null && Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            if (QuestObjectiveManager.Instance != null)
                QuestObjectiveManager.Instance.CheckObjectiveProximity(playerTransform.position);
        }
    }

    private void ShowMarker(QuestObjectiveManager.ObjectiveLocation objective)
    {
        if (!isSceneLoaded)
        {
            pendingObjective = objective;
            Debug.Log($"[WaypointMarker] Scene not loaded yet — buffering objective '{objective.name}'");
            return;
        }

        SpawnMarker(objective);
    }

    private void SpawnMarker(QuestObjectiveManager.ObjectiveLocation objective)
    {
        if (markerPrefab == null)
        {
            Debug.LogError("[WaypointMarker] markerPrefab là null — không thể spawn!");
            return;
        }

        if (markerInstance == null)
        {
            markerInstance = Instantiate(markerPrefab);
            markerInstance.name = "WaypointMarker_Instance";
        }

        basePosition = objective.position;
        markerInstance.transform.position = basePosition;
        markerInstance.SetActive(true);
        isActive = true;

        Debug.Log($"[WaypointMarker] Spawned at {basePosition} for objective '{objective.name}'");
    }

    private void HideMarker(QuestObjectiveManager.ObjectiveLocation objective)
    {
        isActive = false;
        pendingObjective = null;

        if (markerInstance != null)
            markerInstance.SetActive(false);

        Debug.Log($"[WaypointMarker] Hidden — '{objective.name}' reached");
    }
}