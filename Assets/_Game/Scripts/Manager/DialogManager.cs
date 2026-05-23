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
    /// Chơi toàn bộ dialog conversation của NPC
    /// </summary>
    public void PlayDialog(int npcId)
    {
        if (uiDialog != null)
        {
            uiDialog.PlayDialog(npcId);
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
