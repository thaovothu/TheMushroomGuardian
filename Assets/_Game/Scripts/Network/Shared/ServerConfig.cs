using UnityEngine;

/// <summary>
/// ScriptableObject chọn server đang active và chứa URL.
/// Tạo trong Editor: Assets > Create > Game > ServerConfig
/// Đặt file vào thư mục Resources/ để load được bằng Resources.Load.
/// </summary>
[CreateAssetMenu(fileName = "ServerConfig", menuName = "Game/ServerConfig")]
public class ServerConfig : ScriptableObject
{
    private static ServerConfig _instance;
    public static ServerConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ServerConfig>("ServerConfig");
            if (_instance == null)
            {
                // Tạo instance mặc định trong memory — không crash, nhưng settings không persist.
                // Fix: Assets > Create > Game > ServerConfig → đặt vào Assets/Resources/
                _instance = CreateInstance<ServerConfig>();
                Debug.LogWarning("[ServerConfig] Chưa có asset trong Resources/ — dùng config mặc định (PlayFab). Tạo asset để lưu settings.");
            }
            return _instance;
        }
    }

    public enum ServerMode { PlayFab, Server2 }

    [Header("Chọn server đang sử dụng")]
    public ServerMode activeServer = ServerMode.PlayFab;

    [Header("Server 2 — ASP.NET Core + PostgreSQL")]
    [Tooltip("URL của server2. Local: http://localhost:5001 | Mac Mini LAN: http://192.168.x.x:5001")]
    public string server2BaseUrl = "http://localhost:5001";

    public bool   IsServer2Active => activeServer == ServerMode.Server2;
    public string Server2BaseUrl  => server2BaseUrl;
}
