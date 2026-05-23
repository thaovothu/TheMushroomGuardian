using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Icon Lumi nhỏ góc phải màn hình — hiện khi step có LumiHint, ẩn khi không có.
/// Nhấp nháy khi có hint mới, bấm vào hiện/ẩn bubble dialog nhỏ.
///
/// Setup:
///   1. Tạo Canvas (Screen Space Overlay)
///   2. Tạo LumiHintRoot (GameObject, góc phải dưới)
///        ├── LumiIcon (Image — icon Lumi) → gán lumiIcon
///        └── HintBubble (GameObject — ẩn mặc định)
///             └── BubbleText (TextMeshProUGUI) → gán hintText
///   3. Add UILumiHint vào LumiHintRoot
/// </summary>
public class UILumiHint : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject lumiIconRoot;   // Root chứa icon — ẩn/hiện theo hint
    [SerializeField] private Image lumiIcon;            // Icon Lumi để nhấp nháy
    [SerializeField] private Button lumiButton;         // Button bấm vào icon
    [SerializeField] private GameObject hintBubble;     // Bubble dialog nhỏ
    [SerializeField] private TextMeshProUGUI hintText;  // Text trong bubble

    [Header("Pulse Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinAlpha = 0.5f;
    [SerializeField] private float pulseMaxAlpha = 1f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private string currentHint = "";
    private bool isBubbleOpen = false;
    private bool isPulsing = false;
    private Coroutine pulseCoroutine;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.Quest.OnStepChanged += OnStepChanged;
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
        GameEvent.Quest.OnDataLoaded += OnDataLoaded;

        if (lumiButton != null)
            lumiButton.onClick.AddListener(OnIconClicked);

        // Ẩn mặc định
        SetVisible(false);
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        GameEvent.Quest.OnDataLoaded -= OnDataLoaded;

        if (lumiButton != null)
            lumiButton.onClick.RemoveListener(OnIconClicked);
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnStepChanged(int questId, int stepId) => TryShowHint(questId, stepId);
    private void OnQuestChanged(int questId) => TryShowHint(questId, 1);
    private void OnDataLoaded(System.Collections.Generic.List<QuestData> _)
    {
        if (QuestProgressManager.Instance == null) return;
        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        TryShowHint(questId, stepId);
    }

    private void OnIconClicked()
    {
        if (string.IsNullOrEmpty(currentHint)) return;

        isBubbleOpen = !isBubbleOpen;

        if (hintBubble != null)
            hintBubble.SetActive(isBubbleOpen);

        // Dừng nhấp nháy khi đã đọc hint
        if (isBubbleOpen)
            StopPulse();
    }

    // ── Logic ─────────────────────────────────────────────────────────────────

    private void TryShowHint(int questId, int stepId)
    {
        if (QuestDataManager.Instance == null) return;

        var questData = QuestDataManager.Instance.GetQuestStep(questId, stepId);

        // Đóng bubble cũ khi chuyển step
        isBubbleOpen = false;
        if (hintBubble != null) hintBubble.SetActive(false);

        if (questData == null || string.IsNullOrEmpty(questData.lumiHint))
        {
            // Step này không có hint → ẩn icon
            currentHint = "";
            SetVisible(false);
            return;
        }

        // Có hint → hiện icon và nhấp nháy
        currentHint = questData.lumiHint;

        if (hintText != null)
            hintText.text = currentHint;

        SetVisible(true);
        StartPulse();

        Debug.Log($"[UILumiHint] Hint for Quest {questId} Step {stepId}: {currentHint}");
    }

    private void SetVisible(bool visible)
    {
        if (lumiIconRoot != null)
            lumiIconRoot.SetActive(visible);

        if (!visible)
            StopPulse();
    }

    private void StartPulse()
    {
        if (isPulsing) return;
        isPulsing = true;

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseCoroutine());
    }

    private void StopPulse()
    {
        isPulsing = false;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Reset alpha về max
        if (lumiIcon != null)
            lumiIcon.color = new Color(lumiIcon.color.r, lumiIcon.color.g, lumiIcon.color.b, pulseMaxAlpha);
    }

    private IEnumerator PulseCoroutine()
    {
        while (isPulsing)
        {
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

            if (lumiIcon != null)
                lumiIcon.color = new Color(lumiIcon.color.r, lumiIcon.color.g,
                    lumiIcon.color.b, alpha);

            yield return null;
        }
    }
}