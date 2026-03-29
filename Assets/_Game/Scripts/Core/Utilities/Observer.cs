using System;
using System.Collections.Generic;

namespace Game.Core.Utilities
{
    public interface IObserver<T>
    {
        void OnNotify(T data);
    }

    public class Observable<T>
    {
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();

        public void Subscribe(IObserver<T> observer)
        {
            if (observer != null && !observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void Unsubscribe(IObserver<T> observer)
        {
            if (observer != null)
            {
                observers.Remove(observer);
            }
        }

        public void Notify(T data)
        {
            foreach (IObserver<T> observer in observers)
            {
                observer.OnNotify(data);
            }
        }

        public void Clear()
        {
            observers.Clear();
        }

        public int GetObserverCount()
        {
            return observers.Count;
        }
    }

    public class SimpleObservable
    {
        private readonly List<Action> observers = new List<Action>();

        public void Subscribe(Action callback)
        {
            if (callback != null && !observers.Contains(callback))
            {
                observers.Add(callback);
            }
        }

        public void Unsubscribe(Action callback)
        {
            if (callback != null)
            {
                observers.Remove(callback);
            }
        }

        public void Notify()
        {
            foreach (var observer in observers)
            {
                observer?.Invoke();
            }
        }

        public void Clear()
        {
            observers.Clear();
        }

        public int GetObserverCount()
        {
            return observers.Count;
        }
    }
}
