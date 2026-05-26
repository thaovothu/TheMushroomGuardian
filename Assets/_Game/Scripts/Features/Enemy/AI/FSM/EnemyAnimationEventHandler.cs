using UnityEngine;
public class EnemyAnimationEventHandler : MonoBehaviour
{
    EnemyController _controller;

    void Awake()
    {
        // Leo lên parent để tìm EnemyController
        _controller = GetComponentInParent<EnemyController>();
    }

    public void OnAttack()
    {
        _controller?.Attack();
    }
}