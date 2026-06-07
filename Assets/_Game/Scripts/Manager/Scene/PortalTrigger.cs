using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Khi player bước vào cổng → fade out → load scene mới.
///
/// Setup:
///   1. Add component vào GameObject cổng không gian
///   2. Đảm bảo có Collider với Is Trigger = true
///   3. Điền targetScene (tên scene trong Build Settings)
/// </summary>
public class PortalTrigger : MonoBehaviour
{
    [Tooltip("Tên scene cần load (phải có trong Build Settings)")]
    [SerializeField] private string targetScene;

    [Tooltip("Thời gian fade out trước khi load scene")]
    [SerializeField] private float fadeOutDuration = 1f;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;
        StartCoroutine(LoadSceneWithFade());
    }

    private IEnumerator LoadSceneWithFade()
    {
        Debug.Log($"[PortalTrigger] Player bước vào cổng → load scene '{targetScene}'");

        // Fade out nếu có UIFade (tuỳ chọn)
        var uiFade = FindObjectOfType<CanvasGroup>();
        if (uiFade != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                uiFade.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[PortalTrigger] targetScene chưa được điền!");
            yield break;
        }

        SceneManager.LoadScene(targetScene);
    }
}