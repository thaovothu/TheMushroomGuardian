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
}