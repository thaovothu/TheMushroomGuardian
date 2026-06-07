using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Auth với Server2 (ASP.NET Core + PostgreSQL) — mirrors PlayFabManager API.
///
/// Cách dùng:
///   GameServer2Manager.Instance.LoginWithEmail(email, password);
///   GameServer2Manager.Instance.RegisterWithEmail(email, password, username);
///   GameServer2Manager.Instance.LoginAsGuest();
///
/// Kết quả broadcast qua GameEvent.Auth.OnLoginSuccess / OnLoginFailed — giống PlayFabManager.
/// </summary>
public class GameServer2Manager : BaseSingleton<GameServer2Manager>
{
    public string PlayerId    { get; private set; }
    public string DisplayName { get; private set; }
    public string Token       { get; private set; }
    public bool   IsLoggedIn  => !string.IsNullOrEmpty(Token);

    private string BaseUrl => ServerConfig.Instance.Server2BaseUrl;

    // ── Public API (mirrors PlayFabManager) ───────────────────────────────────

    public void RegisterWithEmail(string email, string password, string username)
        => StartCoroutine(RegisterCoroutine(email, password, username));

    public void LoginWithEmail(string email, string password)
        => StartCoroutine(LoginCoroutine(email, password));

    public void LoginWithUsername(string username, string password)
        => StartCoroutine(LoginCoroutine(username, password));

    public void LoginAsGuest()
        => StartCoroutine(GuestLoginCoroutine(SystemInfo.deviceUniqueIdentifier));

    public void Logout()
    {
        Token       = null;
        PlayerId    = null;
        DisplayName = null;
        GameEvent.Auth.OnLogout?.Invoke();
        Debug.Log("[Server2] Logged out");
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator RegisterCoroutine(string email, string password, string username)
    {
        var body = JsonUtility.ToJson(new RegisterRequest { email = email, password = password, username = username });
        using var req = MakePost($"{BaseUrl}/api/auth/register", body);

        Debug.Log($"[Server2] Registering: {username}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            GameEvent.Auth.OnLoginFailed?.Invoke(ParseServerError(req.downloadHandler.text));
            yield break;
        }

        ApplyAuthResponse(JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text));
    }

    private IEnumerator LoginCoroutine(string emailOrUsername, string password)
    {
        var body = JsonUtility.ToJson(new LoginRequest { emailOrUsername = emailOrUsername, password = password });
        using var req = MakePost($"{BaseUrl}/api/auth/login", body);

        Debug.Log("[Server2] Logging in...");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            GameEvent.Auth.OnLoginFailed?.Invoke(ParseServerError(req.downloadHandler.text));
            yield break;
        }

        ApplyAuthResponse(JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text));
    }

    private IEnumerator GuestLoginCoroutine(string deviceId)
    {
        var body = JsonUtility.ToJson(new GuestLoginRequest { deviceId = deviceId });
        using var req = MakePost($"{BaseUrl}/api/auth/login-guest", body);

        Debug.Log($"[Server2] Guest login: {deviceId}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            GameEvent.Auth.OnLoginFailed?.Invoke(ParseServerError(req.downloadHandler.text));
            yield break;
        }

        ApplyAuthResponse(JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ApplyAuthResponse(AuthResponse response)
    {
        Token       = response.token;
        PlayerId    = response.playerId;
        DisplayName = response.displayName;
        Debug.Log($"[Server2] Login OK — PlayerId: {PlayerId}, DisplayName: {DisplayName}");
        GameEvent.Auth.OnLoginSuccess?.Invoke(PlayerId);
    }

    private static string ParseServerError(string responseBody)
    {
        try
        {
            var err = JsonUtility.FromJson<ErrorResponse>(responseBody);
            return err?.message ?? responseBody;
        }
        catch { return responseBody; }
    }

    private static UnityWebRequest MakePost(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    // ── DTOs (private — chỉ dùng nội bộ) ────────────────────────────────────

    [System.Serializable] private class RegisterRequest   { public string email, password, username; }
    [System.Serializable] private class LoginRequest      { public string emailOrUsername, password; }
    [System.Serializable] private class GuestLoginRequest { public string deviceId; }
    [System.Serializable] private class AuthResponse      { public string token, playerId, displayName; }
    [System.Serializable] private class ErrorResponse     { public string message; }
}
