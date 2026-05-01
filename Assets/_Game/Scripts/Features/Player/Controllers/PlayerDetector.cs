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

    public Transform Player { get; private set; }
    CountdownTimer detectionTimer;

    IDetectionStrategy detectionStrategy;

    void Awake()
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        detectionStrategy = new ConnectDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
    }

    void Update()
    {
        detectionTimer.Tick(Time.deltaTime);
        // //Debug.Log("hih"+Player.name);   
    }
    public bool CanDetectPlayer()
    {
        return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
    }

    public bool CanAttackPlayer()
    {
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
