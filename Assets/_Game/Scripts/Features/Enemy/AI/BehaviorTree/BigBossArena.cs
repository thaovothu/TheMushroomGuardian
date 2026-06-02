using UnityEngine;

/// <summary>
/// Đặt SẴN trong scene đấu boss (vd Scene5) để định nghĩa vị trí CỐ ĐỊNH cho các trụ của BigBoss.
/// BigBoss spawn lúc runtime sẽ tự tìm component này (FindObjectOfType) và spawn trụ theo pillarPoints,
/// nên không cần prefab tham chiếu trực tiếp object trong scene.
///
/// Thứ tự ưu tiên khi spawn trụ (trong BigBossAbilities):
///   1. pillarSpawnPoints gán trên chính boss (con của prefab) — toạ độ tương đối boss.
///   2. BigBossArena trong scene — toạ độ cố định trên sàn.  ← dùng cái này cho sàn cố định.
///   3. Tự xếp vòng tròn quanh boss.
/// </summary>
public class BigBossArena : MonoBehaviour
{
    [Tooltip("Các vị trí đặt trụ. Tạo empty GameObject làm con của object này, kéo tới chỗ muốn rồi gán vào đây.")]
    public Transform[] pillarPoints;

    private void OnDrawGizmos()
    {
        if (pillarPoints == null) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.7f);
        foreach (var p in pillarPoints)
            if (p != null)
            {
                Gizmos.DrawWireSphere(p.position, 0.6f);
                Gizmos.DrawLine(p.position, p.position + Vector3.up * 2f);
            }
    }
}
