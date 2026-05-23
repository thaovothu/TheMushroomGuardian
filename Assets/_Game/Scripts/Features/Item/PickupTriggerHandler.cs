using UnityEngine;

/// <summary>
/// Handler nhỏ trên PickupTrigger child — forward OnTriggerEnter lên ItemPickup cha.
/// Tách ra để tránh conflict giữa trigger và physics collider trên cùng 1 GameObject.
/// </summary>
public class PickupTriggerHandler : MonoBehaviour
{
    private ItemPickup parent;

    public void Setup(ItemPickup itemPickup)
    {
        parent = itemPickup;
    }

    private void OnTriggerEnter(Collider other)
    {
        parent?.OnTriggerPickup(other);
    }
}