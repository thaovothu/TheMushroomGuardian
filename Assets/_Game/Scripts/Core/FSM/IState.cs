using System.Collections;
using System.Collections.Generic;

    public interface IState
    {
        void OnEnter() { }
        void OnExit() { }
        void Update() { }
        void FixedUpdate() { }
    }

