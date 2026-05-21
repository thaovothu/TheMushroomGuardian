using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace TheMushroomGuardian.Core.Events
{
    #region Event Enums
    public enum PlayerEvent
    {
        Damaged,
        Healed,
        Died,
        LevelUp,
        ItemPickup
    }

    public enum BossEvent
    {
        Spawned,
        Defeated,
        PhaseChange,
        SpecialAttack
    }

    public enum GameEvent_Enum
    {
        GameStart,
        GameEnd,
        GamePause,
        GameResume,
        LevelComplete,
        LevelFailed
    }

    public enum EnemyEvent
    {
        Spawned,
        Defeated,
        Damaged
    }
    #endregion

    /// <summary>
    /// Global event system manager using enums.
    /// Usage:
    /// GameEvent.Instance.AddEventListener(PlayerEvent.Damaged, OnPlayerDamaged);
    /// GameEvent.Instance.InvokeEvent(PlayerEvent.Damaged);
    /// </summary>
    public class GameEvent : BaseSingleton<GameEvent>
    {
        // Dictionary to store events for each enum type
        private Dictionary<System.Enum, UnityEvent> _events = new();
        private Dictionary<System.Enum, UnityEvent<int>> _eventsInt = new();

        protected override void Awake()
        {
            base.Awake();
            InitializeEvents();
        }

        /// <summary>
        /// Initialize all events in the dictionaries.
        /// </summary>
        private void InitializeEvents()
        {
            // Initialize no-parameter events
            foreach (PlayerEvent evt in System.Enum.GetValues(typeof(PlayerEvent)))
                _events[evt] = new UnityEvent();

            foreach (BossEvent evt in System.Enum.GetValues(typeof(BossEvent)))
                _events[evt] = new UnityEvent();

            foreach (GameEvent_Enum evt in System.Enum.GetValues(typeof(GameEvent_Enum)))
                _events[evt] = new UnityEvent();

            foreach (EnemyEvent evt in System.Enum.GetValues(typeof(EnemyEvent)))
                _eventsInt[evt] = new UnityEvent<int>();
        }

        #region Invoke Methods
        /// <summary>
        /// Invoke an event without parameters.
        /// Usage: GameEvent.Instance.InvokeEvent(PlayerEvent.Damaged);
        /// </summary>
        public void InvokeEvent(PlayerEvent evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.Invoke();
        }

        public void InvokeEvent(BossEvent evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.Invoke();
        }

        public void InvokeEvent(GameEvent_Enum evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.Invoke();
        }

        /// <summary>
        /// Invoke an event with int parameter.
        /// </summary>
        public void InvokeEvent(EnemyEvent evt, int parameter)
        {
            if (_eventsInt.ContainsKey(evt))
                _eventsInt[evt]?.Invoke(parameter);
        }
        #endregion

        #region AddListener Methods
        /// <summary>
        /// Add listener to an event.
        /// Usage: GameEvent.Instance.AddEventListener(PlayerEvent.Damaged, OnDamage);
        /// </summary>
        public void AddEventListener(PlayerEvent evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.AddListener(callback);
        }

        public void AddEventListener(BossEvent evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.AddListener(callback);
        }

        public void AddEventListener(GameEvent_Enum evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.AddListener(callback);
        }

        /// <summary>
        /// Add listener to an event with int parameter.
        /// </summary>
        public void AddEventListener(EnemyEvent evt, UnityAction<int> callback)
        {
            if (_eventsInt.ContainsKey(evt))
                _eventsInt[evt]?.AddListener(callback);
        }
        #endregion

        #region RemoveListener Methods
        /// <summary>
        /// Remove listener from an event.
        /// </summary>
        public void RemoveEventListener(PlayerEvent evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveListener(callback);
        }

        public void RemoveEventListener(BossEvent evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveListener(callback);
        }

        public void RemoveEventListener(GameEvent_Enum evt, UnityAction callback)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveListener(callback);
        }

        public void RemoveEventListener(EnemyEvent evt, UnityAction<int> callback)
        {
            if (_eventsInt.ContainsKey(evt))
                _eventsInt[evt]?.RemoveListener(callback);
        }
        #endregion

        #region Clear Methods
        /// <summary>
        /// Clear all listeners from an event.
        /// </summary>
        public void ClearEvent(PlayerEvent evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveAllListeners();
        }

        public void ClearEvent(BossEvent evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveAllListeners();
        }

        public void ClearEvent(GameEvent_Enum evt)
        {
            if (_events.ContainsKey(evt))
                _events[evt]?.RemoveAllListeners();
        }

        public void ClearEvent(EnemyEvent evt)
        {
            if (_eventsInt.ContainsKey(evt))
                _eventsInt[evt]?.RemoveAllListeners();
        }
        #endregion
    }
}
