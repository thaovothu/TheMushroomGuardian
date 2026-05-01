using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDameGenerator : MonoBehaviour
{
    public static UIDameGenerator current;
    public GameObject prefabDamageText;

    void Awake()
    {
        current = this;
    }
    
    /// <summary>
    /// Hiển thị damage text - Player nhận damage thì yellow, Enemy/Boss nhận damage thì red
    /// </summary>
    public void ShowDamageText(Vector3 position, float damageAmount, bool isPlayerTakingDamage)
    {
        string text = ((int)damageAmount).ToString();
        Color color = isPlayerTakingDamage ? Color.red : Color.yellow;
        Debug.Log($"[UIDameGenerator] ShowDamageText called: {text} damage at {position}, isPlayer taking: {isPlayerTakingDamage}, color: {color}");
        CreateDameTextPopUp(position, text, color);
    }
    
    public void CreateDameTextPopUp(Vector3 position, string text, Color color)
    {
        var popup = Instantiate(prefabDamageText, position, Quaternion.identity);
        var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        temp.text = text;
        temp.faceColor = color;
        Debug.Log($"[UIDameGenerator] Damage text created: {text}");
        Destroy(popup, 2f);
    }
}
