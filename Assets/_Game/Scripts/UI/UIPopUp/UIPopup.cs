using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup hiện khi player đến gần NPC.
/// Dùng 1 Image duy nhất, swap Sprite theo trạng thái.
/// </summary>
public class UIPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button interactButton;
    [SerializeField] private Image iconImage;

    [Header("Sprites")]
    [SerializeField] private Sprite interactSprite;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Sprite unlockSprite;

    [Header("Interact Key")]
    [SerializeField] private KeyCode interactKey = KeyCode.T;

    private int currentNpcId = -1;
    private InteractableNPC.InteractionType currentInteractionType;
    private bool isLocked = false;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactButton != null)
            interactButton.onClick.AddListener(Interact);
    }

    private void OnDisable()
    {
        if (interactButton != null)
            interactButton.onClick.RemoveListener(Interact);
    }

    private void Update()
    {
        if (isLocked) return;
        if (!Input.GetKeyDown(interactKey)) return;
        Interact();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show(string message, int npcId, InteractableNPC.InteractionType type)
    {
        isLocked = false;
        currentNpcId = npcId;
        currentInteractionType = type;

        if (messageText != null) messageText.text = message;
        if (iconImage != null)
            iconImage.sprite = type == InteractableNPC.InteractionType.Unlock
                ? unlockSprite
                : interactSprite;

        if (interactButton != null) interactButton.interactable = true;
        gameObject.SetActive(true);
    }

    public void ShowLocked(string message)
    {
        isLocked = true;
        currentNpcId = -1;

        if (messageText != null) messageText.text = message;
        if (iconImage != null) iconImage.sprite = lockSprite;
        if (interactButton != null) interactButton.interactable = false;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        isLocked = false;
        currentNpcId = -1;
        gameObject.SetActive(false);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Interact()
    {
        if (isLocked || currentNpcId == -1) return;

        if (currentInteractionType == InteractableNPC.InteractionType.Unlock)
        {
            CompleteCurrentStep();
            // Fire event để InteractableNPC biết đã unlock
            GameEvent.NPC.OnUnlocked?.Invoke(currentNpcId);
            Hide();
            return;
        }

        GameEvent.NPC.OnInteract?.Invoke(currentNpcId, currentInteractionType);
        Hide();
    }

    private void CompleteCurrentStep()
    {
        if (QuestProgressManager.Instance == null || QuestDataManager.Instance == null) return;

        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        int maxStepId = QuestDataManager.Instance.GetMaxStepId(questId);

        Debug.Log($"[UIPopup] Unlock → CompleteStep Quest {questId} Step {stepId}");
        QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStepId);
    }
}