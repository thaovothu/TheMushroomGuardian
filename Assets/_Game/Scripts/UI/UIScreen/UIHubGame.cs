using UnityEngine;
using UnityEngine.UI;

public class UIHubGame : MonoBehaviour
{
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button questBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private UIInventory uIInventory;
    [SerializeField] private UIQuestPanel questUI;
    [SerializeField] private UIShop shopUI;

    [Header("Shop Unlock")]
    [SerializeField] private int shopNpcId     = 3;
    [SerializeField] private int unlockQuestId = 2;
    [SerializeField] private int unlockStepId  = 3;

    private const string ShopUnlockedKey = "ShopUnlocked";

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Start()
    {
        bool alreadyUnlocked = PlayerPrefs.GetInt(ShopUnlockedKey, 0) == 1;
        shopBtn.gameObject.SetActive(alreadyUnlocked);
    }

    void OnEnable()
    {
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);
        questBtn.onClick.AddListener(OnQuestBtnClicked);
        shopBtn.onClick.AddListener(OnShopBtnClicked);
        GameEvent.NPC.OnInteract       += OnNPCInteract;
        GameEvent.Quest.OnStepChanged  += OnStepChanged;
    }

    void OnDisable()
    {
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);
        questBtn.onClick.RemoveListener(OnQuestBtnClicked);
        shopBtn.onClick.RemoveListener(OnShopBtnClicked);
        GameEvent.NPC.OnInteract       -= OnNPCInteract;
        GameEvent.Quest.OnStepChanged  -= OnStepChanged;
    }

    // ── Unlock logic ──────────────────────────────────────────────────────────

    // Unlock khi player nói chuyện với NPC shop lần đầu
    private void OnNPCInteract(int npcId, InteractableNPC.InteractionType type)
    {
        if (type == InteractableNPC.InteractionType.Shop && npcId == shopNpcId)
            UnlockShop();
    }

    // Fallback: unlock khi load từ server và quest progress đã vượt qua điều kiện
    private void OnStepChanged(int questId, int stepId)
    {
        if (questId > unlockQuestId || (questId == unlockQuestId && stepId >= unlockStepId))
            UnlockShop();
    }

    private void UnlockShop()
    {
        if (shopBtn.gameObject.activeSelf) return;
        PlayerPrefs.SetInt(ShopUnlockedKey, 1);
        PlayerPrefs.Save();
        shopBtn.gameObject.SetActive(true);
        Debug.Log("[UIHubGame] Shop unlocked.");
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnInventoryBtnClicked() => uIInventory.Show();
    private void OnQuestBtnClicked()     => questUI.Show();

    private void OnShopBtnClicked()
    {
        shopUI.OpenShop(shopNpcId);
    }
}
