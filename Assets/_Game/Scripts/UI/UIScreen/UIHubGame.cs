using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHubGame : MonoBehaviour
{
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button questBtn;
    [SerializeField] private UIInventory uIInventory;
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
        uIInventory.Show();
    }
    private void OnQuestBtnClicked()
    {
        questUI.Show();
    }
}
