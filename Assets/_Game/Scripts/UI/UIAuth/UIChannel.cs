using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Panel chọn server trước khi đăng nhập.
///
/// Server 1 (PlayFab)  — luôn hiện là online.
/// Server 2 (Mac Mini) — ping /health khi mở panel:
///   • Đang kiểm tra... → Hoạt động   → button bật, có thể chọn
///   • Đang kiểm tra... → Đang bảo trì → button tắt, không thể chọn
///
/// Setup trong Editor:
///   Kéo UIAuth panel vào uiAuthPanel (mặc định SetActive false).
///   Kéo 2 card GameObject + các Text/Image tương ứng vào đúng slot.
/// </summary>
public class UIChannel : MonoBehaviour
{
    [Header("Server 1 — PlayFab")]
    [SerializeField] private Button server1Button;

    [Header("Server 2 — Mac Mini")]
    [SerializeField] private Button            server2Button;
    [SerializeField] private TextMeshProUGUI   server2StatusText;
    [SerializeField] private GameObject        server2CheckingIcon;  // spinner
    [SerializeField] private GameObject        server2OnlineIcon;    // chấm xanh
    [SerializeField] private GameObject        server2OfflineIcon;   // chấm đỏ

    [Header("Next panel")]
    [SerializeField] private GameObject uiAuthPanel;

    [Header("Ping timeout (giây)")]
    [SerializeField] private int pingTimeoutSeconds = 3;

    private bool _server2Online;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        server1Button.onClick.AddListener(OnServer1Selected);
        server2Button.onClick.AddListener(OnServer2Selected);

        server2Button.interactable = false;
        SetServer2Status(ServerStatus.Checking);
        StartCoroutine(PingServer2());
    }

    private void OnDisable()
    {
        server1Button.onClick.RemoveListener(OnServer1Selected);
        server2Button.onClick.RemoveListener(OnServer2Selected);
        StopAllCoroutines();
    }

    // ── Ping Server 2 ─────────────────────────────────────────────────────────

    private IEnumerator PingServer2()
    {
        string baseUrl = ServerConfig.Instance.Server2BaseUrl;
        bool online = false;

        // Thử /health trước
        using (var req = UnityWebRequest.Get($"{baseUrl}/health"))
        {
            req.timeout = pingTimeoutSeconds;
            yield return req.SendWebRequest();
            online = req.result == UnityWebRequest.Result.Success;
        }

        // Fallback về / nếu /health chưa có (build cũ chưa deploy)
        if (!online)
        {
            using var req2 = UnityWebRequest.Get($"{baseUrl}/");
            req2.timeout = pingTimeoutSeconds;
            yield return req2.SendWebRequest();
            online = req2.result == UnityWebRequest.Result.Success;
        }

        _server2Online = online;
        server2Button.interactable = online;
        SetServer2Status(online ? ServerStatus.Online : ServerStatus.Offline);

        Debug.Log(online ? "[UIChannel] Server2 online" : "[UIChannel] Server2 offline");
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnServer1Selected()
    {
        ServerConfig.Instance.activeServer = ServerConfig.ServerMode.PlayFab;
        Debug.Log("[UIChannel] Chọn Server1 — PlayFab");
        OpenAuth();
    }

    private void OnServer2Selected()
    {
        if (!_server2Online) return;
        ServerConfig.Instance.activeServer = ServerConfig.ServerMode.Server2;

        Debug.Log("[UIChannel] Chọn Server2 — Mac Mini");
        OpenAuth();
    }

    private void OpenAuth()
    {
        uiAuthPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    // ── Status display ────────────────────────────────────────────────────────

    private enum ServerStatus { Checking, Online, Offline }

    private void SetServer2Status(ServerStatus status)
    {
        if (server2CheckingIcon) server2CheckingIcon.SetActive(status == ServerStatus.Checking);
        if (server2OnlineIcon)   server2OnlineIcon.SetActive(status == ServerStatus.Online);
        if (server2OfflineIcon)  server2OfflineIcon.SetActive(status == ServerStatus.Offline);

        if (server2StatusText != null)
        {
            server2StatusText.text = status switch
            {
                ServerStatus.Checking => "Đang kiểm tra...",
                ServerStatus.Online   => "Hoạt động",
                ServerStatus.Offline  => "Đang bảo trì",
                _                     => string.Empty
            };
        }
    }
}
