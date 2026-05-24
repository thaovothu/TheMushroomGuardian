using System;
using UnityEngine;
public static class BossEventBus
{
    public static Action<GameObject> OnBossSpawned; // ← thêm dòng này
    public static Action<ElementType> OnElementChanged;
    public static Action<float> OnPhaseChanged;
    public static Action OnBossDeath;
}