using System.Collections.Generic;
using UnityEngine;
public class UtilitySelector : Composite
{   
    protected Task currentTask;
    public Task CurrentTask => currentTask;
    protected override void OnEntry()
    {
        currentTask = null;
    }
    protected override void OnDrawGizmos()
    {
        if (currentTask != null)
        {
            currentTask.DrawGizmos();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        if (currentTask != null)
        {
            currentTask.DrawGizmosSelected();
        }
    }
    public override float GetUtility()
    {
        if(currentTask != null)
        {
            return currentTask.GetUtility();
        }
        return DefaultUtility;
    }
    void SelectTask()
    {
        Task bestTask = SelectTaskByUtility(tasks);
        if (bestTask == null)
        {
            Debug.LogWarning("UtilitySelector has no tasks to select from.");
        }

        if (bestTask == currentTask)
        {
            return;
        }
        if (currentTask != null && currentTask.Active)
        {
            Task.Stop(currentTask);
        }

        currentTask = bestTask;
        Task.Start(currentTask);
    }
    protected override TaskStatus OnUpdate()
    {
        SelectTask();
        if (currentTask != null)
        {
            TaskStatus status = Task.Update(currentTask);
            if (status == TaskStatus.Success)
            {
                Task.Stop(currentTask);
                return TaskStatus.Success;
            }
            else if (status == TaskStatus.Failure)
            {
                Task.Stop(currentTask);
                currentTask = null;
            }
        }
        return TaskStatus.Running;
    }
    protected Task SelectTaskByUtility(List<Task> tasks)
    {
        if(tasks == null || tasks.Count == 0)
        {
            return null;
        }
        if (tasks.Count == 1)
        {
            return tasks[0];
        }
        Task bestTask = null;
        float hightestUtility = float.MinValue;
        foreach(Task task in tasks)
        {
            float utility = task.GetUtility();
            if(utility > hightestUtility)
            {
                hightestUtility = utility;
                bestTask = task;
            }
        }
        return bestTask;
    }
}