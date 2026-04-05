using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] float detectionAngle = 60f;
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float innerDetectionRadius = 5f;
    [SerializeField] float detectionCooldown = 1f;

    public Transform Player { get; private set; }
    CountdownTimer detectionTimer;

    IDetectionStrategy detectionStrategy;

    void Start()
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        detectionStrategy = new ConnectDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
    }

    void Update()
    {
        detectionTimer.Tick(Time.deltaTime);
    }
    public bool CanDetectPlayer()
    {
        return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
    }

    public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) => this.detectionStrategy = detectionStrategy;
}
