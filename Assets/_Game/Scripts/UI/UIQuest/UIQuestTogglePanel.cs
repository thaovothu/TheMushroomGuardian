using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Toggle Panel - Hiển thị 6 quest toggles
/// - Quest hiện tại: sáng, checked
/// - Quest sau: mờ nhạt, disabled
/// - Khi click toggle, hiển thị quest info chi tiết
/// </summary>
public class UIQuestTogglePanel : MonoBehaviour
{
    [SerializeField] private Toggle[] questToggles = new Toggle[6];
    [SerializeField] private TextMeshProUGUI[] questTitleTexts = new TextMeshProUGUI[6];
    [SerializeField] private Image[] questToggleImages = new Image[6];
    [SerializeField] private Color activeQuestColor = Color.white;
    [SerializeField] private Color lockedQuestColor = Color.gray;
    
    // Quest info panel
    [SerializeField] private TextMeshProUGUI questInfoTitle;
    [SerializeField] private TextMeshProUGUI questInfoDescription;
    [SerializeField] private TextMeshProUGUI questReward;
    [SerializeField] private Button completeQuestButton;

    private int selectedQuestId = 1;

    private void Start()
    {
        // Subscribe vào event
        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.OnQuestChanged += OnQuestProgressChanged;
        }

        // Setup toggles
        for (int i = 0; i < 6; i++)
        {
            int questId = i + 1; // Quest ID từ 1-6
            if (questToggles[i] != null)
            {
                questToggles[i].onValueChanged.AddListener((isOn) => OnToggleValueChanged(questId, isOn));
            }
        }

        if (completeQuestButton != null)
        {
            completeQuestButton.onClick.AddListener(CompleteCurrentQuest);
        }

        // Khởi tạo UI
        RefreshQuestToggles();
        SelectQuest(QuestProgressManager.Instance.GetCurrentQuestId());
    }

    /// <summary>
    /// Callback khi toggle value thay đổi
    /// </summary>
    private void OnToggleValueChanged(int questId, bool isOn)
    {
        if (isOn)
        {
            SelectQuest(questId);
        }
    }

    /// <summary>
    /// Làm mới trạng thái tất cả toggles
    /// </summary>
    private void RefreshQuestToggles()
    {
        for (int i = 0; i < 6; i++)
        {
            int questId = i + 1;
            bool isActive = QuestProgressManager.Instance.IsQuestActive(questId);
            bool isUnlocked = QuestProgressManager.Instance.IsQuestUnlocked(questId);

            // Cập nhật title
            if (questTitleTexts[i] != null)
            {
                string titleText = GetQuestTitle(questId);
                questTitleTexts[i].text = titleText;
            }

            // Cập nhật màu sắc
            if (questToggleImages[i] != null)
            {
                questToggleImages[i].color = isActive ? activeQuestColor : lockedQuestColor;
            }

            // Cập nhật interactable state
            if (questToggles[i] != null)
            {
                questToggles[i].interactable = isUnlocked;
                // Set checked nếu đang active
                questToggles[i].isOn = isActive;
            }

            Debug.Log($"[UIQuestTogglePanel] Quest {questId}: Active={isActive}, Unlocked={isUnlocked}");
        }
    }

    /// <summary>
    /// Chọn quest để xem chi tiết
    /// </summary>
    private void SelectQuest(int questId)
    {
        if (!QuestProgressManager.Instance.IsQuestUnlocked(questId))
        {
            Debug.LogWarning($"[UIQuestTogglePanel] Quest {questId} is not unlocked!");
            return;
        }

        selectedQuestId = questId;
        DisplayQuestInfo(questId);

        // Set toggle state
        for (int i = 0; i < 6; i++)
        {
            if (questToggles[i] != null)
            {
                questToggles[i].isOn = (i + 1 == questId);
            }
        }

        Debug.Log($"[UIQuestTogglePanel] Selected Quest {questId}");
    }

    /// <summary>
    /// Hiển thị thông tin quest chi tiết
    /// </summary>
    private void DisplayQuestInfo(int questId)
    {
        // Lấy quest data
        var questData = QuestDataManager.Instance.GetQuestStep(questId, 1); // Lấy step 1
        if (questData == null)
        {
            Debug.LogWarning($"[UIQuestTogglePanel] Quest {questId} step 1 not found!");
            return;
        }

        // Hiển thị title
        if (questInfoTitle != null)
            questInfoTitle.text = questData.titleQuest;

        // Hiển thị description
        if (questInfoDescription != null)
            questInfoDescription.text = questData.infoQuest;

        // Hiển thị reward
        if (questReward != null)
        {
            string rewardText = $"Reward: {questData.coinReward} Coin";
            if (!string.IsNullOrEmpty(questData.itemReward1))
                rewardText += $", {questData.itemReward1}";
            questReward.text = rewardText;
        }

        // Cập nhật trạng thái complete button
        if (completeQuestButton != null)
        {
            bool isCurrentQuest = QuestProgressManager.Instance.IsQuestActive(questId);
            completeQuestButton.interactable = isCurrentQuest;
        }

        Debug.Log($"[UIQuestTogglePanel] Displaying Quest {questId} info");
    }

    /// <summary>
    /// Hoàn thành quest hiện tại
    /// </summary>
    private void CompleteCurrentQuest()
    {
        QuestProgressManager.Instance.CompleteCurrentQuest();
    }

    /// <summary>
    /// Callback khi quest progress thay đổi
    /// </summary>
    private void OnQuestProgressChanged(int newQuestId)
    {
        Debug.Log($"[UIQuestTogglePanel] Quest progress changed to {newQuestId}");
        RefreshQuestToggles();
        SelectQuest(newQuestId);
    }

    /// <summary>
    /// Lấy quest title từ QuestDataManager
    /// </summary>
    private string GetQuestTitle(int questId)
    {
        return QuestDataManager.Instance.GetQuestTitle(questId);
    }

    private void OnDestroy()
    {
        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.OnQuestChanged -= OnQuestProgressChanged;
        }

        if (completeQuestButton != null)
            completeQuestButton.onClick.RemoveListener(CompleteCurrentQuest);

        for (int i = 0; i < 6; i++)
        {
            if (questToggles[i] != null)
                questToggles[i].onValueChanged.RemoveAllListeners();
        }
    }
}
