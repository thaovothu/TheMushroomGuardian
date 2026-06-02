using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHubGame : MonoBehaviour
{
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button questBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private UIInventory uIInventory;
    [SerializeField] private UIQuestPanel questUI;
    [SerializeField] private UIShop shopUI;

    void OnEnable()
    {
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);
        questBtn.onClick.AddListener(OnQuestBtnClicked);
        shopBtn.onClick.AddListener(OnShopBtnClicked);
    }
    void OnDisable()
    {
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);
        questBtn.onClick.RemoveListener(OnQuestBtnClicked);
        shopBtn.onClick.RemoveListener(OnShopBtnClicked);
    }

    private void OnInventoryBtnClicked()
    {
        uIInventory.Show();
    }
    private void OnQuestBtnClicked()
    {
        questUI.Show();
    }
    private void OnShopBtnClicked()
    {
        shopUI.Show();
    }
}
