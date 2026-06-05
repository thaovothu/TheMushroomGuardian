using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

/// <summary>
/// Quản lý auth với PlayFab — Register, Login, Guest Login.
///
/// Cách dùng:
///   PlayFabManager.Instance.LoginWithEmail(email, password);
///   PlayFabManager.Instance.RegisterWithEmail(email, password, username);
///   PlayFabManager.Instance.LoginAsGuest();
///
/// Kết quả được broadcast qua GameEvent.Auth.OnLoginSuccess / OnLoginFailed.
/// </summary>
public class PlayFabManager : BaseSingleton<PlayFabManager>
{
    public string PlayFabId { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(PlayFabId);

    // ── Public API ────────────────────────────────────────────────────────────

    public void RegisterWithEmail(string email, string password, string username)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            Username = username,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnFailure);
        Debug.Log($"[PlayFab] Registering user: {username}");
    }

    public void LoginWithEmail(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnFailure);
        Debug.Log("[PlayFab] Logging in...");
    }

    public void LoginWithUsername(string username, string password)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnFailure);
        Debug.Log("[PlayFab] Logging in with username...");
    }

    /// <summary>Login không cần tài khoản — dùng DeviceId làm key. Phù hợp cho guest hoặc test.</summary>
    public void LoginAsGuest()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = deviceId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnFailure);
        Debug.Log($"[PlayFab] Guest login: {deviceId}");
    }

    public void Logout()
    {
        PlayFabId = null;
        DisplayName = null;
        GameEvent.Auth.OnLogout?.Invoke();
        Debug.Log("[PlayFab] Logged out");
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        PlayFabId = result.PlayFabId;
        DisplayName = result.Username;
        Debug.Log($"[PlayFab] Register OK — PlayFabId: {PlayFabId}");
        GameEvent.Auth.OnLoginSuccess?.Invoke(PlayFabId);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        DisplayName = result.InfoResultPayload?.PlayerProfile?.DisplayName;
        Debug.Log($"[PlayFab] Login OK — PlayFabId: {PlayFabId}, NewAccount: {result.NewlyCreated}");
        GameEvent.Auth.OnLoginSuccess?.Invoke(PlayFabId);
    }

    private void OnFailure(PlayFabError error)
    {
        string message = error.GenerateErrorReport();
        Debug.LogError($"[PlayFab] Error: {message}");
        GameEvent.Auth.OnLoginFailed?.Invoke(message);
    }
}
