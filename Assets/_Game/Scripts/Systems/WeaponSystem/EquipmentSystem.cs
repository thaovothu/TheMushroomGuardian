using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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

public class EquipmentSystem : BaseSingleton<EquipmentSystem>
{
    const string WeaponHolderRightName = "WeaponHolderRight";
    const string WeaponHolderLeftName = "WeaponHolderLeft";
    const string PlayerTag = "Player";

    [SerializeField] Transform weaponHolderRight;
    [SerializeField] Transform weaponHolderLeft;
    [SerializeField] WeaponType currentWeaponType ;
    [SerializeField] GameObject[] weaponList;
    [SerializeField] Button weaponNone;
    [SerializeField] Button weaponBow;
    [SerializeField] Button weaponSword;
    GameObject currentWeaponInHandRight;
    GameObject currentWeaponInHandLeft;

    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController PlayerController;

    [SerializeField] private AnimatorOverrideController Sword_Override;
    [SerializeField] private AnimatorOverrideController Bow_Override;

    protected override void Awake()
    {
        base.Awake();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Listen khi player được spawn xong
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
        if (weaponBow != null) weaponBow.onClick.AddListener(() => OnClickButton(WeaponButton.BowButton));
        if (weaponSword != null) weaponSword.onClick.AddListener(() => OnClickButton(WeaponButton.SwordButton));
        if (weaponNone != null) weaponNone.onClick.AddListener(() => OnClickButton(WeaponButton.NoneButton));
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvent.Player.OnSpawned -= OnPlayerSpawned;
        if (weaponBow != null) weaponBow.onClick.RemoveAllListeners();
        if (weaponSword != null) weaponSword.onClick.RemoveAllListeners();
        if (weaponNone != null) weaponNone.onClick.RemoveAllListeners();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset khi load scene mới
        currentWeaponType = WeaponType.None;
        currentWeaponInHandRight = null;
        currentWeaponInHandLeft = null;
    }

    void OnPlayerSpawned(GameObject player)
    {
        Debug.Log("[EquipmentSystem] OnPlayerSpawned called - resolving weapon holders...");
        TryResolveWeaponHolders();
        TryResolveAnimator();
        RebuildWeaponVisuals();
    }

    void TryResolveWeaponHolders()
    {
        // Tìm player object
        var playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
        if (playerObject == null)
        {
            Debug.LogError("[EquipmentSystem] Cannot find Player object with tag 'Player'!");
            return;
        }

        Debug.Log($"[EquipmentSystem] Found player: {playerObject.name}");
        
        // Debug: log tất cả children của player
        Debug.Log($"[EquipmentSystem] Player children: {string.Join(", ", System.Linq.Enumerable.Select(playerObject.GetComponentsInChildren<Transform>(), t => t.name))}");

        // Tìm weapon holders trong hierarchy của player (recursive)
        if (weaponHolderRight == null)
        {
            weaponHolderRight = FindTransformRecursive(playerObject.transform, WeaponHolderRightName);
            if (weaponHolderRight == null)
                Debug.LogError($"[EquipmentSystem] Cannot find '{WeaponHolderRightName}' in player hierarchy!");
            else
                Debug.Log($"[EquipmentSystem] Found WeaponHolderRight: {weaponHolderRight.name}");
        }

        if (weaponHolderLeft == null)
        {
            weaponHolderLeft = FindTransformRecursive(playerObject.transform, WeaponHolderLeftName);
            if (weaponHolderLeft == null)
                Debug.LogError($"[EquipmentSystem] Cannot find '{WeaponHolderLeftName}' in player hierarchy!");
            else
                Debug.Log($"[EquipmentSystem] Found WeaponHolderLeft: {weaponHolderLeft.name}");
        }
    }

    Transform FindTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindTransformRecursive(child, name);
            if (result != null)
                return result;
        }

        return null;
    }

    void TryResolveAnimator()
    {
        if (animator != null)
            return;

        var playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
        if (playerObject == null)
            return;

        animator = playerObject.GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogWarning("[EquipmentSystem] Cannot find Animator on Player object.");
    }

    void RebuildWeaponVisuals()
    {
        if (currentWeaponType == WeaponType.None)
            return;

        if (weaponHolderRight == null || weaponHolderLeft == null)
        {
            Debug.LogWarning("[EquipmentSystem] Weapon holders not resolved yet, skipping visual rebuild.");
            return;
        }

        if (currentWeaponInHandRight != null)
            Destroy(currentWeaponInHandRight);
        if (currentWeaponInHandLeft != null)
            Destroy(currentWeaponInHandLeft);

        currentWeaponInHandRight = null;
        currentWeaponInHandLeft = null;

        if (currentWeaponType == WeaponType.Bow)
            DrawWeaponLeft();
        else if (currentWeaponType == WeaponType.Sword)
            DrawWeaponRight();
    }

    public bool IsAttackNormal(){
        Debug.Log("Current weapon type:asdasda " + currentWeaponType);
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
        Debug.Log("Change weapon to " + type);
        if (currentWeaponType == type) return;
        
        if (currentWeaponInHandRight != null)
            Destroy(currentWeaponInHandRight);
        if (currentWeaponInHandLeft != null)
            Destroy(currentWeaponInHandLeft);

        currentWeaponType = type;
        if (currentWeaponType == WeaponType.Bow)
            DrawWeaponLeft();
        else if (currentWeaponType == WeaponType.Sword)
            DrawWeaponRight();
        SetWeaponAnim(type);
    }

    void SetWeaponAnim(WeaponType type)
    {
        TryResolveAnimator();
        if (animator == null)
            return;

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

    public void DrawWeaponRight()
    {
        if (weaponHolderRight == null)
        {
            Debug.LogError("[EquipmentSystem] weaponHolderRight is null! Cannot draw weapon.");
            return;
        }
        currentWeaponInHandRight = Instantiate(weaponList[(int)currentWeaponType], weaponHolderRight.transform);
    }

    public void DrawWeaponLeft()
    {
        if (weaponHolderLeft == null)
        {
            Debug.LogError("[EquipmentSystem] weaponHolderLeft is null! Cannot draw weapon.");
            return;
        }
        currentWeaponInHandLeft = Instantiate(weaponList[(int)currentWeaponType], weaponHolderLeft.transform);
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
        if (currentWeaponType == WeaponType.Sword)
        {
            currentWeaponInHandRight.GetComponentInChildren<SwordAttack>()?.StartDealDamage();
        }
        else if (currentWeaponType == WeaponType.Bow)
        {
            currentWeaponInHandLeft.GetComponentInChildren<BowAttack>()?.FireArrow();
        }
    }
    public void EndDealDamage()
    {
        if (currentWeaponType == WeaponType.Sword)
        {
            currentWeaponInHandRight.GetComponentInChildren<SwordAttack>()?.EndDealDamage();
        }
    }
}
