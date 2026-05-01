using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText; // Optional: để hiển thị %
    
    private float currentProgress = 0f;

    void OnEnable()
    {
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
        currentProgress = 0f;
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
    }

    /// <summary>
    /// Hoàn thành loading (set 100%)
    /// </summary>
    public void CompleteLoading()
    {
        UpdateProgress(1f);
    }

    /// <summary>
    /// Lấy giá trị tiến trình hiện tại
    /// </summary>
    public float GetProgress()
    {
        return currentProgress;
    }
}
