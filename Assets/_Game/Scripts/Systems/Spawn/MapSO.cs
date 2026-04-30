 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "EntityData/MapData")]
public class MapSO : EntityData
{
    public List<MapData> mapData;
}

[System.Serializable]
public enum EnemyType
{
    Attacker,
    Defender,
    Exploder,
    Boss
}

[System.Serializable]
public class MapEnemyConfig
{
    public BaseEnemySO baseEnemySO;
    public int count;
    public int[] level;
}

[System.Serializable]
public class MapData
{
    public int levelMap;
    public List<MapEnemyConfig> enemyConfigs;
}