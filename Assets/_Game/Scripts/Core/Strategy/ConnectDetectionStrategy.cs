using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectDetectionStrategy : IDetectionStrategy
{
    readonly float detectionAngle = 60f;
    readonly float detectionRadius = 10f;
    readonly float innerDetectionRadius = 5f;
    public ConnectDetectionStrategy(float detectionAngle, float detectionRadius, float innerDetectionRadius)
    {
        this.detectionAngle = detectionAngle;
        this.detectionRadius = detectionRadius;
        this.innerDetectionRadius = innerDetectionRadius;
    }
    public bool Execute(Transform player, Transform detector, CountdownTimer countdownTimer)
    {
        if (countdownTimer.IsRunning) return false;

        var directionPlayer = player.position - detector.position;
        var angleToPlayer = Vector3.Angle(directionPlayer, detector.forward);

        //if player is outside of detection angle or outside of detection radius or inside of inner detection radius, return false
        if (!(angleToPlayer < detectionAngle / 2f) || !(directionPlayer.magnitude < detectionRadius)
        && !(directionPlayer.magnitude > innerDetectionRadius)) return false;

        countdownTimer.Start();
        return true;
    }
}