using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orchestrate toàn bộ startup sequence:
///
///   UILoading (phase 1) — giả lập khởi tạo SDK ~1.5s
///   → UIChannel         — chọn server (PlayFab / Mac Mini), ping Server2
///   → UIAuth            — chờ player login/register/guest
///   → UILoading (phase 2) — load quest.tsv + dialog.tsv
///   → LoadScene         — chuyển sang game scene (y như cũ)
/// </summary>
public class ResourceLoader : MonoBehaviour
{
    [SerializeField] private LoadResource loadResource;
    [SerializeField] private UILoading uiLoading;
    [SerializeField] private GameObject uiChannel;
    [SerializeField] private GameObject uiAuth;
    [SerializeField] private string nextSceneName = "Map1";
    [SerializeField] private float phase1Duration = 1.5f;
    [SerializeField] private float minPhase2Time = 1.5f;

    private bool _resourceLoadComplete = false;
    private bool _loggedIn = false;

    private void Start()
    {
        StartCoroutine(LoadingSequence());
    }

    private IEnumerator LoadingSequence()
    {
        // ── PHASE 1: Brief init loading ───────────────────────────────────────
        uiAuth.SetActive(false);
        uiLoading.gameObject.SetActive(true);
        uiLoading.ResetProgress();

        float t = 0f;
        while (t < phase1Duration)
        {
            t += Time.deltaTime;
            uiLoading.UpdateProgress(Mathf.Clamp01(t / phase1Duration) * 0.3f);
            yield return null;
        }
        uiLoading.UpdateProgress(0.3f);

        // ── UIChannel: chọn server → UIAuth: chờ login ───────────────────────
        uiLoading.gameObject.SetActive(false);

        _loggedIn = false;
        GameEvent.Auth.OnLoginSuccess += OnLoggedIn;

        // UIChannel tự bật UIAuth khi server được chọn
        uiAuth.SetActive(false);
        uiChannel.SetActive(true);

        yield return new WaitUntil(() => _loggedIn);

        GameEvent.Auth.OnLoginSuccess -= OnLoggedIn;
        // UIChannel và UIAuth tự ẩn sau khi login — đảm bảo tắt
        uiChannel.SetActive(false);
        uiAuth.SetActive(false);

        // ── PHASE 2: Load resources ───────────────────────────────────────────
        uiLoading.gameObject.SetActive(true);
        uiLoading.ResetProgress();

        if (loadResource == null)
            loadResource = FindObjectOfType<LoadResource>();

        if (loadResource == null)
        {
            Debug.LogError("[ResourceLoader] LoadResource not found!");
            yield break;
        }

        _resourceLoadComplete = false;
        float elapsed = 0f;

        loadResource.OnProgressUpdate += OnProgressUpdate;
        loadResource.OnLoadComplete   += OnResourceLoadComplete;
        loadResource.LoadAllResources();

        while (!_resourceLoadComplete || elapsed < minPhase2Time)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        loadResource.OnProgressUpdate -= OnProgressUpdate;
        loadResource.OnLoadComplete   -= OnResourceLoadComplete;

        yield return new WaitForSeconds(0.5f);

        // ── Load scene (y như cũ) ─────────────────────────────────────────────
        Debug.Log($"[ResourceLoader] Loading scene: {nextSceneName}");
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        SceneManager.LoadScene(nextSceneName);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void OnLoggedIn(string _) => _loggedIn = true;

    private void OnProgressUpdate(float progress)
    {
        // phase 2 chiếm 70% còn lại (0.3 → 1.0)
        uiLoading.UpdateProgress(0.3f + progress * 0.7f);
    }

    private void OnResourceLoadComplete()
    {
        _resourceLoadComplete = true;
        uiLoading.UpdateProgress(1f);
        Debug.Log("[ResourceLoader] Resources loaded.");
    }

    private static void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameSceneLoaded;
        Debug.Log($"[ResourceLoader] Scene '{scene.name}' loaded — firing OnLoadingComplete");
        UILoading.OnLoadingComplete?.Invoke();
    }

    private void OnDestroy()
    {
        if (loadResource != null)
        {
            loadResource.OnProgressUpdate -= OnProgressUpdate;
            loadResource.OnLoadComplete   -= OnResourceLoadComplete;
        }
        GameEvent.Auth.OnLoginSuccess -= OnLoggedIn;
    }
}
