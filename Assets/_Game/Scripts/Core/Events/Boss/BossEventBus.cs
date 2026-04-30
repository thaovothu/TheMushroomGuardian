using System;

public static class BossEventBus
{
    public static System.Action<ElementType> OnElementChanged;
    public static System.Action<float> OnPhaseChanged;
    public static System.Action OnBossDeath;
}

