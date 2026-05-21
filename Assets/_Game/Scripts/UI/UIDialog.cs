using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image dialogPanel;
    [SerializeField] private Button nextButton; // Nút để next dialog step
    [SerializeField] private CanvasGroup canvasGroup; // Để fade in/out

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float textDisplaySpeed = 0.05f; // Tốc độ hiển thị text

    private List<DialogData> currentDialogSteps = new List<DialogData>();
    private int currentStepIndex = 0;
    private bool isPlaying = false;
    private Coroutine displayCoroutine;

    void OnEnable()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    void OnDisable()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
    }

    /// <summary>
    /// Bắt đầu hiển thị toàn bộ conversation của NPC
    /// </summary>
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

        // LUÔN bật GameObject trước
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log($"[UIDialog] Activated DialogPanel GameObject");
        }

        // Fade in
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            DisplayCurrentStep();
        }
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
        if (currentDialogSteps == null || currentDialogSteps.Count == 0)
            return;

        if (currentStepIndex >= currentDialogSteps.Count)
        {
            // Hết tất cả steps - kết thúc dialog
            EndDialog();
            return;
        }

        var currentDialog = currentDialogSteps[currentStepIndex];

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(DisplayTextWithDuration(currentDialog));
    }

    IEnumerator DisplayTextWithDuration(DialogData dialog)
    {
        dialogText.text = "";

        // Display từng ký tự
        foreach (char c in dialog.text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(textDisplaySpeed);
        }

        // Chờ khoảng thời gian hiển thị
        yield return new WaitForSeconds(dialog.displayDuration);

        // Kiểm tra nếu còn steps tiếp theo
        if (currentStepIndex < currentDialogSteps.Count - 1)
        {
            // Có step tiếp - hiển thị nút next
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // Đây là step cuối cùng - tự động kết thúc
            EndDialog();
        }
    }

    void OnNextButtonClicked()
    {
        if (!isPlaying)
            return;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        // Qua step tiếp
        currentStepIndex++;
        DisplayCurrentStep();
    }

    void EndDialog()
    {
        isPlaying = false;
        currentDialogSteps.Clear();
        currentStepIndex = 0;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        // Fade out
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log("[UIDialog] Dialog ended");
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

    /// <summary>
    /// Kiểm tra xem dialog có đang chạy không
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }

    /// <summary>
    /// Dừng dialog hiện tại
    /// </summary>
    public void StopDialog()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        EndDialog();
    }
}
