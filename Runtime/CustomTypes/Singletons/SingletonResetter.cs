#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUtils.Runtime.CustomTypes.Singletons
{
    internal static class SingletonResetter
    {
        private static readonly List<Action> _resetActions = new();
        private static readonly object _lock = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAllSingletons()
        {
            lock (_lock)
            {
                foreach (var resetAction in _resetActions)
                {
                    try
                    {
                        resetAction();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("[SingletonResetter::ResetAllSingletons] " +
                                       $"Failed to reset singleton: {ex.Message}");
                    }
                }
            }
        }

        internal static void RegisterResetAction(Action resetAction)
        {
            lock (_lock)
            {
                _resetActions.Add(resetAction);
            }
        }
    }
}
#endif