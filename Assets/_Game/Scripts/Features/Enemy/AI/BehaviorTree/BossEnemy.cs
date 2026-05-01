using UnityEngine;
public class BossEnemy : MonoBehaviour
{
    [SerializeField] ElementType bossElement = ElementType.Earth;

    HealthSystem _health;

    void Awake()
    {
        _health = GetComponent<HealthSystem>();
        HealthSystem.OnDeath += OnBossDeath;
    }

    void OnDestroy()
    {
        HealthSystem.OnDeath -= OnBossDeath;
    }

    void OnBossDeath(HealthSystem hs)
    {
        if (hs != _health) return;
        // drop crystal, trigger quest event, v.v.
        //Debug.Log($"[Boss] Dead — drop {bossElement} crystal");
    }

    public ElementType GetElement() => bossElement;
}
