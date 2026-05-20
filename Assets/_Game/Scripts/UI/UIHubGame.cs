using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHubGame : MonoBehaviour
{
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button questBtn;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private UIQuestPanel questUI;

    void OnEnable()
    {
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);
        questBtn.onClick.AddListener(OnQuestBtnClicked);
    }
    void OnDisable()
    {
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);
        questBtn.onClick.RemoveListener(OnQuestBtnClicked);
    }

    private void OnInventoryBtnClicked()
    {
        inventoryUI.Show();
    }
    private void OnQuestBtnClicked()
    {
        questUI.Show();
    }
}
