using UnityEngine;

/// <summary>
/// Chỉ lo show/hide UIPopup khi player vào/ra range.
/// - Lock: chưa đủ điều kiện quest → icon ổ khoá
/// - Unlock: đủ điều kiện → icon chìa khoá → nhấn 1 lần → complete step → đã mở
/// - Đã mở: không hiện popup nữa
/// </summary>
public class InteractableNPC : MonoBehaviour
{
    public enum InteractionType { Dialog, Shop, Trade, Unlock }

    [Header("NPC Config")]
    [SerializeField] private int npcId;
    [SerializeField] private InteractionType interactionType = InteractionType.Dialog;
    [SerializeField] private string npcName = "NPC";

    [Header("Popup")]
    [SerializeField] private string interactMessage = "Bấm T để tương tác";
    [SerializeField] private string popupPanelName = "UIPopUp";

    [Header("Lock Config — để trống nếu NPC luôn mở")]
    [SerializeField] private int requiredQuestId = 0;
    [SerializeField] private int requiredStepId = 0;
    [SerializeField] private string lockedMessage = "Chưa mở khoá";

    private UIPopup popup;
    private bool isUnlocked = false; // Đã unlock rồi → không hiện popup nữa

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.NPC.OnUnlocked += OnNPCUnlocked;
    }

    private void OnDisable()
    {
        GameEvent.NPC.OnUnlocked -= OnNPCUnlocked;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Đã unlock rồi → không hiện popup
        if (interactionType == InteractionType.Unlock && isUnlocked) return;

        var panelGO = UIManager.Instance?.GetPanel(popupPanelName);
        if (panelGO == null) return;

        popup = panelGO.GetComponent<UIPopup>();
        if (popup == null) return;

        if (IsLocked())
            popup.ShowLocked(lockedMessage);
        else
            popup.Show(interactMessage, npcId, interactionType);

        Debug.Log($"[InteractableNPC] Player entered range of {npcName} (locked={IsLocked()})");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        popup?.Hide();
        popup = null;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnNPCUnlocked(int unlockedNpcId)
    {
        if (unlockedNpcId != npcId) return;
        isUnlocked = true;
        popup?.Hide();
        popup = null;
        Debug.Log($"[InteractableNPC] {npcName} đã được unlock — không hiện popup nữa");
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private bool IsLocked()
    {
        if (requiredQuestId <= 0) return false;
        if (QuestProgressManager.Instance == null) return false;

        int currentQuest = QuestProgressManager.Instance.GetCurrentQuestId();
        int currentStep = QuestProgressManager.Instance.GetActiveStepForQuest(currentQuest);

        if (currentQuest < requiredQuestId) return true;
        if (currentQuest == requiredQuestId && currentStep < requiredStepId) return true;

        return false;
    }
}