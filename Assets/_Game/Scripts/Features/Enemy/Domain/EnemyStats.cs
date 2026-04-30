// public class EnemyStats
// {
//     public float HP { get; private set; }
//     public float MaxHP { get; private set; }
//     public int Damage { get; private set; }

//     public event Action<float, float> OnHealthChanged;
//     public event Action OnDie;

//     public EnemyStats(float hp, int damage)
//     {
//         MaxHP = hp;
//         HP = hp;
//         Damage = damage;
//     }

//     public void TakeDamage(float amount)
//     {
//         HP -= amount;
//         OnHealthChanged?.Invoke(HP, MaxHP);

//         if (HP <= 0)
//         {
//             OnDie?.Invoke();
//         }
//     }
// }