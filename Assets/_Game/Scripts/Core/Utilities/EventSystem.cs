// using System;
// using System.Collections.Generic;

// namespace Game.Core.Utilities
// {
//     public static class EventSystem
//     {
//         private static readonly Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

//         public static void Subscribe<T>(string eventName, Action<T> callback)
//         {
//             if (string.IsNullOrEmpty(eventName) || callback == null)
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], callback);
//             }
//             else
//             {
//                 eventDictionary[eventName] = callback;
//             }
//         }

//         public static void Subscribe(string eventName, Action callback)
//         {
//             if (string.IsNullOrEmpty(eventName) || callback == null)
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], callback);
//             }
//             else
//             {
//                 eventDictionary[eventName] = callback;
//             }
//         }

//         public static void Unsubscribe<T>(string eventName, Action<T> callback)
//         {
//             if (string.IsNullOrEmpty(eventName) || callback == null)
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], callback);
//                 if (eventDictionary[eventName] == null)
//                 {
//                     eventDictionary.Remove(eventName);
//                 }
//             }
//         }

//         public static void Unsubscribe(string eventName, Action callback)
//         {
//             if (string.IsNullOrEmpty(eventName) || callback == null)
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], callback);
//                 if (eventDictionary[eventName] == null)
//                 {
//                     eventDictionary.Remove(eventName);
//                 }
//             }
//         }

//         public static void Publish<T>(string eventName, T data)
//         {
//             if (string.IsNullOrEmpty(eventName))
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 if (eventDictionary[eventName] is Action<T> action)
//                 {
//                     action?.Invoke(data);
//                 }
//             }
//         }

//         public static void Publish(string eventName)
//         {
//             if (string.IsNullOrEmpty(eventName))
//                 return;

//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 if (eventDictionary[eventName] is Action action)
//                 {
//                     action?.Invoke();
//                 }
//             }
//         }

//         public static void Clear()
//         {
//             eventDictionary.Clear();
//         }

//         public static void RemoveEvent(string eventName)
//         {
//             if (eventDictionary.ContainsKey(eventName))
//             {
//                 eventDictionary.Remove(eventName);
//             }
//         }
//     }
// }
