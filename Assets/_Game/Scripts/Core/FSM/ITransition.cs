using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

// namespace Game.Core.FSM
// {
    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
    
