using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image dialogPanel;
    [SerializeField] private Button nextButton; // Nút để next dialog
    [SerializeField] private CanvasGroup canvasGroup; // Để fade in/out

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private DialogSO currentDialog;
    private int currentLineIndex = 0;
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
    /// Bắt đầu hiển thị dialog từ DialogSO
    /// </summary>
    public void PlayDialog(DialogSO dialogSO)
    {
        if (dialogSO == null)
        {
            //Debug.LogWarning("[UIDialog] DialogSO is null!");
            return;
        }

        currentDialog = dialogSO;
        currentLineIndex = 0;
        isPlaying = true;

        // Fade in
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            gameObject.SetActive(true);
            DisplayCurrentLine();
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
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (currentDialog == null)
            return;

        if (currentLineIndex >= currentDialog.GetLineCount())
        {
            // Dialog kết thúc
            EndDialog();
            return;
        }

        DialogLine line = currentDialog.GetLine(currentLineIndex);
        if (line != null)
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            displayCoroutine = StartCoroutine(DisplayTextWithSpeed(line));
        }
    }

    IEnumerator DisplayTextWithSpeed(DialogLine line)
    {
        dialogText.text = "";
        float timeElapsed = 0f;

        // Display từng ký tự
        foreach (char c in line.text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(currentDialog.textDisplaySpeed);
            timeElapsed += currentDialog.textDisplaySpeed;
        }

        // Chờ khoảng thời gian hiển thị
        yield return new WaitForSeconds(line.displayDuration);

        // Tự động next nếu còn dòng
        if (currentLineIndex < currentDialog.GetLineCount() - 1)
        {
            currentLineIndex++;
            DisplayCurrentLine();
        }
        else
        {
            // Dòng cuối cùng - chờ người chơi click
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
        }
    }

    void OnNextButtonClicked()
    {
        if (!isPlaying)
            return;

        currentLineIndex++;
        DisplayCurrentLine();

        // Ẩn next button
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }
    }

    void EndDialog()
    {
        isPlaying = false;
        currentDialog = null;
        currentLineIndex = 0;

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

        //Debug.Log("[UIDialog] Dialog ended");
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
