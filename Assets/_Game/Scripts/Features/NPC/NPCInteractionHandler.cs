using UnityEngine;

/// <summary>
/// Lắng nghe GameEvent.NPC.OnInteract và mở đúng UI.
/// Đặt trên scene hoặc DataGame.
///
/// Setup:
///   1. Add NPCInteractionHandler vào scene
///   2. Gán uiDialog, uiShop, uiTrade trong Inspector
/// </summary>
public class NPCInteractionHandler : MonoBehaviour
{
    [SerializeField] private UIDialog uiDialog;
    [SerializeField] private UIShop uiShop;
    // [SerializeField] private UITrade uiTrade; // Mở sau khi có UITrade

    private void OnEnable()
    {
        GameEvent.NPC.OnInteract += HandleInteract;
    }

    private void OnDisable()
    {
        GameEvent.NPC.OnInteract -= HandleInteract;
    }

    private void HandleInteract(int npcId, InteractableNPC.InteractionType type)
    {
        switch (type)
        {
            case InteractableNPC.InteractionType.Dialog:
                if (uiDialog != null)
                    uiDialog.PlayDialog(npcId);
                else
                    Debug.LogWarning("[NPCInteractionHandler] UIDialog chưa gán!");
                break;

            case InteractableNPC.InteractionType.Shop:
                if (uiShop != null)
                    uiShop.OpenShop(npcId);
                else
                    Debug.LogWarning("[NPCInteractionHandler] UIShop chưa gán!");
                break;

            case InteractableNPC.InteractionType.Trade:
                Debug.Log("[NPCInteractionHandler] UITrade chưa implement");
                break;
        }
    }
}