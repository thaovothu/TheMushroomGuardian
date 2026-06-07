using UnityEngine;

public class AncientPump : MonoBehaviour
{
    [SerializeField] private int pumpIndex;
    [SerializeField] private GameObject idleVFX;
    [SerializeField] private GameObject activatedVFX;

    public bool IsActivated { get; private set; } = false;

    public void ReceiveElementHit(ElementType element)
    {
        if (IsActivated) return;

        if (element != ElementType.Earth && element != ElementType.Wind)
        {
            Debug.Log($"[AncientPump {pumpIndex}] Wrong element: {element}");
            return;
        }

        Activate();
        AncientPumpManager.Instance?.OnPumpActivated();
    }

    private void Activate()
    {
        IsActivated = true;
        if (idleVFX != null) idleVFX.SetActive(false);
        if (activatedVFX != null) activatedVFX.SetActive(true);
        Debug.Log($"[AncientPump {pumpIndex}] Activated!");
    }

    public void Reset()
    {
        IsActivated = false;
        if (idleVFX != null) idleVFX.SetActive(true);
        if (activatedVFX != null) activatedVFX.SetActive(false);
    }
}