using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData
{
    [SerializeField] public int weaponId;
    [SerializeField] public string weaponName;
    [SerializeField] public float damageBonus;      // 0.2f = +20%, 0.1f = +10%
    [SerializeField] public float attackSpeed;     // giây (1s = 1f, 3s = 3f)

    public WeaponData(int weaponId, string weaponName, float damageBonus, float attackSpeed)
    {
        this.weaponId = weaponId;
        this.weaponName = weaponName;
        this.damageBonus = damageBonus;
        this.attackSpeed = attackSpeed;
    }
}

[CreateAssetMenu(fileName = "WeaponConfig", menuName = "EntityData/Weapon")]
public class WeaponSO : ScriptableObject
{
    [SerializeField] private List<WeaponData> weapons = new();

    private Dictionary<int, WeaponData> weaponLookup;
    private Dictionary<string, WeaponData> weaponNameLookup;
    private bool isInitialized = false;

    private void Init()
    {
        if (isInitialized) return;

        weaponLookup = new Dictionary<int, WeaponData>();
        weaponNameLookup = new Dictionary<string, WeaponData>();

        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogWarning($"WeaponSO '{name}': No weapon data configured!");
            return;
        }

        foreach (var weapon in weapons)
        {
            if (weapon == null)
            {
                Debug.LogWarning($"WeaponSO '{name}': Null weapon data found, skipping.");
                continue;
            }

            // Kiểm tra duplicate ID
            if (weaponLookup.ContainsKey(weapon.weaponId))
            {
                Debug.LogWarning($"WeaponSO '{name}': Duplicate weapon ID '{weapon.weaponId}', skipping.");
                continue;
            }

            // Kiểm tra duplicate name
            if (!string.IsNullOrEmpty(weapon.weaponName) && weaponNameLookup.ContainsKey(weapon.weaponName))
            {
                Debug.LogWarning($"WeaponSO '{name}': Duplicate weapon name '{weapon.weaponName}', skipping.");
                continue;
            }

            weaponLookup.Add(weapon.weaponId, weapon);
            if (!string.IsNullOrEmpty(weapon.weaponName))
            {
                weaponNameLookup.Add(weapon.weaponName, weapon);
            }
        }

        isInitialized = true;
    }
    public WeaponData GetWeapon(int weaponId)
    {
        Init();

        if (weaponLookup.TryGetValue(weaponId, out WeaponData weapon))
        {
            return weapon;
        }

        Debug.LogWarning($"WeaponSO '{name}': Weapon ID {weaponId} not found!");
        return null;
    }
    public WeaponData GetWeaponByName(string weaponName)
    {
        Init();

        if (weaponNameLookup.TryGetValue(weaponName, out WeaponData weapon))
        {
            return weapon;
        }

        Debug.LogWarning($"WeaponSO '{name}': Weapon '{weaponName}' not found!");
        return null;
    }

    public float GetDamageBonus(int weaponId)
    {
        var weapon = GetWeapon(weaponId);
        return weapon?.damageBonus ?? 0f;
    }

    public float GetAttackSpeed(int weaponId)
    {
        var weapon = GetWeapon(weaponId);
        return weapon?.attackSpeed ?? 0f;
    }

    public int CalculateTotalDamage(int baseDamage, int weaponId)
    {
        float damageBonus = GetDamageBonus(weaponId);
        return Mathf.RoundToInt(baseDamage * (1f + damageBonus));
    }

    public List<WeaponData> GetAllWeapons()
    {
        Init();
        return new List<WeaponData>(weapons);
    }

    /// <summary>
    /// Kiểm tra vũ khí tồn tại
    /// </summary>
    public bool WeaponExists(int weaponId)
    {
        Init();
        return weaponLookup.ContainsKey(weaponId);
    }


    public int GetWeaponCount()
    {
        Init();
        return weaponLookup.Count;
    }
}