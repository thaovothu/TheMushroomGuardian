using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject định nghĩa nhóm enemy sẽ spawn khi một quest step được unlock.
/// Tạo asset: Right-click > Create > Quest > Quest Spawn Config
/// </summary>
[CreateAssetMenu(fileName = "QuestSpawnConfig", menuName = "Quest/Quest Spawn Config")]
public class QuestSpawnConfig : ScriptableObject
{
    [Tooltip("Phải khớp với cột SpawnGroupID trong quest.tsv (VD: 'spawn_q1s2_cactus')")]
    public string spawnGroupId;

    [Tooltip("Danh sách loại enemy và số lượng cần spawn")]
    public List<MapEnemyConfig> enemyConfigs = new List<MapEnemyConfig>();

    [Tooltip("Các vị trí spawn cho nhóm này. Nếu để trống, dùng spawnPoints mặc định của QuestSpawnManager")]
    public Transform[] spawnPoints;
}