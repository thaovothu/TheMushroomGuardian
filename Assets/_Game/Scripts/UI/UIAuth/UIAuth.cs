using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Login / Register — 2 tab chuyển đổi nhau.
/// Sau khi login thành công → ẩn panel, fire GameEvent.Auth.OnLoginSuccess.
///
/// Setup Editor:
///   - loginPanel / registerPanel: 2 sub-panel ẩn hiện theo tab
///   - loadingPanel: hiện khi đang chờ response
///   - errorText: hiện lỗi từ PlayFab
/// </summary>
public class UIAuth : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Login Inputs")]
    [SerializeField] private TMP_InputField loginEmail;       // email hoặc username
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private Button loginButton;

    [Header("Register Inputs")]
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private Button registerButton;

    [Header("Tabs")]
    [SerializeField] private Toggle tabLoginToggle;
    [SerializeField] private Toggle tabRegisterToggle;

    [Header("Other")]
    [SerializeField] private Button guestButton;
    [SerializeField] private TextMeshProUGUI errorText;

    private void OnEnable()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        guestButton.onClick.AddListener(OnGuestClicked);
        tabLoginToggle.onValueChanged.AddListener(isOn => { if (isOn) SwitchTab(true); });
        tabRegisterToggle.onValueChanged.AddListener(isOn => { if (isOn) SwitchTab(false); });

        GameEvent.Auth.OnLoginSuccess += OnLoginSuccess;
        GameEvent.Auth.OnLoginFailed  += OnLoginFailed;

        SwitchTab(true);
        SetLoading(false);
        ClearError();
    }

    private void OnDisable()
    {
        loginButton.onClick.RemoveListener(OnLoginClicked);
        registerButton.onClick.RemoveListener(OnRegisterClicked);
        guestButton.onClick.RemoveListener(OnGuestClicked);
        tabLoginToggle.onValueChanged.RemoveAllListeners();
        tabRegisterToggle.onValueChanged.RemoveAllListeners();

        GameEvent.Auth.OnLoginSuccess -= OnLoginSuccess;
        GameEvent.Auth.OnLoginFailed  -= OnLoginFailed;
    }

    // ── Tab ───────────────────────────────────────────────────────────────────

    private void SwitchTab(bool showLogin)
    {
        loginPanel.SetActive(showLogin);
        registerPanel.SetActive(!showLogin);
        ClearError();
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnLoginClicked()
    {
        string input = loginEmail.text.Trim();
        string password = loginPassword.text;

        if (string.IsNullOrWhiteSpace(input))    { ShowError("Vui lòng nhập email hoặc username."); return; }
        if (string.IsNullOrWhiteSpace(password)) { ShowError("Vui lòng nhập mật khẩu."); return; }
        if (password.Length < 6)                 { ShowError("Mật khẩu tối thiểu 6 ký tự."); return; }

        SetLoading(true);
        AuthRouter.Login(input, password);
    }

    private void OnRegisterClicked()
    {
        if (string.IsNullOrWhiteSpace(registerUsername.text))
        {
            ShowError("Vui lòng nhập tên người dùng.");
            return;
        }
        if (!ValidateInputs(registerEmail.text, registerPassword.text)) return;

        SetLoading(true);
        AuthRouter.Register(
            registerEmail.text.Trim(),
            registerPassword.text,
            registerUsername.text.Trim());
    }

    private void OnGuestClicked()
    {
        SetLoading(true);
        AuthRouter.LoginAsGuest();
    }

    // ── PlayFab callbacks ─────────────────────────────────────────────────────

    private void OnLoginSuccess(string playFabId)
    {
        SetLoading(false);
        gameObject.SetActive(false);
    }

    private void OnLoginFailed(string error)
    {
        SetLoading(false);
        ShowError(ParseError(error));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool ValidateInputs(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))    { ShowError("Vui lòng nhập email."); return false; }
        if (!IsValidEmail(email))                 { ShowError("Email không đúng định dạng."); return false; }
        if (string.IsNullOrWhiteSpace(password)) { ShowError("Vui lòng nhập mật khẩu."); return false; }
        if (password.Length < 6)                 { ShowError("Mật khẩu tối thiểu 6 ký tự."); return false; }
        return true;
    }

    private bool IsValidEmail(string email)
    {
        try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email.Trim(); }
        catch { return false; }
    }

    private void SetLoading(bool isLoading)
    {
        loadingPanel.SetActive(isLoading);
        loginButton.interactable    = !isLoading;
        registerButton.interactable = !isLoading;
        guestButton.interactable    = !isLoading;
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    private string ParseError(string rawError)
    {
        // PlayFab errors
        if (rawError.Contains("InvalidEmailOrPassword") || rawError.Contains("Invalid email address or password") || rawError.Contains("Invalid username or password"))
            return "Email/username hoặc mật khẩu không đúng.";
        if (rawError.Contains("EmailAddressNotAvailable")) return "Email này đã được sử dụng.";
        if (rawError.Contains("UsernameNotAvailable"))     return "Tên người dùng đã tồn tại.";
        if (rawError.Contains("InvalidPassword"))          return "Mật khẩu không hợp lệ (tối thiểu 6 ký tự).";
        if (rawError.Contains("Invalid input parameters")) return "Thông tin không hợp lệ. Kiểm tra lại email và mật khẩu.";

        // Server2 errors
        if (rawError.Contains("Invalid credentials"))  return "Email/username hoặc mật khẩu không đúng.";
        if (rawError.Contains("Email already in use")) return "Email này đã được sử dụng.";
        if (rawError.Contains("Username already taken")) return "Tên người dùng đã tồn tại.";

        Debug.LogWarning($"[UIAuth] Unhandled error: {rawError}");
        return "Đã có lỗi xảy ra. Vui lòng thử lại.";
    }
}
