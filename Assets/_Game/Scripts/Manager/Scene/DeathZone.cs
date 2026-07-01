using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private bool onlyPlayer = true;

    [Header("Debug")]
    [SerializeField] private bool logKill = true;

    private void OnTriggerEnter(Collider other)
    {
        if (onlyPlayer && !other.CompareTag("Player")) return;

        var healthSystem = other.GetComponent<HealthSystem>();
        if (healthSystem == null) return;

        if (logKill)
            Debug.Log($"[DeathZone] {other.name} entered death zone -> kill");

        healthSystem.Kill();
    }
}