using System;
using UnityEngine;
using System.Collections;

    public class Log : Action
    {
        public string Text;
        public Log(string text)
        {
            Text = text;
        }
        protected override TaskStatus OnUpdate()
        {
            Debug.Log(Text);
            return TaskStatus.Success;
        }
    }