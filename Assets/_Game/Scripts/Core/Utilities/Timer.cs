using System;
using UnityEngine;
using Unity.VisualScripting;

public abstract class Timer
{
    protected float initialTime;
    public float Time { get; protected set; }
    public bool IsRunning { get; private set; } = false;

    public float progress => initialTime > 0 ? Time / initialTime : 0f; // 0f if initialTime is 0 to avoid division by zero

    public Action OnTimerStarted = delegate { };
    public Action OnTimerStopped = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
        IsRunning = false;
    }

    public void Start()
    {
        Time = initialTime;
        if (!IsRunning)
        {
            IsRunning = true;
            OnTimerStarted.Invoke();
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            OnTimerStopped.Invoke();
        }
    }
    public void Pause() => IsRunning = false; //stop the timer without resetting it
    public void Resume() => IsRunning = true; //continue from where it was stopped
    public abstract void Tick(float deltaTime);
}

//countdown / cooldown timer
public class CountdownTimer : Timer
{
    public CountdownTimer(float value) : base(value) { }
    public override void Tick(float deltaTime)
    {
        if (IsRunning && Time > 0f)
        {
            Time -= deltaTime;
        }

        if (IsRunning && Time <= 0f)
        {
            Stop();
        }
    }
    public bool IsFinished() => Time <= 0f;
    public void Reset() => Time = initialTime;
    public void Reset(float value)
    {
        initialTime = value;
        Reset();
    }
}

//stopwatch timer
public class StopwatchTimer : Timer
{
    public StopwatchTimer() : base(0) { }
    public override void Tick(float deltaTime)
    {
        if (IsRunning)
        {
            Time += deltaTime;
        }
    }
    public void Reset() => Time = 0f;
    public float GetTime() => Time;
}