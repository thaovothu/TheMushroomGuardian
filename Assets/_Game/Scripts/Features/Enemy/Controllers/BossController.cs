using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BossController - Optional cleanup handler
/// Main initialization now handled by BossBlackboard.OnSpawn()
/// This only handles death cleanup
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BehaviorTree))]
[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(HealthSystem))]
public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] HealthSystem healthSystem;
    [SerializeField] BossBlackboard blackboard;

    void Awake()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (blackboard == null) blackboard = GetComponent<BossBlackboard>();
    }

    public void DieBoss()
    {
        Debug.Log($"[BossController] Boss died, releasing to pool");
        PoolSpawnManager.OnRelease?.Invoke(gameObject);
    }
}

