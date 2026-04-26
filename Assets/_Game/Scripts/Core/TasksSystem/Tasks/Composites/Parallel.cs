public class Parallel : Composite
{
    protected override void OnDrawGizmos()
    {
        foreach (Task task in tasks)
        {
            if (task.Active)
            {
                task.DrawGizmos();
            }
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        foreach(Task task in tasks)
        {
            if (task.Active)
            {
                task.DrawGizmosSelected();
            }
        }
    }
    protected override void OnEntry()
    {
        foreach (Task task in tasks)
        {
            Task.Start(task);
        }
    }
    protected override TaskStatus OnUpdate()
    {
        if (tasks.Count <= 0)
        {
            return TaskStatus.Failure;
        }
        foreach (Task task in tasks)
        {
            if (task.Active == false)
            {
                continue;
            }

            TaskStatus status = Task.Update(task);
            if (status == TaskStatus.Failure)
            {
                Task.Stop(task);
                return TaskStatus.Failure;
            }
            else if (status == TaskStatus.Success)
            {
                Task.Stop(task);
            }
        }
        return TaskStatus.Running;
    }
}