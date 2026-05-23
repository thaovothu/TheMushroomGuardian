using System;
using UnityEngine;
using System.Collections;
public class Idle : ActionNode
{
    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }
}
