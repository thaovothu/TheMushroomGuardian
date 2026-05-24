using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIntroSequence : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float charDelay = 0.04f;
    [SerializeField] private float punctuationDelay = 0.35f;
    [SerializeField] private float lineHoldDuration = 1.8f;
    [SerializeField] private float fadeDuration = 0.7f;
    [SerializeField] private float endHoldDuration = 3f;
    [SerializeField] private float finalFadeOutDuration = 2f;

    [Header("Skip")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private KeyCode skipKeyAlt = KeyCode.Return;

    private static readonly List<string> IntroLines = new List<string>
    {
        "Thuở xa xưa, vùng đất nấm là nơi 4 nguyên tố cùng tồn tại...",
        "Đất nuôi dưỡng những cội rễ ngàn năm.\nGió thổi sinh khí qua từng tầng mây.\nNước thanh tẩy mọi vết thương của đại địa.\nLửa thắp sáng ý chí của muôn loài.",
        "Nhưng từ bóng tối vô tận, Hư Không xuất hiện.",
        "Nó không phá hủy — nó nuốt chửng.\nTừng nguyên tố một, bị phong ấn trong im lặng.\nThế giới bắt đầu tàn lụi từ bên trong.",
        "Và rồi... ngươi thức tỉnh.",
        "Một mầm nấm nhỏ bé, ngủ sâu trong lòng đại địa.\nKhông biết mình là ai. Không biết mình đến từ đâu.\nChỉ biết rằng — thế giới này đang cần ngươi.",
    };

    private bool isSkipped = false;
    private bool isPlaying = false;
    private Coroutine introCoroutine;

    private void OnEnable() => UILoading.OnLoadingComplete += OnLoadingComplete;
    private void OnDisable() => UILoading.OnLoadingComplete -= OnLoadingComplete;
    private void Update()
    {
        if (!isSkipped && isPlaying && (Input.GetKeyDown(skipKey) || Input.GetKeyDown(skipKeyAlt)))
            SkipIntro();
    }

    private void OnLoadingComplete()
    {
        // Guard: chỉ chạy 1 lần duy nhất
        if (isPlaying) return;

        if (PlayerPrefs.GetInt("IntroPlayed", 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        isPlaying = true;
        gameObject.SetActive(true);

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        introText.text = "";

        introCoroutine = StartCoroutine(PlayIntro());
    }

    public void SkipIntro()
    {
        isSkipped = true;
        if (introCoroutine != null) StopCoroutine(introCoroutine);
        StartCoroutine(FadeOutAndFinish());
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < IntroLines.Count; i++)
        {
            if (isSkipped) yield break;

            // Reset text TRƯỚC khi fade in — đảm bảo nền đen sạch
            introText.text = "";

            // Fade in canvas
            yield return StartCoroutine(FadeTo(1f, fadeDuration));

            // Reveal từng ký tự
            yield return StartCoroutine(RevealLine(IntroLines[i]));

            if (isSkipped) yield break;

            // Giữ dòng
            float hold = (i == IntroLines.Count - 1) ? endHoldDuration : lineHoldDuration;
            yield return new WaitForSeconds(hold);

            if (isSkipped) yield break;

            // Fade out canvas
            yield return StartCoroutine(FadeTo(0f, fadeDuration));
        }

        yield return StartCoroutine(FadeOutAndFinish());
    }

    private IEnumerator RevealLine(string line)
    {
        int total = line.Length;

        for (int i = 0; i <= total; i++)
        {
            if (isSkipped) yield break;

            string visible = line.Substring(0, i);
            string hidden = i < total
                ? $"<color=#ffffff00>{line.Substring(i)}</color>"
                : "";

            introText.text = visible + hidden;

            if (i >= total) break;

            char current = line[i];

            // Không delay tại \n
            if (current == '\n') continue;

            bool isPunct = current == '.' || current == ',' ||
                           current == '!' || current == '?' ||
                           current == '—';

            yield return new WaitForSeconds(isPunct ? punctuationDelay : charDelay);
        }

        // Set text sạch không còn tag
        introText.text = line;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        if (canvasGroup == null) yield break;

        float start = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private IEnumerator FadeOutAndFinish()
    {
        yield return StartCoroutine(FadeTo(0f, finalFadeOutDuration));

        isPlaying = false;

        PlayerPrefs.SetInt("IntroPlayed", 1);
        // PlayerPrefs.Save();

        GameEvent.Quest.OnIntroComplete?.Invoke();
        Debug.Log("[UIIntroSequence] Intro complete → Quest 1 bắt đầu");
        gameObject.SetActive(false);
    }
}