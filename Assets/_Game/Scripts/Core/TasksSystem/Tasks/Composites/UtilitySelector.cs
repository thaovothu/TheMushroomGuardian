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
        
        Debug.Log($"[UtilitySelector] Switching from {(currentTask != null ? currentTask.GetType().Name : "NULL")} to {bestTask.GetType().Name}, Utility={bestTask.GetUtility()}");
        
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
            Debug.Log($"[UtilitySelector] Running task: {currentTask.GetType().Name}, Name: {currentTask}");
            TaskStatus status = Task.Update(currentTask);
            Debug.Log($"[UtilitySelector] Task returned: {status}");
            
            if (status == TaskStatus.Success)
            {
                Task.Stop(currentTask);
                Debug.Log("[UtilitySelector] Task succeeded, will reselect next frame");
                return TaskStatus.Success;
            }
            else if (status == TaskStatus.Failure)
            {
                Task.Stop(currentTask);
                currentTask = null;
                Debug.Log("[UtilitySelector] Task failed, will reselect next frame");
            }
        }
        return TaskStatus.Running;
    }
    protected Task SelectTaskByUtility(List<Task> tasks)
    {
        Debug.Log($"[UtilitySelector] SelectTaskByUtility called with {tasks?.Count ?? 0} tasks");
        
        if(tasks == null || tasks.Count == 0)
        {
            Debug.LogWarning("[UtilitySelector] No tasks available!");
            return null;
        }
        if (tasks.Count == 1)
        {
            Debug.Log($"[UtilitySelector] Only 1 task: {tasks[0].GetType().Name}");
            return tasks[0];
        }
        Task bestTask = null;
        float hightestUtility = float.MinValue;
        foreach(Task task in tasks)
        {
            float utility = task.GetUtility();
            Debug.Log($"[UtilitySelector] Task: {task.GetType().Name}, Utility: {utility}");
            if(utility > hightestUtility)
            {
                hightestUtility = utility;
                bestTask = task;
            }
        }
        return bestTask;
    }
}