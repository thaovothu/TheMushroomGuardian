using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Benchmark latency cho Channel 1 (PlayFab) và Channel 2 (ASP.NET Core).
/// Gắn vào bất kỳ GameObject nào trong scene.
///   F10 — bật/tắt panel
///
/// Mỗi test chạy N lần (mặc định 10), tự động lọc IQR rồi in kết quả.
/// Nút "Xuất CSV" ghi file ra Application.persistentDataPath.
///
/// Lưu ý: Server2 cần server đang chạy tại URL trong ServerConfig.
///         PlayFab cần Title ID đã được cấu hình trong Editor.
/// </summary>
public class NetworkBenchmark : MonoBehaviour
{
    [Header("Phím tắt")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F10;

    [Header("Cài đặt")]
    [SerializeField] private int iterations = 10;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool   _showUI  = false;
    private bool   _running = false;
    private string _status  = "Sẵn sàng.";

    // Token Server2 (sau khi login)
    private string _s2Token    = null;
    private string _s2PlayerId = null;
    // PlayFab ID (sau khi login)
    private string _pfId = null;

    // Kết quả từng test
    private readonly List<BenchRow> _rows = new();
    private Vector2 _scrollPos;
    private Rect    _windowRect = new Rect(20, 20, 560, 500);

    // ── IMGUI ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey)) _showUI = !_showUI;
    }

    private void OnGUI()
    {
        if (!_showUI) return;
        _windowRect = GUI.Window(9901, _windowRect, DrawWindow, "Network Benchmark  [F10 đóng]");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label($"Iterations: {iterations}  |  Server2: {S2Url}");
        GUILayout.Label($"Trạng thái: {_status}");
        GUILayout.Space(4);

        // ── Login ──
        GUILayout.Label("── Đăng nhập ──");
        GUILayout.BeginHorizontal();
        GUI.enabled = !_running;
        if (GUILayout.Button("Login Server2 (Guest)")) Run(BenchLoginS2());
        if (GUILayout.Button("Login PlayFab (Guest)")) Run(BenchLoginPF());
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        // ── Load data ──
        GUILayout.Label("── Load dữ liệu (cần đăng nhập trước) ──");
        GUILayout.BeginHorizontal();
        GUI.enabled = !_running;
        if (GUILayout.Button("Load Server2"))  Run(BenchLoadS2());
        if (GUILayout.Button("Load PlayFab"))  Run(BenchLoadPF());
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        // ── Save data ──
        GUILayout.Label("── Save dữ liệu (cần đăng nhập trước) ──");
        GUILayout.BeginHorizontal();
        GUI.enabled = !_running;
        if (GUILayout.Button("Save Server2"))  Run(BenchSaveS2());
        if (GUILayout.Button("Save PlayFab"))  Run(BenchSavePF());
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        // ── Tiện ích ──
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        GUI.enabled = !_running;
        if (GUILayout.Button("Chạy tất cả"))  Run(RunAll());
        if (_rows.Count > 0 && GUILayout.Button("Xuất CSV")) ExportCSV();
        if (_rows.Count > 0 && GUILayout.Button("Xoá kết quả")) _rows.Clear();
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // ── Kết quả ──
        if (_rows.Count > 0)
        {
            GUILayout.Label("channel | operation         | n  | avg(ms) | med | min  | max  | σ");
            GUILayout.Label("─────────────────────────────────────────────────────────────────");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(160));
            foreach (var r in _rows)
                GUILayout.Label(r.Display());
            GUILayout.EndScrollView();
        }

        GUI.DragWindow();
    }

    // ── Điều phối ─────────────────────────────────────────────────────────────

    private void Run(IEnumerator routine)
    {
        if (_running) return;
        StartCoroutine(RunRoutine(routine));
    }

    private IEnumerator RunRoutine(IEnumerator routine)
    {
        _running = true;
        yield return StartCoroutine(routine);
        _running = false;
    }

    private IEnumerator RunAll()
    {
        yield return StartCoroutine(BenchLoginS2());
        yield return StartCoroutine(BenchLoginPF());
        yield return StartCoroutine(BenchLoadS2());
        yield return StartCoroutine(BenchLoadPF());
        yield return StartCoroutine(BenchSaveS2());
        yield return StartCoroutine(BenchSavePF());
        _status = "Xong tất cả.";
    }

    // ── Server2 — Login ───────────────────────────────────────────────────────

    private IEnumerator BenchLoginS2()
    {
        _status = "Login Server2...";
        var samples = new List<long>(iterations);
        string deviceId = $"benchmark_{SystemInfo.deviceUniqueIdentifier}";

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Login Server2 [{i + 1}/{iterations}]";
            var body = $"{{\"deviceId\":\"{deviceId}\"}}";
            using var req = MakePost($"{S2Url}/api/auth/login-guest", body);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            yield return req.SendWebRequest();
            sw.Stop();
            samples.Add(sw.ElapsedMilliseconds);

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                _s2Token    = resp.token;
                _s2PlayerId = resp.playerId;
            }
        }

        AddRow("Server2", "Login (guest)", samples);
        _status = $"Login Server2 xong. Token: {(_s2Token != null ? "OK" : "FAIL")}";
    }

    // ── PlayFab — Login ───────────────────────────────────────────────────────

    private IEnumerator BenchLoginPF()
    {
        _status = "Login PlayFab...";
        var samples = new List<long>(iterations);
        string customId = $"benchmark_{SystemInfo.deviceUniqueIdentifier}";

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Login PlayFab [{i + 1}/{iterations}]";
            bool done = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest { CustomId = customId, CreateAccount = true },
                result => { sw.Stop(); _pfId = result.PlayFabId; done = true; },
                error  => { sw.Stop(); done = true; }
            );

            yield return new WaitUntil(() => done);
            samples.Add(sw.ElapsedMilliseconds);
        }

        AddRow("PlayFab", "Login (guest)", samples);
        _status = $"Login PlayFab xong. ID: {(_pfId != null ? "OK" : "FAIL")}";
    }

    // ── Server2 — Load data ───────────────────────────────────────────────────

    private IEnumerator BenchLoadS2()
    {
        if (string.IsNullOrEmpty(_s2Token))
        {
            _status = "Chưa login Server2. Chạy 'Login Server2' trước.";
            yield break;
        }

        _status = "Load Server2...";
        var samples = new List<long>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Load Server2 [{i + 1}/{iterations}]";
            using var req = MakeGet($"{S2Url}/api/player/data", _s2Token);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            yield return req.SendWebRequest();
            sw.Stop();
            samples.Add(sw.ElapsedMilliseconds);
        }

        AddRow("Server2", "Load data", samples);
        _status = "Load Server2 xong.";
    }

    // ── PlayFab — Load data ───────────────────────────────────────────────────

    private IEnumerator BenchLoadPF()
    {
        if (string.IsNullOrEmpty(_pfId))
        {
            _status = "Chưa login PlayFab. Chạy 'Login PlayFab' trước.";
            yield break;
        }

        _status = "Load PlayFab...";
        var samples = new List<long>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Load PlayFab [{i + 1}/{iterations}]";
            bool done = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            PlayFabClientAPI.GetUserData(
                new GetUserDataRequest(),
                result => { sw.Stop(); done = true; },
                error  => { sw.Stop(); done = true; }
            );

            yield return new WaitUntil(() => done);
            samples.Add(sw.ElapsedMilliseconds);
        }

        AddRow("PlayFab", "Load data", samples);
        _status = "Load PlayFab xong.";
    }

    // ── Server2 — Save data ───────────────────────────────────────────────────

    private IEnumerator BenchSaveS2()
    {
        if (string.IsNullOrEmpty(_s2Token))
        {
            _status = "Chưa login Server2. Chạy 'Login Server2' trước.";
            yield break;
        }

        _status = "Save Server2...";
        var samples = new List<long>(iterations);
        string payload = "{\"questId\":1,\"stepId\":1,\"coins\":0,\"inventoryJson\":\"{}\",\"skillsCsv\":\"\"}";

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Save Server2 [{i + 1}/{iterations}]";
            using var req = MakePut($"{S2Url}/api/player/data", payload, _s2Token);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            yield return req.SendWebRequest();
            sw.Stop();
            samples.Add(sw.ElapsedMilliseconds);
        }

        AddRow("Server2", "Save data", samples);
        _status = "Save Server2 xong.";
    }

    // ── PlayFab — Save data ───────────────────────────────────────────────────

    private IEnumerator BenchSavePF()
    {
        if (string.IsNullOrEmpty(_pfId))
        {
            _status = "Chưa login PlayFab. Chạy 'Login PlayFab' trước.";
            yield break;
        }

        _status = "Save PlayFab...";
        var samples = new List<long>(iterations);
        var data = new Dictionary<string, string> { { "BenchmarkKey", "test_value" } };

        for (int i = 0; i < iterations; i++)
        {
            _status = $"Save PlayFab [{i + 1}/{iterations}]";
            bool done = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            PlayFabClientAPI.UpdateUserData(
                new UpdateUserDataRequest { Data = data },
                result => { sw.Stop(); done = true; },
                error  => { sw.Stop(); done = true; }
            );

            yield return new WaitUntil(() => done);
            samples.Add(sw.ElapsedMilliseconds);
        }

        AddRow("PlayFab", "Save data", samples);
        _status = "Save PlayFab xong.";
    }

    // ── Kết quả và xuất file ──────────────────────────────────────────────────

    private void AddRow(string channel, string op, List<long> samples)
    {
        var floats = new List<float>(samples.Count);
        foreach (var s in samples) floats.Add(s);

        floats.Sort();
        int rawN = floats.Count;
        var kept = FilterIQR(floats);

        float mean   = Mean(kept);
        float std    = StdDev(kept, mean);
        float median = Percentile(kept, 50f);

        var row = new BenchRow
        {
            channel    = channel,
            operation  = op,
            n          = kept.Count,
            rawN       = rawN,
            meanMs     = mean,
            medianMs   = median,
            minMs      = kept.Count > 0 ? kept[0]  : 0f,
            maxMs      = kept.Count > 0 ? kept[^1] : 0f,
            stdMs      = std,
            rawSamples = samples,
        };
        _rows.Add(row);

        Debug.Log($"[NetBench] {channel} / {op}  n={kept.Count}/{rawN}  " +
                  $"avg={mean:F1}ms  med={median:F1}ms  " +
                  $"min={row.minMs:F1}ms  max={row.maxMs:F1}ms  σ={std:F1}ms");
    }

    private void ExportCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("channel,operation,n,mean_ms,median_ms,min_ms,max_ms,std_ms,raw_samples_ms");
        foreach (var r in _rows)
        {
            var raw = string.Join("|", r.rawSamples);
            sb.AppendLine(
                $"{r.channel},{r.operation},{r.n}," +
                $"{r.meanMs:F2},{r.medianMs:F2},{r.minMs:F2},{r.maxMs:F2},{r.stdMs:F2}," +
                $"\"{raw}\""
            );
        }

        string path = Path.Combine(Application.persistentDataPath,
                                   $"perf_network_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[NetBench] Xuất CSV → {path}");
        _status = $"Đã xuất → {path}";
    }

    // ── HTTP helpers (giống GameServer2Manager) ───────────────────────────────

    private static string S2Url => ServerConfig.Instance.Server2BaseUrl;

    private static UnityWebRequest MakePost(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    private static UnityWebRequest MakeGet(string url, string token)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        return req;
    }

    private static UnityWebRequest MakePut(string url, string json, string token)
    {
        var req = new UnityWebRequest(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        return req;
    }

    // ── Thống kê helpers ──────────────────────────────────────────────────────

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

    // ── DTOs ─────────────────────────────────────────────────────────────────

    [Serializable] private class AuthResponse { public string token, playerId, displayName; }

    // ── Model ─────────────────────────────────────────────────────────────────

    private class BenchRow
    {
        public string      channel, operation;
        public int         n, rawN;
        public float       meanMs, medianMs, minMs, maxMs, stdMs;
        public List<long>  rawSamples;

        public string Display() =>
            $"{channel,-8} | {operation,-18} | {n,2} | {meanMs,7:F1} | {medianMs,5:F1} | {minMs,5:F1} | {maxMs,5:F1} | {stdMs,4:F1}";
    }
}
