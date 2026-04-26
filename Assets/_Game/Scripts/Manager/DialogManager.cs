using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private UIDialog uiDialog;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Chơi dialog từ DialogSO
    /// </summary>
    public void PlayDialog(DialogSO dialogSO)
    {
        if (uiDialog != null)
        {
            uiDialog.PlayDialog(dialogSO);
        }
        else
        {
            Debug.LogWarning("[DialogManager] UIDialog is not assigned!");
        }
    }

    /// <summary>
    /// Dừng dialog hiện tại
    /// </summary>
    public void StopDialog()
    {
        if (uiDialog != null && uiDialog.IsPlaying())
        {
            uiDialog.StopDialog();
        }
    }

    /// <summary>
    /// Kiểm tra dialog có đang chạy không
    /// </summary>
    public bool IsDialogPlaying()
    {
        return uiDialog != null && uiDialog.IsPlaying();
    }
}
