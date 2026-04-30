using UnityEngine;

public class Sequence : Composite
{
    int currentTaskIndex = 0;
    Task currentTask
    {
        get
        {
            try
            {
                return tasks[currentTaskIndex];
            }
            catch
            {
                Debug.LogWarning($"Current task index {currentTaskIndex} is out of range for tasks count {tasks.Count}");
                return null;
            }
            
        }
    }
    bool IsCurrentTaskIndexValid()
    {
        return currentTaskIndex >= 0 && currentTaskIndex < tasks.Count;
    }

    protected override void OnDrawGizmos()
    {
        if (IsCurrentTaskIndexValid())
        {
            currentTask.DrawGizmosSelected();
        }
    }
    protected override void OnDrawGizmosSelected() {
        if (IsCurrentTaskIndexValid())
        {
            currentTask.DrawGizmosSelected();
        }
    }
    protected override void OnEntry()
    {
        currentTaskIndex = 0;
        UnityEngine.Debug.Log($"[Sequence] OnEntry - Starting with {tasks.Count} tasks");
        if (IsCurrentTaskIndexValid())
        {
            UnityEngine.Debug.Log($"[Sequence] OnEntry - Starting first task: {currentTask.GetType().Name}");
            Task.Start(currentTask);
        }
    }
    protected override TaskStatus OnUpdate()
    {
        if (tasks.Count <= 0)
        {
            return TaskStatus.Failure;
        }

        UnityEngine.Debug.Log($"[Sequence] Updating task {currentTaskIndex}: {currentTask.GetType().Name}");
        TaskStatus status = Task.Update(currentTask);
        UnityEngine.Debug.Log($"[Sequence] Task {currentTaskIndex} returned: {status}");
        
        if (status == TaskStatus.Success)
        {
            Task.Stop(currentTask);
            currentTaskIndex++;
            if (currentTaskIndex >= tasks.Count)
            {
                UnityEngine.Debug.Log($"[Sequence] All tasks completed! Sequence Success");
                return TaskStatus.Success;
            }
            else
            {
                UnityEngine.Debug.Log($"[Sequence] Moving to next task: {currentTaskIndex}");
                Task.Start(currentTask);
                
            }
        }
        else if (status == TaskStatus.Failure)
        {
            Task.Stop(currentTask);
            UnityEngine.Debug.Log($"[Sequence] Task failed at index {currentTaskIndex}! Sequence Failed");
            return TaskStatus.Failure;
        }
        
        return TaskStatus.Running;
    }
}