using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDetectionStrategy
{
    bool Execute (Transform player, Transform detector, CountdownTimer countdownTimer); 
}
