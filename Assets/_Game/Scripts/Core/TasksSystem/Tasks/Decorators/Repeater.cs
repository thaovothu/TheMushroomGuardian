using System;
using UnityEngine;
using System.Collections;
public class Repeater : Decorator
{
    public bool RepeatForever = true;
    public int Iterations = 10;
    public int IterationsRandom = 0;
    public bool EndOnFailure = false;
    private int timeToRepeat = 0;
    public Repeater(Task childTask, bool forever = true, int iterations = 10, int iterationsRandom = 0) : base(childTask)
    {
        this.RepeatForever = forever;
        this.Iterations = iterations;
        this.IterationsRandom = iterationsRandom;
    }

    protected override void OnEntry()
    {
        base.OnEntry();
        timeToRepeat = Iterations + UnityEngine.Random.Range(0, IterationsRandom + 1);
    }
    protected override TaskStatus OnUpdate()
    {
        if (child == null)
        {
            return TaskStatus.Failure;
        }
        TaskStatus status = Task.Update(child);
        if (status == TaskStatus.Failure && EndOnFailure)
        {
            Task.Stop(child);
            return TaskStatus.Failure;
        }
        bool childFinished = status == TaskStatus.Success || status == TaskStatus.Failure;

        if (RepeatForever && childFinished)
        {
            Task.Restart(child);
        }

        if (!RepeatForever && childFinished)
        {
            timeToRepeat--;
            if (timeToRepeat <= 0)
            {
                return TaskStatus.Success;
            }
            else
            {
                Task.Start(child);
            }
        }
        return TaskStatus.Running;
    }
}