using System;
using System.Collections.Generic;

namespace Game.Core.Utilities
{
    public interface IState<T>
    {
        void OnEnter(T context);
        void OnExit(T context);
        void OnUpdate(T context, float deltaTime);
    }

    public class StateMachineGeneric<T> where T : class
    {
        private IState<T> currentState;
        private readonly Dictionary<Type, IState<T>> states = new Dictionary<Type, IState<T>>();
        private readonly T context;

        private readonly Dictionary<Type, Dictionary<Type, Func<bool>>> transitions =
            new Dictionary<Type, Dictionary<Type, Func<bool>>>();

        public IState<T> CurrentState => currentState;

        public StateMachineGeneric(T context)
        {
            this.context = context;
        }

        public void RegisterState(IState<T> state)
        {
            var stateType = state.GetType();
            if (!states.ContainsKey(stateType))
            {
                states[stateType] = state;
            }
        }

        public void AddTransition<TFrom, TTo>(Func<bool> condition) where TFrom : IState<T> where TTo : IState<T>
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            if (!transitions.ContainsKey(fromType))
            {
                transitions[fromType] = new Dictionary<Type, Func<bool>>();
            }

            transitions[fromType][toType] = condition;
        }

        public void SetState<TState>() where TState : IState<T>
        {
            var stateType = typeof(TState);
            if (!states.ContainsKey(stateType))
                return;

            if (currentState != null)
            {
                currentState.OnExit(context);
            }

            currentState = states[stateType];
            currentState.OnEnter(context);
        }

        public void Update(float deltaTime)
        {
            CheckTransitions();
            currentState?.OnUpdate(context, deltaTime);
        }

        private void CheckTransitions()
        {
            if (currentState == null)
                return;

            var currentType = currentState.GetType();
            if (transitions.ContainsKey(currentType))
            {
                foreach (var transition in transitions[currentType])
                {
                    if (transition.Value?.Invoke() == true)
                    {
                        var nextState = states[transition.Key];
                        currentState.OnExit(context);
                        currentState = nextState;
                        currentState.OnEnter(context);
                        break;
                    }
                }
            }
        }
    }
}
