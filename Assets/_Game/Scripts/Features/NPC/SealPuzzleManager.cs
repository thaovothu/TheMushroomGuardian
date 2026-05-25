using UnityEngine;
using TMPro;

public class SealPuzzleManager : BaseSingleton<SealPuzzleManager>
{
    [Header("4 viên đá — kéo vào đúng thứ tự cần dash")]
    [SerializeField] private AncientSeal[] seals;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Quest")]
    [SerializeField] private int questId = 3;
    [SerializeField] private int stepId = 4;
    [Header("Settings")]
    [SerializeField] private float maxDistance = 5f; // chỉnh to nhỏ trong Inspector


    private int currentIndex = 0;
    private bool puzzleComplete = false;

    private void OnEnable()
    {
        GameEvent.Player.OnDashUsed += OnDashUsed;
        GameEvent.Quest.OnStepChanged += OnStepChanged;
    }

    private void OnDisable()
    {
        GameEvent.Player.OnDashUsed -= OnDashUsed;
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
    }

    private void OnStepChanged(int qId, int sId)
    {
        // Chỉ active khi đúng step
        if (qId == questId && sId == stepId)
        {
            ResetPuzzle();
            Debug.Log("[SealPuzzle] Puzzle activated!");
        }
    }

    private void HandleDash()
    {
        if (puzzleComplete) return;
        if (QuestProgressManager.Instance?.GetCurrentQuestId() != questId) return;
        if (QuestProgressManager.Instance?.GetActiveStepForQuest(questId) != stepId) return;

        var currentSeal = seals[currentIndex];

        // Player phải đang trong trigger của seal
        if (!currentSeal.PlayerInRange)
        {
            Debug.Log($"[SealPuzzle] Player not in range of seal {currentIndex}");
            return;
        }

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) return;

        var rb = playerGO.GetComponent<Rigidbody>();
        Vector3 dashDir = rb != null
            ? new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized
            : playerGO.transform.forward;

        if (dashDir.magnitude < 0.1f)
            dashDir = playerGO.transform.forward;

        CheckDash(dashDir);
    }
    private void OnDashUsed()
    {
        Debug.Log("=== OnDashUsed START ===");
        if (puzzleComplete) return;

        int currentQuest = QuestProgressManager.Instance?.GetCurrentQuestId() ?? -1;
        int currentStep = QuestProgressManager.Instance?.GetActiveStepForQuest(currentQuest) ?? -1;
        if (currentQuest != questId || currentStep != stepId) return;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) return;

        var currentSeal = seals[currentIndex];
        float dist = Vector3.Distance(playerGO.transform.position, currentSeal.transform.position);

        Debug.Log($"[SealPuzzle] Dist to seal {currentIndex}: {dist:F2} | max: {maxDistance}");

        if (dist > maxDistance)
        {
            Debug.Log($"[SealPuzzle] ✗ Too far");
            return;
        }

        var rb = playerGO.GetComponent<Rigidbody>();
        Vector3 dashDir = rb != null
            ? new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized
            : playerGO.transform.forward;

        if (dashDir.magnitude < 0.1f)
            dashDir = playerGO.transform.forward;

        Debug.Log($"[SealPuzzle] dashDir: {dashDir:F2}");
        CheckDash(dashDir);
    }
    private void CheckDash(Vector3 dashDir)
    {
        var currentSeal = seals[currentIndex];

        // Bỏ check direction — chỉ cần dash trong vùng là break seal
        currentSeal.Break();
        currentIndex++;

        if (hintText != null)
            hintText.text = $"{currentIndex}/{seals.Length} phong ấn đã vỡ!";

        Debug.Log($"[SealPuzzle] Seal {currentIndex - 1} broken! {currentIndex}/{seals.Length}");

        if (currentIndex >= seals.Length)
            CompletePuzzle();
        else
            seals[currentIndex].SetActive(true);
    }

    private void ResetPuzzle()
    {
        currentIndex = 0;
        puzzleComplete = false;

        foreach (var seal in seals)
            seal.Reset();

        // Highlight seal đầu tiên
        seals[0].SetActive(true);

        if (hintText != null)
            hintText.text = "Dash theo hướng mũi tên đang sáng!";
    }

    private void CompletePuzzle()
    {
        puzzleComplete = true;
        if (hintText != null) hintText.text = "Phong ấn tan vỡ!";
        Debug.Log("[SealPuzzle] All seals broken!");

        if (QuestProgressManager.Instance != null && QuestDataManager.Instance != null)
        {
            int maxStep = QuestDataManager.Instance.GetMaxStepId(questId);
            QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStep);
        }
    }
}