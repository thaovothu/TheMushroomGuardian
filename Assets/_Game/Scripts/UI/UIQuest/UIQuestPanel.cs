using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Quest Panel - Combine Toggle + Info Display
/// - 6 toggles cho 6 quests (trong ToggleGroup)
/// - Toggle active: sáng, checked
/// - Toggle locked: mờ, disabled
/// - Click toggle → hiển thị quest info
/// - Next/Prev button → duyệt steps
/// </summary>
public class UIQuestPanel : MonoBehaviour
{
    [Header("Toggle Section")]
    [SerializeField] private ToggleGroup questToggleGroup;
    [SerializeField] private Toggle[] questToggles = new Toggle[6];
    [SerializeField] private TextMeshProUGUI[] questToggleTitles = new TextMeshProUGUI[6];
    [SerializeField] private Image[] questToggleImages = new Image[6];
    [SerializeField] private Color activeQuestColor = Color.white;
    [SerializeField] private Color lockedQuestColor = Color.gray;
    
    [Header("Info Section")]
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questInfoText;
    [SerializeField] private TextMeshProUGUI stepDescriptionText;
    [SerializeField] private TextMeshProUGUI rewardCoinText;
    [SerializeField] private TextMeshProUGUI rewardItemText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI mapIdText;
    
    [Header("Navigation")]
    [SerializeField] private Button nextStepButton;
    [SerializeField] private Button prevStepButton;
    [SerializeField] private Button closeUIButton;

    private int currentQuestId = 1;
    private int currentStepId = 1;
    private int maxStepId = 1;

    private void Start()
    {
        // Subscribe vào event
        if (QuestDataManager.Instance != null)
        {
            QuestDataManager.Instance.OnQuestDataLoaded += OnQuestDataLoaded;
            
            // Nếu data đã load rồi, hiển thị ngay
            if (QuestDataManager.Instance.IsDataLoaded)
            {
                InitializeUI();
            }
        }

        // Subscribe vào quest progress change
        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.OnQuestChanged += OnQuestProgressChanged;
        }

        // Setup toggles - ToggleGroup sẽ tự tắt toggle khác
        for (int i = 0; i < 6; i++)
        {
            int questId = i + 1;
            if (questToggles[i] != null)
            {
                // Listener sẽ được call khi toggle được click
                questToggles[i].onValueChanged.AddListener((isOn) => 
                {
                    if (isOn) // Chỉ xử lý khi toggle được turn ON
                    {
                        SelectQuest(questId);
                    }
                });
            }
        }

        // Setup buttons
        
    }
    void OnEnable()
    {
        if (nextStepButton != null)
            nextStepButton.onClick.AddListener(NextStep);

        if (prevStepButton != null)
            prevStepButton.onClick.AddListener(PrevStep);
        if (closeUIButton != null)
            closeUIButton.onClick.AddListener(OnCloseUI);
    }
    void OnDisable()
    {
        if (nextStepButton != null)
            nextStepButton.onClick.RemoveListener(NextStep);

        if (prevStepButton != null)
            prevStepButton.onClick.RemoveListener(PrevStep);
        if (closeUIButton != null)
            closeUIButton.onClick.RemoveListener(OnCloseUI);
    }

    private void OnQuestDataLoaded(System.Collections.Generic.List<QuestData> questList)
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        RefreshQuestToggles();
        SelectQuest(QuestProgressManager.Instance.GetCurrentQuestId());
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
            if (questToggleTitles[i] != null)
            {
                string titleText = QuestDataManager.Instance.GetQuestTitle(questId);
                questToggleTitles[i].text = titleText;
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
                // Không set isOn ở đây - ToggleGroup sẽ handle
            }

            Debug.Log($"[UIQuestPanel] Quest {questId}: Active={isActive}, Unlocked={isUnlocked}");
        }
    }

    /// <summary>
    /// Chọn quest để xem chi tiết
    /// </summary>
    private void SelectQuest(int questId)
    {
        if (!QuestProgressManager.Instance.IsQuestUnlocked(questId))
        {
            Debug.LogWarning($"[UIQuestPanel] Quest {questId} is not unlocked!");
            return;
        }

        currentQuestId = questId;
        currentStepId = 1; // Reset về step 1

        // Hiển thị thông tin quest
        DisplayQuestInfo();
        Debug.Log($"[UIQuestPanel] Selected Quest {questId}");
    }

    /// <summary>
    /// Hiển thị thông tin quest hiện tại
    /// </summary>
    private void DisplayQuestInfo()
    {
        var questData = QuestDataManager.Instance.GetQuestStep(currentQuestId, currentStepId);
        
        if (questData == null)
        {
            Debug.LogWarning($"[UIQuestPanel] Quest {currentQuestId}-{currentStepId} not found!");
            return;
        }

        // Hiển thị title
        if (questTitleText != null)
            questTitleText.text = questData.titleQuest;

        // Hiển thị info
        if (questInfoText != null)
            questInfoText.text = questData.infoQuest;

        // Hiển thị short description
        if (stepDescriptionText != null)
            stepDescriptionText.text = questData.shortDescription;

        // Hiển thị reward
        if (rewardCoinText != null)
            rewardCoinText.text = $"Coin: {questData.coinReward}";

        if (rewardItemText != null)
            rewardItemText.text = $"Item: {questData.itemReward1}";

        if (rewardText != null)
            rewardText.text = $"Reward: {questData.reward}";

        // Hiển thị map ID
        if (mapIdText != null)
            mapIdText.text = $"Map ID: {questData.mapId}";

        // Update step buttons
        var questSteps = QuestDataManager.Instance.GetQuestSteps(currentQuestId);
        maxStepId = questSteps.Count;

        if (prevStepButton != null)
            prevStepButton.interactable = (currentStepId > 1);

        if (nextStepButton != null)
            nextStepButton.interactable = (currentStepId < maxStepId);

        Debug.Log($"[UIQuestPanel] Displaying Quest {currentQuestId} Step {currentStepId}/{maxStepId}");
    }

    /// <summary>
    /// Chuyển sang step tiếp theo
    /// </summary>
    public void NextStep()
    {
        var questSteps = QuestDataManager.Instance.GetQuestSteps(currentQuestId);
        if (currentStepId < questSteps.Count)
        {
            currentStepId++;
            DisplayQuestInfo();
        }
    }

    /// <summary>
    /// Quay lại step trước đó
    /// </summary>
    public void PrevStep()
    {
        if (currentStepId > 1)
        {
            currentStepId--;
            DisplayQuestInfo();
        }
    }

    /// <summary>
    /// Callback khi quest progress thay đổi từ QuestProgressManager
    /// </summary>
    private void OnQuestProgressChanged(int newQuestId)
    {
        Debug.Log($"[UIQuestPanel] Quest progress changed to {newQuestId}");
        RefreshQuestToggles();
        SelectQuest(newQuestId);
    }

    private void OnDestroy()
    {
        if (QuestDataManager.Instance != null)
        {
            QuestDataManager.Instance.OnQuestDataLoaded -= OnQuestDataLoaded;
        }

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.OnQuestChanged -= OnQuestProgressChanged;
        }

        if (nextStepButton != null)
            nextStepButton.onClick.RemoveListener(NextStep);
        
        if (prevStepButton != null)
            prevStepButton.onClick.RemoveListener(PrevStep);

        for (int i = 0; i < 6; i++)
        {
            if (questToggles[i] != null)
                questToggles[i].onValueChanged.RemoveAllListeners();
        }
    }
    private void OnCloseUI()
    {
        gameObject.SetActive(false);
    }
}
