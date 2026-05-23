using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] private List<UIPanel> uiPanels = new List<UIPanel>();
    
    private Dictionary<string, GameObject> uiDictionary = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        // BaseSingleton handles singleton initialization
        base.Awake();
        
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
                Debug.Log($"[UIManager] Registered UI panel: {panel.panelName}");
            }
        }
        Debug.Log($"[UIManager] Total UI panels registered: {uiDictionary.Count}");
    }

    /// <summary>
    /// Show UI panel by name
    /// </summary>
    public void ShowUI(string panelName)
    {
        Debug.Log($"[UIManager] ShowUI called for: {panelName}");
        Debug.Log($"[UIManager] Dictionary contains: {string.Join(", ", uiDictionary.Keys)}");
        
        if (uiDictionary.ContainsKey(panelName))
        {
            GameObject panel = uiDictionary[panelName];
            Debug.Log($"[UIManager] Found panel, setting active: {panelName}");
            Debug.Log($"[UIManager] Panel parent active: {panel.transform.parent?.gameObject.activeSelf}");
            
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                Debug.Log($"[UIManager] CanvasGroup alpha: {canvasGroup.alpha}, blocksRaycasts: {canvasGroup.blocksRaycasts}");
            
            panel.SetActive(true);
            Debug.Log($"[UIManager] Panel active state: {panel.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"[UIManager] UI panel '{panelName}' NOT FOUND in dictionary!");
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
    public GameObject GetPanel(string panelName)
    {
        if (uiDictionary.ContainsKey(panelName))
            return uiDictionary[panelName];
        return null;
    }
}
