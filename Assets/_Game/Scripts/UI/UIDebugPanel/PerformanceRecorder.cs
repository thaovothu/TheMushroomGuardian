using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Đo FPS và Memory trong game. Gắn vào bất kỳ GameObject nào trong scene.
///   F7  — ẩn/hiện overlay
///   F8  — bắt đầu / dừng đo
///   F9  — xuất CSV (sau khi đã có kết quả)
/// Kết quả in ra Console và xuất file vào Application.persistentDataPath.
/// </summary>
public class PerformanceRecorder : MonoBehaviour
{
    [Header("Phím tắt")]
    [SerializeField] private KeyCode hideKey   = KeyCode.F7;
    [SerializeField] private KeyCode recordKey = KeyCode.F8;
    [SerializeField] private KeyCode exportKey = KeyCode.F9;

    [Header("Cài đặt")]
    [Tooltip("Số frame bỏ qua lúc đầu để cho game ổn định trước khi bắt đầu ghi")]
    [SerializeField] private int warmupFrames = 120;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _showUI    = true;
    private bool _recording = false;
    private int  _warmup    = 0;

    private readonly List<float> _dtSamples  = new();
    private readonly List<float> _memSamples = new();

    private PerfResult _result;
    private bool       _hasResult;

    private struct PerfResult
    {
        public float fpsMean, fpsMin, fpsMax, fpsP1, fpsP99, fpsStd;
        public float msMean, msMax;
        public float memAvgMB, memPeakMB;
        public int   rawCount, keptCount;
        public string timestamp;
    }

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(hideKey))                 _showUI = !_showUI;
        if (Input.GetKeyDown(recordKey))               ToggleRecord();
        if (Input.GetKeyDown(exportKey) && _hasResult) ExportCSV();

        if (!_recording) return;

        if (_warmup > 0) { _warmup--; return; }

        _dtSamples.Add(Time.unscaledDeltaTime);
        _memSamples.Add(Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
    }

    // ── Record ────────────────────────────────────────────────────────────────

    private void ToggleRecord()
    {
        if (_recording)
        {
            _recording = false;
            Analyze();
        }
        else
        {
            _dtSamples.Clear();
            _memSamples.Clear();
            _warmup    = warmupFrames;
            _recording = true;
            Debug.Log($"[PerfRecorder] Bắt đầu đo (warmup {warmupFrames} frames)...");
        }
    }

    private void Analyze()
    {
        if (_dtSamples.Count == 0) return;

        // Chuyển dt → FPS và ms
        var fpsList = new List<float>(_dtSamples.Count);
        var msList  = new List<float>(_dtSamples.Count);
        foreach (var dt in _dtSamples)
        {
            if (dt <= 0) continue;
            fpsList.Add(1f / dt);
            msList.Add(dt * 1000f);
        }

        fpsList.Sort();
        int rawN = fpsList.Count;

        // Lọc outlier bằng IQR (giữ lại giá trị trong [Q1-1.5*IQR, Q3+1.5*IQR])
        var kept  = FilterIQR(fpsList);
        int keptN = kept.Count;

        float mean = Mean(kept);
        float std  = StdDev(kept, mean);

        // Frame time: không lọc, giữ worst-case thật
        float msMax = 0f, msSum = 0f;
        foreach (var ms in msList) { msSum += ms; if (ms > msMax) msMax = ms; }

        // Memory
        float memPeak = 0f, memSum = 0f;
        foreach (var m in _memSamples) { memSum += m; if (m > memPeak) memPeak = m; }

        _result = new PerfResult
        {
            fpsMean   = mean,
            fpsMin    = kept.Count > 0 ? kept[0]  : 0f,
            fpsMax    = kept.Count > 0 ? kept[^1] : 0f,
            fpsP1     = Percentile(kept, 1f),
            fpsP99    = Percentile(kept, 99f),
            fpsStd    = std,
            msMean    = msList.Count > 0 ? msSum / msList.Count : 0f,
            msMax     = msMax,
            memAvgMB  = _memSamples.Count > 0 ? memSum / _memSamples.Count : 0f,
            memPeakMB = memPeak,
            rawCount  = rawN,
            keptCount = keptN,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        };
        _hasResult = true;

        Debug.Log(
            $"[PerfRecorder] Xong — {keptN}/{rawN} frames (đã lọc IQR)\n" +
            $"FPS  avg={_result.fpsMean:F1}  min={_result.fpsMin:F1}  max={_result.fpsMax:F1}" +
            $"  1%low={_result.fpsP1:F1}  P99={_result.fpsP99:F1}  σ={_result.fpsStd:F1}\n" +
            $"FrameTime  avg={_result.msMean:F2}ms  max={_result.msMax:F2}ms\n" +
            $"Memory  avg={_result.memAvgMB:F0}MB  peak={_result.memPeakMB:F0}MB"
        );
    }

    // ── Export ────────────────────────────────────────────────────────────────

    private void ExportCSV()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== SUMMARY ===");
        sb.AppendLine("metric,value");
        sb.AppendLine($"timestamp,{_result.timestamp}");
        sb.AppendLine($"samples_raw,{_result.rawCount}");
        sb.AppendLine($"samples_kept_after_IQR_filter,{_result.keptCount}");
        sb.AppendLine($"fps_mean,{_result.fpsMean:F2}");
        sb.AppendLine($"fps_min,{_result.fpsMin:F2}");
        sb.AppendLine($"fps_max,{_result.fpsMax:F2}");
        sb.AppendLine($"fps_1pct_low,{_result.fpsP1:F2}");
        sb.AppendLine($"fps_p99,{_result.fpsP99:F2}");
        sb.AppendLine($"fps_stddev,{_result.fpsStd:F2}");
        sb.AppendLine($"frametime_mean_ms,{_result.msMean:F3}");
        sb.AppendLine($"frametime_max_ms,{_result.msMax:F3}");
        sb.AppendLine($"memory_avg_mb,{_result.memAvgMB:F2}");
        sb.AppendLine($"memory_peak_mb,{_result.memPeakMB:F2}");

        sb.AppendLine();
        sb.AppendLine("=== RAW FRAMES ===");
        sb.AppendLine("frame,dt_ms,fps,mem_alloc_mb");
        for (int i = 0; i < _dtSamples.Count; i++)
        {
            float dt  = _dtSamples[i];
            float fps = dt > 0 ? 1f / dt : 0f;
            float mem = i < _memSamples.Count ? _memSamples[i] : 0f;
            sb.AppendLine($"{i},{dt * 1000f:F3},{fps:F2},{mem:F2}");
        }

        string path = Path.Combine(Application.persistentDataPath,
                                   $"perf_fps_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[PerfRecorder] Xuất CSV → {path}");
    }

    // ── IMGUI overlay ─────────────────────────────────────────────────────────

    private void OnGUI()
    {
        if (!_showUI) return;

        float curFps = Time.unscaledDeltaTime > 0 ? 1f / Time.unscaledDeltaTime : 0f;
        float curMem = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;

        GUILayout.BeginArea(new Rect(10, 10, 290, 330));
        GUILayout.BeginVertical(GUI.skin.box);

        GUILayout.Label($"Live — FPS: {curFps:F0}  |  {Time.unscaledDeltaTime * 1000f:F1} ms");
        GUILayout.Label($"       Mem alloc: {curMem:F0} MB");

        GUILayout.Space(4);

        if (_recording)
        {
            GUILayout.Label(_warmup > 0
                ? $"[WARMUP] còn {_warmup} frames..."
                : $"[ĐANG ĐO] {_dtSamples.Count} frames");
        }
        else if (_hasResult)
        {
            GUILayout.Label("─── Kết quả đo ───");
            GUILayout.Label($"FPS  avg={_result.fpsMean:F1}  σ={_result.fpsStd:F1}");
            GUILayout.Label($"     min={_result.fpsMin:F1}  max={_result.fpsMax:F1}");
            GUILayout.Label($"     1%low={_result.fpsP1:F1}  P99={_result.fpsP99:F1}");
            GUILayout.Label($"Frame  avg={_result.msMean:F2}ms  max={_result.msMax:F2}ms");
            GUILayout.Label($"Mem  avg={_result.memAvgMB:F0}MB  peak={_result.memPeakMB:F0}MB");
            GUILayout.Label($"Samples: {_result.keptCount}/{_result.rawCount} (IQR filtered)");
        }

        GUILayout.Space(4);
        GUILayout.Label($"[{hideKey}] Ẩn/hiện   [{recordKey}] " +
                        (_recording ? "Dừng đo" : "Bắt đầu đo"));
        if (_hasResult)
            GUILayout.Label($"[{exportKey}] Xuất CSV");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // ── Thống kê helpers ──────────────────────────────────────────────────────

    /// <summary>Loại bỏ outlier bằng IQR fence (1.5x). Input phải đã được sort tăng dần.</summary>
    private static List<float> FilterIQR(List<float> sorted)
    {
        if (sorted.Count < 4) return sorted;
        float q1  = Percentile(sorted, 25f);
        float q3  = Percentile(sorted, 75f);
        float iqr = q3 - q1;
        float lo  = q1 - 1.5f * iqr;
        float hi  = q3 + 1.5f * iqr;
        var result = new List<float>(sorted.Count);
        foreach (var v in sorted)
            if (v >= lo && v <= hi) result.Add(v);
        return result.Count > 0 ? result : sorted;
    }

    private static float Percentile(List<float> sorted, float pct)
    {
        if (sorted.Count == 0) return 0f;
        float pos = pct / 100f * (sorted.Count - 1);
        int   lo  = (int)pos;
        int   hi  = Mathf.Min(lo + 1, sorted.Count - 1);
        return Mathf.Lerp(sorted[lo], sorted[hi], pos - lo);
    }

    private static float Mean(List<float> data)
    {
        if (data.Count == 0) return 0f;
        float s = 0f;
        foreach (var v in data) s += v;
        return s / data.Count;
    }

    private static float StdDev(List<float> data, float mean)
    {
        if (data.Count < 2) return 0f;
        float s = 0f;
        foreach (var v in data) s += (v - mean) * (v - mean);
        return Mathf.Sqrt(s / data.Count);
    }
}
