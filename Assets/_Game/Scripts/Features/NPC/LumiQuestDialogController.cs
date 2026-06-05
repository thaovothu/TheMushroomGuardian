using UnityEngine;

/// <summary>
/// Hiện hint của Lumi (UIAttention) khi quest step thay đổi.
/// Không blocking — quest advance ngay, UIAttention tự tắt sau 3s.
///
/// Flow:
///   OnQuestAboutToChange → ConfirmQuestAdvance() ngay (không chờ dialog)
///   OnStepChanged(questId, stepId) → tìm dialog theo (questId, stepId) → UIAttention.Show()
/// </summary>
public class LumiQuestDialogController : MonoBehaviour
{
    [SerializeField] private UIAttention uiAttention;

    private void OnEnable()
    {
        GameEvent.Quest.OnQuestAboutToChange += HandleQuestAboutToChange;
        GameEvent.Quest.OnStepChanged += HandleStepChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnQuestAboutToChange -= HandleQuestAboutToChange;
        GameEvent.Quest.OnStepChanged -= HandleStepChanged;
    }

    private void HandleQuestAboutToChange(int nextQuestId)
    {
        QuestProgressManager.Instance?.ConfirmQuestAdvance();
    }

    private void HandleStepChanged(int questId, int stepId)
    {
        if (LumiDialogDataManager.Instance == null || uiAttention == null) return;

        var dialog = LumiDialogDataManager.Instance.GetDialogStep(questId, stepId);
        if (dialog == null) return;

        uiAttention.Show(dialog.text);
    }
}