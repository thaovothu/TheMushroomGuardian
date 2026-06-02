// using UnityEngine;

// /// <summary>
// /// Trả về Success nếu player gần hơn min range (cần kite).
// /// Dùng cho linh hồn lảng tránh khi player áp sát.
// /// </summary>
// public class IsPlayerTooClose : Task
// {
//     private BossBlackboard bb;
//     private float minRange;

//     public IsPlayerTooClose(float minRange = 3f)
//     {
//         this.minRange = minRange;
//     }

//     protected override void OnAwake()
//     {
//         bb = Owner.GetComponent<BossBlackboard>();
//     }

//     protected override TaskStatus OnUpdate()
//     {
//         if (bb == null || bb.player == null) return TaskStatus.Failure;
//         return bb.distanceToPlayer < minRange
//             ? TaskStatus.Success
//             : TaskStatus.Failure;
//     }
// }