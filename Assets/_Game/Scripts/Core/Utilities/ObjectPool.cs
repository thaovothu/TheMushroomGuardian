using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Utilities
{
    public abstract class ObjectPool<T> where T : Component
    {
        private readonly Stack<T> availableObjects = new Stack<T>();
        private readonly List<T> allObjects = new List<T>();
        private readonly GameObject prefab;
        private readonly int initialSize;
        private readonly Transform parent;

        public int AvailableCount => availableObjects.Count;
        public int TotalCount => allObjects.Count;
        public int ActiveCount => allObjects.Count - availableObjects.Count;

        protected ObjectPool(GameObject prefab, int initialSize = 10, Transform parent = null)
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        protected virtual T CreateNewObject()
        {
            GameObject instance = GameObject.Instantiate(prefab, parent);
            T component = instance.GetComponent<T>();
            allObjects.Add(component);
            availableObjects.Push(component);
            component.gameObject.SetActive(false);
            return component;
        }

        public T Get()
        {
            T instance = availableObjects.Count > 0 ? availableObjects.Pop() : CreateNewObject();
            instance.gameObject.SetActive(true);
            OnGet(instance);
            return instance;
        }

        public void Return(T instance)
        {
            if (instance == null || !allObjects.Contains(instance))
                return;

            OnReturn(instance);
            instance.gameObject.SetActive(false);
            availableObjects.Push(instance);
        }

        public void Clear()
        {
            foreach (var obj in allObjects)
            {
                GameObject.Destroy(obj.gameObject);
            }
            allObjects.Clear();
            availableObjects.Clear();
        }

        protected virtual void OnGet(T instance) { }
        protected virtual void OnReturn(T instance) { }
    }
}
