using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Quản lý các mục tiêu của quest (locations, NPCs, ...)
/// Mapping giữa objective name và in-game position
/// Tracking khi player đến gần mục tiêu
/// </summary>
public class QuestObjectiveManager : BaseSingleton<QuestObjectiveManager>
{
    [System.Serializable]
    public struct ObjectiveLocation
    {
        public string name;          // "Làng Rễ Cây"
        public Vector3 position;     // Vị trí trong game
        public float triggerRadius;  // Bán kính để kích hoạt objective
        public int mapId;            // Map ID của location
    }

    [SerializeField] private List<ObjectiveLocation> objectiveLocations = new List<ObjectiveLocation>();
    
    private Dictionary<string, ObjectiveLocation> locationMap = new Dictionary<string, ObjectiveLocation>();
    private ObjectiveLocation? currentObjective = null;

    public event Action<ObjectiveLocation> OnObjectiveSet;      // Trigger khi set objective
    public event Action<ObjectiveLocation> OnObjectiveReached;  // Trigger khi player tới objective

    protected override void Awake()
    {
        base.Awake();
        RebuildLocationMap();
    }

    private void RebuildLocationMap()
    {
        locationMap.Clear();
        foreach (var location in objectiveLocations)
        {
            if (!string.IsNullOrEmpty(location.name))
            {
                locationMap[location.name] = location;
                Debug.Log($"[QuestObjectiveManager] Registered location: {location.name} at {location.position}");
            }
        }
    }

    /// <summary>
    /// Set objective hiện tại - looks up location by quest ID and location name
    /// </summary>
    public void SetObjective(int questId, string locationName)
    {
        Debug.Log($"[QuestObjectiveManager] Attempting to set objective: '{locationName}' from Quest {questId}");
        Debug.Log($"[QuestObjectiveManager] Available locations: {string.Join(", ", locationMap.Keys)}");
        
        if (locationMap.ContainsKey(locationName))
        {
            currentObjective = locationMap[locationName];
            Debug.Log($"[QuestObjectiveManager] ✅ Objective set: {locationName} at {currentObjective.Value.position} (from Quest {questId})");
            OnObjectiveSet?.Invoke(currentObjective.Value);
        }
        else
        {
            Debug.LogWarning($"[QuestObjectiveManager] ❌ Location '{locationName}' not found in locationMap!");
            Debug.LogWarning($"[QuestObjectiveManager] Available locations: {string.Join(", ", locationMap.Keys)}");
            currentObjective = null;
        }
    }

    /// <summary>
    /// Legacy SetObjective - for backward compatibility
    /// </summary>
    public void SetObjective(string locationName)
    {
        SetObjective(-1, locationName);  // -1 means no specific quest context
    }

    /// <summary>
    /// Clear objective hiện tại
    /// </summary>
    public void ClearObjective()
    {
        currentObjective = null;
        Debug.Log("[QuestObjectiveManager] Objective cleared");
    }

    /// <summary>
    /// Lấy objective hiện tại
    /// </summary>
    public ObjectiveLocation? GetCurrentObjective()
    {
        return currentObjective;
    }

    /// <summary>
    /// Check khi player ở gần objective
    /// (Gọi từ player movement script)
    /// </summary>
    public void CheckObjectiveProximity(Vector3 playerPosition)
    {
        if (!currentObjective.HasValue)
            return;

        float distance = Vector3.Distance(playerPosition, currentObjective.Value.position);
        
        if (distance <= currentObjective.Value.triggerRadius)
        {
            Debug.Log($"[QuestObjectiveManager] Objective reached: {currentObjective.Value.name}");
            OnObjectiveReached?.Invoke(currentObjective.Value);
            ClearObjective();
        }
    }

    /// <summary>
    /// Lấy vị trí của một location
    /// </summary>
    public Vector3? GetLocationPosition(string locationName)
    {
        if (locationMap.ContainsKey(locationName))
        {
            return locationMap[locationName].position;
        }
        return null;
    }

    /// <summary>
    /// Debug: In tất cả locations
    /// </summary>
    public void DebugPrintAllLocations()
    {
        Debug.Log($"[QuestObjectiveManager] Total locations: {locationMap.Count}");
        foreach (var loc in locationMap)
        {
            Debug.Log($"  {loc.Key}: {loc.Value.position} (Radius: {loc.Value.triggerRadius}, Map: {loc.Value.mapId})");
        }
    }
}
