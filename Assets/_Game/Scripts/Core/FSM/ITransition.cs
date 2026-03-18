using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Core.FSM
{
    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
    
}