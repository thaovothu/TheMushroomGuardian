// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.AI;
// public class EnemyChaseState : EnemyBaseState
// {
//     readonly NavMeshAgent agent;
//     readonly Transform player;
//     public EnemyChaseState(EnemyController enemyController, Animator animator, NavMeshAgent agent, Transform player) : base(enemyController, animator)
//     {
//         this.agent = agent;
//         this.player = player;
//     }
//     public override void OnEnter()
//     {
//         _animator.CrossFade(RunHash,crossFadeDuration);
//     }

//     public override void Update()
//     {
//         if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
//         agent.SetDestination(player.position);
//     }
// }


using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyChaseState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Transform player;

    // Kiểm soát tần suất tính lại đường ở phía client
    readonly float repathInterval = 0.15f;   // tối thiểu 0.15s giữa 2 lần cập nhật đích
    readonly float repathThreshold = 0.5f;   // player phải dời quá 0.5 đơn vị mới tính lại
    float repathTimer;
    Vector3 lastDestination;

    public EnemyChaseState(EnemyController enemyController, Animator animator, NavMeshAgent agent, Transform player) : base(enemyController, animator)
    {
        this.agent = agent;
        this.player = player;
    }
    public override void OnEnter()
    {
        _animator.CrossFade(RunHash, crossFadeDuration);

        // Lần đầu vào Chase: đặt đích ngay lập tức (không chờ hết timer)
        agent.SetDestination(player.position);
        lastDestination = player.position;
        repathTimer = repathInterval;
    }

    public override void Update()
    {
        repathTimer -= Time.deltaTime;

        // Chỉ cập nhật đích khi: đã qua khoảng thời gian tối thiểu VÀ player đã dời đủ xa
        if (repathTimer <= 0f &&
            (player.position - lastDestination).sqrMagnitude > repathThreshold * repathThreshold)
        {
            agent.SetDestination(player.position);
            lastDestination = player.position;
            repathTimer = repathInterval;
        }
    }
}