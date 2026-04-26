using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : MonoBehaviour
{
    [SerializeField]
    bool RestartWhenDone = true;

    [SerializeField]
    ExternalBehaviorTree externalBehaviorTree;
    public BehaviorTreeData Data = new();
    bool hasAwaken = false;
    public Task Root;
    internal int Restarts = 0;
    public void Awake()
    {
        hasAwaken = false;
    }
    private void OnDrawGizmos()
    {
        if (Root == null)
        {
            return;
        }

        Root.DrawGizmos();
    }
    private void OnDrawGizmosSelected()
    {
        if (Root == null)
        {
            return;
        }

        Root.DrawGizmosSelected();
    }

    private void Update()
    {
        if (!hasAwaken)
        {
            AwakenRootTask();

            hasAwaken = true;
        }
        if (Root == null)
        {
            return;
        }

        if (Root != null && Root.Status == TaskStatus.Inactive)
        {
            return;
        }
        TaskStatus status = Task.Update(Root);
        if (status == TaskStatus.Success || status == TaskStatus.Failure)
        {
            if(RestartWhenDone)
            {
                Task.Restart(Root);
                Restarts++;
            }
            else
            {
                Task.Stop(Root);
            }
        }
    }       
    private void AwakenRootTask()
    {
        if (externalBehaviorTree != null)
        {
            Root = externalBehaviorTree.CreateBehaviorTree();
            externalBehaviorTree.CreateData(Data);
        }

        if(Root == null)
        {
            Debug.LogError("BehaviorTree has no root task assigned.");
            return;
        }

        Root.SetOwner(gameObject);
        Task.Awaken(Root);
        Task.Start(Root);
    }
}
