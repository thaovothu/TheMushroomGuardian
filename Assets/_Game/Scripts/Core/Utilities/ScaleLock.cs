using UnityEngine;

/// <summary>
/// Khoá localScale của Transform lại, ghi đè bất kỳ thứ gì thay đổi nó trong frame
/// (thường là Animator có scale curve trong clip).
///
/// Cách dùng:
///  - Gắn vào GameObject cụ thể (vd: prefab boss bị animator override scale).
///  - Để <see cref="desiredScale"/> = (0,0,0) thì tự dùng localScale lúc Awake làm scale chuẩn.
///  - Hoặc nhập giá trị mong muốn (vd 2,2,2) trong Inspector.
///
/// Chỉ ảnh hưởng GameObject nào có component này — KHÔNG động chạm boss khác.
/// </summary>
[DisallowMultipleComponent]
public class ScaleLock : MonoBehaviour
{
    [Tooltip("Scale muốn khoá. Để (0,0,0) thì tự lấy localScale hiện tại lúc Awake làm chuẩn.")]
    [SerializeField] private Vector3 desiredScale = Vector3.zero;

    private void Awake()
    {
        if (desiredScale == Vector3.zero)
            desiredScale = transform.localScale;
    }

    // LateUpdate chạy SAU Animator → ghi đè scale mà clip vừa set.
    private void LateUpdate()
    {
        if (transform.localScale != desiredScale)
            transform.localScale = desiredScale;
    }
}
