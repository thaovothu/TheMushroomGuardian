using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [SerializeField] private UILoading uiLoading;
    [SerializeField] private string loadingScreenName = "UILoading"; // tên UI panel của loading screen
    [SerializeField] private bool autoLoadOnStart = false; // Tự động load scene khi start
    [SerializeField] private string autoLoadSceneName = ""; // Tên scene tự động load

    private float totalSteps = 0f;
    private float currentStep = 0f;
    private bool isLoading = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Hiển thị loading screen nếu cần
        if (uiLoading != null)
        {
            uiLoading.gameObject.SetActive(true);
            uiLoading.ResetProgress();
            Debug.Log("[LoadingManager] UILoading is now visible");
        }

        // Tự động load scene nếu được cấu hình
        if (autoLoadOnStart && !string.IsNullOrEmpty(autoLoadSceneName))
        {
            LoadScene(autoLoadSceneName);
        }
    }

    /// <summary>
    /// Bắt đầu load scene mới với loading screen
    /// </summary>
    public void LoadScene(string sceneName, int totalStepsEstimate = 10)
    {
        if (isLoading)
        {
            Debug.LogWarning("[LoadingManager] Already loading a scene!");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName, totalStepsEstimate));
    }

    IEnumerator LoadSceneAsync(string sceneName, int totalStepsEstimate)
    {
        isLoading = true;
        totalSteps = totalStepsEstimate;
        currentStep = 0f;

        // Hiển thị loading screen (trực tiếp hoặc qua UIManager)
        if (uiLoading != null)
        {
            uiLoading.gameObject.SetActive(true);
        }
        else if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowUI(loadingScreenName);
        }
        ResetProgress();

        // Load scene async
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Scene load progress: 0-0.9 (90%)
            float sceneProgress = asyncLoad.progress / 0.9f;
            UpdateProgress(sceneProgress * 0.8f); // Dành 80% cho scene load

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Scene đã load xong, còn 20% cho SO files và initialization
        UpdateProgress(0.8f);
        yield return new WaitForSeconds(0.2f);

        // Hoàn thành
        CompleteLoading();
        
        yield return new WaitForSeconds(0.5f); // Chờ 0.5s trước khi ẩn loading screen

        // Ẩn loading screen
        if (uiLoading != null)
        {
            uiLoading.gameObject.SetActive(false);
        }
        else if (UIManager.Instance != null)
        {
            UIManager.Instance.HideUI(loadingScreenName);
        }

        isLoading = false;
    }

    /// <summary>
    /// Load nhiều Scriptable Objects và cập nhật progress
    /// </summary>
    public void LoadScriptableObjects(List<string> soPaths)
    {
        if (isLoading)
        {
            StartCoroutine(LoadSOsAsync(soPaths));
        }
    }

    IEnumerator LoadSOsAsync(List<string> soPaths)
    {
        float stepSize = 0.2f / soPaths.Count; // Sử dụng 20% còn lại cho SO loading
        float baseProgress = GetProgress();

        foreach (string path in soPaths)
        {
            // Load SO từ Resources hoặc Addressables
            var so = Resources.Load(path);
            if (so != null)
            {
                Debug.Log($"[LoadingManager] Loaded SO: {path}");
            }
            else
            {
                Debug.LogWarning($"[LoadingManager] Failed to load SO: {path}");
            }

            IncrementProgress(stepSize);
            yield return null;
        }
    }

    /// <summary>
    /// Cập nhật tiến trình (0-1)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (uiLoading != null)
        {
            uiLoading.UpdateProgress(progress);
        }
    }

    /// <summary>
    /// Tăng tiến trình
    /// </summary>
    public void IncrementProgress(float amount)
    {
        if (uiLoading != null)
        {
            uiLoading.IncrementProgress(amount);
        }
    }

    /// <summary>
    /// Lấy tiến trình hiện tại
    /// </summary>
    public float GetProgress()
    {
        if (uiLoading != null)
        {
            return uiLoading.GetProgress();
        }
        return 0f;
    }

    /// <summary>
    /// Reset tiến trình
    /// </summary>
    public void ResetProgress()
    {
        if (uiLoading != null)
        {
            uiLoading.ResetProgress();
        }
    }

    /// <summary>
    /// Hoàn thành loading
    /// </summary>
    public void CompleteLoading()
    {
        if (uiLoading != null)
        {
            uiLoading.CompleteLoading();
            // UIManager.Instance.ShowUI("UIWeapon");
            // UIManager.Instance.ShowUI("UIStatus");
        }
    }

    /// <summary>
    /// Kiểm tra xem đang loading không
    /// </summary>
    public bool IsLoading()
    {
        return isLoading;
    }
}
