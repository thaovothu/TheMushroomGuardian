using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Quest Panel — chỉ lo hiển thị, KHÔNG xử lý logic complete step.
/// Logic complete step do QuestObjectiveManager và QuestSpawnManager xử lý.
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
    [SerializeField] private Image stateQuestImg;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite inProgressSprite;
    [SerializeField] private Sprite completedSprite;

    private int currentQuestId = 1;
    private int currentStepId = 1;
    private int maxStepId = 1;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        GameEvent.Quest.OnDataLoaded += OnQuestDataLoaded;
        GameEvent.Quest.OnQuestChanged += OnQuestProgressChanged;
        GameEvent.Quest.OnStepChanged += OnStepProgressChanged;
        GameEvent.Quest.OnStepCompleted += OnQuestStepCompleted;

        if (QuestDataManager.Instance != null && QuestDataManager.Instance.IsDataLoaded)
            InitializeUI();

        for (int i = 0; i < 6; i++)
        {
            int questId = i + 1;
            if (questToggles[i] != null)
                questToggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) SelectQuest(questId);
                });
        }
    }

    private void OnEnable()
    {
        if (nextStepButton != null) nextStepButton.onClick.AddListener(NextStep);
        if (prevStepButton != null) prevStepButton.onClick.AddListener(PrevStep);
        if (closeUIButton != null) closeUIButton.onClick.AddListener(OnCloseUI);
    }

    private void OnDisable()
    {
        if (nextStepButton != null) nextStepButton.onClick.RemoveListener(NextStep);
        if (prevStepButton != null) prevStepButton.onClick.RemoveListener(PrevStep);
        if (closeUIButton != null) closeUIButton.onClick.RemoveListener(OnCloseUI);
    }

    private void OnDestroy()
    {
        GameEvent.Quest.OnDataLoaded -= OnQuestDataLoaded;
        GameEvent.Quest.OnQuestChanged -= OnQuestProgressChanged;
        GameEvent.Quest.OnStepChanged -= OnStepProgressChanged;
        GameEvent.Quest.OnStepCompleted -= OnQuestStepCompleted;

        for (int i = 0; i < 6; i++)
            if (questToggles[i] != null)
                questToggles[i].onValueChanged.RemoveAllListeners();
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnQuestDataLoaded(List<QuestData> _) => InitializeUI();

    private void OnQuestProgressChanged(int newQuestId)
    {
        RefreshQuestToggles();
        SelectQuest(newQuestId);
    }

    private void OnStepProgressChanged(int questId, int stepId)
    {
        if (questId == currentQuestId)
        {
            currentStepId = stepId;
            DisplayQuestInfo();
        }
    }

    private void OnQuestStepCompleted(int questId, int stepId)
    {
        // Reward logic đã được QuestRewardManager xử lý — UIQuestPanel chỉ refresh UI.
        if (questId == currentQuestId && stepId == currentStepId)
            DisplayQuestInfo();

        RefreshQuestToggles();
    }

    // ── UI Logic ──────────────────────────────────────────────────────────────

    private void InitializeUI()
    {
        RefreshQuestToggles();
        SelectQuest(QuestProgressManager.Instance.GetCurrentQuestId());
    }

    private void RefreshQuestToggles()
    {
        for (int i = 0; i < 6; i++)
        {
            int questId = i + 1;
            bool isActive = QuestProgressManager.Instance.IsQuestActive(questId);
            bool isUnlocked = QuestProgressManager.Instance.IsQuestUnlocked(questId);

            if (questToggleTitles[i] != null)
                questToggleTitles[i].text = QuestDataManager.Instance.GetQuestTitle(questId);

            if (questToggleImages[i] != null)
                questToggleImages[i].color = isActive ? activeQuestColor : lockedQuestColor;

            if (questToggles[i] != null)
                questToggles[i].interactable = isUnlocked;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("[UIQuestPanel] Show");
    }

    private void SelectQuest(int questId)
    {
        if (!QuestProgressManager.Instance.IsQuestUnlocked(questId)) return;
        currentQuestId = questId;
        currentStepId = 1;
        DisplayQuestInfo();
    }

    private void DisplayQuestInfo()
    {
        var questData = QuestDataManager.Instance.GetQuestStep(currentQuestId, currentStepId);
        if (questData == null) return;

        bool isActive = QuestProgressManager.Instance.IsStepActive(currentQuestId, currentStepId);
        bool isCompleted = QuestProgressManager.Instance.IsStepCompleted(currentQuestId, currentStepId);

        if (questTitleText != null) questTitleText.text = questData.titleQuest;
        if (questInfoText != null) questInfoText.text = questData.infoQuest;
        if (stepDescriptionText != null) stepDescriptionText.text = questData.shortDescription;
        // itemReward1: số = vàng, "Kiếm"/"Cung" = vũ khí
        bool isCoin = int.TryParse(questData.itemReward1, out int coinAmt);
        if (rewardCoinText != null)
            rewardCoinText.text = isCoin && coinAmt > 0 ? $"Vàng: {coinAmt}" : "";
        if (rewardItemText != null)
            rewardItemText.text = !isCoin && !string.IsNullOrEmpty(questData.itemReward1)
                ? $"Vật phẩm: {questData.itemReward1}" : "";
        if (rewardText != null) rewardText.text = questData.reward;
        if (mapIdText != null) mapIdText.text = $"Map ID: {questData.mapId}";

        var steps = QuestDataManager.Instance.GetQuestSteps(currentQuestId);
        maxStepId = steps.Count;

        if (prevStepButton != null) prevStepButton.interactable = currentStepId > 1;
        if (nextStepButton != null) nextStepButton.interactable = currentStepId < maxStepId;

        if (stateQuestImg != null)
            stateQuestImg.sprite = isCompleted ? completedSprite
                                 : isActive ? inProgressSprite
                                               : lockedSprite;
    }

    public void NextStep()
    {
        if (currentStepId < maxStepId) { currentStepId++; DisplayQuestInfo(); }
    }

    public void PrevStep()
    {
        if (currentStepId > 1) { currentStepId--; DisplayQuestInfo(); }
    }

    private void OnCloseUI() => gameObject.SetActive(false);

}