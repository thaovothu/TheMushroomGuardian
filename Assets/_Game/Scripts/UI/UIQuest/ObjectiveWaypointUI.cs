using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Hiển thị waypoint/marker tại vị trí objective (WorldSpace Billboard)
/// - Marker xuất hiện tại vị trí mục tiêu trong thế giới 3D
/// - Face camera tự động (billboard effect)
/// - Hiển thị tên location + khoảng cách
/// </summary>
public class ObjectiveWaypointUI : MonoBehaviour
{
    [SerializeField] private Canvas waypointCanvas;
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private float markerHeight = 3f; // Độ cao marker so với ground

    private QuestObjectiveManager.ObjectiveLocation? currentObjective;
    private float lastUpdateTime = 0f;
    private bool isActive = false;
    private RenderMode originalRenderMode;
    
    // Static subscription - survives component disable/enable
    private static ObjectiveWaypointUI instance;
    
    // Static constructor - subscribes ONCE and NEVER unsubscribes
    static ObjectiveWaypointUI()
    {
        Debug.Log("[ObjectiveWaypointUI] Static constructor - subscribing to PlayerSpawner.OnPlayerSpawned");
        PlayerSpawner.OnPlayerSpawned += HandlePlayerSpawned;
    }
    
    // Static callback - won't be affected by instance enable/disable
    private static void HandlePlayerSpawned(GameObject player)
    {
        Debug.Log($"[ObjectiveWaypointUI] 🎯 Static HandlePlayerSpawned called! Player: {(player != null ? player.name : "NULL")}");
        if (instance != null)
        {
            instance.OnPlayerSpawned(player);
        }
        else
        {
            Debug.LogWarning("[ObjectiveWaypointUI] HandlePlayerSpawned called but instance is null!");
        }
    }

    private void Awake()
    {
        Debug.Log($"[ObjectiveWaypointUI] Awake called");
        instance = this;
        
        // Lưu render mode gốc
        if (waypointCanvas != null)
        {
            originalRenderMode = waypointCanvas.renderMode;
            // Chuyển sang WorldSpace để marker xuất hiện tại vị trí 3D
            waypointCanvas.renderMode = RenderMode.WorldSpace;
            Debug.Log("[ObjectiveWaypointUI] ✅ Canvas chuyển sang WorldSpace");
            
            // Setup RectTransform cho WorldSpace
            RectTransform canvasRect = waypointCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.sizeDelta = new Vector2(400, 150);
                Debug.Log($"[ObjectiveWaypointUI] Canvas RectTransform size: {canvasRect.sizeDelta}");
            }
        }
    }

    private void Start()
    {
        // WorldSpace canvas không cần đặc biệt setup
    }

    private void OnEnable()
    {
        // Cố gắng tìm player nếu đã spawn rồi (fallback)
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"[ObjectiveWaypointUI] ✅ Found already spawned player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[ObjectiveWaypointUI] Player not found yet - waiting for OnPlayerSpawned event");
            }
        }
        
        // Subscribe vào objective events
        if (QuestObjectiveManager.Instance != null)
        {
            QuestObjectiveManager.Instance.OnObjectiveSet += OnObjectiveSet;
            QuestObjectiveManager.Instance.OnObjectiveReached += OnObjectiveReached;
            Debug.Log("[ObjectiveWaypointUI] Subscribed to objective events");
        }
    }

    private void OnPlayerSpawned(GameObject player)
    {
        Debug.Log($"[ObjectiveWaypointUI] 🎯 OnPlayerSpawned callback CALLED!");
        Debug.Log($"[ObjectiveWaypointUI] OnPlayerSpawned event received, player: {(player != null ? player.name : "NULL")}");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"[ObjectiveWaypointUI] ✅ Player spawned callback - set transform: {player.name}");
        }
        else
        {
            Debug.LogWarning("[ObjectiveWaypointUI] OnPlayerSpawned received null player!");
        }
    }

    private void Update()
    {
        if (!isActive || !currentObjective.HasValue || playerTransform == null)
        {
            if (isActive && currentObjective.HasValue && playerTransform == null)
            {
                Debug.LogWarning("[ObjectiveWaypointUI] Waypoint active but playerTransform is NULL!");
            }
            return;
        }

        // Update waypoint mỗi checkInterval
        if (Time.time - lastUpdateTime >= checkInterval)
        {
            UpdateWaypoint();
            lastUpdateTime = Time.time;
        }
    }

    private void OnObjectiveSet(QuestObjectiveManager.ObjectiveLocation objective)
    {
        currentObjective = objective;
        isActive = true;

        // Di chuyển canvas đến vị trí objective trong thế giới 3D
        if (waypointCanvas != null)
        {
            Vector3 markerPos = objective.position + Vector3.up * markerHeight;
            waypointCanvas.transform.position = markerPos;
            waypointCanvas.gameObject.SetActive(true);
            Debug.Log($"[ObjectiveWaypointUI] ✅ Marker positioned at {markerPos} for objective: {objective.name}");
        }

        // Cập nhật tên
        if (objectiveNameText != null)
        {
            objectiveNameText.text = objective.name;
        }

        Debug.Log($"[ObjectiveWaypointUI] Objective set: {objective.name}");
    }

    private void OnObjectiveReached(QuestObjectiveManager.ObjectiveLocation objective)
    {
        Debug.Log($"[ObjectiveWaypointUI] Objective reached: {objective.name}");
        HideWaypoint();
    }

    private void UpdateWaypoint()
    {
        if (!currentObjective.HasValue || waypointCanvas == null || playerTransform == null)
            return;

        Vector3 playerPos = playerTransform.position;
        Vector3 objectivePos = currentObjective.Value.position;

        // Tính khoảng cách
        float distance = Vector3.Distance(playerPos, objectivePos);

        // Update text khoảng cách
        if (distanceText != null)
        {
            distanceText.text = $"{distance:F1}m";
        }

        // Billboard effect - Canvas luôn face camera
        if (Camera.main != null)
        {
            // Tính hướng từ marker đến camera
            Vector3 dirToCamera = (Camera.main.transform.position - waypointCanvas.transform.position).normalized;
            // Rotation để face camera
            waypointCanvas.transform.rotation = Quaternion.LookRotation(dirToCamera);
            
            Debug.Log($"[ObjectiveWaypointUI] Billboard updated: distance={distance:F1}m, facing camera");
        }

        // Debug visualize
        Debug.DrawLine(playerPos, objectivePos, Color.yellow);
    }

    private void HideWaypoint()
    {
        isActive = false;
        if (waypointCanvas != null)
        {
            waypointCanvas.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // DO NOT unsubscribe from PlayerSpawner.OnPlayerSpawned!
        // Static subscription survives disable/enable cycles
        
        if (QuestObjectiveManager.Instance != null)
        {
            QuestObjectiveManager.Instance.OnObjectiveSet -= OnObjectiveSet;
            QuestObjectiveManager.Instance.OnObjectiveReached -= OnObjectiveReached;
        }
    }
}
