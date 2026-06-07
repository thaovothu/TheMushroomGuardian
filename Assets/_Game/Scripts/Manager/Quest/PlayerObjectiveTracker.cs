using UnityEngine;

/// <summary>
/// Gắn lên Player GameObject
/// - Track player position
/// - Check nếu player tới objective
/// - Trigger quest completion event
/// </summary>
public class PlayerObjectiveTracker : MonoBehaviour
{
    [SerializeField] private float checkInterval = 0.5f; // Check mỗi 0.5 giây
    private float lastCheckTime = 0f;

    private void Update()
    {
        // Check proximity đến objective mỗi checkInterval
        if (Time.time - lastCheckTime >= checkInterval)
        {
            if (QuestObjectiveManager.Instance != null)
            {
                QuestObjectiveManager.Instance.CheckObjectiveProximity(transform.position);
            }
            lastCheckTime = Time.time;
        }
    }
}
