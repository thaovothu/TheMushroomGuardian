using UnityEngine;

public class QuestPortalSpawner : MonoBehaviour
{
    [Tooltip("GameObject cổng không gian đã đặt sẵn trên scene")]
    [SerializeField] private GameObject portalObject;

    [Tooltip("Hiện cổng khi quest này được mở (OnQuestChanged)")]
    [SerializeField] private int activateOnQuest = 2;
    
    private void OnEnable()
    {
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        GameEvent.Quest.OnStepChanged  -= OnStepChanged;
    }

    private void OnQuestChanged(int newQuestId)
    {         // chế độ step-based → bỏ qua
        if (newQuestId != activateOnQuest) return;
        TryActivate();
    }

    private void OnStepChanged(int questId, int stepId)
    {
        if (questId != activateOnQuest) return;
        TryActivate();
    }

    private void TryActivate()
    {
        if (portalObject == null)
        {
            Debug.LogWarning("[QuestPortalSpawner] portalObject chưa được gán!");
            return;
        }

        portalObject.SetActive(true);
    }
}