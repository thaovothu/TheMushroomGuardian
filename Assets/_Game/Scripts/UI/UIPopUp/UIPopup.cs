using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup hiện khi player đến gần NPC.
/// Tự lắng nghe phím T khi đang hiển thị.
/// Bấm T hoặc click button → fire GameEvent.NPC.OnInteract.
/// </summary>
public class UIPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button interactButton;

    [Header("Interact Key")]
    [SerializeField] private KeyCode interactKey = KeyCode.T;

    // Runtime — lưu thông tin NPC hiện tại
    private int currentNpcId = -1;
    private InteractableNPC.InteractionType currentInteractionType;

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
        if (!Input.GetKeyDown(interactKey)) return;
        Interact();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi từ InteractableNPC khi player vào range.
    /// </summary>
    public void Show(string message, int npcId, InteractableNPC.InteractionType interactionType)
    {
        currentNpcId = npcId;
        currentInteractionType = interactionType;

        if (messageText != null)
            messageText.text = message;

        gameObject.SetActive(true);
        Debug.Log($"[UIPopup] Show called for npcId={npcId} type={interactionType}");
    }

    /// <summary>
    /// Gọi từ InteractableNPC khi player rời range.
    /// </summary>
    public void Hide()
    {
        currentNpcId = -1;
        gameObject.SetActive(false);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Interact()
    {
        if (currentNpcId == -1) return;

        Debug.Log($"[UIPopup] Interact npcId={currentNpcId} type={currentInteractionType}");
        GameEvent.NPC.OnInteract?.Invoke(currentNpcId, currentInteractionType);
        Hide();
    }
}