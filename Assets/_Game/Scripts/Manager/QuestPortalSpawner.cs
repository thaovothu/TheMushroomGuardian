using UnityEngine;

/// <summary>
/// Kích hoạt cổng không gian khi quest mới được mở.
/// Cổng đã đặt sẵn trên scene, mặc định SetActive = false.
///
/// Setup:
///   1. Đặt prefab cổng không gian lên scene Map 1, SetActive = false
///   2. Tạo empty GameObject "PortalSpawner"
///   3. Add QuestPortalSpawner
///   4. Gán portal GameObject vào portalObject
///   5. Điền activateOnQuest = 2
/// </summary>
public class QuestPortalSpawner : MonoBehaviour
{
    [Tooltip("GameObject cổng không gian đã đặt sẵn trên scene")]
    [SerializeField] private GameObject portalObject;

    [Tooltip("Hiện cổng khi quest này được mở")]
    [SerializeField] private int activateOnQuest = 2;

    private void OnEnable()
    {
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
    }

    private void OnQuestChanged(int newQuestId)
    {
        if (newQuestId != activateOnQuest) return;

        if (portalObject == null)
        {
            Debug.LogWarning("[QuestPortalSpawner] portalObject chưa được gán!");
            return;
        }

        portalObject.SetActive(true);
        Debug.Log($"[QuestPortalSpawner] Cổng không gian xuất hiện cho Quest {newQuestId}");
    }
}