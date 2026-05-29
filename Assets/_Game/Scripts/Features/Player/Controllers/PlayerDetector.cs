using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] float detectionAngle = 60f;
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float innerDetectionRadius = 5f;
    [SerializeField] float detectionCooldown = 1f;
    [SerializeField] float attackRange = 2f;
    public float AttackRange => attackRange;

    public Transform Player { get; private set; }
    CountdownTimer detectionTimer;

    IDetectionStrategy detectionStrategy;

    void Awake()
    {
        // Debug.Log("[PlayerDetector] Awake started");
        
        detectionTimer = new CountdownTimer(detectionCooldown);
        // Debug.Log("[PlayerDetector] Detection timer created");
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Player = playerObj.transform;
            // Debug.Log("[PlayerDetector] Player found in Awake");
        }
        else
        {
            // Debug.LogWarning("[PlayerDetector] Player not found in Awake - will search later");
        }
        
        try
        {
            detectionStrategy = new ConnectDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
            Debug.Log("[PlayerDetector] Detection strategy created successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerDetector] Error creating detection strategy: {e.Message}");
        }
    }

    void Update()
    {
        detectionTimer.Tick(Time.deltaTime);
        // //Debug.Log("hih"+Player.name);   
    }
    public bool CanDetectPlayer()
    {
        // Tìm lại Player nếu nó null (có thể chưa spawn lúc Awake)
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player = playerObj.transform;
                // Debug.Log("[PlayerDetector] Player found after spawn!");
            }
            else
            {
                // Debug.LogError("[PlayerDetector] Player not found in scene!");
                return false;
            }
        }
        
        if (detectionStrategy == null)
        {
            // Debug.LogError("[PlayerDetector] Detection strategy is null!");
            return false;
        }
        
        if (detectionTimer == null)
        {
            // Debug.LogError("[PlayerDetector] Detection timer is null!");
            return false;
        }
        
        return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
    }

    public bool CanAttackPlayer()
    {
        if (Player == null)
            return false;
            
        var directionToPlayer = Player.position - transform.position;
        return directionToPlayer.magnitude <= attackRange;
    }

    public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) => this.detectionStrategy = detectionStrategy;


    //Draw gizmos to visualize detection radius and angle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);

        Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2f, 0) * transform.forward*detectionRadius;
        Vector3 backwardConeDirection = Quaternion.Euler(0, -detectionAngle / 2f, 0) * transform.forward * detectionRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
        Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
    }
}
