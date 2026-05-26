using UnityEngine;

[System.Serializable]
public class BaseEnemyData
{
    public int levelEnemy;
    public float moveSpeed;
    public float hp;
    public int damage;
    public float recover;

    public EnemyAttackType enemyType;       // Melee hoặc Ranged
    public GameObject bulletPrefab;   // Chỉ dùng khi Ranged
}

public enum EnemyAttackType { Melee, Ranged }