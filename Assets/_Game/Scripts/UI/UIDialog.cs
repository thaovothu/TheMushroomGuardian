using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image dialogPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonText; // Text trong button Next
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float textDisplaySpeed = 0.05f;

    [Header("Button Labels")]
    [SerializeField] private string defaultNextLabel = "▶"; // Label mặc định khi không có playerReply

    private List<DialogData> currentDialogSteps = new List<DialogData>();
    private int currentStepIndex = 0;
    private bool isPlaying = false;
    private Coroutine displayCoroutine;

    void OnEnable()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    void OnDisable()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);

        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
    }

    public void PlayDialog(int npcId)
    {
        if (DialogDataManager.Instance == null)
        {
            Debug.LogWarning("[UIDialog] DialogDataManager not initialized!");
            return;
        }

        currentDialogSteps = DialogDataManager.Instance.GetDialogSteps(npcId);
        if (currentDialogSteps.Count == 0)
        {
            Debug.LogWarning($"[UIDialog] No dialogs found for NPC {npcId}");
            return;
        }

        currentStepIndex = 0;
        isPlaying = true;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (canvasGroup != null)
            StartCoroutine(FadeIn());
        else
            DisplayCurrentStep();
    }

    public void PlayLumiDialog(List<LumiDialogData> steps)
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[UIDialog] PlayLumiDialog: empty steps");
            return;
        }

        currentDialogSteps = steps.ConvertAll(l => new DialogData
        {
            npcId = 0,
            dialogStep = l.stepId,
            text = l.text,
            displayDuration = l.displayDuration,
            playerReply = l.playerReply
        });

        currentStepIndex = 0;
        isPlaying = true;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (canvasGroup != null)
            StartCoroutine(FadeIn());
        else
            DisplayCurrentStep();
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        DisplayCurrentStep();
    }

    void DisplayCurrentStep()
    {
        if (currentDialogSteps == null || currentDialogSteps.Count == 0) return;

        if (currentStepIndex >= currentDialogSteps.Count)
        {
            EndDialog();
            return;
        }

        // Ẩn button khi bắt đầu step mới
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        var currentDialog = currentDialogSteps[currentStepIndex];

        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(DisplayTextWithDuration(currentDialog));
    }

    IEnumerator DisplayTextWithDuration(DialogData dialog)
    {
        dialogText.text = "";

        foreach (char c in dialog.text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(textDisplaySpeed);
        }

        yield return new WaitForSeconds(dialog.displayDuration);

        bool isLastStep = currentStepIndex >= currentDialogSteps.Count - 1;

        if (!isLastStep)
        {
            // Hiện button với text phù hợp
            ShowNextButton(dialog.playerReply);
        }
        else
        {
            EndDialog();
        }
    }

    /// <summary>
    /// Hiện button Next — nếu có playerReply thì dùng text đó, không thì dùng label mặc định.
    /// </summary>
    void ShowNextButton(string playerReply)
    {
        if (nextButton == null) return;

        bool hasReply = !string.IsNullOrEmpty(playerReply);

        if (nextButtonText != null)
            nextButtonText.text = hasReply ? playerReply : defaultNextLabel;

        nextButton.gameObject.SetActive(true);
    }

    void OnNextButtonClicked()
    {
        if (!isPlaying) return;

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        currentStepIndex++;
        DisplayCurrentStep();
    }

    void EndDialog()
    {
        isPlaying = false;
        currentDialogSteps.Clear();
        currentStepIndex = 0;

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (canvasGroup != null)
            StartCoroutine(FadeOut());
        else
            gameObject.SetActive(false);

        Debug.Log("[UIDialog] Dialog ended");
        QuestObjectiveManager.Instance?.OnDialogFinished();
        GameEvent.Quest.OnDialogFinished?.Invoke();
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public bool IsPlaying() => isPlaying;

    public void StopDialog()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
        EndDialog();
    }
}