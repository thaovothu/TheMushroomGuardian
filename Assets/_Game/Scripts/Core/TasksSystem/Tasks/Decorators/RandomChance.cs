using System;
using UnityEngine;
using System.Collections;
public class RandomChance : Decorator
{
    public float Chance = 0.5f;
    private bool chanceIsSuccess = false;
    public RandomChance(Task childTask, float chance = 0.5f) : base(childTask)
    {
        this.Chance = chance;
    }

    bool EvaluateChance(float chance)
    {
        if (chance <= 0f)
        {
            return false;
        }

        if (chance >= 1f)
        {
            return true;
        }

        return chance > UnityEngine.Random.Range(0f, 1f);
    }

    protected override void OnEntry()
    {
        if (EvaluateChance(Chance))
        {
            chanceIsSuccess = true;
            Task.Start(child);
        }
        else
        {
            chanceIsSuccess = false;
        }
    }
    protected override TaskStatus OnUpdate()
    {
        if (child == null)
        {
            return TaskStatus.Failure;
        }
        if (!chanceIsSuccess)
        {
            return TaskStatus.Success;
        }
        TaskStatus status = Task.Update(child);
        if (status == TaskStatus.Success || status == TaskStatus.Failure)
        {
            Task.Stop(child);
            return status;
        }

        return TaskStatus.Running;
    }
}