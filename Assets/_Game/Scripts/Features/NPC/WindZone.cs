using UnityEngine;

public class WindZone : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private float dashSpeedThreshold = 15f;
    [SerializeField] private float slowDownRate = 0.85f;

    [Header("Quest Complete khi dash qua")]
    [SerializeField] private bool completeQuestOnPass = false;
    [SerializeField] private int questId;
    [SerializeField] private int stepId;

    private bool hasCompleted = false;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var rb = other.GetComponent<Rigidbody>();
        var controller = other.GetComponent<PlayerController>();
        if (rb == null || controller == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed >= dashSpeedThreshold)
        {
            controller.IsInWindZone = false;
            return;
        }

        controller.IsInWindZone = true;
        rb.velocity = new Vector3(
            rb.velocity.x * slowDownRate,
            rb.velocity.y,
            rb.velocity.z * slowDownRate
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var controller = other.GetComponent<PlayerController>();
        if (controller != null) controller.IsInWindZone = false;

        // Kiểm tra exit với tốc độ cao = dash qua thành công
        if (!completeQuestOnPass || hasCompleted) return;

        var rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed >= dashSpeedThreshold)
        {
            hasCompleted = true;
            Debug.Log($"[WindZone] Player dashed through! Completing Quest {questId} Step {stepId}");

            if (QuestProgressManager.Instance != null && QuestDataManager.Instance != null)
            {
                int maxStep = QuestDataManager.Instance.GetMaxStepId(questId);
                QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStep);
            }
        }
    }

    // Reset khi quest reset (nếu cần)
    public void ResetZone() => hasCompleted = false;
}