using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // int levelEnemy;
    float moveSpeed;
    float hp;
    int damage;
    float recover;

    public void Init(BaseEnemyData data)
    {
        // levelEnemy = data.levelEnemy;
        moveSpeed = data.moveSpeed;
        hp = data.hp;
        damage = data.damage;
        recover = data.recover;
    }
}
