using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Notification nhỏ góc phải màn hình — hiện text Lumi trong N giây rồi tự tắt.
/// Không blocking. Nếu gọi Show() khi đang hiện thì text cũ bị thay ngay.
///
/// Setup Editor:
///   - RectTransform: anchor bottom-right, pivot (1, 0)
///   - CanvasGroup component bắt buộc
///   - Gán messageText (TMP), lumiIcon (optional)
/// </summary>
public class UIAttention : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private Coroutine _routine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string text, float duration = -1f)
    {
        if (_routine != null)
            StopCoroutine(_routine);

        messageText.text = text;
        _routine = StartCoroutine(ShowRoutine(duration > 0f ? duration : displayDuration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - t / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        _routine = null;
    }
}