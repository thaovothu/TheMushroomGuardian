using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Debug panel — nhấn H để bật/tắt.
/// Nhập QuestID + StepID → load đúng map → jump thẳng đến step đó.
/// Chỉ dùng trong development.
/// </summary>
public class UIDebugPanel : MonoBehaviour
{
[Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField questIdInput;
    [SerializeField] private TMP_InputField stepIdInput;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button resetIntroButton;
    [SerializeField] private TextMeshProUGUI logText;

    [Header("Toggle Key")]
    [SerializeField] private KeyCode toggleKey = KeyCode.H;

    [Header("Map Scene Names — index = mapId")]
    [Tooltip("Index 0 bỏ trống, index 1 = Map1, index 2 = Map2, ...")]
    [SerializeField] private string[] mapSceneNames = { "", "Map1", "Map2", "Map3", "Map4", "Map5" };

    private int pendingQuestId = -1;
    private int pendingStepId = -1;

    private static UIDebugPanel _instance;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (panel != null) panel.SetActive(false);
        if (jumpButton != null) jumpButton.onClick.AddListener(JumpToStep);
        if (resetIntroButton != null) resetIntroButton.onClick.AddListener(ResetIntro);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            panel?.SetActive(!panel.activeSelf);
    }

    // ── Logic ─────────────────────────────────────────────────────────────────

    private void JumpToStep()
    {
        PlayerPrefs.SetInt("IntroPlayed", 1);
        PlayerPrefs.Save();
        if (!int.TryParse(questIdInput?.text, out int targetQuestId) || targetQuestId < 1)
        {
            Log("❌ QuestID không hợp lệ!"); return;
        }
        if (!int.TryParse(stepIdInput?.text, out int targetStepId) || targetStepId < 1)
        {
            Log("❌ StepID không hợp lệ!"); return;
        }

        var pm = QuestProgressManager.Instance;
        var dm = QuestDataManager.Instance;

        if (pm == null || dm == null)
        {
            Log("❌ Manager null!"); return;
        }

        // Lấy mapId từ step data
        var stepData = dm.GetQuestStep(targetQuestId, targetStepId);
        if (stepData == null)
        {
            Log($"❌ Quest {targetQuestId} Step {targetStepId} không tồn tại!"); return;
        }

        int mapId = stepData.mapId;

        // Force set quest progress
        pm.SetCurrentQuestId(targetQuestId);
        int maxStep = dm.GetMaxStepId(targetQuestId);

        for (int s = 1; s < targetStepId; s++)
        {
            if (pm.GetActiveStepForQuest(targetQuestId) == s)
                pm.ForceCompleteStep(targetQuestId, s, maxStep);
        }

        pm.ForceSetActiveStep(targetQuestId, targetStepId);

        Log($"✅ Quest {targetQuestId} Step {targetStepId} — loading Map {mapId}...");
        panel?.SetActive(false);

        // Load scene tương ứng với mapId
        string currentScene = SceneManager.GetActiveScene().name;
        string targetScene = mapId < mapSceneNames.Length ? mapSceneNames[mapId] : "";

        if (string.IsNullOrEmpty(targetScene))
        {
            Log($"❌ Không tìm thấy scene cho mapId={mapId}!");
            // Vẫn fire event nếu đang ở đúng map
            GameEvent.Quest.OnStepChanged?.Invoke(targetQuestId, targetStepId);
            return;
        }

        if (currentScene == targetScene)
        {
            // Đang ở đúng map → fire event thẳng
            pendingQuestId = -1;
            pendingStepId = -1;
            GameEvent.Quest.OnStepChanged?.Invoke(targetQuestId, targetStepId);
            GrantSkippedRewards(targetQuestId, targetStepId);
            Log($"✅ Jumped to Quest {targetQuestId} Step {targetStepId}");
        }
        else
        {
            // Cần load map mới → lưu pending, fire sau khi scene load xong
            pendingQuestId = targetQuestId;
            pendingStepId = targetStepId;
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingQuestId == -1) return;
        // Subscribe trước rồi mới invoke — đảm bảo FireEventAfterLoading nhận được event
        UILoading.OnLoadingComplete += FireEventAfterLoading;
        // ResourceLoader chỉ fire OnLoadingComplete 1 lần (sau Map1). Với debug jump,
        // UIDebugPanel tự fire để UIIntroSequence và các subscriber khác chạy đúng thứ tự.
        UILoading.OnLoadingComplete?.Invoke();
    }

    private void FireEventAfterLoading()
    {
        UILoading.OnLoadingComplete -= FireEventAfterLoading;
        StartCoroutine(FireNextFrame());
    }

    private IEnumerator FireNextFrame()
    {
        yield return null; // chờ intro/các subscriber khác chạy xong

        int qId = pendingQuestId;
        int sId = pendingStepId;
        pendingQuestId = -1;
        pendingStepId = -1;

        QuestProgressManager.Instance?.ForceSetActiveStep(qId, sId);
        GameEvent.Quest.OnQuestChanged?.Invoke(qId);
        GameEvent.Quest.OnStepChanged?.Invoke(qId, sId);
        GrantSkippedRewards(qId, sId);
        Debug.Log($"[UIDebugPanel] ✅ Fired Quest {qId} Step {sId}");
    }
    private void ResetIntro()
    {
        PlayerPrefs.SetInt("IntroPlayed", 0);
        PlayerPrefs.Save();
        Log("✅ IntroPlayed reset — reload Map1 để xem lại intro");
    }

    private void Log(string msg)
    {
        Debug.Log($"[UIDebugPanel] {msg}");
        if (logText != null) logText.text = msg;
    }

    // ── Skipped reward granting ────────────────────────────────────────────────

    /// <summary>
    /// Tự động nhận tất cả phần thưởng hệ thống (ItemReward, SkillReward) của các step
    /// nằm trước targetStep. Dùng cho debug jump để đảm bảo inventory/skill đúng với tiến trình.
    /// </summary>
    private void GrantSkippedRewards(int targetQuestId, int targetStepId)
    {
        var dm = QuestDataManager.Instance;
        if (dm == null) return;

        int grantedItems = 0, grantedSkills = 0;

        for (int qId = 1; qId <= targetQuestId; qId++)
        {
            var steps = dm.GetQuestSteps(qId);
            if (steps == null) continue;

            foreach (var data in steps)
            {
                // Chỉ grant reward của các step TRƯỚC target (không tính target step)
                if (qId == targetQuestId && data.stepId >= targetStepId) continue;

                if (GrantItemReward(data.itemReward1?.Trim())) grantedItems++;
                if (GrantSkillReward(data.skillReward?.Trim())) grantedSkills++;
                GrantExtendReward(data.reward?.Trim());
            }
        }

        Debug.Log($"[UIDebugPanel] Skipped rewards granted: {grantedItems} item(s), {grantedSkills} skill(s)");
    }

    private bool GrantItemReward(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return false;

        if (int.TryParse(raw, out int coins) && coins > 0)
        {
            UIMoney.Instance?.AddCoin(coins);
            return true;
        }

        if (raw == "Kiếm")
        {
            if (InventorySystem.Instance != null && InventorySystem.Instance.GetItemQuantity(ItemType.Sword) == 0)
                InventorySystem.Instance.AddItem((int)ItemType.Sword);
            return true;
        }

        if (raw == "Cung")
        {
            if (InventorySystem.Instance != null && InventorySystem.Instance.GetItemQuantity(ItemType.Bow) == 0)
                InventorySystem.Instance.AddItem((int)ItemType.Bow);
            return true;
        }

        return false;
    }

    private bool GrantSkillReward(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return false;

        ElementType? element = raw.ToLower() switch
        {
            "skill đất" => ElementType.Earth,
            "skill khí" => ElementType.Wind,
            "skill nước" => ElementType.Water,
            "skill lửa" => ElementType.Fire,
            _ => null
        };

        if (element.HasValue)
        {
            GameEvent.Player.OnSkillUnlocked?.Invoke(element.Value);
            return true;
        }

        // "Dash" — chưa có hệ thống unlock riêng, bỏ qua
        return false;
    }

    private void GrantExtendReward(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;

        if (raw == "Tinh linh")
        {
            GameEvent.Player.OnLumiUnlocked?.Invoke();
            // Lumi là standalone GO — tìm qua FindObjectOfType
            var lumi = FindObjectOfType<LumiController>(true);
            lumi?.UnlockAndFollow();
        }
    }
}