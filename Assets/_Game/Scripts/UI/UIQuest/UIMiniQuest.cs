using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mini quest HUD — hiển thị bước nhiệm vụ hiện tại ngay trên màn hình.
/// Chỉ show title quest + info của step đang active.
/// Bấm nút Detail để mở UIQuestPanel đầy đủ.
/// </summary>
public class UIMiniQuest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private Button openDetailButton;
    [SerializeField] private UIQuestPanel questPanel;

    private void Start()
    {
        GameEvent.Quest.OnStepChanged += OnStepChanged;
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
        GameEvent.Quest.OnStepCompleted += OnStepCompleted;
        GameEvent.Quest.OnDataLoaded += OnQuestDataLoaded;

        if (QuestDataManager.Instance != null && QuestDataManager.Instance.IsDataLoaded)
            RefreshDisplay();

        openDetailButton?.onClick.AddListener(OpenDetail);
    }

    private void OnDestroy()
    {
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        GameEvent.Quest.OnStepCompleted -= OnStepCompleted;
        GameEvent.Quest.OnDataLoaded -= OnQuestDataLoaded;

        openDetailButton?.onClick.RemoveListener(OpenDetail);
    }

    private void OnStepChanged(int questId, int stepId) => RefreshDisplay();
    private void OnQuestChanged(int questId) => RefreshDisplay();
    private void OnStepCompleted(int questId, int stepId) => RefreshDisplay();
    private void OnQuestDataLoaded(System.Collections.Generic.List<QuestData> _) => RefreshDisplay();

    private void RefreshDisplay()
    {
        if (QuestProgressManager.Instance == null || QuestDataManager.Instance == null) return;

        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        var questData = QuestDataManager.Instance.GetQuestStep(questId, stepId);
        if (questData == null) return;

        if (questTitleText != null)
            questTitleText.text = questData.infoQuest;
    }

    private void OpenDetail()
    {
        questPanel?.Show();
    }
}
