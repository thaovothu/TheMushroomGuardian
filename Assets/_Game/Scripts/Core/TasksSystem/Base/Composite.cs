using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Composite : Task
{
    protected List<Task> tasks = new List<Task>();
    public IReadOnlyCollection<Task> Tasks => tasks.AsReadOnly();
    public void ClearTasks()
    {
        this.tasks.Clear();
    }
    public void AddTask(Task task)
    {
        tasks.Add(task);
        UnityEngine.Debug.Log($"[Composite] Added task: {task.GetType().Name}, Total: {tasks.Count}");
    }
    public void CreateTasks(params Task[] tasks)
    {
        ClearTasks();
        foreach (Task task in tasks)
        {
            AddTask(task);
        }
        UnityEngine.Debug.Log($"[Composite] CreateTasks: Created {this.tasks.Count} tasks");
    }

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        foreach (Task task in tasks)
        {
            task.SetOwner(owner);
        }
    }
    protected override void OnAwake()
    {
        foreach (Task task in tasks)
        {
            Task.Awaken(task);
        }
    }
    protected override void OnExit()
    {
        foreach(Task task in tasks)        
        {
            if(task.Active)
            {
                Task.Stop(task);
            }
        }
    }

    public override float GetUtility()
    {
        float maxUtility = 0f;
        foreach (Task task in tasks)
        {
            float utility = task.GetUtility();
            if (utility > maxUtility)
            {
                maxUtility = utility;
            }
        }
        return maxUtility + DefaultUtility;
    }
}
