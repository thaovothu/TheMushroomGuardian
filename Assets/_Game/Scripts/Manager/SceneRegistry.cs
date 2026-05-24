using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnGroupEntry
{
    public string spawnGroupId;
    public Transform[] spawnPoints;
}

[System.Serializable]
public struct NPCSpawnEntry
{
    public int questId;
    public int stepId;              // -1 = spawn cho cả quest, bất kể step
    public GameObject npcPrefab;
    public Transform spawnPoint;
    public bool despawnOnNextStep;  // true = biến mất khi sang step tiếp theo
}

public class SceneRegistry : MonoBehaviour
{
    public enum RegistryType { PlayerSpawnPoint, QuestSpawnGroup, NPCSpawnPoint }

    [SerializeField] private RegistryType registryType;

    [Header("PlayerSpawnPoint")]
    [SerializeField] private Transform spawnPoint;

    [Header("QuestSpawnGroup")]
    [SerializeField] private List<SpawnGroupEntry> spawnGroups;

    [Header("NPCSpawnPoint")]
    [SerializeField] private List<NPCSpawnEntry> npcSpawnEntries;

    // key = index trong npcSpawnEntries, value = GameObject đã spawn
    private Dictionary<int, GameObject> spawnedNPCs = new Dictionary<int, GameObject>();

    private void OnEnable()
    {
        if (registryType == RegistryType.NPCSpawnPoint)
        {
            GameEvent.Quest.OnStepChanged += OnStepChanged;
            GameEvent.Quest.OnQuestChanged += OnQuestChanged;
        }
    }

    private void OnDisable()
    {
        if (registryType == RegistryType.NPCSpawnPoint)
        {
            GameEvent.Quest.OnStepChanged -= OnStepChanged;
            GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        }
    }

    private void Start()
    {
        Register();
    }

    private void Register()
    {
        switch (registryType)
        {
            case RegistryType.PlayerSpawnPoint:
                Transform point = spawnPoint != null ? spawnPoint : transform;
                Debug.Log($"[SceneRegistry] PlayerSpawnPoint: {point.position}");
                PlayerSpawner.Instance?.RegisterSpawnPoint(point);
                break;

            case RegistryType.QuestSpawnGroup:
                foreach (var group in spawnGroups)
                    QuestSpawnManager.Instance?.RegisterSceneSpawnPoints(group.spawnGroupId, group.spawnPoints);
                break;

            case RegistryType.NPCSpawnPoint:
                if (QuestProgressManager.Instance != null)
                {
                    int q = QuestProgressManager.Instance.GetCurrentQuestId();
                    int s = QuestProgressManager.Instance.GetActiveStepForQuest(q);
                    RefreshNPCs(q, s);
                }
                break;
        }
    }

    private void OnStepChanged(int questId, int stepId) => RefreshNPCs(questId, stepId);
    private void OnQuestChanged(int questId) => RefreshNPCs(questId, 1);

    private void RefreshNPCs(int questId, int stepId)
    {
        for (int i = 0; i < npcSpawnEntries.Count; i++)
        {
            var entry = npcSpawnEntries[i];
            bool questMatch = entry.questId == questId;
            bool stepMatch = entry.stepId == -1 || entry.stepId == stepId;
            bool shouldShow = questMatch && stepMatch;

            if (shouldShow)
            {
                TrySpawn(i, entry);
            }
            else if (entry.despawnOnNextStep)
            {
                TryDespawn(i);
            }
        }
    }

    private void TrySpawn(int index, NPCSpawnEntry entry)
    {
        if (spawnedNPCs.ContainsKey(index) && spawnedNPCs[index] != null) return;
        if (entry.npcPrefab == null || entry.spawnPoint == null) return;

        var npc = Instantiate(entry.npcPrefab, entry.spawnPoint.position, entry.spawnPoint.rotation);
        spawnedNPCs[index] = npc;
        Debug.Log($"[SceneRegistry] Spawned NPC '{entry.npcPrefab.name}' (index={index})");
    }

    private void TryDespawn(int index)
    {
        if (!spawnedNPCs.TryGetValue(index, out var npc)) return;
        if (npc != null)
        {
            Destroy(npc);
            Debug.Log($"[SceneRegistry] Despawned NPC (index={index})");
        }
        spawnedNPCs.Remove(index);
    }
}