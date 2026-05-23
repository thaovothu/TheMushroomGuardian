using UnityEngine;

/// <summary>
/// Chỉ lo show/hide UIPopup khi player vào/ra range.
/// Mọi logic interact do UIPopup xử lý.
/// </summary>
public class InteractableNPC : MonoBehaviour
{
    public enum InteractionType { Dialog, Shop, Trade }

    [Header("NPC Config")]
    [SerializeField] private int npcId;
    [SerializeField] private InteractionType interactionType = InteractionType.Dialog;
    [SerializeField] private string npcName = "NPC";

    [Header("Popup")]
    [SerializeField] private string interactMessage = "Bấm T để tương tác";
    [SerializeField] private string popupPanelName = "UIPopUp";

    private UIPopup popup;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var panelGO = UIManager.Instance?.GetPanel(popupPanelName);
        if (panelGO != null)
        {
            popup = panelGO.GetComponent<UIPopup>();
            popup?.Show(interactMessage, npcId, interactionType);
        }

        Debug.Log($"[InteractableNPC] Player entered range of {npcName}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        popup?.Hide();
        popup = null;

        Debug.Log($"[InteractableNPC] Player exited range of {npcName}");
    }
}