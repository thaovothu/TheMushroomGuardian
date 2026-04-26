using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public enum TaskStatus
{
    Success,
    Failure,
    Running,
    Inactive
}

public abstract class Task
{
    private bool hasAwaken = false;
    protected GameObject Owner { get; set; }

    public virtual void SetOwner(GameObject owner)
    {
        Owner = owner;
    }

    public float DefaultUtility = 0f; 

    #region Status
    public TaskStatus Status { get; set; } = TaskStatus.Inactive;
    public bool Active => Status != TaskStatus.Inactive;
    public bool CompareStatus(TaskStatus other)
    {
        return Status == other;
    } 
    #endregion

    #region Virtual Methods
    protected virtual void OnDrawGizmos()
    {
    }
    protected virtual void OnDrawGizmosSelected() { }
    protected virtual void OnAwake()
    {
    }

    protected virtual void OnEntry()
    {
    }

    protected virtual void OnExit()
    {
    }

    protected virtual TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }

    public virtual float GetUtility()
    {
        return DefaultUtility;
    }
    #endregion

    #region Static Task Manipulation Methods
    public static void Awaken(Task task)
    {
        if(task == null)
        {
            Debug.LogWarning("Cannot awaken a null task." );
            return;
        }

        if (task.hasAwaken)
        {
            Debug.LogWarning("Task " + task.FullName + " has already been awaken.");
            return;
        }
        task.OnAwake();
        task.hasAwaken = true;
    }

    public static void Start(Task task)
    {
        if (task == null)
        {
            Debug.LogWarning("Cannot start a null task.");
            return;
        }

        if (task.hasAwaken == false)
        {
            Debug.LogWarning("Task has not been awaken yet.");
        }
        if (task.CompareStatus(TaskStatus.Running))
        {
            Debug.LogWarning("Task " + task.FullName + " is already running.");
            return;
        }

        task.Status = TaskStatus.Running;
        task.OnEntry();
    }

    public static void Stop(Task task)
    {
        if (task == null)
        {
            Debug.LogWarning("Cannot stop a null task.");
            return;
        }

        if (task.hasAwaken == false)
        {
            Debug.LogWarning("Task has not been awaken yet.");
        }
        if (task.CompareStatus(TaskStatus.Running))
        {
            Debug.LogWarning("Task " + task.FullName + " is already running.");
            return;
        }

        task.Status = TaskStatus.Inactive;
        task.OnExit();
    }

    public static void Restart(Task task)
    {
        if (task.CompareStatus(TaskStatus.Inactive))
        {
            Start(task);
        }
        else
        {
            Stop(task);
            Start(task);
        }
    }

    public static TaskStatus Update(Task task)
    {
        if(task == null)
        {
            Debug.LogWarning("Cannot update a null task.");
            return TaskStatus.Failure;
        }

        if (task.Status == TaskStatus.Success || task.Status == TaskStatus.Failure)
        {
            Debug.LogWarning("Task " + task.FullName + " is not running. Cannot update.");
            return TaskStatus.Inactive;  
        }
        TaskStatus status = task.OnUpdate();
        task.Status = status;
        return status;
    }

    public void DrawGizmos() => OnDrawGizmos();
    public void DrawGizmosSelected() => OnDrawGizmosSelected();
    #endregion

    #region Name and BehaviorTree Viewer Parameters
    public virtual string Name {get; set;} = "";
    public string FullName
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
            {
                return GetType().Name;
            }
            else
            {
                return $"{Name} ({GetType().Name})";
            }
        }
    }
    internal bool ViewerShowFoldout = true;
    #endregion
}


