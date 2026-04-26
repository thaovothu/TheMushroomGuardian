using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExternalBehaviorTree : ScriptableObject
{
    public abstract Task CreateBehaviorTree();
    public virtual void CreateData(BehaviorTreeData data)
    {
        
    }
}
