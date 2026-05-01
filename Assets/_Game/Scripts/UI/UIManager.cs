using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private List<UIPanel> uiPanels = new List<UIPanel>();
    
    private Dictionary<string, GameObject> uiDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize UI dictionary
        InitializeUIPanels();
    }

    void InitializeUIPanels()
    {
        foreach (var panel in uiPanels)
        {
            if (panel.uiObject != null)
            {
                uiDictionary[panel.panelName] = panel.uiObject;
                // Hide all panels by default
                panel.uiObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Show UI panel by name
    /// </summary>
    public void ShowUI(string panelName)
    {
        //Debug.Log("hihi" + panelName);
        if (uiDictionary.ContainsKey(panelName))
        {
            uiDictionary[panelName].SetActive(true);
            //Debug.Log($"[UIManager] Showing UI: {panelName}");
        }
        else
        {
            //Debug.LogWarning($"[UIManager] UI panel '{panelName}' not found!");
        }
    }

    /// <summary>
    /// Hide UI panel by name
    /// </summary>
    public void HideUI(string panelName)
    {
        if (uiDictionary.ContainsKey(panelName))
        {
            uiDictionary[panelName].SetActive(false);
            //Debug.Log($"[UIManager] Hiding UI: {panelName}");
        }
        else
        {
            //Debug.LogWarning($"[UIManager] UI panel '{panelName}' not found!");
        }
    }

    /// <summary>
    /// Toggle UI panel visibility
    /// </summary>
    public void ToggleUI(string panelName)
    {
        if (uiDictionary.ContainsKey(panelName))
        {
            uiDictionary[panelName].SetActive(!uiDictionary[panelName].activeSelf);
            //Debug.Log($"[UIManager] Toggling UI: {panelName}");
        }
        else
        {
            //Debug.LogWarning($"[UIManager] UI panel '{panelName}' not found!");
        }
    }

    /// <summary>
    /// Hide all UI panels
    /// </summary>
    public void HideAllUI()
    {
        foreach (var panel in uiDictionary.Values)
        {
            panel.SetActive(false);
        }
        //Debug.Log("[UIManager] Hiding all UI panels");
    }

    /// <summary>
    /// Check if UI panel is active
    /// </summary>
    public bool IsUIActive(string panelName)
    {
        if (uiDictionary.ContainsKey(panelName))
        {
            return uiDictionary[panelName].activeSelf;
        }
        return false;
    }

    [System.Serializable]
    public class UIPanel
    {
        public string panelName = "PanelName";
        public GameObject uiObject;
    }
}
