using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class Transition : ITransition
    {
        public IState To { get; private set; }
        public IPredicate Condition { get; private set; }

        public Transition(IState to, IPredicate condition)
        {
            this.To = to;
            this.Condition = condition;
        }
    }
