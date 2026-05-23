using UnityEngine;

/// <summary>
/// Hiện Lumi tại vị trí chuồng khi step 4 bắt đầu.
/// Tự tìm LumiController từ Player — không cần kéo tay.
///
/// Setup trên scene Map 1:
///   1. Tạo empty GameObject "NPCSpawner_Lumi"
///   2. Add QuestNPCSpawner
///   3. Tạo empty child "CagePoint" tại vị trí chuồng → gán vào cagePoint
///   4. Điền spawnOnQuest=1, spawnOnStep=4
/// </summary>
public class QuestNPCSpawner : MonoBehaviour
{
    [Header("Cage Position")]
    [Tooltip("Vị trí chuồng trên scene — Lumi sẽ hiện tại đây")]
    [SerializeField] private Transform cagePoint;

    [Header("Quest Trigger")]
    [Tooltip("Hiện Lumi khi bước vào step này")]
    [SerializeField] private int spawnOnQuest = 1;
    [SerializeField] private int spawnOnStep = 4;

    private LumiController lumiController;

    private void OnEnable()
    {
        GameEvent.Quest.OnStepChanged += OnStepChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
    }

    private void OnStepChanged(int questId, int stepId)
    {
        if (questId != spawnOnQuest || stepId != spawnOnStep) return;

        // Tự tìm LumiController từ Player (kể cả inactive children)
        if (lumiController == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                lumiController = player.GetComponentInChildren<LumiController>(true);
        }

        if (lumiController == null)
        {
            Debug.LogWarning("[QuestNPCSpawner] Không tìm thấy LumiController trong Player!");
            return;
        }

        if (cagePoint == null)
        {
            Debug.LogWarning("[QuestNPCSpawner] cagePoint chưa được gán!");
            return;
        }

        lumiController.ShowAtCage(cagePoint.position);
        Debug.Log($"[QuestNPCSpawner] Lumi hiện tại chuồng: {cagePoint.position}");
    }
}