using UnityEngine;

/// <summary>
/// Container singleton for organizing child singleton managers.
/// Persists across scenes along with all children (ArrowPool, PoolSpawnManager, ItemDropManager, etc.)
/// </summary>
public class DataGame : BaseSingleton<DataGame>
{
    protected override void Awake()
    {
        base.Awake();
        // Persist this object and all children across scene loads
        DontDestroyOnLoad(transform.root.gameObject);
    }
}
