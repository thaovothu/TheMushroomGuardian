using UnityEngine;

public class SwitchElement : Action
{
    private BossBlackboard bb;
    private ElementType targetElement;

    public SwitchElement(ElementType element) => targetElement = element;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb.currentElement == targetElement)
            return TaskStatus.Success; // đã switch rồi, skip

        bb.currentElement = targetElement;
        bb.PlayAnimation(BossAnimState.SwitchElement);

        // Thông báo ra ngoài qua event (Observer pattern đang dùng)
        BossEventBus.OnElementChanged?.Invoke(bb.currentElement);

        //Debug.Log($"[Boss] Switched to {targetElement}");
        return TaskStatus.Success;
    }
}
