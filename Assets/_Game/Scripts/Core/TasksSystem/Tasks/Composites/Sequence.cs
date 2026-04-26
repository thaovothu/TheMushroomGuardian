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
        if (IsCurrentTaskIndexValid())
        {
            Task.Start(currentTask);
        }
    }
    protected override TaskStatus OnUpdate()
    {
        if (tasks.Count <= 0)
        {
            return TaskStatus.Failure;
        }

        TaskStatus status = Task.Update(currentTask);
        if (status == TaskStatus.Success)
        {
            Task.Stop(currentTask);
            currentTaskIndex++;
            if (currentTaskIndex >= tasks.Count)
            {
                return TaskStatus.Success;
            }
            else
            {
                Task.Start(currentTask);
                
            }
        }
        else if (status == TaskStatus.Failure)
        {
            Task.Stop(currentTask);
            return TaskStatus.Failure;
        }
        
        return TaskStatus.Running;
    }
}