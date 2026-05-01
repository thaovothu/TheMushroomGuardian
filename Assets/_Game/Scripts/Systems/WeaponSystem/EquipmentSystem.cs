using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponType
{
    None = 0,
    Bow = 1,
    Sword = 2
}
public enum WeaponButton
{
    NoneButton,
    BowButton,
    SwordButton
}

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject weaponSheath;
    [SerializeField] WeaponType currentWeaponType ;
    [SerializeField] GameObject[] weaponList;
    [SerializeField] Button weaponNone;
    [SerializeField] Button weaponBow;
    [SerializeField] Button weaponSword;
    GameObject currentWeaponInSheath;
    GameObject currentWeaponInHand;

    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController PlayerController;

    [SerializeField] private AnimatorOverrideController Sword_Override;
    [SerializeField] private AnimatorOverrideController Bow_Override;

    void Start()
    {
        currentWeaponType = WeaponType.None;
        // DrawWeapon();
        // SheathWeapon();
    }
    void OnEnable()
    {
        weaponBow.onClick.AddListener(() => OnClickButton(WeaponButton.BowButton));
        weaponSword.onClick.AddListener(() => OnClickButton(WeaponButton.SwordButton));
        weaponNone.onClick.AddListener(() => OnClickButton(WeaponButton.NoneButton));
    }
    private void OnDisable() {
        weaponBow.onClick.RemoveAllListeners();
        weaponSword.onClick.RemoveAllListeners();
        weaponNone.onClick.RemoveAllListeners();
    }

    public bool IsAttackNormal(){
        //Debug.Log("Current weapon type:asdasda " + currentWeaponType);
        if (currentWeaponType == WeaponType.None) return true;
        return false;
    }
    public void SetWeaponInHand()
    {
        //Debug.Log("currentWeaponType"+ currentWeaponType);
        SetWeaponAnim(currentWeaponType);
    }
    public void ChangeWeapon(WeaponType type)
    {
        //Debug.Log("Change weapon to " + type);
        if (currentWeaponType == type) return;
        Destroy(currentWeaponInHand);
        currentWeaponType = type;
        //Debug.Log("currentWeaponTypeHIHIHI"+ currentWeaponType);
        DrawWeapon();
        SetWeaponAnim(type);
    }

    void SetWeaponAnim(WeaponType type)
    {
        //Debug.Log("Set weapon animation for " + type);
        switch (type)
        {
            case WeaponType.None:
                animator.runtimeAnimatorController = PlayerController;
                break;

            case WeaponType.Sword:
                animator.runtimeAnimatorController = Sword_Override;
                break;

            case WeaponType.Bow:
                animator.runtimeAnimatorController = Bow_Override;
                break;
        }
    }

    public void DrawWeapon()
    {
        currentWeaponInHand = Instantiate(weaponList[(int)currentWeaponType], weaponHolder.transform);
        //Debug.Log("Current weapon in hand: " + (int)currentWeaponType);
    }

    public void SheathWeapon()
    {
        currentWeaponInSheath = Instantiate(weaponList[(int)currentWeaponType], weaponSheath.transform);
    }

    public void OnClickButton(WeaponButton button)
    {
        switch (button)
        {
            case WeaponButton.BowButton:
                //Debug.Log("Bow button clicked");
                ChangeWeapon(WeaponType.Bow);
                break;
            case WeaponButton.SwordButton:
                //Debug.Log("Sword button clicked");
                ChangeWeapon(WeaponType.Sword);
                break;
            case WeaponButton.NoneButton:
                //Debug.Log("None button clicked");
                ChangeWeapon(WeaponType.None);
                break;
        }
    }

    public void StartDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().StartDealDamage();
    }
    public void EndDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().EndDealDamage();
    }
}
