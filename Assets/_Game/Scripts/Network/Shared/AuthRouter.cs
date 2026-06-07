/// <summary>
/// Routing layer giữa UIAuth và các auth manager.
/// UIAuth gọi AuthRouter thay vì gọi thẳng PlayFabManager — không cần biết server nào đang active.
///
/// Cách dùng:
///   AuthRouter.Login(emailOrUsername, password);
///   AuthRouter.Register(email, password, username);
///   AuthRouter.LoginAsGuest();
/// </summary>
public static class AuthRouter
{
    private static bool IsServer2 => ServerConfig.Instance.IsServer2Active;

    public static void Login(string emailOrUsername, string password)
    {
        if (IsServer2)
        {
            GameServer2Manager.Instance.LoginWithEmail(emailOrUsername, password);
            return;
        }

        if (IsEmail(emailOrUsername))
            PlayFabManager.Instance.LoginWithEmail(emailOrUsername, password);
        else
            PlayFabManager.Instance.LoginWithUsername(emailOrUsername, password);
    }

    public static void Register(string email, string password, string username)
    {
        if (IsServer2)
            GameServer2Manager.Instance.RegisterWithEmail(email, password, username);
        else
            PlayFabManager.Instance.RegisterWithEmail(email, password, username);
    }

    public static void LoginAsGuest()
    {
        if (IsServer2)
            GameServer2Manager.Instance.LoginAsGuest();
        else
            PlayFabManager.Instance.LoginAsGuest();
    }

    private static bool IsEmail(string input)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address == input.Trim();
        }
        catch { return false; }
    }
}
