using System;
using UnityEngine;
using System.Collections;
    public class Wait : Action
    {
        public float Duration = 1f;
        public float DurationRandom = 0f;
        public Wait(float duration, float durationRandom = 0f)
        {
            Duration = duration;
            DurationRandom = durationRandom;
        }
        protected override void OnEntry()
        {
            timer = Duration + UnityEngine.Random.Range(0f, DurationRandom);
        }
        protected override TaskStatus OnUpdate()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f )
            {
                timer = 0f;
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
