using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHubGame : MonoBehaviour
{
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private InventoryUI inventoryUI;
    void OnEnable()
    {
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);
    }
    void OnDisable()
    {
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);
    }

    private void OnInventoryBtnClicked()
    {
        inventoryUI.Show();
    }
}
