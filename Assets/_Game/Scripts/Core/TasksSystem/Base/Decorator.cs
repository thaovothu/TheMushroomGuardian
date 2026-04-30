using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decorator : Task
{
    protected Task child;
    public Task Child => child;
    public Decorator(Task childTask)    
    {
        child = childTask;
    }
    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        child.SetOwner(owner);
    }
    protected override void OnAwake()
    {
        Task.Awaken(child);
    }
    protected override void OnEntry()
    {
        Task.Start(child);
    }
    protected override void OnExit()
    {
        Task.Stop(child);
    }

}
