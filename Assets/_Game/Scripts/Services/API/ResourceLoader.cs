using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orchestrate việc load toàn bộ resources khi game khởi động
/// - Hiển thị loading screen
/// - Gọi LoadResource để load quest.tsv
/// - Update tiến trình lên UILoading
/// - Tự động chuyển sang scene chính sau khi load xong
/// </summary>
public class ResourceLoader : MonoBehaviour
{
    [SerializeField] private LoadResource loadResource;
    [SerializeField] private UILoading uiLoading;
    [SerializeField] private string nextSceneName = "Map1"; // Scene muốn load sau khi ready
    [SerializeField] private bool autoLoadNextScene = true; // Tự động load scene sau khi resource ready
    [SerializeField] private float minLoadingTime = 2f; // Thời gian tối thiểu hiển thị loading screen

    private float elapsedTime = 0f;
    private bool resourceLoadComplete = false;

    private void Start()
    {
        // Đảm bảo UILoading được activate
        if (uiLoading != null)
        {
            uiLoading.gameObject.SetActive(true);
            uiLoading.ResetProgress();
        }

        // Khởi động quy trình load
        StartCoroutine(LoadingSequence());
    }

    private IEnumerator LoadingSequence()
    {
        elapsedTime = 0f;

        Debug.Log("[ResourceLoader] Starting resource loading sequence...");

        // Ensure LoadResource exists
        if (loadResource == null)
        {
            loadResource = FindObjectOfType<LoadResource>();
            if (loadResource == null)
            {
                Debug.LogError("[ResourceLoader] LoadResource component not found!");
                yield break;
            }
        }

        // Subscribe to LoadResource events
        loadResource.OnProgressUpdate += UpdateLoadingProgress;
        loadResource.OnLoadComplete += OnResourceLoadComplete;

        // Bắt đầu load resources
        loadResource.LoadAllResources();

        // Chờ cho đến khi load xong + minLoadingTime
        while (!resourceLoadComplete || elapsedTime < minLoadingTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[ResourceLoader] All resources loaded!");

        // Chờ thêm 0.5s trước khi chuyển scene
        yield return new WaitForSeconds(0.5f);

        // Chuyển sang scene tiếp theo nếu được cấu hình
        if (autoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[ResourceLoader] Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void UpdateLoadingProgress(float progress)
    {
        if (uiLoading != null)
        {
            uiLoading.UpdateProgress(progress);
        }

        Debug.Log($"[ResourceLoader] Loading progress: {progress:P}");
    }

    private void OnResourceLoadComplete()
    {
        resourceLoadComplete = true;
        if (uiLoading != null)
        {
            Debug.Log("[ResourceLoader] Calling uiLoading.CompleteLoading()");
            uiLoading.CompleteLoading();
        }
        Debug.Log("[ResourceLoader] Resource loading complete!");
    }

    private void OnDestroy()
    {
        // Unsubscribe từ events
        if (loadResource != null)
        {
            loadResource.OnProgressUpdate -= UpdateLoadingProgress;
            loadResource.OnLoadComplete -= OnResourceLoadComplete;
        }
    }
}
