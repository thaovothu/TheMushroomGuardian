using System;
using UnityEngine;
using System.Collections;
public class Idle : Action
{
    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}
