using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText; // Optional: để hiển thị %
    
    private float currentProgress = 0f;
    private static bool hasCompleted = false; // Static flag - persist across scenes

    // Event trigger khi loading hoàn thành
    public static System.Action OnLoadingComplete;

    void OnEnable()
    {
        Debug.Log("[UILoading] OnEnable called");
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
        currentProgress = 0f;
        // Không reset hasCompleted ở đây!
    }

    /// <summary>
    /// Cập nhật thanh slider với tiến trình (0-1)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
        }

        if (progressText != null)
        {
            progressText.text = $"{(currentProgress * 100f):F0}%";
        }

        //Debug.Log($"[UILoading] Progress: {currentProgress:P}");
    }

    /// <summary>
    /// Tăng tiến trình một lượng nhỏ (dùng cho từng step load)
    /// </summary>
    public void IncrementProgress(float amount)
    {
        UpdateProgress(currentProgress + amount);
    }

    /// <summary>
    /// Đặt tiến trình về 0 (khi bắt đầu load mới)
    /// </summary>
    public void ResetProgress()
    {
        UpdateProgress(0f);
        hasCompleted = false; // Reset flag khi reset progress
    }

    /// <summary>
    /// Hoàn thành loading (set 100%) - chỉ trigger event 1 lần
    /// </summary>
    public void CompleteLoading()
    {
        if (hasCompleted)
        {
            Debug.LogWarning("[UILoading] CompleteLoading already called!");
            return;
        }

        hasCompleted = true;
        UpdateProgress(1f);
        // Trigger event để báo các system khác loading xong
        Debug.Log("[UILoading] OnLoadingComplete triggered");
        OnLoadingComplete?.Invoke();
    }

    /// <summary>
    /// Lấy giá trị tiến trình hiện tại
    /// </summary>
    public float GetProgress()
    {
        return currentProgress;
    }
}
