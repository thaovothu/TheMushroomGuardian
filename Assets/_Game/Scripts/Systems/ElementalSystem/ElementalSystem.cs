
using UnityEngine;
using System.Collections;

public static class ElementalSystem
{
    // Bảng từ tài liệu M2:
    // Attacker → Defender → x1.5
    // Earth → Wind, Wind → Water, Water → Fire, Fire → Earth
    // Ngược lại → x0.7

    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        if (attacker == ElementType.None || defender == ElementType.None)
            return 1f;

        if (IsCounter(attacker, defender)) return 1.5f;
        if (IsCounter(defender, attacker)) return 0.7f;  // bị phản lại
        return 1f;
    }

    static bool IsCounter(ElementType a, ElementType b)
    {
        return (a == ElementType.Earth && b == ElementType.Wind) ||
               (a == ElementType.Wind && b == ElementType.Water) ||
               (a == ElementType.Water && b == ElementType.Fire) ||
               (a == ElementType.Fire && b == ElementType.Earth);
    }
}
