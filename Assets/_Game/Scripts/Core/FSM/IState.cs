using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

// namespace Game.Core.FSM
// {
    public interface IState
    {
        void OnEnter() { }
        void OnExit() { }
        void Update() { }
        void FixedUpdate() { }
    }

