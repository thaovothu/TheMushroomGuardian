using UnityEngine;

/// <summary>
/// Di chuyển NPC đến vị trí mới khi objective reached.
/// NPC đã có sẵn trên scene — không spawn mới.
///
/// Setup:
///   1. Gán npcObject = Chiến Binh Rêu GameObject
///   2. Gán targetPosition = vị trí mới (empty GameObject)
///   3. Kéo vào QuestObjectiveManager.npcDialogEntries.npcMover
/// </summary>
public class QuestNPCMover : MonoBehaviour
{
    [Tooltip("NPC đã có sẵn trên scene")]
    [SerializeField] private GameObject npcObject;

    [Tooltip("Vị trí mới NPC sẽ di chuyển đến")]
    [SerializeField] private Transform targetPosition;

    /// <summary>
    /// Gọi từ QuestObjectiveManager khi player đến objective.
    /// Di chuyển NPC đến targetPosition.
    /// </summary>
    public void MoveToTarget()
    {
        if (npcObject == null)
        {
            Debug.LogWarning("[QuestNPCMover] npcObject chưa gán!");
            return;
        }

        if (targetPosition == null)
        {
            Debug.LogWarning("[QuestNPCMover] targetPosition chưa gán!");
            return;
        }

        npcObject.transform.position = targetPosition.position;
        npcObject.transform.rotation = targetPosition.rotation;
        npcObject.SetActive(true);

        Debug.Log($"[QuestNPCMover] {npcObject.name} moved to {targetPosition.position}");
    }
}